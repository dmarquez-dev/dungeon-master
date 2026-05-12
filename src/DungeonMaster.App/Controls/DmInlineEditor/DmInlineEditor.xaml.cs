using MauiApplication = Microsoft.Maui.Controls.Application;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed inline editor that toggles between a read-only label and an editable entry or editor.
///		Supports single-line and multi-line modes, placeholder text, focus glow, and commit-on-unfocus behaviour.
/// </summary>
public partial class DmInlineEditor : ContentView
{
	/// <summary>
	///		Identifies the <see cref="Text"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TextProperty =
		BindableProperty.Create(
			nameof(Text),
			typeof(string),
			typeof(DmInlineEditor),
			string.Empty,
			propertyChanged: (bindable, _, newValue) =>
				((DmInlineEditor)bindable).SyncDisplay((string?)newValue));

	/// <summary>
	///		Identifies the <see cref="Placeholder"/> bindable property.
	/// </summary>
	public static readonly BindableProperty PlaceholderProperty =
		BindableProperty.Create(
			nameof(Placeholder),
			typeof(string),
			typeof(DmInlineEditor),
			string.Empty,
			propertyChanged: (bindable, _, newValue) =>
			{
				var self = (DmInlineEditor)bindable;
				var ph = (string?)newValue ?? string.Empty;
				self.EditEntry.Placeholder = ph;
				self.EditEditor.Placeholder = ph;

				if (string.IsNullOrWhiteSpace(self.Text))
				{
					self.SyncDisplay(self.Text);
				}
			});

	/// <summary>
	///		Identifies the <see cref="FontFamily"/> bindable property.
	/// </summary>
	public static readonly BindableProperty FontFamilyProperty =
		BindableProperty.Create(
			nameof(FontFamily),
			typeof(string),
			typeof(DmInlineEditor),
			"EBGaramondRegular");

	/// <summary>
	///		Identifies the <see cref="FontSize"/> bindable property.
	/// </summary>
	public static readonly BindableProperty FontSizeProperty =
		BindableProperty.Create(
			nameof(FontSize),
			typeof(double),
			typeof(DmInlineEditor),
			18.0);

	/// <summary>
	///		Identifies the <see cref="TextHorizontalAlignment"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TextHorizontalAlignmentProperty =
		BindableProperty.Create(
			nameof(TextHorizontalAlignment),
			typeof(TextAlignment),
			typeof(DmInlineEditor),
			TextAlignment.Start,
			propertyChanged: (bindable, _, newValue) =>
				((DmInlineEditor)bindable).ApplyTextAlignment((TextAlignment)newValue));

	/// <summary>
	///		Identifies the <see cref="AcceptsReturn"/> bindable property.
	///		When <see langword="false"/>, Enter commits the edit (single-line); when <see langword="true"/>, Enter inserts a newline (multi-line).
	/// </summary>
	public static readonly BindableProperty AcceptsReturnProperty =
		BindableProperty.Create(
			nameof(AcceptsReturn),
			typeof(bool),
			typeof(DmInlineEditor),
			true);

	/// <summary>
	///		Raised when the user taps the read-only display label to begin editing.
	/// </summary>
	public event EventHandler? LabelTapped;

	/// <summary>
	///		Raised when the user commits an edit, providing the new text value.
	/// </summary>
	public event EventHandler<string?>? TextCommitted;

