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

    // resize the image
    Mat im = Mat::zeros(size, image.type());
    resize(image, im, size, INTER_AREA);
    imwrite("resized.png", im);

    for (int i = 0; i < size.area(); i++){
        x = floor(i / size.height);
        y = i % size.width;

        Vec3b color = image.at<Vec3b>(Point(x, y));
        Vec4b color2 = image.at<Vec4b>(Point(x, y));
        cout << color << " - " << color2 << endl;
    }

    // cleaning up
    image.release();

    return 0;
}