using System.Net;
using BlazorMonaco;
using MCServerManager.Desktop.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
namespace MCServerManager.Desktop.Pages;

public partial class Raw : IAsyncDisposable
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;

	MonacoEditor Editor { get; set; } = null!;

	bool _loaded;

	string? _error;

	bool _saving;

	string _response = null!;

	StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
	{
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
		if(ServerManager.CurrentServer is null) return;
		
		_response = await Sftp.RawText(ServerManager.CurrentServer.Id, Path);
		switch (Sftp.StatusCode)
		{
			case HttpStatusCode.Accepted:
				_loaded = true;
				await JsRuntime.InvokeVoidAsync("addMonacoTheme");
				StateHasChanged();
				break;
			case HttpStatusCode.InsufficientStorage:
			{
				var split = _response.Split(";");
				var size = long.Parse(split[0]);
				var max = long.Parse(split[1]);

			
				_error = $"Unable to view file, file size is {Math.Round(size / 1e6, 2)}MB but max allowed to view is {Math.Round(max / 1e6, 2)}MB";
				break;
			}
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
		if(ServerManager.CurrentServer is null) return;
		_saving = true;
		StateHasChanged();
		ValidatePath();
		await Sftp.UpdateRawText(Path, ServerManager.CurrentServer.Id, await Editor.GetValue());

		if (Sftp.StatusCode == HttpStatusCode.Accepted)
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
		
		await Editor.SetValue(_response);
		_response = string.Empty;
	}
}