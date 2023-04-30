import os, sys, subprocess, shutil

def buildrid(rid, output, project, fwork):
    ridDir = os.path.join(output,rid)
    
    cmd = ['dotnet', 'publish', project ,'--self-contained', '-f', fwork, '-r', rid, '-c', 'Release', '-p:PublishSingleFile=true', '-o', ridDir]
    try:
        print("")
        print("")
        print("Building for platform '" + rid + "'...")
        print("")
        print("")
        result = subprocess.run(cmd)
        print("")
        print("")
        print("Done building for platform '" + rid + "'.")
        print("")
        print("")
    except Exception as ex:
        print("")
        print("")
        print("Error on platform '" + rid + "', exception caught: " + str(ex))
        print("")
        print("")

if __name__ == '__main__':
    if sys.version_info < (3, 5):
        print('Please upgrade your Python version to 3.5 or higher')
        sys.exit()
    if ("--help" in sys.argv) or ("help" in sys.argv) or ("-help" in sys.argv) or ("/?" in sys.argv):
        print("build.py: Publishes a .NET project for almost every platform.")
        print("USAGE: python build.py [folder] [project-file] [OPTIONS]")
        print("     folder: The folder that hosts the project files.")
        print("     project-file: The project file's path.")
        print("     [ADDITIONAL OPTIONS]")
        print("     help | --help | -help | /? : Displays this screen and exits.")
        print("     only [RIDs] | o [RIDs]: Publishes the project to certaind Runtime IDs.")
        print("     s | skip: Skips publish folder deletion.")
        print("     --framework [framework] | -f [framework]: Determines which .NET framework should be used. Default is netcoreapp3.1.")
        print("     quick | q:  Publishes without asking for intervention.")
        sys.exit()
    rids = ['linux-x64', 'linux-musl-x64','linux-arm','linux-arm64', 'win-x64','win-x86','win-arm','win-arm64','osx-x64',]
    rootFolder = ""
    projectName = ""
    publishDir = os.path.join(rootFolder,"publish")
    framework = "net6.0"
    if len(sys.argv) < 2:
        print("Error: Argument 'folder' is missing.")
        sys.exit()
    else:
        rootFolder = sys.argv[1]
    if len(sys.argv) < 3:
        print("Error: Argument 'project-file' is missing.")
        sys.exit()
    else:
        projectName = sys.argv[2]
    if not("skip" in sys.argv) and not("s" in sys.argv):
        try:
            shutil.rmtree(publishDir)
        except OSError as e:
            print("Error on deleting publish directory - " + e.strerror)
    quickBuild = ("q" in sys.argv) or ("quick" in sys.argv)
    if "--framework" in sys.argv:
        fwPos = sys.argv.index("--framework")
        framework = sys.argv[fwPos +1]
    elif "-f" in sys.argv:
        fwPos = sys.argv.index("-f")
        framework = sys.argv[fwPos +1]
    if "only" in sys.argv:
        onlyPos = sys.argv.index("only")
        onlyrids = sys.argv[onlyPos + 1:]
        for onlyrid in onlyrids:
            if not quickBuild:
                input("Press Enter to build for '" + onlyrid + "'...")
            buildrid(onlyrid, publishDir, projectName, framework)
        print("Done building all.")
    elif "o" in sys.argv:
        onlyPos = sys.argv.index("o")
        onlyrids = sys.argv[onlyPos + 1:]
        for onlyrid in onlyrids:
            if not quickBuild:
                input("Press Enter to build for '" + onlyrid + "'...")
            buildrid(onlyrid, publishDir, projectName, framework)
        print("Done building all.")
    else:
        print("Welcome to haltroy's auto-publisher!")
        print("This script will build a self-contained app for all available platforms.")
        if not quickBuild:
            print("This script will ask for you to press ENTER to continue.")
            print("The reason for this is that if an error shows up, you can easily see it.")
            print("To bypass this, run this script with 'q' or 'quick' option.")
        for rid in rids:
            if not quickBuild:
                input("Press Enter to build for '" + rid + "'...")
            buildrid(rid, publishDir, projectName, framework)
        print("Done building all.")
