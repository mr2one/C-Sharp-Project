#include <iostream>
#include <opencv2/opencv.hpp>
#include <cmath>

using namespace std;
using namespace cv;

int quantizationMatrixY[8][8] = {
    {16, 11, 10, 16, 24, 40, 51, 61},
    {12, 12, 14, 19, 26, 58, 60, 55},
    {14, 13, 16, 24, 40, 57, 69, 56},
    {14, 17, 22, 29, 51, 87, 80, 62},
    {18, 22, 37, 56, 68, 109, 103, 77},
    {24, 35, 55, 64, 81, 104, 113, 92},
    {49, 64, 78, 87, 103, 121, 120, 101},
    {72, 92, 95, 98, 112, 100, 103, 99}
};

int quantizationMatrixCrCb[8][8] = {
    {17, 18, 24, 47, 99, 99, 99, 99},
    {18, 21, 26, 99, 99, 99, 99, 99},
    {24, 26, 56, 99, 99, 99, 99, 99},
    {47, 99, 99, 99, 99, 99, 99, 99},
    {99, 99, 99, 99, 99, 99, 99, 99},
    {99, 99, 99, 99, 99, 99, 99, 99},
    {99, 99, 99, 99, 99, 99, 99, 99},
    {99, 99, 99, 99, 99, 99, 99, 99}
};

// Функция для вычисления гистограммы
vector<int> calculateHistogram(const Mat& grayImage) {
    vector<int> histogram(256, 0);

    for (int y = 0; y < grayImage.rows; y++) {
        for (int x = 0; x < grayImage.cols; x++) {
            uchar pixelValue = grayImage.at<uchar>(y, x);
            histogram[pixelValue]++;
        }
    }

    return histogram;
}

// Функция для вычисления энтропии
double calculateEntropy(const vector<int>& histogram, int totalPixels) {
    double entropy = 0.0;

    for (int i = 0; i < histogram.size(); i++) {
        if (histogram[i] > 0) {
            double pi = static_cast<double>(histogram[i]) / totalPixels; // Вероятность
            entropy += -pi * log2(pi);
        }
    }

    return entropy;
}

// Функция для разбиения на блоки 8x8
vector<Mat> splitIntoBlocks(const Mat& image) {
    vector<Mat> blocks;

    int blockSize = 8;

    for (int y = 0; y < image.rows; y += blockSize) {
        for (int x = 0; x < image.cols; x += blockSize) {
            // Проверяем границы, чтобы размеры изображения были не больше 8
            int blockWidth = min(blockSize, image.cols - x);
            int blockHeight = min(blockSize, image.rows - y);

            Rect blockRect(x, y, blockWidth, blockHeight);
            Mat block = image(blockRect);

            blocks.push_back(block.clone());
        }
    }

    return blocks;
}

// Функция для подвыборки компонентов Cr и Cb
Mat subsample(const Mat& channel) {
    Mat subsampled(channel.rows / 2, channel.cols / 2, channel.type());

    for (int y = 0; y < subsampled.rows; y++) {
        for (int x = 0; x < subsampled.cols; x++) {
            subsampled.at<uchar>(y, x) = channel.at<uchar>(y * 2, x * 2); // Берем пиксели через строку и через столбец
        }
    }

    return subsampled;
}

// Функция для применения ДКП
Mat applyDCT(const Mat& block) {
    Mat floatBlock;
    block.convertTo(floatBlock, CV_32F);
    Mat dctBlock;
    dct(floatBlock, dctBlock);

    return dctBlock;
}

// Функция для квантования блока ДКП
Mat quantizeDCT(const Mat& dctBlock, const int quantizationMatrix[8][8]) {
    Mat quantizedBlock = dctBlock.clone();

    // Квантование блока
    for (int i = 0; i < 8; i++) {
        for (int j = 0; j < 8; j++) {
            quantizedBlock.at<float>(i, j) = round(quantizedBlock.at<float>(i, j) / quantizationMatrix[i][j]);
        }
    }

    return quantizedBlock;
}

// Функция для зиг-заг сканирования
vector<int> zigzagScan(const Mat& block) {
    vector<int> zigzagVector(64);
    int idx = 0;
    // Перебор всех диагоналей
    for (int sum = 0; sum < 15; sum++) {
        int rowStart = sum < 8 ? 0 : sum - 7;
        int colStart = sum < 8 ? sum : 7;

        while (rowStart < 8 && colStart >= 0) {
            zigzagVector[idx++] = block.at<float>(rowStart, colStart);
            rowStart++;
            colStart--;
        }
    }
    return zigzagVector;
}

