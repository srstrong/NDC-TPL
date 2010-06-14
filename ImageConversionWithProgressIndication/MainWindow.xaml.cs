using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageConversionWithProgressIndication
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

            var imgBuffer = new ImageBuffer(bitmap);

            imgBuffer.RowReady += RowReady;

            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
                                                          imgBuffer.ConvertImageToSepia()));
        }

        private void RowReady(byte[] buffer, Int32Rect dirtyRect, int stride, int offset)
        {
            image1.Dispatcher.Invoke(new Action(
                                         () =>
                                         ((WriteableBitmap) image1.Source).WritePixels(dirtyRect, buffer, stride, offset)), new object[0]);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("pack://application:,,,/ImageConversionWithProgressIndication;component/parallel_bars.jpg");
            image.EndInit();

            var bitmap = new WriteableBitmap(image);
            image1.Source = bitmap;
        }
    }
}
