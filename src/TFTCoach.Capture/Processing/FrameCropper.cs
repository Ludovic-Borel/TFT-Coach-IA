using TFTCoach.Core.Models;

namespace TFTCoach.Capture.Processing;

public static class FrameCropper
{
    public static CaptureFrame Crop(
        CaptureFrame source,
        CaptureRectangle zone)
    {
        byte[] pixels = new byte[zone.Width * zone.Height * 4];

        int sourceStride = source.Width * 4;
        int destinationStride = zone.Width * 4;

        for (int y = 0; y < zone.Height; y++)
        {
            Buffer.BlockCopy(
                source.Pixels!,
                ((zone.Y + y) * sourceStride) + (zone.X * 4),
                pixels,
                y * destinationStride,
                destinationStride);
        }

        return new CaptureFrame
        {
            Width = zone.Width,
            Height = zone.Height,
            Pixels = pixels
        };
    }
}