﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsDesktop;

namespace BetterDesktop {
    #region Helper structs

    [StructLayout(LayoutKind.Sequential)]
    public struct DwmThumbnailProperties {
        public int dwFlags;
        public Rect rcDestination;
        public Rect rcSource;
        public byte opacity;
        public bool fVisible;
        public bool fSourceClientAreaOnly;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect {
        public Rect(int left, int top, int right, int bottom) {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Psize {
        public int x;
        public int y;
    }

    enum DWMWINDOWATTRIBUTE : uint {
        NCRenderingEnabled = 1,
        NCRenderingPolicy,
        TransitionsForceDisabled,
        AllowNCPaint,
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

    #endregion

    public static class Utils {
        #region Constants

        public static readonly int DWM_TNP_RECTDESTINATION = 0x00000001;
        public static readonly int DWM_TNP_RECTSOURCE = 0x00000002;
        public static readonly int DWM_TNP_OPACITY = 0x00000004;
        public static readonly int DWM_TNP_VISIBLE = 0x00000008;
        public static readonly int DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;

        static readonly int GWL_STYLE = -16;


        static readonly ulong WS_VISIBLE = 0x10000000L;
        static readonly ulong WS_BORDER = 0x00800000L;
        static readonly ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        #endregion

        #region DWM functions

        [DllImport("dwmapi.dll")]
        static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out Psize size);

        [DllImport("dwmapi.dll")]
        static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DwmThumbnailProperties props);

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out bool pvAttribute, int cbAttribute);

        #endregion

        #region Win32 helper functions

        [DllImport("user32.dll")]
        static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);

        delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        public static Dictionary<IntPtr, string> LoadWindows() {
            Dictionary<IntPtr, string> ret = new Dictionary<IntPtr, string>();

            EnumWindows((hwnd, lParam) => {
                    if ((GetWindowLongA(hwnd, GWL_STYLE) & TARGETWINDOW) != TARGETWINDOW) {
                        return true; //continue enumeration
                    }

                    StringBuilder sb = new StringBuilder(100);
                    GetWindowText(hwnd, sb, sb.Capacity);
//                    Console.WriteLine("Getting window: {0} : {1}", hwnd, sb.ToString());
                    if (IsInvisibleWin10BackgroundAppWindow(hwnd)) {
                        Console.WriteLine("Ignoring invisible window: {0}", sb);
                    }
                    else {
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

        public static IntPtr CreateThumbnail(IntPtr destination, IntPtr source, IntPtr oldHandle, Rect dest) {
            if (oldHandle != IntPtr.Zero) {
                DwmUnregisterThumbnail(oldHandle);
            }

            IntPtr newhandle;

            int ret = DwmRegisterThumbnail(destination, source, out newhandle);

            if (ret != 0) {
                return newhandle;
            }

            DwmThumbnailProperties props = new DwmThumbnailProperties();

            props.dwFlags =
                DWM_TNP_SOURCECLIENTAREAONLY |
                DWM_TNP_VISIBLE |
                DWM_TNP_OPACITY |
                DWM_TNP_RECTDESTINATION;

            props.fSourceClientAreaOnly = false;
            props.fVisible = true;
            props.opacity = 255;
            props.rcDestination = dest;

            DwmUpdateThumbnailProperties(newhandle, ref props);

            return newhandle;
        }
    }
}