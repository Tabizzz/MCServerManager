using System.Collections.Concurrent;
using System.Net;
using MCServerManager.Desktop.Controllers;
using MCServerManager.Desktop.Managers;
using MCServerManager.Desktop.Models;
namespace MCServerManager.Desktop.Services;

public class FileSystemService
{
	readonly IDictionary<string, List<SftpFileEntry>> _entriesPerPath = new ConcurrentDictionary<string, List<SftpFileEntry>>();

	readonly SftpController _sftp;

	readonly ServerManager _serverManager;

	public FileSystemService(SftpController sftp, ServerManager serverManager)
	{
		_sftp = sftp;
		_serverManager = serverManager;
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
		if (_serverManager.CurrentServer is not null)
		{
			var response = await _sftp.ListFiles(_serverManager.CurrentServer.Id, path);
			if (_sftp.StatusCode == HttpStatusCode.Accepted)
			{
				_entriesPerPath[path] = response.ToList();
				return (path, GetCacheEntries(path));
			}
		}
		if (HasEntriesForPath(path))
		{
			_entriesPerPath.Remove(path);
		}
		return (path, new ());
	}
}