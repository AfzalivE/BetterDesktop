using System;
using System.Collections.Generic;
using System.Windows;
using WindowsDesktop;

namespace BetterDesktop {
    public class Desktop {
        public Guid Id;
        public List<WindowItem> windows = new List<WindowItem>();
        public VirtualDesktop desktop;
        public UIElement desktopElement;
    }
}