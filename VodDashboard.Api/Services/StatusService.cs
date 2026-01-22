using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;
using VodDashboard.Api.Models;
using VodDashboard.Api.DTO;

namespace VodDashboard.Api.Services;

public partial class StatusService(IOptions<PipelineSettings> settings)
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

        // Read last non-empty line efficiently by reading from the end
        string? lastLine;
        try
        {
            lastLine = ReadLastNonEmptyLine(logPath);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
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
            var percentMatch = PercentExtractPattern().Match(lastLine);
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

    /// <summary>
    /// Efficiently reads the last non-empty line from a file by reading from the end.
    /// This avoids loading the entire file into memory for large log files.
    /// </summary>
    private static string? ReadLastNonEmptyLine(string filePath)
    {
        const int bufferSize = 4096;
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        
        if (fileStream.Length == 0)
        {
            return null;
        }

        var buffer = new byte[bufferSize];
        var position = fileStream.Length;
        var currentLine = new List<byte>();
        
        while (position > 0)
        {
            var bytesToRead = (int)Math.Min(bufferSize, position);
            position -= bytesToRead;
            fileStream.Seek(position, SeekOrigin.Begin);
            
            int bytesRead = 0;
            while (bytesRead < bytesToRead)
            {
                int read = fileStream.Read(buffer, bytesRead, bytesToRead - bytesRead);
                if (read == 0)
                {
                    break;
                }
                bytesRead += read;
            }
            
            // Process bytes in reverse order
            for (int i = bytesRead - 1; i >= 0; i--)
            {
                byte b = buffer[i];
                
                if (b == '\n' || b == '\r')
                {
                    if (currentLine.Count > 0)
                    {
                        currentLine.Reverse();
                        var line = System.Text.Encoding.UTF8.GetString(currentLine.ToArray());
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            return line;
                        }
                        currentLine.Clear();
                    }
                }
                else
                {
                    currentLine.Add(b);
                }
            }
        }
        
        // Handle the first line (or single line file)
        if (currentLine.Count > 0)
        {
            currentLine.Reverse();
            var line = System.Text.Encoding.UTF8.GetString(currentLine.ToArray());
            if (!string.IsNullOrWhiteSpace(line))
            {
                return line;
            }
        }
        
        return null;
    }

    [GeneratedRegex(@"\(\s*\d+%\s*\)")]
    private static partial Regex PercentagePattern();

    [GeneratedRegex(@"(\d+)%")]
    private static partial Regex PercentExtractPattern();

    [GeneratedRegex(@"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\]")]
    private static partial Regex TimestampPattern();

    private static string ExtractAfter(string line, string marker, bool stripPercentage)
    {
        int idx = line.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return string.Empty;
        string result = line[(idx + marker.Length)..].Trim();
        
        // Only strip percentage notation from stage names, not from filenames
        if (stripPercentage)
        {
            string trimmedResult = result.TrimEnd();
            Match match = PercentagePattern().Match(trimmedResult);
            if (match.Success && match.Index + match.Length == trimmedResult.Length)
            {
                result = trimmedResult[..match.Index].TrimEnd();
            }
        }
        
        return result;
    }

    private static DateTime? ParseTimestamp(string line)
    {
        // Expected format: [2026-01-21 14:33:12]
        var match = TimestampPattern().Match(line);
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
