using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using VodDashboard.Api.Models;
using VodDashboard.Api.DTO;

namespace VodDashboard.Api.Services;

public class StatusService(IOptions<PipelineSettings> settings)
{
    #region Private Data

    private readonly PipelineSettings _settings = settings.Value;

    #endregion

    #region Public Methods

    public virtual StatusDTO GetStatus()
    {
        var logPath = Path.Combine(_settings.OutputDirectory, "pipeline.log");

        if (!File.Exists(logPath))
        {
            return new StatusDTO(
                IsRunning: false,
                CurrentFile: null,
                Stage: null,
                Percent: null,
                LastUpdated: null);
        }

        // Read last non-empty line
        var lastLine = File.ReadLines(logPath)
                           .Reverse()
                           .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));

        if (lastLine == null)
        {
            return new StatusDTO(
                IsRunning: false,
                CurrentFile: null,
                Stage: null,
                Percent: null,
                LastUpdated: null);
        }

        // Parse timestamp from log line if present
        DateTime? lastUpdated = ParseTimestamp(lastLine);

        // Example log formats:
        // [2026-01-21 14:33:12] Processing file: myvideo.mp4
        // [2026-01-21 14:33:15] Stage: silence removal (42%)
        // [2026-01-21 14:33:20] Completed file: myvideo.mp4

        if (lastLine.Contains("Processing file:"))
        {
            return new StatusDTO(
                IsRunning: true,
                CurrentFile: ExtractAfter(lastLine, "Processing file:"),
                Stage: "Starting",
                Percent: null,
                LastUpdated: lastUpdated);
        }
        else if (lastLine.Contains("Stage:"))
        {
            var stage = ExtractAfter(lastLine, "Stage:");
            
            // Extract percent if present
            int? percent = null;
            var percentMatch = Regex.Match(lastLine, @"(\d+)%");
            if (percentMatch.Success)
            {
                percent = int.Parse(percentMatch.Groups[1].Value);
            }

            return new StatusDTO(
                IsRunning: true,
                CurrentFile: null,
                Stage: stage,
                Percent: percent,
                LastUpdated: lastUpdated);
        }
        else if (lastLine.Contains("Completed file:"))
        {
            return new StatusDTO(
                IsRunning: false,
                CurrentFile: null,
                Stage: null,
                Percent: null,
                LastUpdated: lastUpdated);
        }

        return new StatusDTO(
            IsRunning: false,
            CurrentFile: null,
            Stage: null,
            Percent: null,
            LastUpdated: lastUpdated);
    }

    #endregion

    #region Private Methods

    private static string ExtractAfter(string line, string marker)
    {
        var idx = line.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return string.Empty;
        var result = line[(idx + marker.Length)..].Trim();
        
        // Remove percentage if present in the extracted text
        var percentIndex = result.IndexOf('(');
        if (percentIndex > 0)
        {
            result = result[..percentIndex].Trim();
        }
        
        return result;
    }

    private static DateTime? ParseTimestamp(string line)
    {
        // Expected format: [2026-01-21 14:33:12]
        var match = Regex.Match(line, @"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]");
        if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var timestamp))
        {
            return timestamp;
        }
        return null;
    }

    #endregion
}
