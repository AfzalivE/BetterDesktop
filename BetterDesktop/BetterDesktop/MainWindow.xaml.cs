using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using WindowsDesktop;

namespace BetterDesktop {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        readonly WindowInteropHelper _wih;

        private ObservableCollection<KeyValuePair<string, IntPtr>> _availableWindows =
            new ObservableCollection<KeyValuePair<string, IntPtr>>();

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

        private Dictionary<Guid, UIElement> CreateDesktopGrid() {
            Dictionary<Guid, UIElement> desktopsDict = new Dictionary<Guid, UIElement>();
            var desktopGridChildren = DesktopGrid.Children;
            VirtualDesktop[] desktops = VirtualDesktop.GetDesktops();
            int desktopsLength = desktops.Length;

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
                VirtualDesktop desktop = desktops[i];
                desktopsDict.Add(desktop.Id, desktopGridChildren[i]);
            }

            return desktopsDict;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            var desktopGrid = CreateDesktopGrid();

            LoadWindows(desktopGrid);
        }

        private void LoadWindows(Dictionary<Guid, UIElement> desktopGrid) {
            Dictionary<IntPtr, string> windows = Utils.LoadWindows();
            Dictionary<Guid, int> windowCounts =
                new Dictionary<Guid, int>(desktopGrid.Count); // number of windows in each desktop, to create the correct grid

            foreach (KeyValuePair<IntPtr, string> entry in windows) {
                Console.WriteLine(entry.Value);
                var vDesktop = VirtualDesktop.FromHwnd(entry.Key);
                if (vDesktop == null) {
                    continue;
                }

                int count;
                windowCounts.TryGetValue(vDesktop.Id, out count);

                count = count + 1;
                windowCounts[vDesktop.Id] = count;
            }

            foreach (var desktop in desktopGrid) {
                int count;
                if (!windowCounts.TryGetValue(desktop.Key, out count)) {
                    continue;
                }

                Grid grid = CreateGrid(windowCounts[desktop.Key]);
                ShowWindowsInDesktop(windows, grid, desktop);
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

        void ShowWindowsInDesktop(Dictionary<IntPtr, string> windows, Grid grid, KeyValuePair<Guid, UIElement> desktopElement) {
            Dictionary<IntPtr, string>.Enumerator e = windows.GetEnumerator();

            for (int hi = 0; hi < grid.Height; hi++) {
                for (int wi = 0; wi < grid.Width; wi++) {
                    if (!e.MoveNext()) {
                        e.Dispose();
                        break;
                    }
                    KeyValuePair<IntPtr, string> entry = e.Current;

                    VirtualDesktop virtualDesktop = VirtualDesktop.FromHwnd(entry.Key);
                    Console.WriteLine("At Window: {0}, in Desktop: {1}", entry.Value, virtualDesktop);
                    if (virtualDesktop == null) {
                        continue;
                    }

                    if (virtualDesktop.Id != desktopElement.Key) {
                        continue; // this window doesn't belong in this desktop
                    }

                    UIElement desktop = desktopElement.Value;

                    double parentWidth = desktop.RenderSize.Width;
                    double parentHeight = desktop.RenderSize.Height;
                    Point origin = desktop.TransformToAncestor(this).Transform(new Point(0, 0));
                    double x = origin.X;
                    double y = origin.Y;
                    // figure out width and height per item
                    double widthPerItem = parentWidth / grid.Width;
                    double heightPerItem = parentHeight / grid.Height;

                    int startXPos = (int) (wi * widthPerItem + x);
                    int startYPos = (int) (hi * heightPerItem + y);
                    int endXPos = (int) ((wi + 1) * widthPerItem + x);
                    int endYPos = (int) ((hi + 1) * heightPerItem + y);
                    DrawRectForWindow(entry.Key, startXPos, startYPos, endXPos, endYPos);
                }
            }
        }

        private void DrawRectForWindow(IntPtr handle, int left, int top, int right, int bottom) {
            IntPtr thisHandle = _wih.Handle;
            Rect rect = new Rect(left, top, right, bottom);
            double scale = GetSystemScale();
            //var scaledRect = new Rect(0, 0, (int)(this.ActualWidth * scale), (int)(this.ActualHeight * scale));
            IntPtr dwmHandle;
            if (!_dwmHandles.TryGetValue(handle, out dwmHandle)) {
                dwmHandle = IntPtr.Zero;
            }

            dwmHandle = Utils.CreateThumbnail(thisHandle, handle, dwmHandle, rect);
            _dwmHandles.Add(handle, dwmHandle);
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