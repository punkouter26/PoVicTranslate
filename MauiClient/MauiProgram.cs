using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace MauiClient;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Register TranslationApiService with HttpClient
		builder.Services.AddScoped<Services.TranslationApiService>(sp =>
		{
			// Use platform-specific handler if needed
			return new Services.TranslationApiService(new HttpClient());
		});

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
