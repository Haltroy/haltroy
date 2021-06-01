@ECHO OFF

REM ''''''''''''''
REM ' HTALT DOCS '
REM '''''''''''''
 
REM Documentations of HTAlt can be built with Wyam, install it with this command: dotnet tool install -g Wyam.Tool
REM You also need the latest .NET Core. Download it from dotnet.microsoft.com
REM Similar to this script, build.sh can also be used for Linux OSes.Yo need to mark it as executable first via your fiel manager or chmod.
REM This script is only for a Windows machine. 

echo Building website... Can take a couple of minutes...
wyam build
echo Build done.
PAUSE >nul