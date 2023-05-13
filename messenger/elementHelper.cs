using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace messenger
{
    public static class ElementHelper
    {
        public static byte[] GetImageBytesFromPictureBox(PictureBox pictureBox)
        {
            if (pictureBox.Image == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                string format = pictureBox.Image.RawFormat.ToString();
                ImageFormat imageFormat = ImageFormat.Jpeg;

                if (format.Equals("png", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Png;
                }
                else if (format.Equals("jpg", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Jpeg;
                }
                else if (format.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Jpeg;
                }

                pictureBox.Image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }
        public static void OpenPhotoAndAddPhotoToPictureBox(PictureBox pictureBox)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox.Image = Image.FromFile(openFileDialog.FileName);
            }
        }
        public static void SetImageFromBytes(byte[] imageBytes, PictureBox pictureBox)
        {
            using (MemoryStream memoryStream = new MemoryStream(imageBytes))
            {
                Image image = Image.FromStream(memoryStream);
                pictureBox.Image = image;
            }
        }
    }
}