using System.Drawing.Imaging;

namespace AiHands.Automation;

/// <summary>
/// Holds the result of an image comparison, including pixel-level diff statistics.
/// </summary>
public record DiffResult(bool Match, double DiffPercent, int DiffPixels, int TotalPixels, string? DiffImage);

/// <summary>
/// Performs pixel-level comparison between two images and optionally produces a diff visualization.
/// </summary>
public static class ImageDiff
{
    /// <summary>
    /// Compares two images pixel-by-pixel and returns diff statistics.
    /// </summary>
    /// <param name="threshold">Per-channel color difference (0-255) below which pixels are considered matching.</param>
    /// <param name="outputPath">If provided, saves a diff visualization image highlighting changed pixels.</param>
    public static DiffResult Compare(string path1, string path2, int threshold = 10, string? outputPath = null)
    {
        using var bmp1 = new Bitmap(path1);
        using var bmp2 = new Bitmap(path2);

        int width = Math.Min(bmp1.Width, bmp2.Width);
        int height = Math.Min(bmp1.Height, bmp2.Height);
        int totalPixels = width * height;
        int diffPixels = 0;

        Bitmap? diffBmp = outputPath is not null ? new Bitmap(width, height, PixelFormat.Format32bppArgb) : null;

        try
        {
            // Use LockBits for performance
            var rect = new Rectangle(0, 0, width, height);
            var bd1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bd2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData? bdDiff = diffBmp?.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                for (int y = 0; y < height; y++)
                {
                    byte* row1 = (byte*)bd1.Scan0 + y * bd1.Stride;
                    byte* row2 = (byte*)bd2.Scan0 + y * bd2.Stride;
                    byte* rowDiff = bdDiff is not null ? (byte*)bdDiff.Scan0 + y * bdDiff.Stride : null;

                    for (int x = 0; x < width; x++)
                    {
                        int offset = x * 4;
                        int db = Math.Abs(row1[offset] - row2[offset]);
                        int dg = Math.Abs(row1[offset + 1] - row2[offset + 1]);
                        int dr = Math.Abs(row1[offset + 2] - row2[offset + 2]);

                        bool isDiff = db > threshold || dg > threshold || dr > threshold;
                        if (isDiff) diffPixels++;

                        if (rowDiff is not null)
                        {
                            if (isDiff)
                            {
                                // Red highlight for different pixels
                                rowDiff[offset] = 0;       // B
                                rowDiff[offset + 1] = 0;   // G
                                rowDiff[offset + 2] = 255;  // R
                                rowDiff[offset + 3] = 255;  // A
                            }
                            else
                            {
                                // Dimmed original
                                rowDiff[offset] = (byte)(row1[offset] / 3);
                                rowDiff[offset + 1] = (byte)(row1[offset + 1] / 3);
                                rowDiff[offset + 2] = (byte)(row1[offset + 2] / 3);
                                rowDiff[offset + 3] = 255;
                            }
                        }
                    }
                }
            }

            bmp1.UnlockBits(bd1);
            bmp2.UnlockBits(bd2);
            if (bdDiff is not null) diffBmp!.UnlockBits(bdDiff);

            string? diffImagePath = null;
            if (outputPath is not null && diffBmp is not null)
            {
                diffBmp.Save(outputPath, ImageFormat.Png);
                diffImagePath = Path.GetFullPath(outputPath);
            }

            double diffPercent = totalPixels > 0 ? (double)diffPixels / totalPixels * 100.0 : 0;

            return new DiffResult(
                Match: diffPixels == 0,
                DiffPercent: Math.Round(diffPercent, 2),
                DiffPixels: diffPixels,
                TotalPixels: totalPixels,
                DiffImage: diffImagePath
            );
        }
        finally
        {
            diffBmp?.Dispose();
        }
    }
}
