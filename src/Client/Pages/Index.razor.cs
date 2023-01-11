using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WebServerManager.Client.Services;
using WebServerManager.Shared;
namespace WebServerManager.Client.Pages;

public partial class Index
{
	[Inject]
	public CredentialService CredentialService { get; set; } = null!;

	public String ErrorMessage { get; set; } = "";
	
	public bool AuthLoading { get; set; }

	public async Task OnClickConnect()
	{
		ErrorMessage = "";
		AuthLoading = true;
		StateHasChanged();
		var response = await Http.PostAsJsonAsync("Auth", CredentialService.SftpCredentials);
		AuthLoading = false;
		var token = await response.Content.ReadFromJsonAsync<SftpCredentials>();
		if (response.StatusCode == HttpStatusCode.Accepted && token != null)
		{
			CredentialService.SftpCredentials.Token = token.Token;
			CredentialService.SftpCredentials.IsValid = true;
		}
		else if (response.StatusCode == HttpStatusCode.Unauthorized && token != null)
		{
			ErrorMessage = token.Token!;
		}
		else
		{
			ErrorMessage = "An unknow error hass ocurred";
		}
		StateHasChanged();
	}
}
