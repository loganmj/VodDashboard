namespace VodDashboard.Api.DTO
{
    public record RawFileDTO(string FileName, long SizeBytes, DateTimeOffset Created);
}
