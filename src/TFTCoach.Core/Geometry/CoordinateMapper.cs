using TFTCoach.Core.Models;

namespace TFTCoach.App.Geometry;

public static class CoordinateMapper
{
    public static CaptureRectangle ToCaptureRectangle(
        CaptureRectangle displayRectangle,
        int displayWidth,
        int displayHeight,
        int captureWidth,
        int captureHeight)
    {
        double scaleX = (double)captureWidth / displayWidth;
        double scaleY = (double)captureHeight / displayHeight;

        return new CaptureRectangle(
            X: (int)Math.Round(displayRectangle.X * scaleX),
            Y: (int)Math.Round(displayRectangle.Y * scaleY),
            Width: (int)Math.Round(displayRectangle.Width * scaleX),
            Height: (int)Math.Round(displayRectangle.Height * scaleY));
    }
}