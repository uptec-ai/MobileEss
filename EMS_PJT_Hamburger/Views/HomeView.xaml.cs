using DevExpress.Xpf.Map;
using EMS_PJT_Hamburger.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace EMS_PJT_Hamburger.Views
{
    /// <summary>
    /// HomeView.xaml에 대한 상호 작용 논리
    ///
    /// GPS + 지도(DevExpress MapControl, 완전 오프라인 래스터):
    /// - 로컬 타일 폴더(Maps\tiles\{z}\{x}\{y}.png)를 file:// 로 표출 (네트워크 사용 안 함)
    /// - 중심(CenterPoint)은 XAML 에서 VM(MapCenterLatitude/Longitude)에 바인딩 → GPS follow
    /// - 현재 위치를 중앙 마커(좌표 라벨)로 표시, 배율은 우하단 콤보(15/12/10/8)로 선택
    /// GPS 수신/재연결 폴링은 HomeViewModel 이 담당하며, 앱 종료 시 App.OnExit → HomeVm.Dispose 에서 해제된다.
    /// 이 뷰는 VM 을 소유/Dispose 하지 않고, Loaded/Unloaded 에서 이벤트 구독만 관리한다(네비게이션 재진입 안전).
    /// </summary>
    public partial class HomeView : UserControl
    {
        private const double DefaultZoom = 12; // 시작 배율(콤보 기본 선택). 콤보에서 15/12/10/8 로 변경.

        private HomeViewModel _homeVm;
        private MapCustomElement _marker;
        private string _tilesDir;      // 타일 루트(빌드 복사 없이 직접 참조하는 절대경로)
        private bool _mapConfigured;
        private bool _gpsSubscribed;

        public HomeView()
        {
            InitializeComponent();
            Loaded += HomeView_Loaded;
            Unloaded += HomeView_Unloaded;
        }

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _homeVm = DataContext as HomeViewModel;

            ConfigureMap();

            // GPS 위치/지역명 갱신 구독(중복 방지). VM 은 App 가 소유하므로 여기서 Dispose 하지 않는다.
            if (_homeVm != null && !_gpsSubscribed)
            {
                _homeVm.GpsPositionChanged += OnGpsPositionChanged;
                _homeVm.GpsRegionChanged += OnGpsRegionChanged;
                _gpsSubscribed = true;
            }
        }

        private void HomeView_Unloaded(object sender, RoutedEventArgs e)
        {
            // 네비게이션으로 화면을 벗어날 때 이벤트 구독만 해제(핸들러 누수/중복 방지).
            if (_homeVm != null && _gpsSubscribed)
            {
                _homeVm.GpsPositionChanged -= OnGpsPositionChanged;
                _homeVm.GpsRegionChanged -= OnGpsRegionChanged;
                _gpsSubscribed = false;
            }
        }

        // ─── 지도 설정 ───────────────────────────────────────────────────
        private void ConfigureMap()
        {
            if (_mapConfigured || Map == null) return;
            _mapConfigured = true;

            // 완전 오프라인 래스터: 로컬 타일 폴더(Maps\tiles\{z}\{x}\{y}.png)를 file:// 로 읽는다.
            // 타일은 빌드 시 출력폴더로 복사하지 않고(대용량 → 빌드 지연 방지) 소스/배포 폴더를 직접 참조.
            // 코드에서 레이어 생성 → 디자이너엔 안 뜸(네트워크 0).
            _tilesDir = ResolveTilesDir();
            if (_tilesDir != null)
            {
                string baseUri = new Uri(_tilesDir + System.IO.Path.DirectorySeparatorChar).AbsoluteUri;
                var provider = new OpenStreetMapDataProvider
                {
                    TileUriTemplate = baseUri + "{tileLevel}/{tileX}/{tileY}.png"
                };
                Map.Layers.Insert(0, new ImageLayer { DataProvider = provider });
            }

            // 중심(초기 + follow)은 XAML CenterPoint 바인딩이 담당(애니메이션 방지). 여기선 설정하지 않는다.

            // 사용자 휠 줌은 잠금(배율은 콤보로만 변경) + 마우스 드래그(패닝) 허용
            Map.EnableZooming = false;  // 휠 줌 잠금
            Map.EnableScrolling = true; // 마우스 드래그로 지도 이동
            Map.EnableRotation = false;

            // 현재 위치 마커(1개). follow 모드이므로 항상 지도 중심 근처. 라벨 = 시/도/군(GpsRegion).
            double lat = _homeVm?.MapCenterLatitude ?? HomeViewModel.DefaultCenterLat;
            double lon = _homeVm?.MapCenterLongitude ?? HomeViewModel.DefaultCenterLng;
            _marker = new MapCustomElement
            {
                Location = new GeoPoint(lat, lon),
                Content = _homeVm?.GpsRegion ?? "--",
                ContentTemplate = (DataTemplate)Resources["MarkerLabelTemplate"]
            };
            MarkerStorage.Items.Add(_marker);

            // 배율 콤보 기본 선택(=DefaultZoom) → SelectionChanged → ApplyZoom 호출
            SelectZoom(DefaultZoom);
        }

        /// <summary>
        /// 타일 루트 폴더를 결정한다(빌드 복사 없이 직접 참조).
        /// 1) exe 옆 Maps\tiles  (배포: 타일을 exe 옆에 함께 둔 경우)
        /// 2) 프로젝트 소스 Maps\tiles  (개발: exe 는 bin\Debug 이므로 ..\..\Maps\tiles = 프로젝트 폴더)
        /// 둘 다 없으면 null → 타일 레이어를 만들지 않는다(빈 지도).
        /// </summary>
        private static string ResolveTilesDir()
        {
            string bd = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates =
            {
                System.IO.Path.Combine(bd, "Maps", "tiles"),
                System.IO.Path.Combine(bd, "..", "..", "Maps", "tiles"),
            };
            foreach (string c in candidates)
                if (System.IO.Directory.Exists(c))
                    return System.IO.Path.GetFullPath(c); // ..\.. 정규화 → 절대경로
            return null;
        }

        // ─── 배율(줌) 선택 ───────────────────────────────────────────────
        private void ZoomCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ZoomCombo?.SelectedItem is ComboBoxItem item &&
                double.TryParse(item.Content?.ToString(), out double z))
                ApplyZoom(z);
        }

        /// <summary>콤보에서 지정 배율 항목을 선택. 선택되면 SelectionChanged→ApplyZoom 이 호출됨.</summary>
        private void SelectZoom(double zoom)
        {
            if (ZoomCombo == null) return;
            string target = ((int)zoom).ToString();
            foreach (ComboBoxItem it in ZoomCombo.Items)
            {
                if (it.Content?.ToString() != target) continue;
                if (ReferenceEquals(ZoomCombo.SelectedItem, it))
                    ApplyZoom(zoom);             // 이미 선택돼 SelectionChanged 안 뜰 때 대비
                else
                    ZoomCombo.SelectedItem = it;  // 변경 → SelectionChanged → ApplyZoom
                return;
            }
        }

        /// <summary>
        /// 고정 배율 적용: Min=Max=Zoom 으로 잠그고 라벨 갱신.
        /// 해당 배율 타일 폴더(Maps\tiles\{z})가 없으면 라벨에 경고 표시.
        /// </summary>
        private void ApplyZoom(double zoom)
        {
            if (Map == null) return;

            // Min>Max 충돌 없이 안전하게 변경: 범위를 넓힌 뒤 값 설정하고 다시 잠금.
            Map.MinZoomLevel = 1;
            Map.MaxZoomLevel = 25;
            Map.ZoomLevel = zoom;
            Map.MinZoomLevel = zoom;
            Map.MaxZoomLevel = zoom;

            bool hasTiles = _tilesDir != null &&
                System.IO.Directory.Exists(System.IO.Path.Combine(_tilesDir, ((int)zoom).ToString()));

            if (ZoomLabel != null)
                ZoomLabel.Text = hasTiles ? "오프라인" : $"⚠ z{zoom:0} 타일없음";
        }

        // ─── GPS 위치 → 지도 반영 ────────────────────────────────────────
        // ViewModel(UI 스레드)에서 유효 fix마다 호출. 방어적으로 디스패처 확인.
        private void OnGpsPositionChanged(double lat, double lon)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.InvokeAsync(() => OnGpsPositionChanged(lat, lon));
                return;
            }

            // 지도 중심은 XAML CenterPoint 바인딩(MapCenterLatitude/Longitude)이 자동 반영. 여기선 마커 위치만 갱신.
            // 마커 라벨(시/도/군)은 OnGpsRegionChanged 에서 갱신.
            if (_marker != null)
                _marker.Location = new GeoPoint(lat, lon);
        }

        // VM 의 시/도/군(GpsRegion) 변경 → 중앙 마커 라벨 갱신.
        private void OnGpsRegionChanged(string region)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.InvokeAsync(() => OnGpsRegionChanged(region));
                return;
            }

            if (_marker != null)
                _marker.Content = string.IsNullOrEmpty(region) ? "--" : region;
        }

        // ─── 더블클릭 → 현재 위치(마커/GPS 좌표)로 복귀 ──────────────────
        private void Map_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return; // 더블클릭만 처리(단일 클릭 드래그는 그대로)

            // CenterPoint 은 VM(MapCenterLatitude/Longitude)에 OneWay 바인딩. 드래그로 벗어난 지도를
            // 바인딩 강제 재평가로 현재 좌표(=마커 위치)에 재중심한다. follow 유지.
            BindingOperations.GetBindingExpressionBase(Map, MapControl.CenterPointProperty)?.UpdateTarget();
            e.Handled = true; // 더블클릭 시 팬 시작 방지
        }

        // ─── 홈 화면 장비 클릭 → 상세 화면 네비게이션 ────────────────────
        private void PCS_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var app = (App)Application.Current;

                mainWindow.NaviFrame.Content = app.PCSView;
                mainWindow.Btn_PcsStatus.IsSelected = true;
            }
        }

        private void PCS_TouchDown(object sender, TouchEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var app = (App)Application.Current;

                mainWindow.NaviFrame.Content = app.PCSView;
                mainWindow.Btn_PcsStatus.IsSelected = true;
            }
        }

        private void BMS_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var app = (App)Application.Current;

                mainWindow.NaviFrame.Content = app.BMSView;
                mainWindow.Btn_BmsStatus.IsSelected = true;
            }
        }

        private void BMS_TouchDown(object sender, TouchEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var app = (App)Application.Current;

                mainWindow.NaviFrame.Content = app.BMSView;
                mainWindow.Btn_BmsStatus.IsSelected = true;
            }
        }
    }
}
