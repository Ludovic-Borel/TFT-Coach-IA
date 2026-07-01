using System.Text.Json;
using TFTCoach.Core.Models;
using System.Text.Json.Serialization;

namespace TFTCoach.Infrastructure.Services;

public sealed class ZoneConfigurationService
{
    private static readonly string FileName =
    Path.Combine(
        AppContext.BaseDirectory,
        "zones.json");

    public void Save(List<CaptureZone> zones)
    {
        var config = new CaptureZonesConfiguration
        {
            Zones = zones
        };

        var json = JsonSerializer.Serialize(
    config,
    new JsonSerializerOptions
    {
        WriteIndented = true
    });

        Directory.CreateDirectory(Path.GetDirectoryName(FileName)!);

        System.Diagnostics.Debug.WriteLine("DOSSIER : " + Path.GetDirectoryName(FileName));
        System.Diagnostics.Debug.WriteLine("FICHIER : " + FileName);

        File.WriteAllText(FileName, json);

        System.Diagnostics.Debug.WriteLine("EXISTE : " + File.Exists(FileName));
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "notepad.exe",
            Arguments = "\"" + FileName + "\"",
            UseShellExecute = true
        });
    }

    public List<CaptureZone> Load()
    {
        if (!File.Exists(FileName))
            return new();

        var json = File.ReadAllText(FileName);

        var config =
            JsonSerializer.Deserialize<CaptureZonesConfiguration>(json);

        return config?.Zones ?? new();
    }
}