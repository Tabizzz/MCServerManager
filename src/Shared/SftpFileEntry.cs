namespace WebServerManager.Shared;

public class SftpFileEntry
{
	public required string Path { get; init; }
	
	public required bool IsFolder { get; init; }
	
	public required long Size { get; init; }
	
	public required string Name { get; init; }
	
	public required DateTime LastWrite { get; init; }
}