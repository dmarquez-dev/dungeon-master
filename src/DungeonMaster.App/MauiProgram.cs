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

				fonts.AddFont(
					"Cinzel-Regular.ttf",
					"CinzelRegular");

				fonts.AddFont(
					"Cinzel-SemiBold.ttf",
					"CinzelSemiBold");

				fonts.AddFont(
					"CinzelDecorative-Regular.ttf",
					"CinzelDecorative");

				fonts.AddFont(
					"EBGaramond-Regular.ttf",
					"EBGaramondRegular");

				fonts.AddFont(
					"EBGaramond-Italic.ttf",
					"EBGaramondItalic");

				fonts.AddFont(
					"rpgawesome-webfont.ttf",
					"RpgAwesome");
			});

		// Strip WinUI native border/focus chrome from all Entry and Editor controls
		// so our themed Border wrappers render cleanly without the platform's purple focus ring
		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(
			"DmNativeStyle",
			(handler, _) =>
			{
#if WINDOWS
				if (handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox tb)
				{
					tb.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
					tb.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBorderBrush"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBorderBrushPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBorderBrushFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBackground"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBackgroundPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBackgroundFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
				}
#endif
			});

		Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(
			"DmNativeStyle",
			(handler, _) =>
			{
#if WINDOWS
				if (handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox tb)
				{
					tb.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
					tb.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBorderBrush"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBorderBrushPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBorderBrushFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBackground"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBackgroundPointerOver"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
					tb.Resources["TextControlBackgroundFocused"] = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
				}
#endif
			});

		var dbFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"DungeonMaster");
		Directory.CreateDirectory(dbFolder);

		var dbPath = Path.Combine(dbFolder, "dungeon-master.db");
		var connectionString = $"Data Source={dbPath}";

		builder.Services.AddApplicationServices();
		builder.Services.AddInfrastructureServices(connectionString);

		builder.Services.AddTransient<DungeonListViewModel>();
		builder.Services.AddTransient<DungeonListPage>();
		builder.Services.AddTransient<CreateDungeonViewModel>();
		builder.Services.AddTransient<CreateDungeonPopup>();
		builder.Services.AddTransient<DungeonDetailViewModel>();
		builder.Services.AddTransient<DungeonDetailPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		app.Services.MigrateDatabase();

		return app;
	}
}
