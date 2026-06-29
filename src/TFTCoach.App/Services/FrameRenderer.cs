using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Media.Imaging;
using TFTCoach.Core.Models;

namespace TFTCoach.App.Services;

public static class FrameRenderer
{
    public static WriteableBitmap CreateBitmap(CaptureFrame frame)
    {
        WriteableBitmap bitmap =
            new(frame.Width, frame.Height);

        using Stream stream = bitmap.PixelBuffer.AsStream();

        stream.Write(
            frame.Pixels!,
            0,
            frame.Pixels!.Length);

        bitmap.Invalidate();

        return bitmap;
    }
}
