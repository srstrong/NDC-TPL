using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UIThread
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int _largeImagePixelHeight;
        private readonly int _largeImagePixelWidth;
        private readonly int _largeImageStride;
        private readonly int _colCount;
        private readonly int _rowCount;
        private int _tilePixelHeight;
        private int _tilePixelWidth;
        private int _fileCount;
        private PixelFormat _format;

        public MainWindow()
        {
            InitializeComponent();

            // For this example, values are hard-coded to a mosaic of 8x8 tiles.
            // Each tile is 50 pixels high and 66 pixels wide and 32 bits per pixel.
            _colCount = 12;
            _rowCount = 8;
            _tilePixelHeight = 50;
            _tilePixelWidth = 66;
            _largeImagePixelHeight = _tilePixelHeight * _rowCount;
            _largeImagePixelWidth = _tilePixelWidth * _colCount;
            _largeImageStride = _largeImagePixelWidth * (32 / 8);
            image1.Width = _largeImagePixelWidth;
            image1.Height = _largeImagePixelHeight;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // For best results use 1024 x 768 jpg files at 32bpp.
            string[] files = System.IO.Directory.GetFiles(@"C:\Users\Public\Pictures\Sample Pictures\", "*.jpg");

            _fileCount = files.Length;
            var loadImageTasks = new Task<byte[]>[_fileCount];

            // Spin off a task to load each image
            for (var i = 0; i < _fileCount; i++)
            {
                var x = i;
                loadImageTasks[x] = Task.Factory.StartNew(() => LoadImage(files[x]));
            }

            // When they've all been loaded, tile them into a single byte array.
            var tiledImageTask = Task.Factory.ContinueWhenAll(loadImageTasks, i => TileImages(i));

            // We are currently on the UI thread. Save the sync context and pass it to
            // the next task so that it can access the UI control "image1".
            var uiSyncContext = TaskScheduler.FromCurrentSynchronizationContext();

            // On the UI thread, put the bytes into a bitmap and
            // and display it in the Image control.
            tiledImageTask.ContinueWith(antedecent => LoadTiledImage(antedecent.Result), uiSyncContext);
        }

        void LoadTiledImage(byte[] tiledImage)
        {
            var m = PresentationSource.FromVisual(Application.Current.MainWindow)
                                      .CompositionTarget
                                      .TransformToDevice;
            var dpiX = m.M11;
            var dpiY = m.M22;

            BitmapSource bms = BitmapSource.Create(_largeImagePixelWidth,
                                               _largeImagePixelHeight,
                                               dpiX,
                                               dpiY,
                                               _format,
                                               null,
                                               tiledImage,
                                               _largeImageStride);
            image1.Source = bms;
        }

        // Load the image
        byte[] LoadImage(string filename)
        {
            var myBitmapImage = new BitmapImage();

            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(filename);
            _tilePixelHeight = myBitmapImage.DecodePixelHeight = _tilePixelHeight;
            _tilePixelWidth = myBitmapImage.DecodePixelWidth = _tilePixelWidth;
            myBitmapImage.EndInit();

            _format = myBitmapImage.Format;

            var stride = (int)myBitmapImage.Width * 4;
            var dest = new byte[stride * _tilePixelHeight];

            myBitmapImage.CopyPixels(dest, stride, 0);

            return dest;
        }

        byte[] TileImages(Task<byte[]>[] sourceImages)
        {
            var largeImage = new byte[_largeImagePixelHeight * _largeImageStride];
            var tileImageStride = _tilePixelWidth * 4; // hard coded to 32bpp
            var rand = new Random();

            // Nice use of Parallel.For
            Parallel.For(0, _rowCount * _colCount, i =>
            {
                // Pick one of the images at random for this tile.
                int cur = rand.Next(0, sourceImages.Length);
                byte[] pixels = sourceImages[cur].Result;

                // Get the starting index for this tile.
                int row = i / _colCount;
                int col = i % _colCount;
                int idx = ((row * (_largeImageStride * _tilePixelHeight)) + (col * tileImageStride));

                // Write the pixels for the current tile. The pixels are not contiguous
                // in the array, therefore we have to advance the index by the image stride
                // (minus the stride of the tile) for each scanline of the tile.
                int tileImageIndex = 0;
                for (var j = 0; j < _tilePixelHeight; j++)
                {
                    // Write the next scanline for this tile.
                    for (var k = 0; k < tileImageStride; k++)
                    {
                        largeImage[idx++] = pixels[tileImageIndex++];
                    }
                    // Advance to the beginning of the next scanline.
                    idx += _largeImageStride - tileImageStride;
                }
            });

            return largeImage;
        }
    }
}
