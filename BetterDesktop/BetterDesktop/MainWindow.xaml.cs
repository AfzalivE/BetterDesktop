using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using WindowsDesktop;

namespace BetterDesktop {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        readonly WindowInteropHelper _wih;

        private readonly Dictionary<IntPtr, IntPtr> _dwmHandles = new Dictionary<IntPtr, IntPtr>();

        public MainWindow() {
            InitializeComponent();
            _wih = new WindowInteropHelper(this);
            this.WindowStyle = WindowStyle.None;
            //this.WindowState = WindowState.Maximized;
            this.Left = 0;
            this.Top = 0;
            this.Height = SystemParameters.MaximizedPrimaryScreenHeight;
            this.Width = SystemParameters.MaximizedPrimaryScreenWidth;

            //RegisterHotkeys();

            // WINDOWS
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            var desktopGrid = CreateDesktopGrid();

            LoadWindows(desktopGrid);
        }

        private Dictionary<Guid, Desktop> CreateDesktopGrid() {
            Dictionary<Guid, Desktop> desktopsDict = new Dictionary<Guid, Desktop>();
            var desktopGridChildren = DesktopGrid.Children;
            VirtualDesktop[] vDesktops = VirtualDesktop.GetDesktops();
            int desktopsLength = vDesktops.Length;

            Console.WriteLine("Found {0} desktops", desktopsLength);

            if (desktopsLength != 9 || desktopGridChildren.Count != 9) {
                // Need 9 desktops for now, until dynamic # is allowed
                var errorMsg = "Didn't find 9 desktops, or 9 items in the desktop grid, exiting.";
                Console.WriteLine(errorMsg);
                MessageBox.Show(this, errorMsg);
                Application.Current.Shutdown();
                return desktopsDict;
            }

            // TODO automatically populate the grid with desktopLength
            int minLen = Math.Min(desktopsLength, desktopGridChildren.Count);

            for (int i = 0; i < minLen; i++) {
                VirtualDesktop vDesktop = vDesktops[i];
                var desktop = new Desktop() {
                    Id = vDesktop.Id,
                    vDesktop = vDesktop,
                    desktopElement = desktopGridChildren[i]
                };
                desktopsDict.Add(vDesktop.Id, desktop);
            }

            return desktopsDict;
        }

        private void LoadWindows(Dictionary<Guid, Desktop> desktops) {
            Dictionary<IntPtr, string> windows = Utils.LoadWindows();

            foreach (KeyValuePair<IntPtr, string> entry in windows) {
                var vDesktop = VirtualDesktop.FromHwnd(entry.Key);
                if (vDesktop == null) {
                    continue;
//                    vDesktop = VirtualDesktop.Current; // show in current desktop as fallback ??
                }

                Desktop desktop;
                if (!desktops.TryGetValue(vDesktop.Id, out desktop)) {
                    // this desktop is not in out desktops dict, how can a window be not in the dict of all desktops??
                    MessageBox.Show(this, "Found window in non-existent desktop!");
                    return;
                }


                var windowItem = new WindowItem() {
                    handle = entry.Key,
                    title = entry.Value
                };

                Rect windowRect;
                if (Utils.GetWindowRect(entry.Key, out windowRect)) {
                    windowItem.windowRect = windowRect;
                }

                desktop.windows.Add(windowItem);
            }

            foreach (var desktop in desktops) {
                List<WindowItem> desktopWindows = desktop.Value.windows;
                Grid grid = CreateGrid(desktopWindows.Count);
                ShowWindowsInDesktop(desktopWindows, grid, desktop.Value);
            }
        }

        private static Grid CreateGrid(int numWindows) {
            //  handle zero case
            if (numWindows == 0) {
                return new Grid(1, 1);
            }
            int w = (int) Math.Ceiling(Math.Sqrt(numWindows));
            int h = numWindows > w * (w - 1) ? w : w - 1;

            Console.WriteLine("For {2} = Grid: {0} x {1}", w, h, numWindows);

            return new Grid(w, h);
        }

        void ShowWindowsInDesktop(List<WindowItem> windows, Grid grid, Desktop desktop) {
            List<WindowItem>.Enumerator e = windows.GetEnumerator();

            for (int hi = 0; hi < grid.Height; hi++) {
                for (int wi = 0; wi < grid.Width; wi++) {
                    if (!e.MoveNext()) {
                        e.Dispose();
                        break;
                    }
                    WindowItem entry = e.Current;
                    if (entry == null) {
                        continue;
                    }

                    Console.WriteLine("Window: {0}, in Desktop: {1}", entry.title, desktop.Id);

                    UIElement desktopElement = desktop.desktopElement;

                    double parentWidth = desktopElement.RenderSize.Width;
                    double parentHeight = desktopElement.RenderSize.Height;
                    Point origin = desktopElement.TransformToAncestor(this).Transform(new Point(0, 0));
                    double x = origin.X;
                    double y = origin.Y;
                    // figure out width and height per item
                    double widthPerItem = parentWidth / grid.Width;
                    double heightPerItem = parentHeight / grid.Height;

                    int startXPos = (int) (wi * widthPerItem + x);
                    int startYPos = (int) (hi * heightPerItem + y);
                    int endXPos = (int) ((wi + 1) * widthPerItem + x);
                    int endYPos = (int) ((hi + 1) * heightPerItem + y);
                    DrawRectForWindow(entry, startXPos, startYPos, endXPos, endYPos);
                }
            }
        }

        private void DrawRectForWindow(WindowItem window, int left, int top, int right, int bottom) {
            IntPtr thisHandle = _wih.Handle;
            double scale = GetSystemScale();
            // keep original window aspect ratio
            var windowRect = window.windowRect;
            double windowWidth = windowRect.Right - windowRect.Left;
            double windowHeight = windowRect.Bottom - windowRect.Top;

            double rectHeight = bottom - top;
            double scaleFactor = rectHeight / windowHeight;

            windowHeight = scaleFactor * windowHeight;
            windowWidth = scaleFactor * windowWidth;

            // origins remain the same as provided
            var rect = new Rect((int) (left), (int) (top), (int) (left + windowWidth), (int) (top + windowHeight));

            var scaledRect = new Rect((int) (left * scale), (int) (top * scale), (int) ((left + windowWidth) * scale), (int) ((top + windowHeight) * scale));
            IntPtr dwmHandle;

            if (!_dwmHandles.TryGetValue(window.handle, out dwmHandle)) {
                dwmHandle = IntPtr.Zero;
            }

            dwmHandle = Utils.CreateThumbnail(thisHandle, window.handle, dwmHandle, scaledRect);
            _dwmHandles.Add(window.handle, dwmHandle);
        }

        public double GetSystemScale() {
            double dpi = 1.0;
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
                dpi = graphics.DpiX / 96.0;
            }
            return dpi;
        }
    }
}