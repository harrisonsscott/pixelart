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

    cv::Mat image = cv::imread(argv[1], cv::IMREAD_UNCHANGED);
    if (image.channels() == 3) {
        cv::cvtColor(image, image, cv::COLOR_BGR2BGRA);
    }
    if (image.empty()){
        cout << "Error loading image " << argv[1] << endl;
        return -1;
    }

    Size size = Size(atoi(argv[3]), atoi(argv[4]));

    if (argv[4][0] == 'x'){
        size.height = (int)(atoi(argv[3]) * (image.size().height / (float)image.size().width));
    }

    if (size.area() < 0){
        cout << "Invalid dimensions!" << endl;
        return -1;
    }

    int width = max(size.width, size.height);

    Mat im0;
    Mat im = Mat::zeros(width, width, image.type());
    resize(image, im0, size);
    
    size = Size(width,width);

    Rect roi(Point(0,0), im0.size());
    im0.copyTo(im(roi));


    // return 0;

    int threshold = atoi(argv[5]);

    vector<Vec4b> palette; 
    vector<int> amount;
    json j;
    ofstream jsonFile(argv[2]);

    if (!jsonFile.is_open()){
        cout << "Error opening file " << argv[2] << endl;
        return -1;
    }

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

    int seq = 1; // amount of sequential numbers (for compression)
    int seqNumber = 0;
    int seqIndex = 0;
    int prevNum = 0;

    for (int x = 0; x < size.width; x++){
        for (int y = 0; y < size.height; y++){
            int similarity = 10000000;
            int index = prevNum+1;

            Vec4b& color = im.at<Vec4b>(Point(x, y));
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
            palette.erase(palette.begin() + i);
            for (int v = 0; v < seqIndex; v++){
                int n = j["data"][v]["number"];
                if (n >= i){
                    j["data"][v]["number"] = n - 1;
                }
            }
        }
    }

    for (int i = 0; i < palette.size(); i++){
        int pos = i * 4;
        Vec4b& color = palette[i];

        j["keys"][pos] = color[2] / 255.0;
        j["keys"][pos + 1] = color[1] / 255.0;
        j["keys"][pos + 2] = color[0] / 255.0;
        j["keys"][pos + 3] = color[3] / 255.0;
    }

    // cv::imwrite("out.png", im);

    jsonFile << j.dump();

    image.release();
    im.release();
    jsonFile.close();

    return 0;
}