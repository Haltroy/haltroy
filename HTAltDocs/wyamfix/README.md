# WyamFix
Wyam is a great program to make API docs quikly, but sometimes the pages that are created might have a problem. One big problem is that some links doesn't include `.html` file extension so some sites (such as mine) will look for a folder (or file with no file extension) and couldn't find it so it redirects to 404 page.

This could be fixed with editing .htaccess file but that method breaks compability with index.html files.

## Usage:
 1. Get a list of all ".html" files (except index.html files) with anything, this example works with Powershell: `get-childitem F:\Source\haltroy\HTAltDocs\output -recurse >> F:\test.txt`
 2. Remove ".html" from file names and append `<Search Text="` in the beginning of the each line and append `" />` to the end of each line. Copy them and paste it under root node in "File.xml" file.
 3. Open the project, change arguments in Project>Right-Click>Properties>Debug.
 4. Run the program.

