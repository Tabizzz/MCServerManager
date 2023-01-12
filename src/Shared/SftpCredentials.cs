namespace WebServerManager.Shared;

public class SftpCredentials : ICloneable
{
	public String Host { get; set; }  = "";
	
	public String? Token { get; set; }
	
	public String User { get; set; }  = "";

	public String Password { get; set; } = "";

	public int Port { get; set; } = 22;
	
	public bool IsValid { get; set; }

	public bool ShareToken { get; set; } = true;
	
	public bool SaveCredentials { get; set; }

	public object Clone()
	{
		return new SftpCredentials()
		{
			Host = Host,
			Token = Token,
			User = User,
			Password = Password,
			Port = Port,
			IsValid = IsValid,
			ShareToken = ShareToken,
			SaveCredentials = SaveCredentials
		};
	}
}