import os, sys, shutil

def trydel(dir):
    try:
    	if os.path.exists(dir):
            print("Remove folder: " + dir)
            shutil.rmtree(dir)
    except OSError as e:
        print("Error: %s - %s." % (e.filename, e.strerror))	

def removefolder(rootfolder):
    subfolders = [ f.path for f in os.scandir(rootfolder) if f.is_dir() ]
    print("Removing all bin and obj folders in " + rootfolder + "...")
    for dir in subfolders:
        # Detect and skip the other folders
        if ("packages" in dir) or (".vs" in dir) or (".git" in dir) or (".github" in dir) or (".vscode" in dir):
            continue
        bindir = os.path.join(dir,"bin");
        objdir = os.path.join(dir,"obj");
        pubdir = os.path.join(dir,"publish");
        trydel(bindir)
        trydel(objdir)
        trydel(pubdir)
        removefolder(dir)
       
       
if __name__ == "__main__":
    if sys.argv[1] is not None:
        print(sys.argv[1])
        removefolder(sys.argv[1])
    else:
        removefolder(os.getcwd())
print("Done.")
