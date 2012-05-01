using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace BooksHouse.Gui.Converters
{
    [ValueConversion(typeof(System.Drawing.Image), typeof(System.Windows.Media.ImageSource))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return null; }

            var image = (System.Drawing.Image)value;
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, image.RawFormat);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
