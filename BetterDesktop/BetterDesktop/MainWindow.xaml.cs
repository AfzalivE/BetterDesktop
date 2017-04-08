﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsDesktop;

namespace BetterDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly WindowInteropHelper _wih;
        private ObservableCollection<KeyValuePair<string, IntPtr>> AvailableWindows = new ObservableCollection<KeyValuePair<string, IntPtr>>();

        public Dictionary<IntPtr, IntPtr> DWMHandles { get; private set; } = new Dictionary<IntPtr, IntPtr>();

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;
            //this.WindowState = WindowState.Maximized;
            this.Left = 0;
            this.Top = 0;
            this.Height = SystemParameters.MaximizedPrimaryScreenHeight;
            this.Width = SystemParameters.MaximizedPrimaryScreenWidth;
            //RegisterHotkeys();

            //var desktops = VirtualDesktop.GetDesktops();
            //int len = desktops.Length;

            //Console.WriteLine("Found {0} desktops", len);

            //for (int i = 0; i < desktops.Length; i++)
            //{
            //    //VirtualDesktop desktop = desktops[i];
            //}

            // WINDOWS
            Loaded += onWindowLoaded;
        }

        private void onWindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadWindows();
        }

        private void LoadWindows()
        {
            Dictionary<IntPtr, string> windows = Utils.LoadWindows();

            foreach (KeyValuePair<IntPtr, string> entry in windows)
            {
                Console.WriteLine(entry.Value);
            }

            Grid grid = createGrid(windows.Count());
            showWindows(windows, grid);
        }

        private Grid createGrid(int numWindows)
        {
            //  handle zero case
            if (numWindows == 0)
            {
                return new Grid(1, 1);
            }
            int w = (int)Math.Ceiling(Math.Sqrt(numWindows));
            int h = numWindows > w * (w - 1) ? w : w - 1;

            Console.WriteLine("For {2} = Grid: {0} x {1}", w, h, numWindows);

            return new Grid(w, h);
        }

        void showWindows(Dictionary<IntPtr, string> windows, Grid grid)
        {
            // figure out width and height per item
            double widthPerItem = this.Width / grid.width;
            double heightPerItem = this.Height / grid.height;

            Dictionary<IntPtr, string>.Enumerator e = windows.GetEnumerator();

            for (int hi = 0; hi < grid.height; hi++)
            {
                for (int wi = 0; wi < grid.width; wi++)
                {
                    if (!e.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<IntPtr, string> entry = e.Current;
                    int startXPos = (int)(wi * widthPerItem);
                    int startYPos = (int)(hi * heightPerItem);
                    int endXPos = (int)((wi + 1) * widthPerItem);
                    int endYPos = (int)((hi + 1) * heightPerItem);
                    drawRectForWindow(entry.Key, startXPos, startYPos, endXPos, endYPos);
                }
            }
        }

        void drawRectForWindow(IntPtr handle, int left, int top, int right, int bottom)
        {
            var thisHandle = new WindowInteropHelper(this).Handle;
            var rect = new Rect(left, top, right, bottom);
            var scale = GetSystemScale();
            //var scaledRect = new Rect(0, 0, (int)(this.ActualWidth * scale), (int)(this.ActualHeight * scale));
            IntPtr dwmHandle;
            if (!DWMHandles.TryGetValue(handle, out dwmHandle))
            {
                dwmHandle = IntPtr.Zero;
            }

            dwmHandle = Utils.CreateThumbnail(thisHandle, handle, dwmHandle, rect);
            DWMHandles.Add(handle, dwmHandle);
        }

        public double GetSystemScale()
        {
            var dpi = 1.0;
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            {
                dpi = graphics.DpiX / 96.0;
            }
            return dpi;
        }
    }
}