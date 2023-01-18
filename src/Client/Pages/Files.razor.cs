using System.Net;
using System.Net.Http.Json;
using System.Web;
using MCServerManager.Client.Dialogs;
using MCServerManager.Client.Shared;
using MCServerManager.Client.Utils;
using MCServerManager.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using RenameDialog = MCServerManager.Client.Dialogs.RenameDialog;
namespace MCServerManager.Client.Pages;

public partial class Files : IDisposable
{
	[CascadingParameter] public MainLayout Layout { get; set; } = null!;

	public List<SftpFileEntry>? FileEntries { get; set; }

	public bool BackgroundLoadingFiles { get; set; }

	protected override async Task OnInitializedAsync()
	{
		Layout.RequireSftp = true;
		if (!CredentialService.SftpCredentials.IsValid) return;

		await UpdateFiles();

		NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
	}

	async Task UpdateFiles(bool restore = true)
	{
		ValidatePath();
		FileEntries = null;
		StateHasChanged();

		if (restore && FileSystem.HasEntriesForPath(Path))
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
		InvokeAsync(async () => { await UpdateFiles(); });
	}

	public void Dispose()
	{
		NavigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
	}

	string GetLinkFOrFileEntry(SftpFileEntry file)
	{
		var path = (file.IsFolder ? "/files" : "/raw") + file.Path;
		return path;
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
		DialogService.Show<FileCreateDialog>($"Create new directory", parameters);
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

	string GetFileSizeStr(long fileSize)
	{
		double size = fileSize;
		if (size < 1024)
			return size + " B";
		size = Math.Round(fileSize / 1024d, 2);
		if (size < 1024)
			return size + " KB";
		size = Math.Round(fileSize / 1024d / 1024d, 2);
		if (size < 1024)
			return size + " MB";
		return size + " GB";
	}

	MudMessageBox Mbox { get; set; } = null!;
	
	async Task DeleteFile(SftpFileEntry file)
	{
		var result = await Mbox.Show();
		if (result.HasValue && result.Value)
		{
			FileEntries?.Remove(file);
			StateHasChanged();
			var toDelete = HttpUtility.UrlEncode(file.Path);
			var response = await Http.PostAsJsonAsync($"Sftp/DeleteFile?path={toDelete}", CredentialService.SftpCredentials);
			if (response.StatusCode == HttpStatusCode.Accepted)
			{
				Snackbar.Add((file.IsFolder ? "Folder" : "File") + " deleted", Severity.Success);
			}
			else
			{
				Snackbar.Add("Unable to delete " + (file.IsFolder ? "folder" : "file"), Severity.Error);
			}
			await UpdateFiles(false);
		}
	}

	async Task DownloadFile(SftpFileEntry file)
	{
		var path = HttpUtility.UrlEncode(file.Path);
		var token = HttpUtility.UrlEncode(CredentialService.SftpCredentials.Token);
		await Js.InvokeVoidAsync("triggerFileDownload", file.Name, $"Sftp/ReadFile?path={path}&tokenstr={token}");
	}

	async Task RenameFile(SftpFileEntry file)
	{
		var parameters = new DialogParameters
		{
			{ "Path", Path },
			{ "Directory", file.IsFolder },
			{ "Src", file.Path }
		};
		var dialog = await DialogService.ShowAsync<RenameDialog>("Rename", parameters);
		if ((await dialog.Result).Data is true)
		{
			await UpdateFiles();
		}
	}
}