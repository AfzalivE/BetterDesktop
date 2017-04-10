using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using WindowsDesktop;

namespace BetterDesktop {
    public static class DwmUtils {
        #region Constants

        static readonly int GWL_STYLE = -16;

        static readonly ulong WS_VISIBLE = 0x10000000L;
        static readonly ulong WS_BORDER = 0x00800000L;
        static readonly ulong TARGET_WINDOW = WS_BORDER | WS_VISIBLE;

        #endregion

        #region Win32 helper functions

        [DllImport("user32.dll", EntryPoint = "GetWindowPlacement")]
        private static extern bool InternalGetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string classname, string title);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll")]
        private static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);

        private delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        #region DWM functions

        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr source, out IntPtr hthumbnail);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr hThumbnail);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumbnail, ref DwmThumbnailProperties props);

        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr hThumbnail, out Psize size);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out bool pvAttribute, int cbAttribute);

        #endregion

        public static bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement placement) {
            placement = new WindowPlacement();
            placement.Length = Marshal.SizeOf(typeof(WindowPlacement));
            return InternalGetWindowPlacement(hWnd, ref placement);
        }

        public static Dictionary<IntPtr, string> LoadWindows() {
            Dictionary<IntPtr, string> ret = new Dictionary<IntPtr, string>();

            EnumWindows((hwnd, lParam) => {
                    if ((GetWindowLongA(hwnd, GWL_STYLE) & TARGET_WINDOW) != TARGET_WINDOW) {
                        return true; //continue enumeration
                    }

                    StringBuilder sb = new StringBuilder(100);
                    GetWindowText(hwnd, sb, sb.Capacity);
//                    Console.WriteLine("Getting window: {0} : {1}", hwnd, sb.ToString());
                    if (IsInvisibleWin10BackgroundAppWindow(hwnd)) {
                        Console.WriteLine("Ignoring invisible window: {0}", sb);
                    } else {
                        ret.Add(hwnd, sb.ToString());
                    }

                    return true; //continue enumeration
                }
                , 0);

            return ret;
        }

        private static bool IsInvisibleWin10BackgroundAppWindow(IntPtr hWnd) {
            return VirtualDesktop.FromHwnd(hWnd) == null;
        }

        public static double GetSystemScale() {
            double dpi = 1.0;
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
                dpi = graphics.DpiX / 96.0;
            }
            return dpi;
        }
    }

    public struct Pos {
        public int X;
        public int Y;
    }

    public struct Psize {
        public int Width, Height;
    }

    public struct WindowPlacement {
        public int Length;
        public int Flags;
        public int ShowCmd;
        public Pos MinPosition;
        public Pos MaxPosition;
        public Rect NormalPosition;
    }

    public struct DwmThumbnailProperties {
        public ThumbnailFlags Flags;
        public Rect Destination;
        public Rect Source;
        public Byte Opacity;
        public bool Visible;
        public bool SourceClientAreaOnly;
    }

    public struct Rect {
        public Rect(int x, int y, int x1, int y1) {
            this.Left = x;
            this.Top = y;
            this.Right = x1;
            this.Bottom = y1;
        }

        public int Left, Top, Right, Bottom;
    }

    public enum DwmWindowAttribute : uint {
        NcRenderingEnabled = 1,
        NcRenderingPolicy,
        TransitionsForceDisabled,
        AllowNcPaint,
        CaptionButtonBounds,
        NonClientRtlLayout,
        ForceIconicRepresentation,
        Flip3DPolicy,
        ExtendedFrameBounds,
        HasIconicBitmap,
        DisallowPeek,
        ExcludedFromPeek,
        Cloak,
        Cloaked,
        FreezeRepresentation
    }

    [Flags]
    public enum ThumbnailFlags : int {
        RectDetination = 1,
        RectSource = 2,
        Opacity = 4,
        Visible = 8,
        SourceClientAreaOnly = 16
    }

    public enum GetWindowCmd : uint {
        First = 0,
        Last = 1,
        Next = 2,
        Prev = 3,
        Owner = 4,
        Child = 5,
        EnabledPopup = 6
    }
}