using System.Drawing;

namespace ImageConversion
{
    public static class ExtensionMethods
    {
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