	/// <summary>
	///		Gets or sets the current text content of the editor.
	/// </summary>
	public string? Text
	{
		get => (string?)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	/// <summary>
	///		Gets or sets the placeholder text shown when <see cref="Text"/> is empty or whitespace.
	/// </summary>
	public string? Placeholder
	{
		get => (string?)GetValue(PlaceholderProperty);
		set => SetValue(PlaceholderProperty, value);
	}

	/// <summary>
	///		Gets or sets the font family applied to the display label and edit controls.
	/// </summary>
	public string FontFamily
	{
		get => (string)GetValue(FontFamilyProperty);
		set => SetValue(FontFamilyProperty, value);
	}

	/// <summary>
	///		Gets or sets the font size applied to the display label and edit controls.
	/// </summary>
	public double FontSize
	{
		get => (double)GetValue(FontSizeProperty);
		set => SetValue(FontSizeProperty, value);
	}

	/// <summary>
	///		Gets or sets the horizontal text alignment applied to the display label and edit controls.
	/// </summary>
	public TextAlignment TextHorizontalAlignment
	{
		get => (TextAlignment)GetValue(TextHorizontalAlignmentProperty);
		set => SetValue(TextHorizontalAlignmentProperty, value);
	}

	/// <summary>
	///		Gets or sets a value indicating whether the editor accepts multi-line input.
	///		When <see langword="false"/>, Enter commits the edit; when <see langword="true"/>, Enter inserts a newline.
	/// </summary>
	public bool AcceptsReturn
	{
		get => (bool)GetValue(AcceptsReturnProperty);
		set => SetValue(AcceptsReturnProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmInlineEditor"/> and loads the XAML component tree.
	/// </summary>
	public DmInlineEditor()
	{
		InitializeComponent();
	}

	/// <summary>
	///		Programmatically enters edit mode, focusing the appropriate input control.
	/// </summary>
	public void ActivateEditor()
	{
		DisplayLabel.IsVisible = false;

		if (!AcceptsReturn)
		{
			EditEntry.Text = Text;
			EditEntry.IsVisible = true;
			EditEntry.Focus();
		}
		else
		{
			EditEditor.Text = Text;
			EditEditor.IsVisible = true;
			EditEditor.Focus();
		}
	}

	/// <summary>
	///		Commits any in-progress edit without user interaction by unfocusing the active input control.
	/// </summary>
	public void TryCommit()
	{
		if (EditEntry.IsVisible)
		{
			EditEntry.Unfocus();
		}
		else if (EditEditor.IsVisible)
		{
			EditEditor.Unfocus();
		}
	}

	private void ApplyTextAlignment(TextAlignment alignment)
	{
		DisplayLabel.HorizontalTextAlignment = alignment;
		EditEntry.HorizontalTextAlignment = alignment;
		EditEditor.HorizontalTextAlignment = alignment;
	}

	private void SyncDisplay(string? value)
	{
		var isEmpty = string.IsNullOrWhiteSpace(value);
		DisplayLabel.Text = isEmpty ? Placeholder : value;
		DisplayLabel.TextColor = isEmpty
			? (Color)MauiApplication.Current!.Resources["DMTextMuted"]
			: (Color)MauiApplication.Current!.Resources["DMTextPrimary"];
		DisplayLabel.FontAttributes = isEmpty ? FontAttributes.Italic : FontAttributes.None;
	}

	private void OnLabelTapped(object? sender, TappedEventArgs e)
	{
		LabelTapped?.Invoke(this, EventArgs.Empty);
		ActivateEditor();
	}

	private void OnInputFocused(object? sender, FocusEventArgs e)
	{
		var gold = (Color)MauiApplication.Current!.Resources["DMScrollGoldLight"];
		FocusBorder.Stroke = gold;
		FocusBorder.BackgroundColor = Color.FromArgb("#08C4862A");
		FocusBorder.Shadow = new Shadow
		{
			Brush = new SolidColorBrush(gold),
			Offset = new Point(0, 0),
			Radius = 10,
			Opacity = 0.4f
		};
	}

	// Entry.Completed fires when the user presses Enter — treat as commit.
	private void OnEntryCompleted(object? sender, EventArgs e) =>
		EditEntry.Unfocus();

	private void OnEntryUnfocused(object? sender, FocusEventArgs e)
	{
		ClearFocusGlow();
		Commit(EditEntry.Text);
		EditEntry.IsVisible = false;
		DisplayLabel.IsVisible = true;
	}

	private void OnEditorUnfocused(object? sender, FocusEventArgs e)
	{
		ClearFocusGlow();
		Commit(EditEditor.Text);
		EditEditor.IsVisible = false;
		DisplayLabel.IsVisible = true;
	}

	private void Commit(string? value)
	{
		Text = value;
		SyncDisplay(value);
		TextCommitted?.Invoke(this, value);
	}

	private void ClearFocusGlow()
	{
		FocusBorder.Stroke = Colors.Transparent;
		FocusBorder.ClearValue(VisualElement.BackgroundColorProperty);
		FocusBorder.ClearValue(VisualElement.ShadowProperty);
	}
}
