# Python script that removes the last line on first file (</root>) and combines two files into one output folder.
# Mostly made by ChatGPT but the code works because this is simple.

import os
import sys

def update_files_in_folder(input_folder1, input_folder2, output_folder):
    # Ensure the output folder exists
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)

    # Iterate through the contents of the first input folder
    for filename in os.listdir(input_folder1):
        input_file1 = os.path.join(input_folder1, filename)
        input_file2 = os.path.join(input_folder2, filename)  # Match files with the same name in the second folder

        # Generate the output file path
        output_file = os.path.join(output_folder, filename)

        try:
            # Perform the update
            update_file(input_file1, input_file2, output_file)
        except:
            print(f'Error on file {filename}, skipping...')

def update_file(input_file1, input_file2, output_file):
    # Your previous implementation of updating a file
    with open(input_file1, 'r') as file1:
        lines = file1.readlines()

    if lines:
        lines.pop()

    with open(input_file2, 'r') as file2:
        lines += file2.readlines()

    with open(output_file, 'w') as output_file:
        output_file.writelines(lines)

if __name__ == "__main__":
    # Check if the correct number of command-line arguments is provided
    if len(sys.argv) != 4:
        print("Usage: python myscript.py <input_folder1> <input_folder2> <output_folder>")
        sys.exit(1)

    # Extract command-line arguments
    input_folder1 = sys.argv[1]
    input_folder2 = sys.argv[2]
    output_folder = sys.argv[3]

    # Call the update function
    update_files_in_folder(input_folder1, input_folder2, output_folder)
