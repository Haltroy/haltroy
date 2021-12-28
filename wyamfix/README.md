# WyamFix
Wyam is a great program to make API docs quikly, but sometimes the pages that are created might have a problem. One big problem is that some links doesn't include `.html` file extension so some sites (such as mine) will look for a folder (or file with no file extension) and couldn't find it so it redirects to 404 page.

This could be fixed with editing .htaccess file but that method breaks compability with index.html files.

To use this tool, you need .NET Core 3.1 or newer (.NET 5 and newer works fine too).

## Usage:
Do these after building the API docs with Wyam:
 1. Open a terminal in this folder.
   - On windows: Type either "powershell", "pwsh" or "cmd" into the address bar and hit enter. If you have Windows Terminal installed, the option is available in the right-click menu.
   - Most GNU\Linux distributions' desktop environments have a right-click option to do this.
 2. Execute `dotnet run -- [Path to Wyam output folder]`
   - Tip: On most systems, dragging the folder to command just pastes the path so you can just type `dotnet run -- ` (**Don't forget the empty space after "--"**) and drag-drop the folder into your terminal.

