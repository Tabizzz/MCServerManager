using System.Net;
using System.Net.Http.Json;
using System.Web;
using BlazorMonaco;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebServerManager.Client.Shared;
namespace WebServerManager.Client.Pages;

public partial class Raw : IAsyncDisposable
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;

	MonacoEditor Editor { get; set; } = null!;

	bool _loaded;

	string? _error;
	
	StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
	{
		Console.WriteLine("configuring editors");
		return new()
		{
			Language = "yaml",
			GlyphMargin = true,
			Theme = "wsm",
			WordWrap = "on",
			Minimap = new()
			{
				Enabled = false
			},
			ScrollBeyondLastLine = false
		};
	}

	protected override async Task OnInitializedAsync()
	{
		Layout.RequireSftp = true;
		if (!CredentialService.SftpCredentials.IsValid) return;
		
		var urlEncode = HttpUtility.UrlEncode(Path);
		var response = await Http.PostAsJsonAsync($"Sftp/RawText?path={urlEncode}", CredentialService.SftpCredentials);
		if (response.StatusCode == HttpStatusCode.Accepted && (await response.Content.ReadAsStringAsync()) is { } str)
		{
			_loaded = true;
			await JsRuntime.InvokeVoidAsync("addMonacoTheme");
			StateHasChanged();
			await Editor.SetValue(str);
			await Editor.Layout();
		}
		else if(response.StatusCode == HttpStatusCode.InsufficientStorage && (await response.Content.ReadAsStringAsync()) is { } error)
		{
			var split = error.Split(";");
			var size = long.Parse(split[0]);
			var max = long.Parse(split[1]);

			
			_error = $"Unable to view file, file size is {Math.Round(size / 1e6, 2)}MB but max allowed to view is {Math.Round(max / 1e6, 2)}MB";
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_loaded)
		{
			await Editor.SetValue("");
		}
	}

	async Task SaveFile()
	{
		var text = await Editor.GetValue();
		
	}
}