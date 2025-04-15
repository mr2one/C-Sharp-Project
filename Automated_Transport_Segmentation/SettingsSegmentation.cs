using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automated_Transport_Segmentation
{
    public partial class SettingsSegmentation : Form
    {
        MainWindow connect;

        public SettingsSegmentation()
        {
            InitializeComponent();
        }

        public SettingsSegmentation(MainWindow link)
        {
            InitializeComponent();
            connect = link;
            pictureBox1.Image = link.ResultF2.Image;

            Bitmap add_empty = new Bitmap(pictureBox1.Image);
            for (int y = 0; y < add_empty.Width; y++)
            {
                for (int x = 0; x < add_empty.Height; x++)
                {
                    add_empty.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }
            pictureBox2.Image = add_empty;
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        struct Save_undo
        {
            public Bitmap SavePictureUndo;
        }

        Stack<Save_undo> stack = new Stack<Save_undo>();
        public void saver()
        {
            var a = new Save_undo();
            if (pictureBox2.Image != null)
                a.SavePictureUndo = new Bitmap(pictureBox2.Image);
            stack.Push(a);
        }

        private void Binarization(object sender, EventArgs e) //Применение бинаризации изображения
        {
            this.Width = 1045;
            this.Height = 361;
            saver();
            int NumThreshold = int.Parse(label3.Text);
            Bitmap SourceImage = new Bitmap(pictureBox1.Image); //загрузка исходного изображения
            Bitmap BinaryMask = new Bitmap(SourceImage.Width, SourceImage.Height); //создание бинарной маски
            int threshold = NumThreshold; //установка порогового значения бинаризации
            for (int y = 0; y < SourceImage.Height; y++)
            {
                for (int x = 0; x < SourceImage.Width; x++)
                {
                    Color pixel = SourceImage.GetPixel(x, y);
                    int grayScale = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114); //перевод в оттенки серого
                    if (grayScale > threshold)
                    {
                        BinaryMask.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        BinaryMask.SetPixel(x, y, Color.Black);
                    }
                }
            }
            pictureBox2.Image = BinaryMask;
            connect.ResultF2.Image = BinaryMask;
        }

        public static Bitmap MorphologicalClosing(Bitmap inputBitmap, int iterations) //Операция: морфологическое закрытие
        {
            Bitmap outputBitmap = new Bitmap(inputBitmap);

            for (int i = 0; i < iterations; i++)
            {
                outputBitmap = Dilation(outputBitmap);
                outputBitmap = Erosion(outputBitmap);
            }

            return outputBitmap;
        }

        public static Bitmap MorphologicalOpen(Bitmap inputBitmap, int iterations) //Операция: морфологическое открытие
        {
            Bitmap outputBitmap = new Bitmap(inputBitmap);

            for (int i = 0; i < iterations; i++)
            {
                outputBitmap = Erosion(outputBitmap);
                outputBitmap = Dilation(outputBitmap);
            }

            return outputBitmap;
        }

        public static Bitmap Dilation(Bitmap inputBitmap) //Операция: морфологическая дилатация
        {
            Bitmap outputBitmap = new Bitmap(inputBitmap);

            for (int i = 0; i < inputBitmap.Width; i++)
            {
                for (int j = 0; j < inputBitmap.Height; j++)
                {
                    Color maxColor = Color.Black;

                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {
                            int x = i + k;
                            int y = j + l;

                            if (x >= 0 && x < inputBitmap.Width && y >= 0 && y < inputBitmap.Height)
                            {
                                Color color = inputBitmap.GetPixel(x, y);

                                if (color.R > maxColor.R)
                                {
                                    maxColor = color;
                                }
                            }
                        }
                    }

                    outputBitmap.SetPixel(i, j, maxColor);
                }
            }
            return outputBitmap;
        }

        public static Bitmap Erosion(Bitmap inputBitmap) //Операция: морфологическая эрозия
        {
            Bitmap outputBitmap = new Bitmap(inputBitmap);

            for (int i = 0; i < inputBitmap.Width; i++)
            {
                for (int j = 0; j < inputBitmap.Height; j++)
                {
                    Color minColor = Color.White;

                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {
                            int x = i + k;
                            int y = j + l;

                            if (x >= 0 && x < inputBitmap.Width && y >= 0 && y < inputBitmap.Height)
                            {
                                Color color = inputBitmap.GetPixel(x, y);

                                if (color.R < minColor.R)
                                {
                                    minColor = color;
                                }
                            }
                        }
                    }

                    outputBitmap.SetPixel(i, j, minColor);
                }
            }

            return outputBitmap;
        }

        private void UseErosion(object sender, EventArgs e) //Применение морфологической дилатации к изображению
        {
            saver();
            Bitmap outputBitmap = new Bitmap(pictureBox2.Image);
            outputBitmap = Dilation(outputBitmap);
            pictureBox2.Image = outputBitmap;
            connect.ResultF2.Image = outputBitmap;
        }

        private void UseDilatation(object sender, EventArgs e) //Применение морфологической эрозии к изображению
        {
            saver();
            Bitmap outputBitmap = new Bitmap(pictureBox2.Image);
            outputBitmap = Erosion(outputBitmap);
            pictureBox2.Image = outputBitmap;
            connect.ResultF2.Image = outputBitmap;
        }

        private void UseOpen(object sender, EventArgs e) //Применение морфологического открытия к изображению
        {
            saver();
            Bitmap souBitmap = new Bitmap(pictureBox2.Image);
            Bitmap ouBitmap = MorphologicalOpen(souBitmap, 2);
            pictureBox2.Image = ouBitmap;
            connect.ResultF2.Image = ouBitmap;
        }

        private void UseClose(object sender, EventArgs e)//Применение морфологического закрытия к изображению
        {
            saver();
            Bitmap sourceBitmap = new Bitmap(pictureBox2.Image);
            Bitmap outputBitmap = MorphologicalClosing(sourceBitmap, 2);
            pictureBox2.Image = outputBitmap;
            connect.ResultF2.Image = outputBitmap;
        }

        private void RemoveNoiseW(object sender, EventArgs e) //Применение удаления помех к изображению (1 способ)
        {
            saver();
            Bitmap bmp = new Bitmap(pictureBox2.Image); //загрузка исходного изображения
            bool[,] pixels = new bool[bmp.Width, bmp.Height]; //массив для хранения пикселей
            for (int x = 0; x < bmp.Width; x++) //преобразование изображения в массив пикселей
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    pixels[x, y] = (color.R + color.G + color.B) / 3 > 127;
                }
            }

            int[,] objects = new int[bmp.Width, bmp.Height]; //массив для хранения объектов
            int currentObject = 0; //хранение числа объектов
            for (int x = 0; x < bmp.Width; x++) //определение объектов на изображении
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (pixels[x, y] && objects[x, y] == 0) //если пиксель белый и не был помечен как часть объекта, то начинается поиск нового объекта
                    {
                        currentObject++;
                        Queue<Point> q = new Queue<Point>();
                        q.Enqueue(new Point(x, y));
                        while (q.Count > 0)
                        {
                            Point p = q.Dequeue();
                            int px = p.X;
                            int py = p.Y;
                            if (px >= 0 && px < bmp.Width && py >= 0 && py < bmp.Height && pixels[px, py] && objects[px, py] == 0)
                            {
                                objects[px, py] = currentObject;
                                q.Enqueue(new Point(px - 1, py));
                                q.Enqueue(new Point(px + 1, py));
                                q.Enqueue(new Point(px, py - 1));
                                q.Enqueue(new Point(px, py + 1));
                            }
                        }
                    }
                }
            }

            Dictionary<int, int> objectCounts = new Dictionary<int, int>(); //словарь для хранения количества пикселей в каждом объекте
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    int objectNumber = objects[x, y];
                    if (objectNumber > 0)
                    {
                        if (!objectCounts.ContainsKey(objectNumber))
                        {
                            objectCounts.Add(objectNumber, 0);
                        }
                        objectCounts[objectNumber]++;
                    }
                }
            }

            int maxObject = 0;
            int maxObjectCount = 0;
            foreach (KeyValuePair<int, int> kvp in objectCounts) //поиск наибольшего объекта
            {
                if (kvp.Value > maxObjectCount)
                {
                    maxObject = kvp.Key;
                    maxObjectCount = kvp.Value;
                }
            }

            for (int x = 0; x < bmp.Width; x++) //окрашиваем все объекты, кроме самого большого, в черный цвет
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (objects[x, y] != maxObject)
                    {
                        bmp.SetPixel(x, y, Color.Black);
                    }
                }
            }
            pictureBox2.Image = bmp;
            connect.ResultF2.Image = bmp;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void RemoveNoiseB(object sender, EventArgs e) //Применение удаления помех к изображению (2 способ)
        {
            saver();
            Bitmap bmp = new Bitmap(pictureBox2.Image); //загрузка исходного изображения
            bool[,] pixels = new bool[bmp.Width, bmp.Height]; //массив для хранения пикселей
            for (int x = 0; x < bmp.Width; x++) //преобразование изображения в массив пикселей
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    pixels[x, y] = (color.R + color.G + color.B) / 3 < 127;
                }
            }

            int[,] objects = new int[bmp.Width, bmp.Height]; //массив для хранения объектов
            int currentObject = 0;//хранение числа объектов
            for (int x = 0; x < bmp.Width; x++)//определение объектов на изображении
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (pixels[x, y] && objects[x, y] == 0) //если пиксель черный и не был помечен как часть объекта, то начинается поиск нового объекта
                    {
                        currentObject++;
                        Queue<Point> q = new Queue<Point>();
                        q.Enqueue(new Point(x, y));
                        while (q.Count > 0)
                        {
                            Point p = q.Dequeue();
                            int px = p.X;
                            int py = p.Y;
                            if (px >= 0 && px < bmp.Width && py >= 0 && py < bmp.Height && pixels[px, py] && objects[px, py] == 0)
                            {
                                objects[px, py] = currentObject;
                                q.Enqueue(new Point(px - 1, py));
                                q.Enqueue(new Point(px + 1, py));
                                q.Enqueue(new Point(px, py - 1));
                                q.Enqueue(new Point(px, py + 1));
                            }
                        }
                    }
                }
            }

            Dictionary<int, int> objectCounts = new Dictionary<int, int>(); //словарь для хранения количества пикселей в каждом объекте
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    int objectNumber = objects[x, y];
                    if (objectNumber > 0)
                    {
                        if (!objectCounts.ContainsKey(objectNumber))
                        {
                            objectCounts.Add(objectNumber, 0);
                        }
                        objectCounts[objectNumber]++;
                    }
                }
            }

            int maxObject = 0;
            int maxObjectCount = 0;
            foreach (KeyValuePair<int, int> kvp in objectCounts) //поиск наибольшего объекта
            {
                if (kvp.Value > maxObjectCount)
                {
                    maxObject = kvp.Key;
                    maxObjectCount = kvp.Value;
                }
            }

            for (int x = 0; x < bmp.Width; x++) //окрашиваем все объекты, кроме самого большого, в белый цвет
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    if (objects[x, y] != maxObject)
                    {
                        bmp.SetPixel(x, y, Color.White);
                    }
                }
            }
            pictureBox2.Image = bmp;
            connect.ResultF2.Image = bmp;
        }

        private void StepUndo(object sender, EventArgs e) //Откат на шаг назад
        {
            if (stack.Count > 0)
            {
                var a = stack.Pop();
                pictureBox2.Image = a.SavePictureUndo;
            }
            connect.ResultF2.Image = pictureBox2.Image;
        }

        private void Cleaning(object sender, EventArgs e) //Очистка изображения
        {
            Bitmap CleaningImage = new Bitmap(pictureBox1.Image);
            for (int y = 0; y < CleaningImage.Width; y++)
            {
                for (int x = 0; x < CleaningImage.Height; x++)
                {
                    CleaningImage.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }
            pictureBox2.Image = CleaningImage;
            connect.ResultF2.Image = CleaningImage;
        }

        private void ThresholdValue(object sender, EventArgs e) //Настройка порога бинаризации
        {
            label3.Text = trackBar1.Value.ToString();
        }
    }
}
