using Microsoft.UI.Xaml;
using ScreenCapture.NET;
using System;
using System.Linq;
using TFTCoach.Core.Interfaces;
using TFTCoach.App.Services;

namespace TFTCoach.App;

public sealed partial class MainWindow : Window
{
    private readonly IProcessService _processService;
    private readonly ICaptureService _captureService;

    public MainWindow()
    {
        InitializeComponent();
        var captureService = new DX11ScreenCaptureService();

        var cards = captureService.GetGraphicsCards().ToList();
        var displays = captureService.GetDisplays(cards[0]).ToList();
        var capture = captureService.GetScreenCapture(displays[0]);

        TxtStatus.Text = $"Cartes : {cards.Count}";
        TxtStatus.Text += " - Capture OK";

        TxtRect.Text = $"Écrans : {displays.Count}";


        _processService = new Infrastructure.Services.TftProcessService();
        _captureService = new Capture.Services.CaptureService();
    }

    private async void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        if (_processService.IsRunning())
        {
            TxtStatus.Text = "TFT détecté";

            var process = (Infrastructure.Services.TftProcessService)_processService;

            if (process.TryGetWindowRect(out var rect))
            {
                TxtRect.Text =
                    $"X={rect.Left}  Y={rect.Top}  W={rect.Right - rect.Left}  H={rect.Bottom - rect.Top}";

                try
                {
                    var initialized = _captureService.Initialize(
                        process.GetWindowHandle(),
                        rect.Left,
                        rect.Top,
                        rect.Right - rect.Left,
                        rect.Bottom - rect.Top);

                    if (initialized)
                    {
                        var frame = await _captureService.CaptureAsync();

                        if (frame != null)
                        {
                            TxtRect.Text =
                                $"{frame.Width} x {frame.Height} - {frame.Pixels!.Length} octets";

                            ImgCapture.Source = FrameRenderer.CreateBitmap(frame);
                        }
                    }

                    TxtStatus.Text += initialized
                        ? " - Capture prête"
                        : " - Capture non prête";
                }
                catch (Exception ex)
                {
                    TxtStatus.Text = ex.GetType().Name;
                    TxtRect.Text = ex.Message;
                }
            }
        }
        else
        {
            TxtStatus.Text = "TFT non détecté";
            TxtRect.Text = "";
        }
    }
}
