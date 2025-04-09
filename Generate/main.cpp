#include <iostream>
#include <fstream>
#include <cmath>
#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include "json.hpp"

using namespace cv;
using namespace std;
using json = nlohmann::json;

float getSimilarity(Vec4b color1, Vec4b color2){
    return
        0.11 * std::pow(color1[0] - color2[0], 2) +
        0.59 * std::pow(color1[1] - color2[1], 2) +
        0.3 * std::pow(color1[2] - color2[2], 2) +
        0.1 * std::pow(color1[3] - color2[3], 2); // Adjust the weight for alpha
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

    j["name"] = name;

    // resize the image
    Mat im = Mat::zeros(size, image.type());
    resize(image, im, size, INTER_AREA);
    imwrite("resized.png", im);
    

    for (int i = 0; i < size.area(); i++){
        x = floor(i / size.height);
        y = i % size.width;

        Vec3b color = image.at<Vec3b>(Point(x, y));
        Vec4b color2 = image.at<Vec4b>(Point(x, y));
        // cout << color << " - " << color2 << endl;
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