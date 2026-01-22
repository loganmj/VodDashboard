using Microsoft.Extensions.Options;
using System.Globalization;
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
        if (string.IsNullOrWhiteSpace(_settings.OutputDirectory))
        {
            throw new InvalidOperationException("PipelineSettings.OutputDirectory is not configured.");
        }

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
        string? lastLine;
        try
        {
            lastLine = File.ReadLines(logPath)
                           .Reverse()
                           .FirstOrDefault(l => !string.IsNullOrWhiteSpace(l));
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Unable to read pipeline status log.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("Unable to read pipeline status log.", ex);
        }

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
                CurrentFile: ExtractAfter(lastLine, "Processing file:", stripPercentage: false),
                Stage: "Starting",
                Percent: null,
                LastUpdated: lastUpdated);
        }
        else if (lastLine.Contains("Stage:"))
        {
            var stage = ExtractAfter(lastLine, "Stage:", stripPercentage: true);
            
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
            LastUpdated: null);
    }

    #endregion

    #region Private Methods

    private static string ExtractAfter(string line, string marker, bool stripPercentage)
    {
        int idx = line.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return string.Empty;
        string result = line[(idx + marker.Length)..].Trim();
        
        // Only strip percentage notation from stage names, not from filenames
        if (stripPercentage)
        {
            int percentIndex = result.IndexOf('(');
            if (percentIndex >= 0)
            {
                string parenthetical = result[percentIndex..];
                if (Regex.IsMatch(parenthetical, @"\(\s*\d+%\s*\)"))
                {
                    result = result[..percentIndex].TrimEnd();
                }
            }
        }
        
        return result;
    }

    private static DateTime? ParseTimestamp(string line)
    {
        // Expected format: [2026-01-21 14:33:12]
        var match = Regex.Match(line, @"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]");
        if (match.Success && DateTime.TryParseExact(
            match.Groups[1].Value,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var timestamp))
        {
            return timestamp;
        }
        return null;
    }

    #endregion
}
