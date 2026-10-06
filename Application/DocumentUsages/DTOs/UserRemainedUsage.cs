namespace Application.DocumentUsages.DTOs;

public class UserRemainingUsage
{
    public int MaxAllowedLink { get; set; }
    public int MaxAllowedFiles { get; set; }
    public int MaxAllowedRequest { get; set; }

    public int RemainedLinkRequest { get; set; }
    public int RemainedFileRequest { get; set; }
    public int RemainedMediaRequest { get; set; }
}