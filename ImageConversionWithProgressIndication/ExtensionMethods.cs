using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageConversionWithProgressIndication
{
    public static class ExtensionMethods
    {
        public static Color GetPixel(this byte[] buffer, int x, int y, int stride)
        {
            var offset = buffer.GetOffset(x, y, stride);

            var red = buffer[offset + 2];
            var green = buffer[offset + 1];
            var blue = buffer[offset];

            return Color.FromArgb(0, red, green, blue);
        }

        public static void SetPixel(this byte[]buffer, int x, int y, int stride, Color pixel)
        {
            var offset = buffer.GetOffset(x, y, stride);

            buffer[offset + 2] = pixel.R;
            buffer[offset + 1] = pixel.G;
            buffer[offset] = pixel.B;
        }

        public static int GetOffset(this byte[] buffer, int x, int y, int stride)
        {
            return y*stride + x*4;
        }

        public static Color ToSepia(this Color original)
        {
            var newRed = (int)((original.R * .393) + (original.G * .769) + (original.B * .189));
            var newGreen = (int)((original.R * .349) + (original.G * .686) + (original.B * .168));
            var newBlue = (int)((original.R * .272) + (original.G * .534) + (original.B * .131));

            if (newRed > 255) newRed = 255;
            if (newGreen > 255) newGreen = 255;
            if (newBlue > 255) newBlue = 255;

            return Color.FromArgb(0, (byte) newRed, (byte) newGreen, (byte) newBlue);
        }
    }
}
