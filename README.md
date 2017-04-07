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

### The Desktops

- Get all desktops
- Show 9 desktops in a 3x3 grid, (later) with their respective backgrounds

https://github.com/Grabacr07/VirtualDesktop/

### The Windows

- Get all windows, and show their thumbnails in the desktops they belong

Note: The VirtualDesktop library allows querying which desktop a window ptr belongs to

- OR get the Task View window for all desktops simultaneously, and show that in a grid. Not sure about this one

### Global hotkeys

- Showing a form with a text box, showing the user which key combo was pressed
- or perhaps a dropdown to pick the key combo
- Register the hotkeys
- Unregister, and re-register when the hotkeys are modified

