#Copyright © 2021 - 2022 haltroy

#Use of this source code is governed by a GNU General Public License version 3.0 that can be found in github.com/haltroy/Foster/blob/master/COPYING

import os, sys, subprocess, shutil
if sys.version_info < (3, 5):
    print('Please upgrade your Python version to 3.5 or higher')
    sys.exit()
rootFolder = os.getcwd()
publishDir = os.path.join(rootFolder,"output")

if not("--skip-folder-deletion" in sys.argv) and not("-s" in sys.argv):
    try:
        shutil.rmtree(publishDir)
    except OSError as e:
        print("Error on deleting publish directory - " + e.strerror)
       
cmd = ['wyam', 'build']
print("Building...")
try:
    result = subprocess.run(cmd, stdout=subprocess.PIPE, check=True)
except Exception as ex:
    print(result.stdout.decode('utf-8'))
    print("Error on building, exception caught: " + str(ex))
print("Done.")