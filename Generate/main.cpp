#include <iostream>
#include <fstream>
#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include "json.hpp"

using namespace cv;
using namespace std;
using json = nlohmann::json;

float getSimilarity(Vec3b color1, Vec3b color2){
	return
		0.11 * std::pow(color1[0] - color2[0], 2) +
		0.59 * std::pow(color1[1] - color2[1], 2) +
		0.3 * std::pow(color1[2] - color2[2], 2);
}

string toHex(Vec3b color){
    int n;
    char r[4];
    char g[4];
    char b[4];
    sprintf(b, "%X", color[0]);
    sprintf(g, "%X", color[1]);
    sprintf(r, "%X", color[2]);
    return string("#") + r + g + b;
}

int main(int argc, char *argv[]){
    if (argc < 5){
        cout << "./pixelArt [Image] [Output] [Size X] [Size Y] [Threshold]" << endl;
        return -1;
    }

    Mat image = imread(argv[1]);
    if (image.empty()){
        cout << "Error loading image " << argv[1] << endl;
        return -1;
    }

    Size size = Size(atoi(argv[3]), atoi(argv[4]));
    if (size.area() < 0){
        cout << "Invalid dimensions!" << endl;
        return -1;
    }

    Mat im;
    vector<Vec3b> palette; 
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

            Vec3b& color = im.at<Vec3b>(Point(y, x));
            Vec3b selectedColor = color;

            for (int i = 0; i < palette.size(); i++){
                int similarity2 = getSimilarity(color, palette[i]);
                if (similarity2 < similarity){
                    similarity = similarity2;
                    selectedColor = palette[i];
                    index = i;
                }
            }

            if (similarity > 100){
                int pos = palette.size() * 3;

                j["keys"][pos] = color[2] / 255.0;
                j["keys"][pos + 1] = color[1] / 255.0;
                j["keys"][pos + 2] = color[0] / 255.0;

                palette.push_back(color);
            }

            j["data"][x * size.width + y] = index;

            im.at<Vec3b>(Point(y, x)) = selectedColor;
        }
    }


    imwrite("out.png", im);

    jsonFile << j.dump(4);

    image.release();
    im.release();
    jsonFile.close();

    return 0;
}