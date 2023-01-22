using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using MCServerManager.Desktop.Controllers;
using MCServerManager.Desktop.Models;
namespace MCServerManager.Desktop.Services;

public class FileSystemService
{
	readonly IDictionary<string, List<SftpFileEntry>> _entriesPerPath = new ConcurrentDictionary<string, List<SftpFileEntry>>();

	readonly SftpController _sftp;

	readonly CredentialService _credentialService;

	public FileSystemService(SftpController sftp, CredentialService credentialService)
	{
		_sftp = sftp;
		_credentialService = credentialService;
	}

	public bool HasEntriesForPath(string path) => _entriesPerPath.ContainsKey(path);
	
	public List<SftpFileEntry> GetCacheEntries(string path)
	{
		_entriesPerPath[path].Sort((a, b) =>
		{
			var dev = b.IsFolder.CompareTo(a.IsFolder);
			return dev == 0 ? string.Compare(a.Name, b.Name, StringComparison.InvariantCulture) : dev;
		});
		
		return _entriesPerPath[path].ToList();
	}

	public async Task<(string, List<SftpFileEntry>)> UpdatePath(string path)
	{
		var response = await _sftp.ListFiles(_credentialService.SftpCredentials, path);
		if (_sftp.StatusCode == HttpStatusCode.Accepted)
		{
			_entriesPerPath[path] = response.ToList();
			return (path, GetCacheEntries(path));
		}
		if (HasEntriesForPath(path))
		{
			_entriesPerPath.Remove(path);
		}
		return (path, new ());
	}
}