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
    vector<int> amount;
    json j;
    ofstream jsonFile(argv[2]);

    if (!jsonFile.is_open()){
        cout << "Error opening file " << argv[2] << endl;
        return -1;
    }

    resize(image, im, size);

    int seq = 1; // amount of sequential numbers (for compression)
    int seqNumber = 0;
    int seqIndex = 0;
    int prevNum = 0;


    for (int x = 0; x < size.width; x++){
        for (int y = 0; y < size.height; y++){
            int similarity = 10000000;
            int index = 0;

            Vec4b& color = im.at<Vec4b>(Point(y, x));
            Vec4b selectedColor = color;

            for (int i = 0; i < palette.size(); i++){
                int similarity2 = getSimilarity(color, palette[i]);
                if (similarity2 <= similarity){
                    similarity = similarity2;
                    selectedColor[3] = selectedColor[3] == 1 ? 1 : 0;
                    selectedColor = palette[i];
                    index = i;
                }
            }

            if (similarity >= threshold){
                int pos = palette.size() * 4;

                j["keys"][pos] = color[2] / 255.0;
                j["keys"][pos + 1] = color[1] / 255.0;
                j["keys"][pos + 2] = color[0] / 255.0;
                j["keys"][pos + 3] = color[3] / 255.0;
                palette.push_back(color);

                // shader can only render numbers up to 99
                if (palette.size() > 99){
                    cout << "Can't go above 99 colors, increase the threshold!" << endl;
                    return -1;
                }
            }


            if (index == prevNum){
                seqNumber = index;
                seq++;
            } else {
                j["data"][seqIndex]["number"] = seqNumber; // number
                j["data"][seqIndex]["length"] = seq; // amount of sequential numbers
                
                while (amount.size() <= seqNumber){
                    amount.push_back(0);
                }

                amount[seqNumber] += seq;

                seqIndex++;
                seq = 1;
                seqNumber = index;

            }

            // uncompressed form

            // j["data"][x * size.width + y] = index;

            // if (color[3] < 255.0){
            //     j["alpha"][x * size.width + y] = false;
            // } else {
            //     j["alpha"][x * size.width + y] = true;
            // }

            j["solved"][x * size.width + y] = 0;

            im.at<Vec4b>(Point(y, x)) = selectedColor;

            prevNum = index;
        }
    }

    j["size"][0] = size.width;
    j["size"][1] = size.height;

    for (int i = 0; i < amount.size(); i++){
        if (amount[i] == 0){
            for (int v = 0; v < seqIndex; v++){
                int n = j["data"][v]["number"];
                if (n >= i){
                    j["data"][v]["number"] = n - 1;
                }
            }
        }
        cout << amount[i] << endl;
    }

    cv::imwrite("out.png", im);

    jsonFile << j.dump();

    image.release();
    im.release();
    jsonFile.close();

    return 0;
}