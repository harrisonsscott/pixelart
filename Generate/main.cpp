#include <iostream>
#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>

using namespace cv;
using namespace std;

int main(int argc, char *argv[]){
    if (argc < 4){
        cout << "./pixelArt [Image] [Size X] [Size Y]" << endl;
        return -1;
    }

    Mat image = imread(argv[1]);
    if (image.empty()){
        cout << "Error loading image " << argv[1] << endl;
        return -1;
    }

    Size size = Size(atoi(argv[2]), atoi(argv[3]));
    if (size.area() < 0){
        cout << "Invalid dimensions!" << endl;
        return -1;
    }

    cout << size << endl;
        
    return 0;
}