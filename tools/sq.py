from PIL import Image
import os, sys

'''
Don't forget to install Pillow:
pip install Pillow

Auto-square images
by haltroy
------------------
haltroy.com
'''

def resize(image_pil, width , height, red, green, blue):
    ratio_w = width / image_pil.width
    ratio_h = height / image_pil.height
    if ratio_w < ratio_h:
        # It must be fixed by width
        resize_width = width
        resize_height = round(ratio_w * image_pil.height)
    else:
        # Fixed by height
        resize_width = round(ratio_h * image_pil.width)
        resize_height = height
    image_resize = image_pil.resize((resize_width, resize_height), Image.ANTIALIAS)
    background = Image.new('RGB', (width, height), (0,0,0))
    offset = (round((width - resize_width) / 2), round((height - resize_height) / 2))
    background.paste(image_resize, offset)
    return background.convert('RGB')
    
def printHelp():
    print("USAGE: python sq.py [Image Folder] [Target Folder] [Size]* [BG Color R]* [BG Color G]* [BG Color B]*")
    print("Arguments marked with * are NOT required.")
    exit()

if len(sys.argv) < 2:
    printHelp()
    
if(sys.argv[1] == "--help") or (sys.argv[1] == "-h") or (sys.argv[1] == "help") or (sys.argv[1] == "?"):
    printHelp()
    
else:
    filetypes = ['png', 'jpg', 'jpeg']
    dir = sys.argv[1]
    if len(sys.argv) < 3:
        printHelp()
    
    output = sys.argv[2]
    size = 1024
    if len(sys.argv) >= 4:
        size = int(sys.argv[3])
    
    red = 0
    green = 0
    blue = 0
    if len(sys.argv) >= 7:
        red = int(sys.argv[4])
        green = int(sys.argv[5])
        blue = int(sys.argv[6])
    
    files = [[f for f in os.listdir(dir) if f.endswith(type_)] for type_ in filetypes]
    for types in files:
        for val in types:
            inputFile = os.path.join(dir, val)
            outputFile = os.path.join(output, val)
            try:
                resize(Image.open(inputFile),size,size, red, green, blue).save(outputFile)
                print(inputFile + " --> " + outputFile)
            except Exception as ex:
                print("Error at file" + val + ", exception caught: " + ex)
    