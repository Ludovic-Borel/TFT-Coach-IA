using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFTCoach.Core.Models;

public sealed record CaptureRectangle(
    int X,
    int Y,
    int Width,
    int Height);
