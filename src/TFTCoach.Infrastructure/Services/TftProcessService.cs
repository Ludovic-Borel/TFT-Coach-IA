using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TFTCoach.Core.Interfaces;

namespace TFTCoach.Infrastructure.Services;

public class TftProcessService : IProcessService
{
    public Process? GetGameProcess()
    {
        foreach (var process in Process.GetProcessesByName("League of Legends"))
        {
            try
            {
                if (process.MainWindowTitle == "League of Legends (TM) Client")
                    return process;
            }
            catch
            {
            }
        }

        return null;
    }

    public bool IsRunning()
    {
        return GetGameProcess() != null;
    }

    public IntPtr GetWindowHandle()
    {
        var process = GetGameProcess();

        return process?.MainWindowHandle ?? IntPtr.Zero;
    }
    [DllImport("user32.dll")]
private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

public bool TryGetWindowRect(out RECT rect)
{
    rect = default;

    var hwnd = GetWindowHandle();

    if (hwnd == IntPtr.Zero)
        return false;

    return GetWindowRect(hwnd, out rect);
}

public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}
}