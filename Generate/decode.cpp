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
    const int mask = 255; // for isolating colors from the hex


    // load in json file
    ifstream f(argv[1]);
    json jsonData = json::parse(f);
    json data = jsonData["data"];
    json keys = jsonData["keys"];
    json sizeRaw = jsonData["size"];
    Size size = Size(int(sizeRaw[0]), int(sizeRaw[1]));

    vector<Vec3b> palette;

    // load the palette
    for (int i = 0; i < keys.size(); i++){
        string hexColor = keys[i];
        int number = stoi(hexColor, nullptr, 16);
        uchar r = number >> 16;
        uchar g = (number >> 8) & mask;
        uchar b = number & mask;
        Vec3b color = Vec3b(b, g, r);

        // the number in binary is represented as abcdefgh ijklmnop qrstuvwx
        // r isolates the first byte by shifting it two bytes to the right to make it 00000000 00000000 abcdefgh
        //      no mask is needed because the other two bytes have been shifted out
        // g isolates the second byte by shifting it one byte right to make it 00000000 abcdefgh ijklmnop
        //      and then applying a mask of 00000000 00000000 11111111, to get the final result of 00000000 00000000 ijklmnop
        // b isolates the third byte by just applying a mask, because the third byte is already in the right place, so we get 00000000 00000000 qrstuvwx

        cout << color << endl;
        palette.push_back(color);
    }

    // loop over every pixel to draw the image
    Mat outImage = Mat::zeros(size, CV_8UC3);
    int index = 0;
    for (int i = 0; i < data.size(); i+=1){
        json entry = data[i];
        Vec3b color = palette[entry["number"]];
        
        for (int v = 0; v < entry["length"]; v++){
            ushort x = (index+v) % size.width;
            ushort y = floor((index+v) / size.height);
            
            outImage.at<Vec3b>(x, y) = color;
        }

        index += int(entry["length"]);
    }

    imwrite("decodedImage.png", outImage);

    outImage.release();
    f.close();

    return 0;
}