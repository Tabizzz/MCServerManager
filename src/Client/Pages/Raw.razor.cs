using System.Net;
using System.Net.Http.Json;
using System.Web;
using BlazorMonaco;
using MCServerManager.Client.Shared;
using MCServerManager.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
namespace MCServerManager.Client.Pages;

public partial class Raw : IAsyncDisposable
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;

	MonacoEditor Editor { get; set; } = null!;

	bool _loaded;

	string? _error;

	bool _saving;
	
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
			ScrollBeyondLastLine = false,
			AutomaticLayout = true
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
		_saving = true;
		StateHasChanged();
		ValidatePath();
		var ressponse = await Http.PostAsJsonAsync("Sftp/UpdateRawText", new UpdateTextFileRequest()
		{
			Content = await Editor.GetValue(),
			Path = Path,
			Credentials = CredentialService.SftpCredentials
		});

		if (ressponse.StatusCode == HttpStatusCode.Accepted)
		{
			Snackbar.Add("File sucessfully saved", Severity.Success, key: "raw/fileSaved");
		}
		else
		{
			Snackbar.Add("Unable to save the file", Severity.Error, key: "raw/fileSavedError");
		}
		_saving = false;
		StateHasChanged();
	}

	async Task EditorOnDidInit()
	{
		await Editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_S, (_, _) =>
		{
			InvokeAsync(SaveFile);
		});
	}
}