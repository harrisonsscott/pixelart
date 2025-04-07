import os, time

directory = os.fsencode("./batchInput/")

for file in os.listdir(directory):
    fileName = os.fsdecode(file)
    newName = "".join(map(lambda c: c if c != " " else "", fileName))
    jsonName = newName.split(".")[0] + ".json"

    # rename files if they have spaces
    if fileName != newName:
        os.rename(f"./batchInput/{fileName}", f"./batchInput/{newName}")

    os.system(f"./pixelArt ./batchInput/{newName} ./batchOutput/{jsonName} 32 32 500")