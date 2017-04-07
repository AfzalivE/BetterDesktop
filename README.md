# BetterDesktop

This application hopes to become for Windows 10 what TotalSpaces2 is for Mac. It has a long way to go.

- Assignable global hotkeys to invoke functions in the app
- Show an overview of all desktops in a grid
- Switch to other desktops using arrow keys (left/right wrapping, and up and down later)
- Switch to other desktops using modifier + numbers for desktops 1-9
- Move active window to another desktop using modifier + arrows/numbers
- Assign certain apps to open windows in a specified desktop (and then move to that desktop, later with a whitelist for this behavior)


A bit of API usage will come from some code in this app:
https://github.com/Grabacr07/SylphyHorn

It uses this library:
https://github.com/Grabacr07/VirtualDesktop


### Prerequisites:

- Open Start menu, type "multitasking", press enter, scroll down in the Multitasking settings to the Virtual Desktops section, change both options to "All desktops"

This is possibly needed to be able to query for all the windows from any desktop.

### The Desktops

- Get all desktops
- Show 9 desktops in a 3x3 grid, (later) with their respective backgrounds
- Clicking a desktop switches to that one
- (Later) Animations

https://github.com/Grabacr07/VirtualDesktop/

### The Windows

- Get all windows, and show their thumbnails in the desktops they belong
- (Later) Clicking a window switches to that desktop and brings that window to the top

Note: The VirtualDesktop library allows querying which desktop a window ptr belongs to

https://www.google.com/search?q=wpf+dwm+thumbnail+of+all+windows&oq=wpf+dwm+thumbnail+of+all+windows&aqs=chrome..69i57.12168j0j1&sourceid=chrome&ie=UTF-8

https://github.com/jakubsuchybio/DWM-Proof-Of-Concept

https://msdn.microsoft.com/en-us/library/windows/desktop/aa969541(v=vs.85).aspx

https://github.com/MathewSachin/Thumbs

http://www.11011.net/archives/000651.html

http://blogs.developpeur.org/tom/archive/2006/09/28/24172.aspx

http://web.archive.org/web/20071213213302/http://blogs.labo-dotnet.com/simon/archive/2006/09/12/11116.aspx

http://www.11011.net/archives/000653.html

- OR get the Task View window for all desktops simultaneously, and show that in a grid. Not sure about this one

### Global hotkeys

- Showing a form with a text box, showing the user which key combo was pressed
- or perhaps a dropdown to pick the key combo
- Register the hotkeys
- Unregister, and re-register when the hotkeys are modified
- Switch to other desktops using modifier + numbers for desktops 1-9
- Move active window to another desktop using modifier + arrows/numbers

http://southworks.com/blog/2011/03/15/wpfshortcutkeys/

https://www.codeproject.com/Articles/2213/Beginner-s-Tutorial-Using-global-hotkeys

https://blog.magnusmontin.net/2015/03/31/implementing-global-hot-keys-in-wpf/

### App locked to specified desktop

- Assign certain apps to open windows in a specified desktop (and then move to that desktop, later with a whitelist for this behavior)

