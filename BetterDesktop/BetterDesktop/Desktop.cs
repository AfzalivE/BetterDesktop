using System;
using System.Collections.Generic;
using System.Windows;
using WindowsDesktop;

namespace BetterDesktop {
    public class Desktop {
        private Guid Id;
        private List<Window> windows;
        private VirtualDesktop desktop;
        private UIElement desktopElement;
    }
}