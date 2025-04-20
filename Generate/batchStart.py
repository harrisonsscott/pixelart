import os, time
from PIL import Image
import secrets

directory = os.fsencode("./batchInput/")
inputDir = "./batchInput/"
outputDir = "./batchOutput/"


for file in os.listdir(directory):
    fileName = os.fsdecode(file)
    newName = "".join(map(lambda c: c if c != " " else "", fileName))
    jsonName = newName.split(".")[0] + ".json"
    print(jsonName)

    image = Image.open(f"{inputDir}{fileName}");
    width, height = image.size

    # rename files if they have spaces
    if fileName != newName:
        os.rename(f"{inputDir}{fileName}", f"{inputDir}{newName}")

    os.system(f"./pixelArt {inputDir}{newName} {outputDir}{secrets.token_hex(16)}.json {width} {height} 1000 0")