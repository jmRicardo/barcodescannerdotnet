using System.Drawing;
using ZXing;
using ZXing.Common;

public class BitmapLuminanceSource : BaseLuminanceSource
{
    public BitmapLuminanceSource(Bitmap bitmap)
        : base(bitmap.Width, bitmap.Height)
    {
        var rgbData = new byte[bitmap.Width * bitmap.Height];
        int index = 0;

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                var luminance = (byte)((color.R + color.G + color.B) / 3);
                rgbData[index++] = luminance;
            }
        }

        luminances = rgbData;
    }

    protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
    {
        return new BitmapLuminanceSource(newLuminances, width, height);
    }

    private BitmapLuminanceSource(byte[] luminances, int width, int height)
        : base(width, height)
    {
        this.luminances = luminances;
    }
}