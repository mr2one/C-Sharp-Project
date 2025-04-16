#include <iostream> 
#include <opencv2/opencv.hpp> 
using namespace cv;
using namespace std;

void binary_erosion(const Mat& input_image, Mat& output_image) {
	output_image = input_image.clone();

	int rows = input_image.rows;
	int cols = input_image.cols;

	for (int i = 1; i < rows - 1; ++i) {
		for (int j = 1; j < cols - 1; ++j) {
			// Проверяем 3x3 окрестность
			bool erode_pixel = true;
			for (int x = -1; x <= 1; ++x) {
				for (int y = -1; y <= 1; ++y) {
					if (input_image.at<uchar>(i + x, j + y) == 0) {
						erode_pixel = false; // Нашли хотя бы один черный пиксель
						break;
					}
				}
				if (!erode_pixel) break;
			}
			if (!erode_pixel) {
				output_image.at<uchar>(i, j) = 0; // Ставим в центр структурного элемента черный пиксель
			}
		}
	}
}

void binary_dilation(const Mat& input_image, Mat& output_image) {
	output_image = input_image.clone();

	int rows = input_image.rows;
	int cols = input_image.cols;

	for (int i = 1; i < rows - 1; ++i) {
		for (int j = 1; j < cols - 1; ++j) {
			// Проверяем 3x3 окрестность
			bool dilate_pixel = false;
			for (int x = -1; x <= 1; ++x) {
				for (int y = -1; y <= 1; ++y) {
					if (input_image.at<uchar>(i + x, j + y) == 255) {
						dilate_pixel = true; // Нашли хотя бы один белый пиксель
						break;
					}
				}
				if (dilate_pixel) break;
			}
			if (dilate_pixel) {
				output_image.at<uchar>(i, j) = 255; // Ставим в центр структурного элемента белый пиксель
			}
		}
	}
}

void binary_opening(const Mat& input_image, Mat& output_image) {
	Mat temp_image;
	binary_erosion(input_image, temp_image);
	binary_dilation(temp_image, output_image);
}

void binary_closing(const Mat& input_image, Mat& output_image) {
	Mat temp_image;
	binary_dilation(input_image, temp_image);
	binary_erosion(temp_image, output_image);
}

void erosion(const Mat& input_img, Mat& output_img, int size = 3) {
	int half_size = size / 2;
	output_img = Mat::zeros(input_img.size(), CV_8U);

	for (int i = half_size; i < input_img.cols - half_size; i++)
		for (int j = half_size; j < input_img.rows - half_size; j++) {
			uchar min_value = 255;
			for (int ii = -half_size; ii <= half_size; ii++)
				for (int jj = -half_size; jj <= half_size; jj++) {
					uchar Y = input_img.at<uchar>(j + jj, i + ii);
					if (Y < min_value)
						min_value = Y;
				}
			output_img.at<uchar>(j, i) = min_value;
		}
}

void dilation(const Mat& input_img, Mat& output_img, int size = 3) {
	int half_size = size / 2;
	output_img = Mat::zeros(input_img.size(), CV_8U);

	for (int i = half_size; i < input_img.cols - half_size; i++)
		for (int j = half_size; j < input_img.rows - half_size; j++) {
			uchar max_value = 0;
			for (int ii = -half_size; ii <= half_size; ii++)
				for (int jj = -half_size; jj <= half_size; jj++) {
					uchar Y = input_img.at<uchar>(j + jj, i + ii);
					if (Y > max_value)
						max_value = Y;
				}
			output_img.at<uchar>(j, i) = max_value;
		}
}

void opening(const Mat& input_img, Mat& output_img) {
	Mat temp_image;
	erosion(input_img, temp_image);
	dilation(temp_image, output_img);
}

void closing(const Mat& input_img, Mat& output_img) {
	Mat temp_image;
	dilation(input_img, temp_image);
	erosion(temp_image, output_img);
}

void extract_contours(const Mat& input_img, Mat& output_img) {
	Mat dilated_img, eroded_img;

	dilation(input_img, dilated_img);
	erosion(input_img, eroded_img);

	// Контуры - это разница между дилатированным и эрозированным изображениями
	output_img = Mat::zeros(input_img.size(), CV_8U);
	for (int i = 0; i < input_img.cols; i++)
		for (int j = 0; j < input_img.rows; j++) {
			output_img.at<uchar>(j, i) = abs(dilated_img.at<uchar>(j, i) - eroded_img.at<uchar>(j, i));
		}
}

