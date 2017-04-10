using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace BetterDesktop {
    public class WindowItem : Control {
        public IntPtr Handle;
        public string Title;
        public Rect WindowRect;

        private IntPtr _thumb;
        private HwndSource _target;

        static WindowItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowItem), new FrameworkPropertyMetadata(typeof(WindowItem)));
        }

        public WindowItem(IntPtr handle, string title) {
            this.Handle = handle;
            this.Title = title;

            this.BorderBrush = Brushes.OrangeRed;
            this.BorderThickness = new Thickness(2);
            DwmUtils.GetWindowRect(handle, out WindowRect);

//            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
//            this.Unloaded += new RoutedEventHandler(OnUnloaded);
        }

//        private void DrawRectForWindow(int left, int top, int right, int bottom) {
//            IntPtr thisHandle = _wih.Handle;
//            double scale = DWM.GetSystemScale();
//            // keep original window aspect ratio
//            double windowWidth = windowRect.Right - windowRect.Left;
//            double windowHeight = windowRect.Bottom - windowRect.Top;
//
//            double rectHeight = bottom - top;
//            double scaleFactor = rectHeight / windowHeight;
//
//            windowHeight = scaleFactor * windowHeight;
//            windowWidth = scaleFactor * windowWidth;
//
//            // origins remain the same as provided
//            var rect = new Rect((int) (left), (int) (top), (int) (left + windowWidth), (int) (top + windowHeight));
//
//            var scaledRect = new Rect((int) (left * scale), (int) (top * scale), (int) ((left + windowWidth) * scale), (int) ((top + windowHeight) * scale));
//            IntPtr dwmHandle;
//
//            if (!_dwmHandles.TryGetValue(handle, out dwmHandle)) {
//                dwmHandle = IntPtr.Zero;
//            }
//
//            dwmHandle = Utils.CreateThumbnail(thisHandle, handle, dwmHandle, scaledRect);
//            _dwmHandles.Add(window.handle, dwmHandle);
//        }

        public void DrawRectForWindow() {
            if (IntPtr.Zero == _thumb) {
                InitializeThumbnail();
            }

            if (IntPtr.Zero == _thumb) {
                return;
            }

            if (!_target.RootVisual.IsAncestorOf(this)) {
                //we are no longer in the visual tree
                ReleaseThumbnail();
                return;
            }

            double scale = DwmUtils.GetSystemScale();
            // keep original window aspect ratio
            double windowWidth = WindowRect.Right - WindowRect.Left;
            double windowHeight = WindowRect.Bottom - WindowRect.Top;

            Point origin = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));

            double scaleFactor = Height / windowHeight;

            windowHeight = scaleFactor * windowHeight;
            windowWidth = scaleFactor * windowWidth;

            // origins remain the same as provided
            var rect = new Rect(
                (int) (origin.X),
                (int) (origin.Y),
                (int) (origin.X + windowWidth),
                (int) (origin.Y + windowHeight));

            var scaledRect = new Rect(
                (int) (origin.X * scale),
                (int) (origin.Y * scale),
                (int) ((origin.X + windowWidth) * scale),
                (int) ((origin.Y + windowHeight) * scale));
            
            DwmThumbnailProperties props = new DwmThumbnailProperties();
            props.Visible = true;
            props.Destination = scaledRect;
//            props.Destination = new DWM.Rect(
//                (int) Math.Ceiling(a.X), (int) Math.Ceiling(a.Y),
//                (int) Math.Ceiling(b.X), (int) Math.Ceiling(b.Y));

            props.Flags = ThumbnailFlags.Visible | ThumbnailFlags.RectDetination;
            DwmUtils.DwmUpdateThumbnailProperties(_thumb, ref props);
        }

        private void InitializeThumbnail() {
            if (IntPtr.Zero != _thumb) {
                // release the old thumbnail
                ReleaseThumbnail();
            }

            if (IntPtr.Zero == Handle) {
                return;
            }

            // find our parent hwnd
            _target = (HwndSource) HwndSource.FromVisual(this);

            // if we have one, we can attempt to register the thumbnail
            if (_target == null || 0 != DwmUtils.DwmRegisterThumbnail(_target.Handle, Handle, out this._thumb)) {
                return;
            }

            DwmThumbnailProperties props = new DwmThumbnailProperties();
            props.Visible = false;
            props.SourceClientAreaOnly = false;
            props.Opacity = (byte) (255 * this.Opacity);
            props.Flags = ThumbnailFlags.Visible |
                          ThumbnailFlags.SourceClientAreaOnly |
                          ThumbnailFlags.Opacity;

            DwmUtils.DwmUpdateThumbnailProperties(_thumb, ref props);
        }

        private void ReleaseThumbnail() {
            DwmUtils.DwmUnregisterThumbnail(_thumb);
            this._thumb = IntPtr.Zero;
            this._target = null;
        }

        public void SetContainerRect(int startXPos, int startYPos, int endXPos, int endYPos) {
            this.Width = endXPos - startXPos;
            this.Height = endYPos - startYPos;
            Canvas.SetLeft(this, startXPos);
            Canvas.SetRight(this, endXPos);
            Canvas.SetTop(this, startYPos);
            Canvas.SetBottom(this, endYPos);
        }
    }
}