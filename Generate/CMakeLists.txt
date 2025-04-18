cmake_minimum_required(VERSION 3.10)

project(pixelArt)

set(OpenCV_DIR $ENV{OPENCV})
find_package(OpenCV REQUIRED)
add_executable(${PROJECT_NAME} ./main.cpp base64.cpp)
add_executable(decode ./decode.cpp)
target_link_libraries(${PROJECT_NAME} ${OpenCV_LIBS})
target_link_libraries(decode ${OpenCV_LIBS})
include_directories(./json/include)