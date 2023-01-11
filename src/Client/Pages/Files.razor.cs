using System.Net;
using System.Net.Http.Json;
using System.Web;
using Microsoft.AspNetCore.Components;
using WebServerManager.Client.Shared;
using WebServerManager.Shared;
namespace WebServerManager.Client.Pages;

public partial class Files
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;
	
	[Parameter]
	public string? Path { get; set; }
	
	public SftpFileEntry[]? FileEntries { get; set; }

	protected override async Task OnInitializedAsync()
	{
		Layout.RequireSftp = true;
		if(!CredentialService.SftpCredentials.IsValid) return;
		OnParametersSet();
		Console.WriteLine("dir {0}", Path);
		var path = HttpUtility.UrlEncode(Path);
		Console.WriteLine("listing {0}", path);
		var response = await Http.PostAsJsonAsync($"Sftp/ListFiles?path={path}", CredentialService.SftpCredentials);
		if (response.StatusCode == HttpStatusCode.Accepted)
		{
			var list = await response.Content.ReadFromJsonAsync<SftpFileEntry[]>();
			FileEntries = list ?? Array.Empty<SftpFileEntry>();
		}
	}

	protected override void OnParametersSet()
	{
		if (string.IsNullOrWhiteSpace(Path)) Path = string.Empty;
		if(!Path.StartsWith("/"))
			Path = "/" + Path;
	}
}