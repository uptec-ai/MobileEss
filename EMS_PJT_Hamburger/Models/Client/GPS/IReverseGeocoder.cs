using System.Threading;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.GPS
{
    /// <summary>
    /// 좌표(lat/lng) → 행정구역명(시/도/군) 변환 훅.
    /// 오프라인 구현은 <see cref="KoreaRegionLookup"/> (KOSTAT 시군구 GeoJSON point-in-polygon).
    /// </summary>
    public interface IReverseGeocoder
    {
        /// <summary>
        /// 좌표에 해당하는 시/도/군 명칭을 반환합니다.
        /// 조회 실패 또는 미지원 시 null/빈 문자열을 반환합니다.
        /// </summary>
        Task<string> GetRegionNameAsync(double latitude, double longitude, CancellationToken ct);
    }

    /// <summary>
    /// 기본(no-op) 역지오코더. 항상 null을 반환하여 역지오코딩을 비활성화합니다.
    /// </summary>
    public sealed class NullReverseGeocoder : IReverseGeocoder
    {
        public Task<string> GetRegionNameAsync(double latitude, double longitude, CancellationToken ct)
            => Task.FromResult<string>(null);
    }
}
