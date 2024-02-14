using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace _4932Assignment2
{
    public partial class Form1 : Form
    {
        private Bitmap loadedBMap;
        private Bitmap resizedMap;
        public Form1()
        {
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (resizedMap != null)
            {
                // Draw the image onto the form
                e.Graphics.DrawImage(resizedMap, Point.Empty);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadedBMap = new Bitmap(openFileDialog.FileName);
                resizedMap = new Bitmap(loadedBMap, this.ClientSize.Width, this.ClientSize.Height);
                Refresh();
            }
        }

        private byte[] ConvertToYCbCr(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            byte[] finalData= new byte[(int)(width* height * 1.5F + 4)];
            float[,] Y = new float[width, height];
            float[,] Cb = new float[width, height];
            float[,] Cr = new float[width, height];

            // Loop through each pixel in the bitmap
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    // Extract RGB values
                    float R = pixel.R;
                    float G = pixel.G;
                    float B = pixel.B;

                    // Convert to YCrCb
                    Y[x, y] = 0.299f * R + 0.587f * G + 0.114f * B;
                    Cb[x, y] = (-0.168736f * R) + (-0.331264f * G) + (-0.5f * B) + 128;
                    Cr[x, y] = (0.5f * R) + (-0.418688f * G) + (-0.081312f * B) + 128;
                }
            }
            Cb = Subsample(Cb);
            Cr = Subsample(Cr);
            int i = 0;
            finalData[i++] = (byte)(width >> 8);
            finalData[i++] = (byte)(width & 0xFF);   
            finalData[i++] = (byte)(height >> 8);    
            finalData[i++] = (byte)(height & 0xFF);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    finalData[i++] = (byte)(Y[x, y]);
                }
            }
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width / 2; x++)
                {
                    finalData[i++] = (byte)(Cb[x, y]);
                }
            }
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width / 2; x++)
                {
                    finalData[i++] = (byte)(Cr[x, y]);
                }
            }
            return finalData;
        }
        private float[,] Subsample(float[,] data)
        {
            int height = data.GetLength(1) / 2;
            int width = data.GetLength(0) / 2;
            float[,] result = new float[height, width];

            for (int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    result[x,y] = data[x * 2, y * 2];
                }
            }
            return result;
        }

        private void ConvertFromYCbCr(byte[] fileData)
        {
            int width = fileData[0] << 8 | fileData[1];  
            int height = fileData[2] << 8 | fileData[3];  
            float[,] Y = new float[width, height];
            float[,] Cb = new float[width, height];
            float[,] Cr = new float[width, height];

            int i = 4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Y[x, y] = fileData[i++];
                }
            }
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width / 2; x++)
                {
                    Cb[x, y] = fileData[i++];
                }
            }
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width / 2; x++)
                {
                    Cr[x, y] = fileData[i++];
                }
            }

            Cb = Upsample(Cb);
            Cr = Upsample(Cr);

            Bitmap rgb = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Perform necessary operations to convert YCrCb to RGB
                    float Y_val = Y[x, y];
                    float Cb_val = Cb[x , y] - 128;
                    float Cr_val = Cr[x, y] - 128;

                    // Convert YCrCb to RGB
                    float R = 1.0f * Y_val +0.0f * Cb_val + 1.4f * Cr_val;
                    float G = 1.0f * Y_val + -0.343f * Cb_val + -0.711f * Cr_val;
                    float B = 1.0f * Y_val + 1.765f * Cb_val + 0.0f * Cr_val;


                    // Ensure RGB values are within the valid range
                    R = Math.Max(0, Math.Min(255, R));
                    G = Math.Max(0, Math.Min(255, G));
                    B = Math.Max(0, Math.Min(255, B));

                    // Set RGB values in the bitmap
                    rgb.SetPixel(x, y, Color.FromArgb((int)R, (int)G, (int)B));

                    Console.WriteLine(R);
                    Console.WriteLine(G);
                    Console.WriteLine(B);
                }
            }
            SavePicture(rgb);
        }

        private float[,] Upsample(float[,] data)
        {
            int width = data.GetLength(0) * 2;
            int height = data.GetLength(1) * 2;
            float[,] result = new float[width, height];

            int cy = 0;
            for (int y = 0; y < height / 2; y++)
            {
                int cx = 0;
                for (int x = 0; x < width / 2; x++)
                {
                    result[cx, cy] = data[x, y];
                    result[cx + 1, cy] = data[x, y];
                    result[cx, cy + 1] = data[x, y];
                    result[cx + 1, cy + 1] = data[x, y];
                    cx += 2;
                }
                cy += 2;
            }

            return result;
        }

        private void WriteToFile(byte[] array)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "YCrCb|*.custom|All files|*.*";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string savePath = saveFileDialog.FileName;
                    File.WriteAllBytes(savePath, array);
                }
            }
        }

        private void SavePicture(Bitmap picture)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "bmp|*.bmp|All files|*.*";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string savePath = saveFileDialog.FileName;
                    picture.Save(savePath);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void toYCbCrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] fileData = ConvertToYCbCr(loadedBMap);
            WriteToFile(fileData);
        }

        private void toRGBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                byte[] finalData = File.ReadAllBytes(openFileDialog.FileName);
                ConvertFromYCbCr(finalData);
            }
        }
    }
}