// Функция для RLE
vector<pair<int, int>> runLengthEncode(const vector<int>& zigzagVector) {
    vector<pair<int, int>> rleVector;
    int count = 0;
    int currentValue = zigzagVector[0];

    for (size_t i = 0; i < zigzagVector.size(); ++i) {
        if (zigzagVector[i] == currentValue) {
            count++; // Считаем количество одинаковых значений
        }
        else {
            rleVector.push_back({ count - 1, currentValue });
            currentValue = zigzagVector[i];
            count = 1;
        }
    }
    rleVector.push_back({ count - 1, currentValue }); // Добавляем последнюю пару
    return rleVector;
}

// Подсчёт количества бит RLE
int calculateRLEBits(const vector<pair<int, int>>& rleBlock) {
    int totalBits = 0;

    for (const auto& pair : rleBlock) {
        int run = pair.first;
        int value = pair.second;

        int runBits = run > 0 ? static_cast<int>(ceil(log2(run + 1))) : 1;

        int valueBits = value != 0 ? static_cast<int>(ceil(log2(abs(value) + 1))) + 1 : 1;

        totalBits += runBits + valueBits;
    }

    return totalBits;
}

// Обратное зигзаг-сканирование
Mat inverseZigzagScan(const vector<int>& zigzagVector) {
    Mat block(8, 8, CV_32F, Scalar(0));
    int idx = 0;

    for (int sum = 0; sum < 15; sum++) {
        int rowStart = sum < 8 ? 0 : sum - 7;
        int colStart = sum < 8 ? sum : 7;

        while (rowStart < 8 && colStart >= 0) {
            block.at<float>(rowStart, colStart) = zigzagVector[idx++];
            rowStart++;
            colStart--;
        }
    }

    return block;
}

// Обратное квантование блока
Mat dequantizeDCT(const Mat& quantizedBlock, const int quantizationMatrix[8][8]) {
    Mat dequantizedBlock = quantizedBlock.clone();

    for (int i = 0; i < 8; i++) {
        for (int j = 0; j < 8; j++) {
            dequantizedBlock.at<float>(i, j) *= quantizationMatrix[i][j];
        }
    }

    return dequantizedBlock;
}

// Применение обратной ДКП
Mat applyIDCT(const Mat& block) {
    Mat idctBlock;
    idct(block, idctBlock);
    idctBlock.convertTo(idctBlock, CV_8U);
    return idctBlock;
}

// Обратное восстановление изображения
Mat reconstructImage(const vector<Mat>& blocks, int imageWidth, int imageHeight) {
    Mat reconstructed(imageHeight, imageWidth, CV_8U, Scalar(0));
    int blockSize = 8;
    int idx = 0;

    for (int y = 0; y < imageHeight; y += blockSize) {
        for (int x = 0; x < imageWidth; x += blockSize) {
            Rect roi(x, y, blockSize, blockSize);
            blocks[idx++].copyTo(reconstructed(roi));
        }
    }

    return reconstructed;
}

