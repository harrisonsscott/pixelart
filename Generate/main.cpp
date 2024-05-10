#include <iostream>
#include <fstream>
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
    if (argc < 5){
        cout << "./pixelArt [Image] [Output] [Size X] [Size Y] [Threshold]" << endl;
        return -1;
    }

    Mat image = imread(argv[1], IMREAD_UNCHANGED);
    if (image.empty()){
        cout << "Error loading image " << argv[1] << endl;
        return -1;
    }

    Size size = Size(atoi(argv[3]), atoi(argv[4]));
    if (size.area() < 0){
        cout << "Invalid dimensions!" << endl;
        return -1;
    }

    int threshold = atoi(argv[5]);

    Mat im;
    vector<Vec4b> palette; 
    json j;
    ofstream jsonFile(argv[2]);


    if (!jsonFile.is_open()){
        cout << "Error opening file " << argv[2] << endl;
        return -1;
    }

    resize(image, im, size);

    for (int x = 0; x < size.width; x++){
        for (int y = 0; y < size.height; y++){
            int similarity = 10000000;
            int index = 0;

            Vec4b& color = im.at<Vec4b>(Point(y, x));
            Vec4b selectedColor = color;

            for (int i = 0; i < palette.size(); i++){
                int similarity2 = getSimilarity(color, palette[i]);
                if (similarity2 < similarity){
                    similarity = similarity2;
                    selectedColor = palette[i];
                    index = i;
                }
            }

            if (similarity > threshold){
                int pos = palette.size() * 4;

                j["keys"][pos] = color[2] / 255.0;
                j["keys"][pos + 1] = color[1] / 255.0;
                j["keys"][pos + 2] = color[0] / 255.0;
                j["keys"][pos + 3] = color[3] / 255.0;
                palette.push_back(color);
            }

            j["data"][x * size.width + y] = index;
            if (color[3] < 255.0){
                j["alpha"][x * size.width + y] = false;
            } else {
                j["alpha"][x * size.width + y] = true;
            }


            im.at<Vec4b>(Point(y, x)) = selectedColor;
        }
    }

    j["size"][0] = size.width;
    j["size"][1] = size.height;

    imwrite("out.png", im);

    jsonFile << j.dump(4);

    image.release();
    im.release();
    jsonFile.close();

    return 0;
}