using CommunityToolkit.Maui;
using DungeonMaster.Application;
using DungeonMaster.App.ViewModels;
using DungeonMaster.App.Views;
using DungeonMaster.Infrastructure;
using DungeonMaster.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace DungeonMaster.App;

/// <summary>
///		Provides the entry point for building and configuring the MAUI application.
/// </summary>
public static class MauiProgram
{
	/// <summary>
	///		Creates and configures the <see cref="MauiApp"/> instance with all required services.
	/// </summary>
	/// <returns>
	///		The fully configured <see cref="MauiApp"/>.
	/// </returns>
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont(
					"OpenSans-Regular.ttf",
					"OpenSansRegular");

				fonts.AddFont(
					"OpenSans-Semibold.ttf",
					"OpenSansSemibold");
			});

		var dbPath = Path.Combine(
			FileSystem.AppDataDirectory,
			"dungeon-master.db");
		var connectionString = $"Data Source={dbPath}";

		builder.Services.AddApplicationServices();
		builder.Services.AddInfrastructureServices(connectionString);

		builder.Services.AddTransient<DungeonListViewModel>();
		builder.Services.AddTransient<DungeonListPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.EnsureCreated();
		}

		return app;
	}
}
