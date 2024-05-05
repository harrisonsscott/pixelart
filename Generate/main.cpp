#include <iostream>
#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>

using namespace cv;
using namespace std;

float getSimilarity(Vec3b color1, Vec3b color2){
	return
		0.11 * std::pow(color1[0] - color2[0], 2) +
		0.59 * std::pow(color1[1] - color2[1], 2) +
		0.3 * std::pow(color1[2] - color2[2], 2);
}

int main(int argc, char *argv[]){
    if (argc < 4){
        cout << "./pixelArt [Image] [Size X] [Size Y] [Threshold]" << endl;
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

    Mat im;
    vector<Vec3b> palette; 

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
                palette.push_back(color);
            }

            im.at<Vec3b>(Point(y, x)) = selectedColor;
        }
    }


    imwrite("out.png", im);

    image.release();
    im.release();

    return 0;
}