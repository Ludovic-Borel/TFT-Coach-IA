using System;
using System.Collections.Generic;
using System.Text;

namespace TFTCoach.Core.Models;

public sealed class CaptureFrame
{
    public int Width { get; init; }

    public int Height { get; init; }

    public byte[]? Pixels { get; init; }
}
