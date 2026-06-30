using TFTCoach.Core.Interfaces;
using TFTCoach.Core.Models;
using ScreenCapture.NET;
using HPPH;
using System.Linq;

namespace TFTCoach.Capture.Services;

public sealed class CaptureService : ICaptureService
{
    private readonly DX11ScreenCaptureService _service;

    private DX11ScreenCapture? _capture;
    private CaptureZone<ColorBGRA>? _zone;

    public CaptureService()
    {
        _service = new DX11ScreenCaptureService();
    }

    public bool Initialize(
    IntPtr windowHandle,
    int left,
    int top,
    int width,
    int height)


    {
        if (windowHandle == IntPtr.Zero)
            return false;
        

        var card = _service.GetGraphicsCards().First();

        var display = _service
            .GetDisplays(card)
            .First();

        _capture = _service.GetScreenCapture(display);

        _zone = _capture.RegisterCaptureZone(
            left,
            top,
            width,
            height);

        

        return true;
    }

    public Task<CaptureFrame?> CaptureAsync()
    {
        if (_capture == null || _zone == null)
            return Task.FromResult<CaptureFrame?>(null);

        var ok = _capture.CaptureScreen();

        if (!ok)
            return Task.FromResult<CaptureFrame?>(null);

        using (_zone.Lock())
        {
            return Task.FromResult<CaptureFrame?>(
                new CaptureFrame
                {
                    Width = _zone.Width,
                    Height = _zone.Height,
                    Pixels = _zone.RawBuffer.ToArray()
                });
        }
    }
}