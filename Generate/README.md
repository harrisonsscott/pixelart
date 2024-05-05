This folder is converting images into json files that can be used by the the game

OpenCV is required for the script to run, install with
sudo apt install libopencv-dev

VSCode will probably think that the OpenCV includes aren't valid but it will compile just fine.

The way the script is it resizes an image, loops over every pixel, puts the similar colors in a palette, and then writes all the pixels with
the corresponding palette color to json.

Run with
./pixelArt [Image] [Output] [Size X] [Size Y] [Threshold]

Where Size X and Y is the size of the outputted image, and threshold is the similarity between palette colors