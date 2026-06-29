using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFTCoach.Core.Models;

public sealed record CaptureZone(
    CaptureZoneType Type,
    CaptureRectangle Rectangle);
