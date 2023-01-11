using Microsoft.AspNetCore.Components;
using WebServerManager.Client.Services;
namespace WebServerManager.Client.Pages;

public partial class Index
{
	[Inject]
	public CredentialService CredentialService { get; set; } = null!;
}
