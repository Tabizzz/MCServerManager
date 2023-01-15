using System.Net;
using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using WebServerManager.Client.Dialogs;
using WebServerManager.Client.Shared;
using WebServerManager.Client.Utils;
using WebServerManager.Shared;
namespace WebServerManager.Client.Pages;

public partial class Files : IDisposable
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;

	public SftpFileEntry[]? FileEntries { get; set; }

	public bool BackgroundLoadingFiles { get; set; }

	protected override async Task OnInitializedAsync()
	{
		Layout.RequireSftp = true;
		if (!CredentialService.SftpCredentials.IsValid) return;

		await UpdateFiles();

		NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
	}

	async Task UpdateFiles()
	{
		ValidatePath();

		if (FileSystem.HasEntriesForPath(Path))
		{
			FileEntries = FileSystem.GetCacheEntries(Path);
			BackgroundLoadingFiles = true;
			StateHasChanged();
		}
		var res = await FileSystem.UpdatePath(Path);
		if (res.Item1 == Path)
		{
			FileEntries = res.Item2;
		}
		BackgroundLoadingFiles = false;
		StateHasChanged();
	}

	void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs e)
	{
		if (!NavigationManager.ToBaseRelativePath(e.Location).StartsWith("files"))
			return;
		FileEntries = null;
		StateHasChanged();
		InvokeAsync(async () => { await UpdateFiles(); });
	}

	public void Dispose()
	{
		NavigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
	}

	string GetLinkFOrFileEntry(SftpFileEntry file)
	{
		var path = (file.IsFolder ? "/files" : "raw") + file.Path;
		if (file.IsFolder || System.IO.Path.GetExtension(file.Path) is ".yml" or ".json" or ".txt" or ".properties")
		{
			return path;
		}
		return null!;
	}

	void CreateFile()
	{
		var parameters = new DialogParameters
		{
			{ "Path", Path }
		};
		var options = new DialogOptions
		{
			CloseButton = true,
			MaxWidth = MaxWidth.Medium,
			FullWidth = true,
			Position = DialogPosition.TopCenter
		};
		DialogService.Show<FileCreateDialog>($"Create new file", parameters, options);

	}

	void CreateDirectory()
	{
		var parameters = new DialogParameters
		{
			{ "Path", Path },
			{ "Directory", true }
		};
		var options = new DialogOptions
		{
			CloseButton = true,
			MaxWidth = MaxWidth.Medium,
			FullWidth = true,
			Position = DialogPosition.TopCenter
		};
		DialogService.Show<FileCreateDialog>($"Create new directory", parameters, options);
	}

	bool _uploading;
	
	long _uploaded;

	double _percentage;

	long _toUpload;

	async Task UploadFiles(InputFileChangeEventArgs arg)
	{
		var file = arg.File;
		_uploading = true;
		_percentage = 0;
		_toUpload = file.Size;
		StateHasChanged();

		using var content = new MultipartFormDataContent();
		try
		{
			var fileStream = file.OpenReadStream(int.MaxValue);
			var streamContent = new ProgressiveStreamContent(fileStream, 1024 * 2500, (u, p) =>
			{
				// Set the values of the _uploaded & _percentage fields to the value provided from the event
				_uploaded = u;
				_percentage = p;

				// Call StateHasChanged() to notify the component about this change to re-render the UI
				StateHasChanged();
			});
			
			content.Add(
				content: streamContent,
				name: "file",
				fileName: file.Name);
			var urlEncode = HttpUtility.UrlEncode(System.IO.Path.Combine(Path, file.Name));
			var token = HttpUtility.UrlEncode(CredentialService.SftpCredentials.Token);
			var client = new HttpClient()
			{
				BaseAddress = new (HostEnvironment.BaseAddress),
				Timeout = TimeSpan.FromMilliseconds(-1)
			};
			var response = await client.PostAsync($"Sftp/UploadFile?path={urlEncode}&tokenstr={token}", content);
			if (response.StatusCode == HttpStatusCode.Accepted)
			{
				await UpdateFiles();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("{0} not uploaded (Err: 6): {1}",
				file.Name, ex.Message);
		}
		_uploading = false;
		StateHasChanged();
	}
}