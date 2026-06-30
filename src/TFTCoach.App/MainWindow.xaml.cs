using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using ScreenCapture.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using TFTCoach.App.Geometry;
using TFTCoach.App.Services;
using TFTCoach.Core.Interfaces;
using TFTCoach.Core.Models;

namespace TFTCoach.App;

public sealed partial class MainWindow : Window
{
    private readonly IProcessService _processService;
    private readonly ICaptureService _captureService;

    private bool _isDrawing = false;

    private Windows.Foundation.Point _startPoint;

    private Rectangle? _selectionRectangle;

    private TFTCoach.Core.Models.CaptureRectangle? _selectedZone;
    private TFTCoach.Core.Models.CaptureFrame? _currentFrame;
    private readonly List<CaptureZone> _zones = new();
    private void BtnDeleteZone_Click(object sender, RoutedEventArgs e)
    {
        var selectedType = (CaptureZoneType)ZoneTypeComboBox.SelectedIndex;

        _zones.RemoveAll(z => z.Type == selectedType);

        RefreshZonesList();

        TxtStatus.Text = $"Zones enregistrées : {_zones.Count}";
    }

    private void DrawTestRectangle()
    {
        OverlayCanvas.Children.Clear();

        Rectangle rect = new()
        {
            Width = 220,
            Height = 120,
            Stroke = new SolidColorBrush(Microsoft.UI.Colors.Red),
            StrokeThickness = 3
        };

        Canvas.SetLeft(rect, 150);
        Canvas.SetTop(rect, 100);

        OverlayCanvas.Children.Add(rect);
    }
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
        private void RefreshZonesList()
    {
        ZonesListView.Items.Clear();

        foreach (var zone in _zones.OrderBy(z => z.Type))
        {
            ZonesListView.Items.Add(
                $"{zone.Type} ({zone.Rectangle.Width} x {zone.Rectangle.Height})");
        }
    }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
    {
        _selectedZone = null;
        _currentFrame = null;
        _selectionRectangle = null;

        OverlayCanvas.Children.Clear();

        _zones.Clear();
        RefreshZonesList();
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
                        _currentFrame = frame;
                        if (frame != null)
                        {
                            TxtRect.Text =
                                $"{frame.Width} x {frame.Height} - {frame.Pixels!.Length} octets";

                            if (_selectedZone != null)
                            {
                                var cropped = TFTCoach.Capture.Processing.FrameCropper.Crop(
                                    frame,
                                    _selectedZone);

                                ImgCapture.Source = FrameRenderer.CreateBitmap(cropped);
                            }
                            else
                            {
                                ImgCapture.Source = FrameRenderer.CreateBitmap(frame);
                            }

                           //DrawTestRectangle();
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
    private void OverlayCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_currentFrame == null)
        {
            TxtStatus.Text = "Clique d'abord sur « Tester TFT »";
            return;
        }

        _selectedZone = null;

        _isDrawing = true;

        _startPoint = e.GetCurrentPoint(OverlayCanvas).Position;

        OverlayCanvas.Children.Clear();

        _selectionRectangle = new Rectangle
        {
            Width = 0,
            Height = 0,
            Stroke = new SolidColorBrush(Microsoft.UI.Colors.Lime),
            StrokeThickness = 2
        };

        Canvas.SetLeft(_selectionRectangle, _startPoint.X);
        Canvas.SetTop(_selectionRectangle, _startPoint.Y);

        OverlayCanvas.Children.Add(_selectionRectangle);
    }

    private void OverlayCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || _selectionRectangle == null)
            return;

        var current = e.GetCurrentPoint(OverlayCanvas).Position;

        double x = Math.Min(_startPoint.X, current.X);
        double y = Math.Min(_startPoint.Y, current.Y);

        double width = Math.Abs(current.X - _startPoint.X);
        double height = Math.Abs(current.Y - _startPoint.Y);

        Canvas.SetLeft(_selectionRectangle, x);
        Canvas.SetTop(_selectionRectangle, y);

        _selectionRectangle.Width = width;
        _selectionRectangle.Height = height;
    }

    private void OverlayCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_currentFrame == null)
        {
            TxtStatus.Text = "Clique d'abord sur « Tester TFT »";
            return;
        }

        if (!_isDrawing)
            return;

        _isDrawing = false;

        if (_selectionRectangle == null)
            return;
        if (double.IsNaN(_selectionRectangle.Width) ||
    double.IsNaN(_selectionRectangle.Height) ||
    _selectionRectangle.Width < 5 ||
    _selectionRectangle.Height < 5)
        {
            TxtStatus.Text = $"Rectangle invalide : W={_selectionRectangle.Width} H={_selectionRectangle.Height}";

            OverlayCanvas.Children.Clear();
            _selectionRectangle = null;
            return;
        }
        if (_currentFrame == null)
        {
            TxtStatus.Text = "Clique d'abord sur « Tester TFT » pour capturer une image.";
            return;
        }

        var displayRectangle = new TFTCoach.Core.Models.CaptureRectangle(
    X: (int)Canvas.GetLeft(_selectionRectangle),
    Y: (int)Canvas.GetTop(_selectionRectangle),
    Width: (int)_selectionRectangle.Width,
    Height: (int)_selectionRectangle.Height);

        TxtStatus.Text =
     $"Display : {_selectionRectangle.Width} x {_selectionRectangle.Height}";

        _selectedZone = CoordinateMapper.ToCaptureRectangle(
            displayRectangle,
            displayWidth: (int)ImgCapture.ActualWidth,
            displayHeight: (int)ImgCapture.ActualHeight,
            captureWidth: _currentFrame.Width,
            captureHeight: _currentFrame.Height);

        var selectedType = (CaptureZoneType)ZoneTypeComboBox.SelectedIndex;

        var existing = _zones.FindIndex(z => z.Type == selectedType);

        if (existing >= 0)
        {
            _zones[existing] = new CaptureZone(
                selectedType,
                _selectedZone);
        }
        else
        {
            _zones.Add(new CaptureZone(
                selectedType,
                _selectedZone));
        }

        RefreshZonesList();

        TxtRect.Text = "";

        foreach (var zone in _zones)
        {
            TxtRect.Text +=
                $"{zone.Type} : " +
                $"{zone.Rectangle.X}, " +
                $"{zone.Rectangle.Y}, " +
                $"{zone.Rectangle.Width}, " +
                $"{zone.Rectangle.Height}\n";
        }

        TxtStatus.Text = $"Zones enregistrées : {_zones.Count}";

        TxtRect.Text =
            $"X={_selectedZone.X}  " +
            $"Y={_selectedZone.Y}  " +
            $"W={_selectedZone.Width}  " +
            $"H={_selectedZone.Height}";

        _selectionRectangle = null;
    }
}
