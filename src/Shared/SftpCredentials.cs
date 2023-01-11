namespace WebServerManager.Shared;

public class SftpCredentials
{
	public String Host { get; set; }  = "";
	
	public String? Token { get; set; }
	
	public String User { get; set; }  = "";

	public String Password { get; set; } = "";

	public int Port { get; set; } = 22;
	
	public bool IsValid { get; set; }
}