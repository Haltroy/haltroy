from PIL import Image
import numpy as np
import os, sys, subprocess, shutil

'''
Don't forget to install Pillow & FFMPEG:
PILLOW: pip install Pillow
FFMPEG: https://www.ffmpeg.org/

Auto-square video
by haltroy
------------------
haltroy.com
'''

if sys.version_info < (3, 5):
    print('Please upgrade your Python version to 3.5 or higher')
    sys.exit()


if len(sys.argv) < 2:
    printHelp()
    
if(sys.argv[1] == "--help") or (sys.argv[1] == "-h") or (sys.argv[1] == "help") or (sys.argv[1] == "?"):
    printHelp()
    
else:
    filetypes = ['mp4']
    dir = sys.argv[1]
    if len(sys.argv) < 3:
        printHelp()
    
    output = sys.argv[2]


    print("Starting...")
    files = [[f for f in os.listdir(dir) if f.endswith(type_)] for type_ in filetypes]
    for types in files:
        for val in types:
            inputFile = os.path.join(dir, val)
            outputFile = os.path.join(output, val)
            try:
                cmd = ['ffmpeg', '-i', inputFile, '-c:v', 'libx264', '-pix_fmt', 'yuv420p' , '-aspect','1:1','-c', 'copy', outputFile]
                result = subprocess.run(cmd, stdout=subprocess.PIPE, check=True)
                print(inputfile + " => " + outputFile)
            except:
                print("Error at file " + val + ".")
                pass

    print("Done.")
