/*Code Source Taken Here: https://stackoverflow.com/a/73835879/1862452*/

using System;
using System.Drawing.Imaging;
using System.Drawing;

namespace ToF_Fishing_Bot
{
    public static class BitmapExtension
    {
        unsafe public static Bitmap Crop(this Bitmap bitmap, int left, int top, int width, int height)
        {
            Bitmap cropped = new Bitmap(width, height);
            BitmapData originalData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            BitmapData croppedData = cropped.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            int* srcPixel = (int*)originalData.Scan0 + (left + originalData.Width * top);
            int nextLine = originalData.Width - width;

            for (int y = 0, i = 0; y < height; y++, srcPixel += nextLine)
            {
                for (int x = 0; x < width; x++, i++, srcPixel++)
                {
                    *((int*)croppedData.Scan0 + i) = *srcPixel;
                }
            }

            bitmap.UnlockBits(originalData);
            cropped.UnlockBits(croppedData);

            return cropped;
        }

        unsafe public static Bitmap CropSmall(this Bitmap bitmap, int left, int top, int width, int height)
        {
            Bitmap cropped = new Bitmap(width, height);
            BitmapData originalData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            BitmapData croppedData = cropped.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            Span<int> srcPixels = new Span<int>((void*)originalData.Scan0, originalData.Width * originalData.Height);

            int nextLine = originalData.Width - width;

            for (int y = 0, i = 0, s = left + originalData.Width * top; y < height; y++, s += nextLine)
            {
                for (int x = 0; x < width; x++, i++, s++)
                {
                    *((int*)croppedData.Scan0 + i) = srcPixels[s];
                }
            }

            bitmap.UnlockBits(originalData);
            cropped.UnlockBits(croppedData);

            return cropped;
        }
    }
}
