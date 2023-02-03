namespace MCServerManager.Desktop.Models;

public class SftpCredentials : ICloneable
{
	public String Host { get; set; }  = "";
	
	public String User { get; set; }  = "";

	public String Password { get; set; } = "";

	public ushort Port { get; set; } = 22;

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