using TFTCoach.Core.Models;

namespace TFTCoach.Core.Interfaces;

public interface ICaptureService
{
    bool Initialize(
    IntPtr windowHandle,
    int left,
    int top,
    int width,
    int height);

    Task<CaptureFrame?> CaptureAsync();
}