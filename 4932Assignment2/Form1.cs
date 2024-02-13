using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _4932Assignment2
{
    public partial class Form1 : Form
    {
        private Bitmap loadedBMap;
        public Form1()
        {
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (loadedBMap != null)
            {
                // Draw the image onto the form
                e.Graphics.DrawImage(loadedBMap, Point.Empty);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadedBMap = new Bitmap(openFileDialog.FileName);
                loadedBMap = new Bitmap(loadedBMap, loadedBMap.Width, loadedBMap.Height);
                Refresh();
            }
        }

        private void ConvertToYCbCr(Bitmap bmp)
        {
            float[,] Y = new float[bmp.Width, bmp.Height];
            float[,] Cb = new float[bmp.Width, bmp.Height];
            float[,] Cr = new float[bmp.Width, bmp.Height];

            // Loop through each pixel in the bitmap
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    // Extract RGB values
                    float R = pixel.R;
                    float G = pixel.G;
                    float B = pixel.B;

                    // Convert to YCrCb
                    Y[x, y] = 0.299f * R + 0.587f * G + 0.114f * B;
                    Cb[x, y] = (-0.168736f * R) + (-0.331264f * G) + (-0.5f * B) + 128;
                    Cb[x, y] = (0.5f * R) + (-0.418688f * G) + (-0.081312f * B) + 128;
                    Cr[x, y] = (0.5f * R) + (-0.418688f * G) + (-0.081312f * B) + 128;
                }
            }
        }

        private void ConvertFromYCbCr(Bitmap bmp)
        {
            float[,] Y = new float[bmp.Width, bmp.Height];
            float[,] Cb = new float[bmp.Width / 2, bmp.Height / 2];
            float[,] Cr = new float[bmp.Width / 2, bmp.Height / 2];

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    // Perform necessary operations to convert YCrCb to RGB
                    float Y_val = Y[x, y];
                    float Cb_val = Cb[x / 2, y / 2];
                    float Cr_val = Cr[x / 2, y / 2];

                    // Convert YCrCb to RGB
                    float R = (1f * Y_val) + (0 * Cb_val) + (1.4f * Cr_val);
                    float G = (1f * Y_val) + (-0.343f * Cb_val) + (-0.711f * Cr_val);
                    float B = (1f * Y_val) + (1.765f * Cb_val) + (0 * Cr_val);

                    // Ensure RGB values are within the valid range
                    R = Math.Max(0, Math.Min(255, R));
                    G = Math.Max(0, Math.Min(255, G));
                    B = Math.Max(0, Math.Min(255, B));

                    // Set RGB values in the bitmap
                    bmp.SetPixel(x, y, Color.FromArgb((int)R, (int)G, (int)B));
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
    }
}
