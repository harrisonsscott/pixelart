#include <iostream>
#include <fstream>
#include <cmath>
#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include "json.hpp"

using namespace cv;
using namespace std;
using json = nlohmann::json;

// returns a number from 0 to ~71,000
float getSimilarity(Vec4b color1, Vec4b color2){
    return
        0.11 * std::pow(color1[0] - color2[0], 2) +
        0.59 * std::pow(color1[1] - color2[1], 2) +
        0.3 * std::pow(color1[2] - color2[2], 2) +
        0.1 * std::pow(color1[3] - color2[3], 2); // Adjust the weight for alpha
}

Vec3b Vec4bTo3b(Vec4b i){
    return Vec3b(i[0], i[1], i[2]);
}

int main(int argc, char *argv[]){
    const Size maxSize(256, 256); // the maximum size an output image can be (recommended to be 256x256)
    ushort x, y;

    if (argc < 5){
        cout << "./pixelArt [Image] [Output] [Size X] [Size Y] [Threshold]" << endl;
        return -1;
    }

    // Load the image with 3 or 4 color channels
    cv::Mat image = cv::imread(argv[1], cv::IMREAD_UNCHANGED);
    if (image.empty()){
        cout << "Error loading image " << argv[1] << endl;
        return -1;
    }
    
    // if (image.channels() == 3){
    //     cvtColor(image, image, COLOR_BGR2BGRA);
    // }

    Size size = Size(atoi(argv[3]), atoi(argv[4]));

    if (size.area() <= 0){
        cout << "Invalid output size! " << size << endl;
        return -1;
    }

    // images need to be square
    int sideLength = max(size.width, size.height);
    
    // equal to 256*256
    if (sideLength*sideLength > maxSize.area()){
        cout << "Image is too large, max size is " << maxSize << endl;
        return -1;
    }

    if (image.size().area() < sideLength * sideLength){
        cout << "Output size is large than the input size!" << endl;
        return -1;
    }
    
    if (sideLength*sideLength != size.area()){
        cout << "Image isn't square, converting to " << Size(sideLength, sideLength) << endl;
        size = Size(sideLength, sideLength);
    }

    int threshold = atoi(argv[5]);

    if (threshold < 0){
        cout << "Color threshold cannot be below 0!" << endl;
        return -1;
    }

    vector<Vec4b> palette; 
    vector<int> amount;
    json j;

    // the output name of the file
    string name = argv[2];

    size_t slashPos = name.find_last_of('/');
    size_t dotPos = name.find_last_of('.');

    string lastElement;
    if (slashPos != string::npos && dotPos != string::npos) {
        name = name.substr(slashPos + 1, dotPos - slashPos - 1);
    } else if (slashPos != string::npos) {
        name = name.substr(slashPos + 1);
    } else if (dotPos != string::npos) {
        name = name.substr(0, dotPos);
    } else {
        name = name;
    }

    // adding metadata to the json
    j["name"] = name;
    j["size"][0] = size.width;
    j["size"][1] = size.height;

    // resize the image
    Mat im = Mat::zeros(size, image.type());
    resize(image, im, size, 0, 0);
    
    // loop over all the pixel to encode them to the json
    uint sim; // how different the current pixel is from all the others (0 = identical to an existing color)
    uint sim2; // temporary value for calculating similarity
    Vec3b col3b; // temporary value for calculating color
    Vec4b color; // color of the current pixel
    Vec4b closestColor; // current color in the palette the current pixel is closest to
    int closestIndex; // index of the closestColor in the palette

    for (int i = 0; i < size.area(); i++){
        x = floor(i / size.height);
        y = i % size.width;
        sim = 1410065407; // start at an extremely high number and decrease to the smallest possible value

        col3b = im.at<Vec3b>(Point(x, y)); // temporary value
        color = Vec4b(col3b[0], col3b[1], col3b[2], 255);
        closestColor = color;

        if (i == 0){
            palette.push_back(color);
            j["data"][0] = 0;
            continue;
        }

        for (int v = 0; v < palette.size(); v++){
            sim2 = getSimilarity(color, palette[v]);
            if (sim2 <= sim){
                sim = sim2; // set similarity to the new, smaller value
                closestColor = palette[v];
                closestIndex = v;
            }
        }

        // if the current pixel's color is different from all the other pixels, add it to the palette
        if (sim >= threshold){
            palette.push_back(color);
        }

        j["data"][i] = closestIndex;

        im.at<Vec3b>(Point(x, y)) = Vec4bTo3b(closestColor);
    }

    imwrite("resized.png", im);

    // encode the colors as hex values
    for (int i = 0; i < palette.size(); i++){
        stringstream ssr, ssg, ssb;
        string r, g, b;

        ssr << hex << int(palette[i][2]);
        ssg << hex << int(palette[i][1]);
        ssb << hex << int(palette[i][0]);

        r = ssr.str();
        g = ssg.str();
        b = ssb.str();

        j["keys"][i] = r + g + b;
    }

    ofstream outputFile(argv[2]); // output file for the json

    if (!outputFile.is_open()){
        cout << "Error opening file " << argv[2] << endl;
        return -1;
    }

    outputFile << j.dump(); // export the json data

    // cleaning up
    image.release();
    outputFile.close();

    return 0;
}