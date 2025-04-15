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
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
            Bitmap add_empty = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            for (int y = 0; y < add_empty.Width; y++)
            {
                for (int x = 0; x < add_empty.Height; x++)
                {
                    add_empty.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }
            pictureBox1.Image = add_empty;
            pictureBox2.Image = add_empty;
        }

        public PictureBox linkF2;
        public PictureBox ResultF2;

        private void ImageUpload(object sender, EventArgs e) //Загрузка входного изображения
        {
            try
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Title = "Open";
                openFile.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
                if (openFile.ShowDialog() == DialogResult.Cancel) return;
                Bitmap OpenImage = new Bitmap(openFile.FileName);
                pictureBox1.Image = OpenImage;
                pictureBox2.Image = OpenImage;
            }

            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
        }

        private void StripMenuRGB(object sender, EventArgs e) //Строка меню RGB
        {
            linkF2 = pictureBox1;
            ResultF2 = pictureBox2;
            SettingsRGB newForm = new SettingsRGB(this);
            newForm.Show();
        }

        private void StripMenuSegmentation(object sender, EventArgs e) //Строка меню сегментации
        {
            linkF2 = pictureBox1;
            ResultF2 = pictureBox2;
            SettingsSegmentation newForm = new SettingsSegmentation(this);
            newForm.Show();
        }

        private void ImageSave(object sender, EventArgs e) //Сохранение результата
        {
            if (pictureBox2.Image != null)
            {
                SaveFileDialog SaveImage = new SaveFileDialog();
                SaveImage.Title = "Save as";
                SaveImage.OverwritePrompt = true;
                SaveImage.CheckPathExists = true;
                SaveImage.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.PNG)|*.PNG|All files(*.*)|*.*";
                SaveImage.ShowHelp = true;
                if (SaveImage.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBox2.Image.Save(SaveImage.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("unable to save image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
