using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.GPS
{
    /// <summary>
    /// 좌표(lat/lng) → "시도 시군구"(한글) 오프라인 조회.
    /// KOSTAT 시군구 GeoJSON(Maps\skorea_muni.json)을 읽어 point-in-polygon 으로 판별한다.
    /// - name: 시군구 한글명(예: 종로구)
    /// - code 앞 2자리: 시도 코드 → 시도 한글명
    /// 네트워크/역지오코딩 API 불필요(완전 오프라인). IReverseGeocoder 로 뷰모델에 주입.
    /// </summary>
    public sealed class KoreaRegionLookup : IReverseGeocoder
    {
        private sealed class Poly
        {
            public double[] OX, OY;             // 외곽 링
            public List<double[]> HX, HY;        // 홀(구멍) 링들
        }
        private sealed class Region
        {
            public string Sido, Sigungu;
            public double MinX, MinY, MaxX, MaxY;
            public List<Poly> Polys;
        }

        // 이 KOSTAT 시군구 GeoJSON 의 시도 코드 체계(비표준, code 앞 2자리)
        private static readonly Dictionary<string, string> SidoByCode = new Dictionary<string, string>
        {
            {"11","서울특별시"}, {"21","부산광역시"}, {"22","대구광역시"}, {"23","인천광역시"},
            {"24","광주광역시"}, {"25","대전광역시"}, {"26","울산광역시"}, {"29","세종특별자치시"},
            {"31","경기도"},     {"32","강원특별자치도"}, {"33","충청북도"}, {"34","충청남도"},
            {"35","전북특별자치도"}, {"36","전라남도"}, {"37","경상북도"}, {"38","경상남도"}, {"39","제주특별자치도"},
        };

        private readonly string _path;
        private readonly object _lock = new object();
        private volatile List<Region> _regions;

        public KoreaRegionLookup(string dataPath = null)
        {
            _path = dataPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps", "skorea_muni.json");
            // 시작과 동시에 백그라운드 로드(첫 조회 지연 방지)
            Task.Run(() => EnsureLoaded());
        }

        /// <summary>IReverseGeocoder: 백그라운드 스레드에서 조회(무거운 파싱이 UI를 막지 않도록).</summary>
        public Task<string> GetRegionNameAsync(double latitude, double longitude, CancellationToken ct)
            => Task.Run(() => Lookup(latitude, longitude), ct);

        /// <summary>좌표 → "시도 시군구". 미로드/미매칭 시 null.</summary>
        public string Lookup(double lat, double lng)
        {
            EnsureLoaded();
            var regs = _regions;
            if (regs == null) return null;

            double x = lng, y = lat; // GeoJSON 좌표는 [lng, lat]
            foreach (var r in regs)
            {
                if (x < r.MinX || x > r.MaxX || y < r.MinY || y > r.MaxY) continue;
                foreach (var p in r.Polys)
                {
                    if (!InRing(p.OX, p.OY, x, y)) continue;
                    bool inHole = false;
                    if (p.HX != null)
                        for (int i = 0; i < p.HX.Count; i++)
                            if (InRing(p.HX[i], p.HY[i], x, y)) { inHole = true; break; }
                    if (!inHole)
                        return string.IsNullOrEmpty(r.Sido) ? r.Sigungu : r.Sido + " " + r.Sigungu;
                }
            }
            return null;
        }

        // 광선 투사(ray casting) point-in-polygon
        private static bool InRing(double[] xs, double[] ys, double x, double y)
        {
            bool inside = false;
            int n = xs.Length;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((ys[i] > y) != (ys[j] > y)) &&
                    (x < (xs[j] - xs[i]) * (y - ys[i]) / (ys[j] - ys[i]) + xs[i]))
                    inside = !inside;
            }
            return inside;
        }

        private void EnsureLoaded()
        {
            if (_regions != null) return;
            lock (_lock)
            {
                // 로드를 락 안에서 수행 → 로딩 중 진입한 다른 스레드는 완료될 때까지 대기 후 완료본 사용.
                // (첫 조회가 로딩 경합으로 null 을 반환해 지역이 '--' 로 남던 문제 방지)
                if (_regions != null) return;
                try
                {
                    if (!File.Exists(_path)) return; // _regions=null 유지 → 다음 호출에서 재시도
                    using (var doc = JsonDocument.Parse(File.ReadAllText(_path)))
                    {
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("features", out var feats)) return;

                    var list = new List<Region>(feats.GetArrayLength());
                    foreach (var f in feats.EnumerateArray())
                    {
                        string name = null, code = null;
                        if (f.TryGetProperty("properties", out var props))
                        {
                            if (props.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
                                name = n.GetString();
                            if (props.TryGetProperty("code", out var c))
                                code = c.ValueKind == JsonValueKind.String ? c.GetString() : c.GetRawText();
                        }
                        string sido = null;
                        if (!string.IsNullOrEmpty(code) && code.Length >= 2)
                            SidoByCode.TryGetValue(code.Substring(0, 2), out sido);

                        if (!f.TryGetProperty("geometry", out var geom)) continue;
                        string gtype = geom.TryGetProperty("type", out var gt) ? gt.GetString() : null;
                        if (!geom.TryGetProperty("coordinates", out var coords)) continue;

                        var polys = new List<Poly>();
                        if (gtype == "Polygon") AddPolygon(coords, polys);
                        else if (gtype == "MultiPolygon")
                            foreach (var pc in coords.EnumerateArray()) AddPolygon(pc, polys);
                        if (polys.Count == 0) continue;

                        var reg = new Region
                        {
                            Sido = sido, Sigungu = name, Polys = polys,
                            MinX = double.MaxValue, MinY = double.MaxValue,
                            MaxX = double.MinValue, MaxY = double.MinValue
                        };
                        foreach (var p in polys)
                            for (int i = 0; i < p.OX.Length; i++)
                            {
                                double xx = p.OX[i], yy = p.OY[i];
                                if (xx < reg.MinX) reg.MinX = xx;
                                if (xx > reg.MaxX) reg.MaxX = xx;
                                if (yy < reg.MinY) reg.MinY = yy;
                                if (yy > reg.MaxY) reg.MaxY = yy;
                            }
                        list.Add(reg);
                    }
                        _regions = list;
                    }
                }
                catch
                {
                    _regions = null; // 실패 시 다음 호출에서 재시도
                }
            }
        }

        // polygon = [ ring0(외곽), ring1(홀), ... ], ring = [[lng,lat], ...]
        private static void AddPolygon(JsonElement polygon, List<Poly> outp)
        {
            if (polygon.ValueKind != JsonValueKind.Array || polygon.GetArrayLength() == 0) return;
            var poly = new Poly();
            int r = 0;
            foreach (var ring in polygon.EnumerateArray())
            {
                int n = ring.GetArrayLength();
                var xs = new double[n];
                var ys = new double[n];
                int i = 0;
                foreach (var pt in ring.EnumerateArray())
                {
                    xs[i] = pt[0].GetDouble();
                    ys[i] = pt[1].GetDouble();
                    i++;
                }
                if (r == 0) { poly.OX = xs; poly.OY = ys; }
                else
                {
                    if (poly.HX == null) { poly.HX = new List<double[]>(); poly.HY = new List<double[]>(); }
                    poly.HX.Add(xs); poly.HY.Add(ys);
                }
                r++;
            }
            if (poly.OX != null) outp.Add(poly);
        }
    }
}
