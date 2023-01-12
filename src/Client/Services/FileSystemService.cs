using System.Net;
using System.Net.Http.Json;
using System.Web;
using WebServerManager.Shared;
namespace WebServerManager.Client.Services;

public class FileSystemService
{
	readonly IDictionary<string, List<SftpFileEntry>> _entriesPerPath = new Dictionary<string, List<SftpFileEntry>>();

	HttpClient Http;

	CredentialService CredentialService;

	public FileSystemService(HttpClient http, CredentialService credentialService)
	{
		Http = http;
		CredentialService = credentialService;
	}

	public bool HasEntriesForPath(string path) => _entriesPerPath.ContainsKey(path);
	
	public SftpFileEntry[] GetCacheEntries(string path)
	{
		_entriesPerPath[path].Sort((a, b) =>
		{
			var dev = b.IsFolder.CompareTo(a.IsFolder);
			return dev == 0 ? string.Compare(a.Name, b.Name, StringComparison.InvariantCulture) : dev;
		});
		
		return _entriesPerPath[path].ToArray();
	}

	public async Task UpdatePath(string Path)
	{
		var path = HttpUtility.UrlEncode(Path);
		
		var response = await Http.PostAsJsonAsync($"Sftp/ListFiles?path={path}", CredentialService.SftpCredentials);
		if (response.StatusCode == HttpStatusCode.Accepted)
		{
			var array = await response.Content.ReadFromJsonAsync<SftpFileEntry[]>();
			var list = array?.ToList() ?? new ();
			_entriesPerPath[Path] = list;
		}
		else if (HasEntriesForPath(Path))
		{
			_entriesPerPath.Remove(Path);
		}
	}
}