namespace MCServerManager.Desktop.Models;

public class SftpCredentials : ICloneable
{
	public String Host { get; init; }  = "";
	
	public String User { get; init; }  = "";

	public String Password { get; init; } = "";

	public int Port { get; init; } = 22;

	public object Clone()
	{
		return new SftpCredentials()
		{
			Host = Host,
			User = User,
			Password = Password,
			Port = Port
		};
	}
}