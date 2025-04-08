#include <iostream>
#include <fstream>
#include <cmath>
#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include "json.hpp"

using namespace cv;
using namespace std;
using json = nlohmann::json;

int main(int argc, char *argv[]){
    // load in json file
    ifstream f(argv[1]);
    json jsonData = json::parse(f);
    json data = jsonData["data"];
    json keys = jsonData["keys"];
    json sizeRaw = jsonData["size"];
    Size size = Size(int(sizeRaw[0]), int(sizeRaw[1]));

    vector<Vec4b> palette;

    // load the palette
    for (int i = 0; i < keys.size(); i+=4){
        // converting from BGR to RGB
        Vec4b color = Vec4f(keys[i + 2], keys[i+1], keys[i], keys[i+3]) * 255.0f;
        palette.push_back(color);
    }

    // loop over every pixel to draw the image
    Mat outImage = Mat::zeros(size, CV_8UC4);
    int index = 0;

    for (int i = 0; i < data.size(); i++){
        json entry = data[i];
        Vec4b color = palette[entry["number"]];
        ushort x = floor(i / size.height);
        ushort y = i % size.width;

        outImage.at<Vec4b>(x, y) = color;
        index += int(entry["length"]);
    }

    imwrite("decodedImage.png", outImage);

    outImage.release();
    f.close();

    return 0;
}