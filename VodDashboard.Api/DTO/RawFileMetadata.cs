namespace VodDashboard.Api.DTO
{
    public record RawFileMetadata(string FileName, long SizeBytes, DateTimeOffset Created);
}
