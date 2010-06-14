using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageConversionWithProgressIndication
{
    class ImageBuffer
    {
        private readonly byte[] _buffer;
        private readonly int _height;
        private readonly int _width;
        private readonly int _stride;

        public event Action<byte[], Int32Rect, int, int> RowReady;

        public ImageBuffer(BitmapSource bitmap)
        {
            _height = bitmap.PixelHeight;
            _width = bitmap.PixelWidth;
            _stride = _width*4;
            _buffer = new byte[_height * _stride];
            bitmap.CopyPixels(Int32Rect.Empty, _buffer, _stride, 0);
        }

        public void ConvertImageToSepia()
        {
            for (var y = 0; y < _height; y++)
            //Parallel.For(0, _height, y =>
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var pixel = _buffer.GetPixel(x, y, _stride);

                        pixel = pixel.ToSepia();

                        _buffer.SetPixel(x, y, _stride, pixel);
                    }

                    RowReady(_buffer, new Int32Rect(0, y, _width, 1), _stride,
                            _buffer.GetOffset(0, y, _stride));
                }
            //);
        }
    }
}