int main() {
    setlocale(LC_ALL, "Russian");

    Mat image = imread("koleso.png");
    if (image.empty()) {
        cout << "Ошибка: файл изображения не найден." << endl;
        return -1;
    }

    imshow("Исходное изображение", image);

    // Преобразование RGB в YCrCb
    Mat ycrcbImage;
    cvtColor(image, ycrcbImage, COLOR_BGR2YCrCb);
    
    // Разделяем на каналы Y, Cr, Cb
    vector<Mat> channels;
    split(ycrcbImage, channels);

    // Подвыборка Cr и Cb
    Mat subsampledCr = subsample(channels[1]);
    Mat subsampledCb = subsample(channels[2]);

    // Построение гистограммы яркости
    vector<int> histogram = calculateHistogram(channels[0]);

    // Вычисление энтропии
    int totalPixels = channels[0].rows * channels[0].cols;
    double entropy = calculateEntropy(histogram, totalPixels);

    // Вывод результатов
    cout << "Энтропия изображения: " << entropy << endl;

    // Подсчет избыточности
    double R = 1 - (entropy / 8);
    cout << "Избыточность изображения: " << R << endl;

    // Визуализация гистограммы
    int histWidth = 512, histHeight = 400;
    int binWidth = cvRound((double)histWidth / 256);

    Mat histImage(histHeight, histWidth, CV_8UC1, Scalar(255)); // Белое изображение для рисования гистограммы

    // Нормализация гистограммы для отображения
    int maxHist = *max_element(histogram.begin(), histogram.end());
    for (int i = 0; i < 256; i++) {
        histogram[i] = ((double)histogram[i] / maxHist) * histHeight;
    }

    // Отрисовка гистограммы
    for (int i = 0; i < 256; i++) {
        line(histImage, Point(binWidth * i, histHeight),
            Point(binWidth * i, histHeight - histogram[i]),
            Scalar(0), 1, 8, 0);
    }

    imshow("Гистограмма яркости", histImage);

    // Разбиение на блоки 8x8
    vector<Mat> blocksY = splitIntoBlocks(channels[0]); // Блоки Y
    vector<Mat> blocksCr = splitIntoBlocks(subsampledCr); // Блоки Cr
    vector<Mat> blocksCb = splitIntoBlocks(subsampledCb); // Блоки Cb

    // Применение ДКП к каждому блоку
    vector<Mat> dctBlocksY;
    for (Mat& block : blocksY) {
        Mat dctBlock = applyDCT(block);
        dctBlocksY.push_back(dctBlock);
    }

    vector<Mat> dctBlocksCr;
    for (Mat& block : blocksCr) {
        Mat dctBlock = applyDCT(block);
        dctBlocksCr.push_back(dctBlock);
    }

    vector<Mat> dctBlocksCb;
    for (Mat& block : blocksCb) {
        Mat dctBlock = applyDCT(block);
        dctBlocksCb.push_back(dctBlock);
    }

    // Квантование блоков ДКП для каждого канала
    vector<Mat> quantizedBlocksY;
    for (Mat& block : dctBlocksY) {
        Mat quantizedBlock = quantizeDCT(block, quantizationMatrixY);
        quantizedBlocksY.push_back(quantizedBlock);
    }

    vector<Mat> quantizedBlocksCr;
    for (Mat& block : dctBlocksCr) {
        Mat quantizedBlock = quantizeDCT(block, quantizationMatrixCrCb);
        quantizedBlocksCr.push_back(quantizedBlock);
    }

    vector<Mat> quantizedBlocksCb;
    for (Mat& block : dctBlocksCb) {
        Mat quantizedBlock = quantizeDCT(block, quantizationMatrixCrCb);
        quantizedBlocksCb.push_back(quantizedBlock);
    }

    // Зигзаг сканирование для канала Y
    vector<vector<int>> zigzagBlocksY;
    for (Mat& block : quantizedBlocksY) {
        vector<int> zigzagBlock = zigzagScan(block);
        zigzagBlocksY.push_back(zigzagBlock);
    }

    // Зигзаг сканирование для канала Cr
    vector<vector<int>> zigzagBlocksCr;
    for (Mat& block : quantizedBlocksCr) {
        vector<int> zigzagBlock = zigzagScan(block);
        zigzagBlocksCr.push_back(zigzagBlock);
    }

    // Зигзаг сканирование для канала Cb
    vector<vector<int>> zigzagBlocksCb;
    for (Mat& block : quantizedBlocksCb) {
        vector<int> zigzagBlock = zigzagScan(block);
        zigzagBlocksCb.push_back(zigzagBlock);
    }

    // RLE для канала Y
    vector<vector<pair<int, int>>> rleBlocksY;
    for (const auto& zigzagBlock : zigzagBlocksY) {
        vector<pair<int, int>> rleBlock = runLengthEncode(zigzagBlock);
        rleBlocksY.push_back(rleBlock);
    }

    // RLE для канала Cr
    vector<vector<pair<int, int>>> rleBlocksCr;
    for (const auto& zigzagBlock : zigzagBlocksCr) {
        vector<pair<int, int>> rleBlock = runLengthEncode(zigzagBlock);
        rleBlocksCr.push_back(rleBlock);
    }

    // RLE для канала Cb
    vector<vector<pair<int, int>>> rleBlocksCb;
    for (const auto& zigzagBlock : zigzagBlocksCb) {
        vector<pair<int, int>> rleBlock = runLengthEncode(zigzagBlock);
        rleBlocksCb.push_back(rleBlock);
    }

    // Выводим все блоки RLE для канала YCrCb
    cout << "RLE для канала Y: " << endl;
    for (size_t blockIndex = 0; blockIndex < rleBlocksY.size(); ++blockIndex) {
        cout << "Блок " << blockIndex << ": ";
        for (const auto& pair : rleBlocksY[blockIndex]) {
            cout << "(" << pair.first << "," << pair.second << ") ";
        }
        cout << endl;
    }

    cout << "RLE для канала Cr: " << endl;
    for (size_t blockIndex = 0; blockIndex < rleBlocksCr.size(); ++blockIndex) {
        cout << "Блок " << blockIndex << ": ";
        for (const auto& pair : rleBlocksCr[blockIndex]) {
            cout << "(" << pair.first << "," << pair.second << ") ";
        }
        cout << endl;
    }

    cout << "RLE для канала Cb: " << endl;
    for (size_t blockIndex = 0; blockIndex < rleBlocksCb.size(); ++blockIndex) {
        cout << "Блок " << blockIndex << ": ";
        for (const auto& pair : rleBlocksCb[blockIndex]) {
            cout << "(" << pair.first << "," << pair.second << ") ";
        }
        cout << endl;
    }

    // Подсчёт бит для канала Y
    int totalBitsY = 0;
    for (const auto& rleBlock : rleBlocksY) {
        totalBitsY += calculateRLEBits(rleBlock);
    }
    cout << "Общее количество бит для канала Y: " << totalBitsY << endl;

    // Подсчёт бит для канала Cr
    int totalBitsCr = 0;
    for (const auto& rleBlock : rleBlocksCr) {
        totalBitsCr += calculateRLEBits(rleBlock);
    }
    cout << "Общее количество бит для канала Cr: " << totalBitsCr << endl;

    // Подсчёт бит для канала Cb
    int totalBitsCb = 0;
    for (const auto& rleBlock : rleBlocksCb) {
        totalBitsCb += calculateRLEBits(rleBlock);
    }
    cout << "Общее количество бит для канала Cb: " << totalBitsCb << endl;

    // Итоговый вывод бит
    int totalBitsAll = totalBitsY + totalBitsCr + totalBitsCb;
    cout << "Общее количество бит для свернутого изображения: " << totalBitsAll << endl;
    int mainImg = image.rows * image.cols * 8;
    cout << "Общее количество бит для исходного изображения: " << mainImg << endl;
    double coef = (double)mainImg / (double)totalBitsAll;
    cout << "Коэффициент сжатия: " << coef << endl;

    // Восстановление изображение
    vector<Mat> idctBlocksY, idctBlocksCr, idctBlocksCb;

    for (const auto& zigzagBlock : zigzagBlocksY) {
        Mat block = inverseZigzagScan(zigzagBlock); // Обратное зигзаг-сканирование
        Mat dequantizedBlock = dequantizeDCT(block, quantizationMatrixY); // Обратное квантование
        Mat idctBlock = applyIDCT(dequantizedBlock); // Обратная ДКП
        idctBlocksY.push_back(idctBlock);
    }

    for (const auto& zigzagBlock : zigzagBlocksCr) {
        Mat block = inverseZigzagScan(zigzagBlock);
        Mat dequantizedBlock = dequantizeDCT(block, quantizationMatrixCrCb);
        Mat idctBlock = applyIDCT(dequantizedBlock);
        idctBlocksCr.push_back(idctBlock);
    }

    for (const auto& zigzagBlock : zigzagBlocksCb) {
        Mat block = inverseZigzagScan(zigzagBlock);
        Mat dequantizedBlock = dequantizeDCT(block, quantizationMatrixCrCb);
        Mat idctBlock = applyIDCT(dequantizedBlock);
        idctBlocksCb.push_back(idctBlock);
    }

    // Воссоединяем изображение
    Mat reconstructedY = reconstructImage(idctBlocksY, channels[0].cols, channels[0].rows);
    Mat reconstructedCr = reconstructImage(idctBlocksCr, subsampledCr.cols, subsampledCr.rows);
    Mat reconstructedCb = reconstructImage(idctBlocksCb, subsampledCb.cols, subsampledCb.rows);

    // Увеличиваем Cr и Cb до исходного размера
    Mat resizedCr, resizedCb;
    resize(reconstructedCr, resizedCr, channels[1].size(), 0, 0, INTER_LINEAR);
    resize(reconstructedCb, resizedCb, channels[2].size(), 0, 0, INTER_LINEAR);

    // Собираем обратно YCrCb изображение
    vector<Mat> reconstructedChannels = { reconstructedY, resizedCr, resizedCb };
    Mat reconstructedYCrCb;
    merge(reconstructedChannels, reconstructedYCrCb);

    // Преобразуем обратно в RGB
    Mat reconstructedImage;
    cvtColor(reconstructedYCrCb, reconstructedImage, COLOR_YCrCb2BGR);

    // Вывод результата
    imshow("Восстановленное изображение", reconstructedImage);

    waitKey(0);
    return 0;
}
