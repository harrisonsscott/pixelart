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
    cv::Mat image = cv::imread(argv[1], cv::IMREAD_UNCHANGED);
    cout << image.channels() << endl;
    if (image.channels() == 3){
        cvtColor(image, image, COLOR_BGR2BGRA);
    }

    for (int y = 0; y < 8; y++){
        for (int x = 0; x < 8; x++){
            Vec3b color = image.at<Vec3b>(Point(x, y));
            Vec4b color2 = image.at<Vec4b>(Point(x, y));
            cout << color << " - " << color2 << endl;
        }  
    }


    return 0;
}