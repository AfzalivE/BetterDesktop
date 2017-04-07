using System;
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

        public IntPtr DWMHandle { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            //RegisterHotkeys();

            //var desktops = VirtualDesktop.GetDesktops();
            //int len = desktops.Length;

            //Console.WriteLine("Found {0} desktops", len);

            //for (int i = 0; i < desktops.Length; i++)
            //{
            //    //VirtualDesktop desktop = desktops[i];
            //}

            // WINDOWS
            RefreshWindows();

        }

        void RefreshWindows()
        {
            Dictionary<IntPtr, string> windows = Utils.LoadWindows();

            comboBox.ItemsSource = windows.Select(x => $"{x.Key};{x.Value}").ToArray();
            SynchronizationContext context = SynchronizationContext.Current;

            Task.Delay(1000).ContinueWith(t =>
            {

                context.Post(ignore =>
                {
                    foreach (KeyValuePair<IntPtr, string> entry in windows)
                    {
                        // do something with entry.Value or entry.Key

                        //Console.WriteLine("IntPtr: {0} on Desktop {1}", entry.Key, desktop.Id);

                        //drawRectForWindow(entry.Key);
                        //break;
                    }
                }, null);
            });

        }

        void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            var thisHandle = new WindowInteropHelper(this).Handle;
            var handle = new IntPtr(int.Parse(cb.SelectedItem.ToString().Split(';')[0]));
            var desktop = VirtualDesktop.FromHwnd(handle);
            if (desktop != null)
            {
                Console.WriteLine("IntPtr: {0} on Desktop {1}", handle, desktop.Id);
            }
            drawRectForWindow(handle);
        }

        void drawRectForWindow(IntPtr handle)
        {
            var thisHandle = new WindowInteropHelper(this).Handle;
            var rect = new Rect(0, 0, (int)this.ActualWidth, (int)this.ActualHeight);
            var scale = GetSystemScale();
            var scaledRect = new Rect(0, 0, (int)(this.ActualWidth * scale), (int)(this.ActualHeight * scale));
            DWMHandle = Utils.CreateThumbnail(thisHandle, handle, DWMHandle, scaledRect);
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
