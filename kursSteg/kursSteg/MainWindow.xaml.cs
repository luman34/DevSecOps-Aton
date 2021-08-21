using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.IO;
using System.Windows.Input;


namespace kursSteg
{
  
    using Image = System.Drawing.Image;
    using Color = System.Drawing.Color;

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PuthImgBut_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            GetFileName(dlg, ImgTB, true);
        }

        private void PuthQrBut_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            GetFileName(dlg, QrTB, true);
        }

        private void PathStegoContBut_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            GetFileName(dlg, StegoContTB, true);
        }

        private void PathExtrBut_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            GetFileName(dlg, ExtrTB, true);
        }

        private void HideBut_Click(object sender, RoutedEventArgs e)
        {
            string PathImg = ImgTB.Text;
            string PathQr = QrTB.Text;
            string PathStegoCont = StegoContTB.Text;

            this.Cursor = Cursors.Wait;

            //Image QrBmp = Image.FromFile(PathQr);
            byte[] hiddenBytes = Util.BitmapToByteArray(Image.FromFile(PathQr));
            Encode(hiddenBytes, PathImg, PathStegoCont);

            this.Cursor = Cursors.Arrow;

        }

        private void ExtrBut_Click(object sender, RoutedEventArgs e)
        {
            string PathStegoCont = StegoContTB.Text;
            string PathExtr = ExtrTB.Text;

            this.Cursor = Cursors.Wait;

            byte[] loadedHiddenBytes = Decode(PathStegoCont);
            Util.ByteArrayToBitmap(loadedHiddenBytes).Save(PathExtr, ImageFormat.Bmp);

            this.Cursor = Cursors.Arrow;

        }


        public static void Encode(byte[] hiddenBytes, string inputImageFileName, string outputImageFileName)
        {
            // Loading the data we want to hide to a byte array
            byte[] hiddenLengthBytes = BitConverter.GetBytes(hiddenBytes.Length);
            byte[] hiddenCombinedBytes = Util.Combine(hiddenLengthBytes, hiddenBytes);

            // Loading an innocuous image we want to store the hidden data in to a byte array
            Image ImgBmp = Image.FromFile(inputImageFileName);
            byte[] rgbComponents = Util.RgbComponentsToBytes(ImgBmp);

            // Encoding the hidden data into the innocuous image, and storing it to file.
            byte[] encodedRgbComponents = EncodeBytes(hiddenCombinedBytes, rgbComponents);
            Bitmap encodedBmp = Util.ByteArrayToBitmap(encodedRgbComponents, ImgBmp.Width, ImgBmp.Height);
            encodedBmp.Save(outputImageFileName, ImageFormat.Bmp);
        }

        private static byte[] EncodeBytes(byte[] hiddenBytes, byte[] ImgBytes)
        {
            BitArray hiddenBits = new BitArray(hiddenBytes);
            byte[] encodedBitmapRgbComponents = new byte[ImgBytes.Length];
            for (int i = 0; i < ImgBytes.Length; i++)
            {
                if (i < hiddenBits.Length)
                {
                    byte evenByte = (byte)(ImgBytes[i] - ImgBytes[i] % 2);
                    encodedBitmapRgbComponents[i] = (byte)(evenByte + (hiddenBits[i] ? 1 : 0));
                }
                else
                {
                    encodedBitmapRgbComponents[i] = ImgBytes[i];
                }
            }
            return encodedBitmapRgbComponents;
        }

        public static byte[] Decode(string imageFileName)
        {
            // Loading the seemingly innocuous image with hidden data into a byte array
            Bitmap loadedEncodedBmp = new Bitmap(imageFileName);
            byte[] loadedEncodedRgbComponents = Util.RgbComponentsToBytes(loadedEncodedBmp);

            const int bytesInInt = 4;
            byte[] loadedHiddenLengthBytes = DecodeBytes(loadedEncodedRgbComponents, 0, bytesInInt);
            int loadedHiddenLength = BitConverter.ToInt32(loadedHiddenLengthBytes, 0);
            byte[] loadedHiddenBytes = DecodeBytes(loadedEncodedRgbComponents, bytesInInt, loadedHiddenLength);
            return loadedHiddenBytes;
        }

        private static byte[] DecodeBytes(byte[] ImgLookingData, int byteIndex, int byteCount)
        {
            const int bitsInBytes = 8;
            int bitCount = byteCount * bitsInBytes;
            int bitIndex = byteIndex * bitsInBytes;
            bool[] loadedHiddenBools = new bool[bitCount];
            for (int i = 0; i < bitCount; i++)
            {
                loadedHiddenBools[i] = ImgLookingData[i + bitIndex] % 2 == 1;
            }
            BitArray loadedHiddenBits = new BitArray(loadedHiddenBools);
            byte[] loadedHiddenBytes = new byte[loadedHiddenBits.Length / bitsInBytes];
            loadedHiddenBits.CopyTo(loadedHiddenBytes, 0);
            return loadedHiddenBytes;
        }


        private void GetFileName(FileDialog dialog, TextBox control, bool useFilter)
        {
            if (useFilter)
            {
                dialog.Filter = "Bmp Photo (*.bmp)|*.bmp";
            }
            if (dialog.ShowDialog(this) == true)
            {
                control.Text = dialog.FileName;
            }
        }

        class Util
        {
            public static byte[] BitmapToByteArray(Image img)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    img.Save(ms, ImageFormat.Bmp);
                    return ms.ToArray();
                }
            }

            public static Image ByteArrayToBitmap(byte[] bytes)
            {
                //using (MemoryStream ms = new MemoryStream(bytes))
                //{
                //    return Image.FromStream(ms);
                //}

                ImageConverter ic = new ImageConverter();
                return (Image)ic.ConvertFrom(bytes);


            }

            public static byte[] Combine(byte[] left, byte[] right)
            {
                byte[] combined = new byte[left.Length + right.Length];
                Buffer.BlockCopy(left, 0, combined, 0, left.Length);
                Buffer.BlockCopy(right, 0, combined, left.Length, right.Length);
                return combined;
            }

            public static byte[] RgbComponentsToBytes(Image innocuousImg)
            {
                Bitmap innocuousBmp = new Bitmap(innocuousImg);
                int counter = 0;
                byte[] components = new byte[3 * innocuousBmp.Width * innocuousBmp.Height];
                for (int y = 0; y < innocuousBmp.Height; y++)
                {
                    for (int x = 0; x < innocuousBmp.Width; x++)
                    {
                        Color c = innocuousBmp.GetPixel(x, y);
                        components[counter++] = c.R;
                        components[counter++] = c.G;
                        components[counter++] = c.B;
                    }
                }
                return components;
            }

            public static Bitmap ByteArrayToBitmap(byte[] rgbComponents, int width, int hight)
            {
                Queue<byte> rgbComponentQueue = new Queue<byte>(rgbComponents);
                Bitmap bitmap = new Bitmap(width, hight);
                for (int y = 0; y < hight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        bitmap.SetPixel(x, y, Color.FromArgb(rgbComponentQueue.Dequeue(), rgbComponentQueue.Dequeue(), rgbComponentQueue.Dequeue()));
                    }
                }
                return bitmap;
            }

        }

    }
}
