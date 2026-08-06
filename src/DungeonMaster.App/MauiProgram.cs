using CommunityToolkit.Maui;
using Plugin.Maui.KeyListener;
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
			.UseKeyListener()
			.ConfigureFonts(fonts =>
			{
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

		// Strip native border/focus chrome from all Entry and Editor controls so our themed
		// Border wrappers render cleanly without the platform's default focus ring / underline.
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
#elif MACCATALYST
				// On macCatalyst the Entry platform view is a UITextField.
				// Remove the system border/background so our MAUI Border wrapper is the only visible frame.
				if (handler.PlatformView is UIKit.UITextField tf)
				{
					tf.BorderStyle = UIKit.UITextBorderStyle.None;
					tf.BackgroundColor = UIKit.UIColor.Clear;
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
#elif MACCATALYST
				// On macCatalyst the Editor platform view is a UITextView.
				// Clear its default system background so the MAUI Border wrapper is the sole visual frame.
				if (handler.PlatformView is UIKit.UITextView tv)
				{
					tv.BackgroundColor = UIKit.UIColor.Clear;
					// UITextView draws a system border via its layer; clear it.
					tv.Layer.BorderWidth = 0;
					tv.Layer.BorderColor = CoreGraphics.CGColor.CreateSrgb(0, 0, 0, 0);
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
