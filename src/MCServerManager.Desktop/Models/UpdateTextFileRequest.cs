namespace MCServerManager.Shared;

public class UpdateTextFileRequest
{
	public required SftpCredentials Credentials { get; init; }
	public required string Content { get; init; }
	public required string Path { get; init; }
}