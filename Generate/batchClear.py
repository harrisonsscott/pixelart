import os, time
from PIL import Image
import secrets

outputDir = "./batchOutput/"
directory = os.fsencode(outputDir)


for file in os.listdir(directory):
    fileName = os.fsdecode(file)
    os.remove(f"{outputDir}{fileName}");