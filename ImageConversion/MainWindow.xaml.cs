using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageConversion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConvertImage(object sender, RoutedEventArgs e)
        {
            var bitmap = (WriteableBitmap) image1.Source;
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;
            var stride = bitmap.BackBufferStride;

            bitmap.Lock();

            var backBuffer = bitmap.BackBuffer;

            for (var y = 0; y < height; y++)
            //Parallel.For(0, height, y =>
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var pixel = GetPixel(backBuffer, x, y, stride);

                            pixel = pixel.ToSepia();

                            SetPixel(pixel, backBuffer, x, y, stride);
                        }
                    }
            //);

            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));

            bitmap.Unlock();
        }

        public static Color GetPixel(IntPtr backBuffer, int x, int y, int stride)
        {
            var pixelLocation = GetPixelLocation(backBuffer, x, y, stride);

            int pixel;
            unsafe
            {
                pixel = *((int*)pixelLocation);
            }

            var red = (byte)((pixel >> 16) & 0xFF);
            var green = (byte)((pixel >> 8) & 0xFF);
            var blue = (byte)(pixel & 0xFF);

            return Color.FromArgb(0, red, green, blue);
        }

        public static void SetPixel(Color pixel, IntPtr backBuffer, int x, int y, int stride)
        {
            var pixelLocation = GetPixelLocation(backBuffer, x, y, stride);

            var bmpPixel = pixel.R << 16 | pixel.G << 8 | pixel.B;

            unsafe
            {
                *((int*)pixelLocation) = bmpPixel;
            }
        }

        private static IntPtr GetPixelLocation(IntPtr backBuffer, int x, int y, int stride)
        {
            backBuffer += y * stride;
            backBuffer += x * 4;
            return backBuffer;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("pack://application:,,,/ImageConversion;component/parallel_bars.jpg");
            image.EndInit();

            var bitmap = new WriteableBitmap(image);

            image1.Source = bitmap;
        } 
    }
}
