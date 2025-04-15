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
    public partial class SettingsRGB : Form
    {
        public SettingsRGB()
        {
            InitializeComponent();
        }

        public SettingsRGB(MainWindow link)
        {
            InitializeComponent();
            connect = link;
            pictureBox1.Image = link.linkF2.Image;
            Bitmap add_empty = new Bitmap(pictureBox1.Image);
            for (int y = 0; y < add_empty.Width; y++)
            {
                for (int x = 0; x < add_empty.Height; x++)
                {
                    add_empty.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }
            pictureBox8.Image = add_empty;
        }

        private void HistogramRGB(object sender, EventArgs e) //Построение гистограмм RGB
        {
            button7.Enabled = false;
            this.Width = 1832;
            this.Height = 678;
            label6.Visible = true;
            label11.Visible = true;
            label24.Visible = true;
            label9.Visible = true;
            label8.Visible = true;
            label10.Visible = true;
            label12.Visible = true;
            label18.Visible = true;
            label14.Visible = true;
            label19.Visible = true;
            label16.Visible = true;
            label13.Visible = true;
            label21.Visible = true;
            label15.Visible = true;
            label20.Visible = true;
            label17.Visible = true;
            pictureBox5.Visible = true;
            pictureBox6.Visible = true;
            pictureBox7.Visible = true;

            Bitmap GisImage = new Bitmap(pictureBox1.Image);

            //Создание массивов для хранения гистограммных значений
            int[] histogramValuesR = new int[256];
            int[] histogramValuesG = new int[256];
            int[] histogramValuesB = new int[256];

            //Расчет гистограммных значений для каждого канала цвета
            for (int i = 0; i < GisImage.Width; i++)
            {
                for (int j = 0; j < GisImage.Height; j++)
                {
                    Color pixelColor = GisImage.GetPixel(i, j);

                    histogramValuesR[pixelColor.R]++;
                    histogramValuesG[pixelColor.G]++;
                    histogramValuesB[pixelColor.B]++;
                }
            }

            //Поиск максимального значения гистограммы для каждого канала цвета
            int maxHistogramValueR = histogramValuesR.Max();
            int maxHistogramValueG = histogramValuesG.Max();
            int maxHistogramValueB = histogramValuesB.Max();
            label11.Text = Convert.ToString(maxHistogramValueR);
            label12.Text = Convert.ToString(maxHistogramValueG);
            label13.Text = Convert.ToString(maxHistogramValueB);

            //Создание Bitmap-изображений для гистограмм каждого канала цвета RGB
            Bitmap histogramImageR = new Bitmap(512, 256);
            Bitmap histogramImageG = new Bitmap(512, 256);
            Bitmap histogramImageB = new Bitmap(512, 256);

            //Создание объектов для отрисовки гистограмм
            Graphics histogramGraphicsR = Graphics.FromImage(histogramImageR);
            Graphics histogramGraphicsG = Graphics.FromImage(histogramImageG);
            Graphics histogramGraphicsB = Graphics.FromImage(histogramImageB);
            Pen penR = new Pen(Color.Red);
            penR.Width = 2f;
            Pen penG = new Pen(Color.Green);
            penG.Width = 2f;
            Pen penB = new Pen(Color.Blue);
            penB.Width = 2f;
            for (int i = 0; i < 256; i++) //Отрисовка гистограмм для каждого канала цвета RGB
            {
                float xR = i * 2f;
                float yR = (float)(histogramValuesR[i] * (histogramImageR.Height) / (double)maxHistogramValueR);
                histogramGraphicsR.DrawLine(penR, xR, histogramImageR.Height, xR, histogramImageR.Height - yR);
                float xG = i * 2f;
                float yG = (float)(histogramValuesG[i] * (histogramImageG.Height) / (double)maxHistogramValueG);
                histogramGraphicsG.DrawLine(penG, xG, histogramImageG.Height, xG, histogramImageG.Height - yG);
                float xB = i * 2f;
                float yB = (float)(histogramValuesB[i] * (histogramImageB.Height) / (double)maxHistogramValueB);
                histogramGraphicsB.DrawLine(penB, xB, histogramImageB.Height, xB, histogramImageB.Height - yB);
            }

            pictureBox5.Image = histogramImageR;
            pictureBox6.Image = histogramImageG;
            pictureBox7.Image = histogramImageB;
        }

        MainWindow connect;

        struct Save_undo
        {
            public Bitmap SavePictureUndo;
        }

        Stack<Save_undo> stack = new Stack<Save_undo>();

        public void saver()
        {
            var a = new Save_undo();
            if (pictureBox8.Image != null)
                a.SavePictureUndo = new Bitmap(pictureBox8.Image);
            stack.Push(a);
        }

        private void PixelHistogramR(object sender, MouseEventArgs e) //Отрисовка пикселей по нажатию на гистограмму
        {
            Bitmap image = new Bitmap(pictureBox1.Image);
            //Координаты точки, на которую кликнули
            int mouseX = e.X;
            int mouseY = e.Y;

            //Вычисляем соответствующий диапазон значений пикселей по оси X
            int rangeStart = (int)Math.Round((mouseX / 512.0) * 255.0);
            int rangeEnd = (int)Math.Round(((mouseX + 1) / 512.0) * 255.0);

            Bitmap filteredImage = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++) //Отрисовка изображения с отфильтрованными пикселями
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (pixel.R >= rangeStart && pixel.R <= rangeEnd)
                    {
                        filteredImage.SetPixel(x, y, pixel);
                    }
                    else
                    {
                        filteredImage.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }
                }
            }
            pictureBox2.Image = filteredImage;
        }

        private void PixelHistogramG(object sender, MouseEventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.Image);
            //Координаты точки, на которую кликнули
            int mouseX = e.X;
            int mouseY = e.Y;

            //Вычисляем соответствующий диапазон значений пикселей по оси X
            int rangeStart = (int)Math.Round((mouseX / 512.0) * 255.0);
            int rangeEnd = (int)Math.Round(((mouseX + 1) / 512.0) * 255.0);

            Bitmap filteredImage = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++) //Отрисовка изображения с отфильтрованными пикселями
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (pixel.G >= rangeStart && pixel.G <= rangeEnd)
                    {
                        filteredImage.SetPixel(x, y, pixel);
                    }
                    else
                    {
                        filteredImage.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }
                }
            }
            pictureBox3.Image = filteredImage;
        }

        private void PixelHistogramB(object sender, MouseEventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.Image);
            //Координаты точки, на которую кликнули
            int mouseX = e.X;
            int mouseY = e.Y;

            //Вычисляем соответствующий диапазон значений пикселей по оси X
            int rangeStart = (int)Math.Round((mouseX / 512.0) * 255.0);
            int rangeEnd = (int)Math.Round(((mouseX + 1) / 512.0) * 255.0);

            // Создаем новое изображение с отфильтрованными пикселями
            Bitmap filteredImage = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++) //Отрисовка изображения с отфильтрованными пикселями
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (pixel.B >= rangeStart && pixel.B <= rangeEnd)
                    {
                        filteredImage.SetPixel(x, y, pixel);
                    }
                    else
                    {
                        filteredImage.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    }
                }
            }
            pictureBox4.Image = filteredImage;
        }

        private void AddPixelR(object sender, EventArgs e) //Добавление пикселей по каналу R
        {
            try
            {
                if (pictureBox2.Image == null)
                {
                    throw new Exception("Empty Picture!");
                }
                saver();
                Bitmap Result = new Bitmap(pictureBox8.Image);
                Bitmap OriginalPicture = new Bitmap(pictureBox1.Image);
                Bitmap bmp = new Bitmap(pictureBox2.Image);
                for (int y = 0; y < bmp.Width; y++)
                {
                    for (int x = 0; x < bmp.Height; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        Color pixel_d = OriginalPicture.GetPixel(x, y);
                        if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0 && pixel.A == 0)
                        {

                        }
                        else
                        {
                            Result.SetPixel(x, y, pixel_d);
                        }

                    }
                }
                pictureBox8.Image = Result;
                connect.ResultF2.Image = Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void AddPixelG(object sender, EventArgs e) //Добавление пикселей по каналу G
        {
            try
            {
                if (pictureBox3.Image == null)
                {
                    throw new Exception("Empty Picture!");
                }
                saver();
                Bitmap Result = new Bitmap(pictureBox8.Image);
                Bitmap OriginalPicture = new Bitmap(pictureBox1.Image);
                Bitmap bmp = new Bitmap(pictureBox3.Image);
                for (int y = 0; y < bmp.Width; y++)
                {
                    for (int x = 0; x < bmp.Height; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        Color pixel_d = OriginalPicture.GetPixel(x, y);
                        if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0 && pixel.A == 0)
                        {

                        }
                        else
                        {
                            Result.SetPixel(x, y, pixel_d);
                        }

                    }
                }
                pictureBox8.Image = Result;
                connect.ResultF2.Image = Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddPixelB(object sender, EventArgs e) //Добавление пикселей по каналу B
        {
            try
            {
                if (pictureBox4.Image == null)
                {
                    throw new Exception("Empty Picture!");
                }
                saver();
                Bitmap Result = new Bitmap(pictureBox8.Image);
                Bitmap OriginalPicture = new Bitmap(pictureBox1.Image);
                Bitmap bmp = new Bitmap(pictureBox4.Image);
                for (int y = 0; y < bmp.Width; y++)
                {
                    for (int x = 0; x < bmp.Height; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        Color pixel_d = OriginalPicture.GetPixel(x, y);
                        if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0 && pixel.A == 0)
                        {

                        }
                        else
                        {
                            Result.SetPixel(x, y, pixel_d);
                        }

                    }
                }
                pictureBox8.Image = Result;
                connect.ResultF2.Image = Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Cleaning(object sender, EventArgs e) //Очистка изображения
        {
            Bitmap Result = new Bitmap(pictureBox1.Image);
            for (int y = 0; y < Result.Width; y++)
            {
                for (int x = 0; x < Result.Height; x++)
                {
                    Result.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }
            pictureBox8.Image = Result;
            connect.ResultF2.Image = Result;
        }

        private void RangeOfValuesRGB(object sender, EventArgs e) //Формирования диапазона яркости RGB
        {
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = true;
            button10.Enabled = true;
            int num1 = int.Parse(numericUpDown1.Text);
            int num2 = int.Parse(numericUpDown2.Text);
            int min = Math.Min(num1, num2);
            int max = Math.Max(num1, num2);

            List<int> numbers = new List<int>();
            for (int i = min; i <= max; i++)
            {
                numbers.Add(i);
            }

            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Color clr1;

            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    clr1 = bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    foreach (int c in numbers)
                    {
                        if (clr1.R == c)
                        {
                            clr1 = Color.FromArgb(clr1.A, clr1.R, 0, 0);
                            bmp.SetPixel(x, y, clr1);
                        }
                    }
                }
            pictureBox2.Image = bmp;

            bmp = new Bitmap(pictureBox1.Image);
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    clr1 = bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    foreach (int c in numbers)
                    {
                        if (clr1.G == c)
                        {
                            clr1 = Color.FromArgb(clr1.A, 0, clr1.G, 0);
                            bmp.SetPixel(x, y, clr1);
                        }
                    }
                }
            pictureBox3.Image = bmp;

            bmp = new Bitmap(pictureBox1.Image);
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    clr1 = bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    foreach (int c in numbers)
                    {
                        if (clr1.B == c)
                        {
                            clr1 = Color.FromArgb(clr1.A, 0, 0, clr1.B);
                            bmp.SetPixel(x, y, clr1);
                        }
                    }
                }
            pictureBox4.Image = bmp;
        }

        private void StepUndo(object sender, EventArgs e) //Откат на шаг назад
        {
            if (stack.Count > 0)
            {
                var a = stack.Pop();
                pictureBox8.Image = a.SavePictureUndo;
            }
            connect.ResultF2.Image = pictureBox8.Image;
        }

        private void PaintCountR(object sender, PaintEventArgs e) //Надпись Count для гистограммы R
        {
            e.Graphics.Clear(this.BackColor);
            e.Graphics.RotateTransform(-90);
            SizeF textSize = e.Graphics.MeasureString(label24.Text, label24.Font);
            label24.Width = (int)textSize.Height + 2;
            label24.Height = (int)textSize.Width + 2;
            e.Graphics.TranslateTransform(-label24.Height / 2, label24.Width / 2);
            e.Graphics.DrawString(label24.Text, label24.Font, Brushes.Black, -(textSize.Width / 2), -(textSize.Height / 2));
        }

        private void PaintCountG(object sender, PaintEventArgs e) //Надпись Count для гистограммы G
        {
            e.Graphics.Clear(this.BackColor);
            e.Graphics.RotateTransform(-90);
            SizeF textSize = e.Graphics.MeasureString(label18.Text, label18.Font);
            label18.Width = (int)textSize.Height + 2;
            label18.Height = (int)textSize.Width + 2;
            e.Graphics.TranslateTransform(-label18.Height / 2, label18.Width / 2);
            e.Graphics.DrawString(label18.Text, label18.Font, Brushes.Black, -(textSize.Width / 2), -(textSize.Height / 2));

        }

        private void PaintCountB(object sender, PaintEventArgs e) //Надпись Count для гистограммы B
        {
            e.Graphics.Clear(this.BackColor);
            e.Graphics.RotateTransform(-90);
            SizeF textSize = e.Graphics.MeasureString(label21.Text, label21.Font);
            label21.Width = (int)textSize.Height + 2;
            label21.Height = (int)textSize.Width + 2;
            e.Graphics.TranslateTransform(-label21.Height / 2, label21.Width / 2);
            e.Graphics.DrawString(label21.Text, label21.Font, Brushes.Black, -(textSize.Width / 2), -(textSize.Height / 2));
        }

        private void DeletePixelR(object sender, EventArgs e) //Удаление пикселей по каналу R
        {
            try
            {
                if (pictureBox2.Image == null)
                {
                    throw new Exception("Empty Picture!");
                }
                saver();
                Bitmap Result = new Bitmap(pictureBox8.Image);
                Bitmap bmp = new Bitmap(pictureBox2.Image);
                for (int y = 0; y < bmp.Width; y++)
                {
                    for (int x = 0; x < bmp.Height; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0 && pixel.A == 0)
                        {

                        }
                        else
                        {
                            Result.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                        }

                    }
                }
                pictureBox8.Image = Result;
                connect.ResultF2.Image = Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeletePixelG(object sender, EventArgs e) //Удаление пикселей по каналу G
        {
            try
            {
                if (pictureBox3.Image == null)
                {
                    throw new Exception("Empty Picture!");
                }
                saver();
                Bitmap Result = new Bitmap(pictureBox8.Image);
                Bitmap bmp = new Bitmap(pictureBox3.Image);
                for (int y = 0; y < bmp.Width; y++)
                {
                    for (int x = 0; x < bmp.Height; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0 && pixel.A == 0)
                        {

                        }
                        else
                        {
                            Result.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                        }

                    }
                }
                pictureBox8.Image = Result;
                connect.ResultF2.Image = Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeletePixelB(object sender, EventArgs e) //Удаление пикселей по каналу B
        {
            try
            {
                if (pictureBox4.Image == null)
                {
                    throw new Exception("Empty Picture!");
                }
                saver();
                Bitmap Result = new Bitmap(pictureBox8.Image);
                Bitmap bmp = new Bitmap(pictureBox4.Image);
                for (int y = 0; y < bmp.Width; y++)
                {
                    for (int x = 0; x < bmp.Height; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0 && pixel.A == 0)
                        {

                        }
                        else
                        {
                            Result.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                        }

                    }
                }
                pictureBox8.Image = Result;
                connect.ResultF2.Image = Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
