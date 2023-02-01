using Renci.SshNet;
using Renci.SshNet.Sftp;
namespace MCServerManager.Desktop.Utils.Extensions;

public static class SftpClientExtensions
{
	public static Task<IEnumerable<ISftpFile>> ListDirectoryAsync(this SftpClient client, string path, Action<int>? listCallbak = null) => 
		Task.Factory.FromAsync(
			(a, b, c, d) => client.BeginListDirectory(a, c, d, b), 
			client.EndListDirectory, 
			path, 
			listCallbak, 
			null
		);
}