void multiscale_morphological_gradient(const Mat& input_img, Mat& output_img) {
	Mat result_img = Mat::zeros(input_img.size(), CV_8U);
	Mat dilated_img, eroded_img, gradient;

	// Начинаем цикл с разными размерами структурных элементов
	for (int scale = 1; scale <= 3; scale++) {
		int size = 2 * scale + 1; // Размер структурного элемента

		dilation(input_img, dilated_img, size);
		erosion(input_img, eroded_img, size);

		gradient = Mat::zeros(input_img.size(), CV_8U);
		for (int i = 0; i < input_img.cols; i++)
			for (int j = 0; j < input_img.rows; j++) {
				gradient.at<uchar>(j, i) = abs(dilated_img.at<uchar>(j, i) - eroded_img.at<uchar>(j, i));
			}

		// Суммируем результат для каждого масштаба
		for (int i = 0; i < input_img.cols; i++)
			for (int j = 0; j < input_img.rows; j++) {
				result_img.at<uchar>(j, i) += gradient.at<uchar>(j, i) / 3;  // Усредняем результат
			}
	}

	output_img = result_img;
}

void binarize(const Mat& input_image, Mat& output_image, double threshold_value) {
	threshold(input_image, output_image, threshold_value, 255, THRESH_BINARY);
}

void clearConsole() {
	// Очищаем консоль
	system("cls");
}

int main(int argc, char** argv)
{
	setlocale(LC_ALL, "Russian");

	// Загрузка изображения
	Mat image = imread("koleso.jpg", IMREAD_GRAYSCALE);
	Mat out_image = imread("koleso.jpg");
	Mat binary_image; // Переменная для хранения бинаризованного изображения

	if (image.empty()) {
		cout << "Image File " << "Not Found" << endl;
		cin.get();
		return -1;
	}

	// Запрос порогового значения для бинаризации
	double threshold_value;
	cout << "Введите пороговое значение для бинаризации (0-255): ";
	cin >> threshold_value;

	binarize(image, binary_image, threshold_value);
	imshow("Бинаризованное изображение", binary_image);
	waitKey(0);

	int choice;
	do {
		clearConsole();

		cout << "Выберите действие с изображением:" << endl;
		cout << "1. Применить бинарную эрозию" << endl;
		cout << "2. Пренить бинарную дилатацию" << endl;
		cout << "3. Применить оператор открытия бинарного изображения" << endl;
		cout << "4. Применить оператор закрытия бинарного изображения" << endl;
		cout << "5. Применить эрозию полутонового изображения" << endl;
		cout << "6. Применить дилатацию полутонового изображения" << endl;
		cout << "7. Применить оператор открытия полутонового изображения" << endl;
		cout << "8. Применить оператор закрытия полутонового изображения" << endl;
		cout << "9. Применить оператор выделения контуров полутонового изображения" << endl;
		cout << "10. Применить многомасштабный морфологический градиент полутонового изображения" << endl;
		cout << "0. Выход" << endl;

		cout << "Введите номер действия: ";
		cin >> choice;

		switch (choice) {
		case 1:
			binary_erosion(binary_image, out_image);
			imshow("Бинаризованное изображение", binary_image);
			imshow("Бинарная эрозия", out_image);
			break;
		case 2:
			binary_dilation(binary_image, out_image);
			imshow("Бинаризованное изображение", binary_image);
			imshow("Бинарная дилатация", out_image);
			break;
		case 3:
			binary_opening(binary_image, out_image);
			imshow("Бинаризованное изображение", binary_image);
			imshow("Оператор открытия", out_image);
			break;
		case 4:
			binary_closing(binary_image, out_image);
			imshow("Бинаризованное изображение", binary_image);
			imshow("Оператор закрытия", out_image);
			break;
		case 5:
			erosion(image, out_image);
			imshow("Исходное изображение", image);
			imshow("Эрозия", out_image);
			break;
		case 6: {
			dilation(image, out_image);
			imshow("Исходное изображение", image);
			imshow("Дилатация", out_image);
			break;
		}
		case 7: {
			opening(image, out_image);
			imshow("Исходное изображение", image);
			imshow("Открытие", out_image);
			break;
		}
		case 8:
			closing(image, out_image);
			imshow("Исходное изображение", image);
			imshow("Закрытие", out_image);
			break;
		case 9:
			extract_contours(image, out_image);
			imshow("Исходное изображение", image);
			imshow("Контуры", out_image);
			break;
		case 10:
			multiscale_morphological_gradient(image, out_image);
			imshow("Исходное изображение", image);
			imshow("Многомасштабный морфологический градиент", out_image);
			break;
		case 0:
			cout << "Выход..." << endl;
			exit(0);
			break;
		default:
			cout << "Неверный выбор. Пожалуйста, попробуйте снова." << endl;
			break;
		}

		waitKey(0);
	} while (choice != 0);

	return 0;
}