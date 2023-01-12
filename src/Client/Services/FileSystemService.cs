using System.Net;
using System.Net.Http.Json;
using System.Web;
using WebServerManager.Shared;
namespace WebServerManager.Client.Services;

public class FileSystemService
{
	readonly IDictionary<string, List<SftpFileEntry>> _entriesPerPath = new Dictionary<string, List<SftpFileEntry>>();

	readonly HttpClient _http;

	readonly CredentialService _credentialService;

	public FileSystemService(HttpClient http, CredentialService credentialService)
	{
		_http = http;
		_credentialService = credentialService;
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

	public async Task UpdatePath(string path)
	{
		var urlEncode = HttpUtility.UrlEncode(path);
		
		var response = await _http.PostAsJsonAsync($"Sftp/ListFiles?path={urlEncode}", _credentialService.SftpCredentials);
		if (response.StatusCode == HttpStatusCode.Accepted)
		{
			var array = await response.Content.ReadFromJsonAsync<SftpFileEntry[]>();
			var list = array?.ToList() ?? new ();
			_entriesPerPath[path] = list;
		}
		else if (HasEntriesForPath(path))
		{
			_entriesPerPath.Remove(path);
		}
	}
}