using TFTCoach.Core.Models;

namespace TFTCoach.Core.Interfaces;

public interface IOcrService
{
    Task<string> ReadAsync(
        CaptureFrame frame,
        CaptureRectangle zone);
}