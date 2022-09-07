using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretImages
{
    public partial class Form_main : Form
    {
        public Form_main()
        {
            InitializeComponent();

            userInputEvent += OnUserInput;
            startedEncoding += OnStartedEncoding;
            finishedEncoding += OnFinishedEncoding;

            button_encode.Enabled = false;
            button_encode_saveImage.Enabled = false;
        }

        delegate void emptyDel();
        event emptyDel userInputEvent;
        event emptyDel startedEncoding;
        event emptyDel finishedEncoding;

        Bitmap imageEncode;
        Bitmap imageDecode;

        int maxCharAmount = 0;

        ProgressBar p;

        Bitmap LoadImage()
        {
            Bitmap newImage;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image";
            ofd.RestoreDirectory = true;
            ofd.Multiselect = false;
            ofd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                newImage = new Bitmap(ofd.FileName);
            }
            else
            {
                return null;
            }

            return newImage;
        }

        void SaveImage(Image image)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Title = "Save Image";
            sfd.RestoreDirectory = true;
            sfd.Filter = "PNG|*.png|JPEG|*jpg|Bitmap|*.bmp";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ImageFormat format = ImageFormat.Png;
                string ext = System.IO.Path.GetExtension(sfd.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".png":
                        format = ImageFormat.Png;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                }

                image.Save(sfd.FileName, format);
            }
        }

        char[] GetTextInArray()
        {
            string plainText = textBox_enocode.Text;
            byte[] byteArray = System.Text.UTF8Encoding.ASCII.GetBytes(plainText);
            string binaryText = string.Join("", byteArray.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0'))); //https://stackoverflow.com/questions/5664345/string-to-binary-in-c-sharp
            return binaryText.ToCharArray();
        }

        void OnUserInput()
        {
            if (imageEncode != null && textBox_enocode.Text.Length > 0)
            {
                button_encode.Enabled = true;
            } else
            {
                button_encode.Enabled = false;
            }
        }

        void OnStartedEncoding()
        {
            p = new ProgressBar();
            p.Location = new Point(this.Height - 20, 20);
            p.Size = new Size(100, 20);
            p.MarqueeAnimationSpeed = 30;
            p.Style = ProgressBarStyle.Marquee;
            tabPage_encode.Controls.Add(p);
        }

        void OnFinishedEncoding()
        {
            button_encode_saveImage.Enabled = true;
        }

        private void button_encode_loadImage_Click(object sender, EventArgs e)
        {
            imageEncode = LoadImage();
            if (imageEncode != null)
            {
                pictureBox_enocode.Image = imageEncode;
                pictureBox_enocode.Update();
                maxCharAmount = imageEncode.Width * imageEncode.Height / 8;
                Console.WriteLine("Max Char Amount" + maxCharAmount);
                userInputEvent?.Invoke();
            }
        }

        private void button_encode_saveImage_Click(object sender, EventArgs e)
        {
            if (pictureBox_enocode.Image != null)
            {
                SaveImage(pictureBox_enocode.Image);
            }
        }

        private void textBox_enocode_TextChanged(object sender, EventArgs e)
        {
            userInputEvent?.Invoke();

            textBox_enocode.MaxLength = maxCharAmount;
        }

        private void button_encode_Click(object sender, EventArgs e)
        {
            startedEncoding?.Invoke();

            char[] array = GetTextInArray();

            Bitmap workingImage = imageEncode;
            Bitmap encodedImage = imageEncode;
            imageEncode = null;

            for (int y = 0; y < workingImage.Height; y++)
            {
                for (int x = 0; x < workingImage.Width; x++)
                {
                    if (workingImage.Width * y + x < array.Length)
                    {
                        Color c = workingImage.GetPixel(x, y);
                        int r = c.R;
                        int g = c.G;
                        int b = c.B;

                        //Console.WriteLine("{0} {1} {2}", Convert.ToString(r, 2), Convert.ToString(g, 2), Convert.ToString(b, 2));
                        //Console.WriteLine(Convert.ToString(b, 2) + " " + array[workingImage.Width * y + x]);

                        // SET BIT
                        if (array[workingImage.Width * y + x] == '0')
                        {
                            b &= ~(1 << 0);
                        }
                        else
                        {
                            b |= 1 << 0;
                        }

                        //Console.WriteLine(Convert.ToString(b, 2));
                        //Console.WriteLine();
                        encodedImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
            }

            imageEncode = encodedImage;
            pictureBox_enocode.Image = imageEncode;
            pictureBox_enocode.Update();
            finishedEncoding?.Invoke();
        }

        private void button_decode_Click(object sender, EventArgs e)
        {
            Bitmap workingImage = imageDecode;

            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < workingImage.Height; y++)
            {
                for (int x = 0; x < workingImage.Width; x++)
                {
                    Color c = workingImage.GetPixel(x, y);
                    int r = c.R;
                    int g = c.G;
                    int b = c.B;

                    sb.Append(Convert.ToString(b, 2).ToArray().Last());
                }
            }

            // cut string every 8 places, to byte array

            List<byte> data = new List<byte>();
            string fullString = sb.ToString();

            for (int i = 0; i < sb.Length; i += 8)
            {
                data.Add(Convert.ToByte(Convert.ToInt32(fullString.Substring(i, 8), 2)));
            }

            textBox_decode.Text = Encoding.ASCII.GetString(data.ToArray());
        }

        private void button_deocode_loadImage_Click(object sender, EventArgs e)
        {
            imageDecode = LoadImage();
            if (imageDecode != null)
            {
                pictureBox_decode.Image = imageDecode;
                pictureBox_decode.Update();
            }
        }
    }
}
