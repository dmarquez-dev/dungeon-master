using DungeonMaster.App.Theme;

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
	///		The default mirrors <c>DmFontBody</c> and stays a literal deliberately: a bindable default
	///		is evaluated in this static field initialiser, which can run before
	///		<c>Application.Current.Resources</c> exists, so it cannot read <c>DmTheme</c>.
	///		<c>BindablePropertyDefaultsMatchTokensTests</c> fails if the two drift apart.
	/// </summary>
	public static readonly BindableProperty FontFamilyProperty =
		BindableProperty.Create(
			nameof(FontFamily),
			typeof(string),
			typeof(DmInlineEditor),
			"EBGaramondRegular");

	/// <summary>
	///		Identifies the <see cref="FontSize"/> bindable property.
	///		The default mirrors <c>DmFontSizeBody</c> and stays a literal for the same reason given on
	///		<see cref="FontFamilyProperty"/>.
	/// </summary>
	public static readonly BindableProperty FontSizeProperty =
		BindableProperty.Create(
			nameof(FontSize),
			typeof(double),
			typeof(DmInlineEditor),
			18.0);

	/// <summary>
	///		Identifies the <see cref="TextColor"/> bindable property.
	///		Defaults to the dark-tavern primary text gold; parchment-hosted editors override it with an ink colour.
	///		The default arrives through <c>defaultValueCreator</c> rather than as a constant because a
	///		constant is evaluated in this static field initialiser, which can run before
	///		<c>Application.Current.Resources</c> exists; the creator runs on first read of an instance.
	/// </summary>
	public static readonly BindableProperty TextColorProperty =
		BindableProperty.Create(
			nameof(TextColor),
			typeof(Color),
			typeof(DmInlineEditor),
			propertyChanged: (bindable, _, _) =>
			{
				var self = (DmInlineEditor)bindable;
				self.SyncDisplay(self.Text);
			},
			defaultValueCreator: _ =>
				DmTheme.TextTavern);

	/// <summary>
	///		Identifies the <see cref="PlaceholderColor"/> bindable property.
	///		Defaults to the dark-tavern muted text colour, resolved lazily for the reason given on
	///		<see cref="TextColorProperty"/>.
	/// </summary>
	public static readonly BindableProperty PlaceholderColorProperty =
		BindableProperty.Create(
			nameof(PlaceholderColor),
			typeof(Color),
			typeof(DmInlineEditor),
			propertyChanged: (bindable, _, _) =>
			{
				var self = (DmInlineEditor)bindable;
				self.SyncDisplay(self.Text);
			},
			defaultValueCreator: _ =>
				DmTheme.GoldMuted);

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
	///		Gets or sets the colour used for non-empty text in the display label and edit controls.
	/// </summary>
	public Color TextColor
	{
		get => (Color)GetValue(TextColorProperty);
		set => SetValue(TextColorProperty, value);
	}

	/// <summary>
	///		Gets or sets the colour used for placeholder text in the display label and edit controls.
	/// </summary>
	public Color PlaceholderColor
	{
		get => (Color)GetValue(PlaceholderColorProperty);
		set => SetValue(PlaceholderColorProperty, value);
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

	/// <summary>Raised when the internal input control gains focus.</summary>
	public event EventHandler? EditingStarted;

	/// <summary>Raised when the internal input control loses focus.</summary>
	public event EventHandler? EditingEnded;

	private bool _isEditing;

	// Set by TryCommit, which commits up front; cleared by whichever comes first — the pending
	// Unfocused it belongs to, or the next time the user starts editing. Clearing it on focus
	// means a swallowed Unfocused cannot leave the flag set and eat a later genuine commit.
	private bool _suppressUnfocusCommit;

	/// <summary>
	///		Initializes a new instance of <see cref="DmInlineEditor"/> and loads the XAML component tree.
	/// </summary>
	public DmInlineEditor()
	{
		InitializeComponent();

		// SyncDisplay is the sole writer of DisplayLabel's Text, TextColor and FontAttributes, so
		// it has to run once here to put the label in a defined state. It used to be reached only
		// when Text, Placeholder, TextColor or PlaceholderColor was assigned, and DisplayLabel's
		// colour came from a XAML binding in the meantime — the double writer MASTER-077 names.
		SyncDisplay(Text);
	}

	/// <summary>
	///		Programmatically enters edit mode, focusing the appropriate input control.
	/// </summary>
	public void ActivateEditor()
	{
		DisplayLabel.Opacity = 0;
		DisplayLabel.InputTransparent = true;

		if (!AcceptsReturn)
		{
			EditEntry.Text = Text;
			ShowInput(EditEntry);
			EditEntry.Focus();
		}
		else
		{
			EditEditor.Text = Text;
			ShowInput(EditEditor);
			EditEditor.Focus();
		}
	}

	/// <summary>
	///		Commits any in-progress edit without user interaction, then unfocuses the active input control.
	/// </summary>
	/// <remarks>
	///		The commit happens synchronously, before the unfocus is requested. WinUI raises
	///		<see cref="VisualElement.Unfocused"/> on a later tick, so a caller that unfocused and
	///		waited for the event would see the commit land after whatever it did next — long enough
	///		for the caller to have moved on to a different entity and for the edit to be saved
	///		against the wrong one, or dropped entirely. Callers of this method expect the value to
	///		be committed by the time it returns.
	/// </remarks>
	public void TryCommit()
	{
		if (!_isEditing)
		{
			return;
		}

		// The pending Unfocused must not commit a second time for the same edit, and a second
		// TryCommit before that event arrives must be a no-op rather than a repeat commit.
		// The order is load-bearing and unchanged: the latch is raised BEFORE the edit session
		// ends, so nothing between the two can see a finished session with the latch still down.
		_suppressUnfocusCommit = true;
		EndEditing();

		if (!AcceptsReturn)
		{
			Commit(EditEntry.Text);
			EditEntry.Unfocus();
		}
		else
		{
			Commit(EditEditor.Text);
			EditEditor.Unfocus();
		}
	}

	private static void ShowInput(VisualElement input)
	{
		input.Opacity = 1;
		input.InputTransparent = false;
	}

	private static void HideInput(VisualElement input)
	{
		input.Opacity = 0;
		input.InputTransparent = true;
	}

	private void ApplyTextAlignment(TextAlignment alignment)
	{
		DisplayLabel.HorizontalTextAlignment = alignment;
		EditEntry.HorizontalTextAlignment = alignment;
		EditEditor.HorizontalTextAlignment = alignment;
	}

	// The single owner of DisplayLabel's appearance. All three facets are derived from the same
	// fact — whether the value is empty — and are computed here rather than stored anywhere. The
	// label carries no TextColor binding: an imperative write wins under MAUI value precedence, so
	// a binding beside this method would be silently clobbered on the first call. TextColor and
	// PlaceholderColor both call back into here when they change, which is what the binding gave.
	private void SyncDisplay(string? value)
	{
		var isEmpty = string.IsNullOrWhiteSpace(value);
		DisplayLabel.Text = isEmpty ? Placeholder : value;
		DisplayLabel.TextColor = isEmpty ? PlaceholderColor : TextColor;
		DisplayLabel.FontAttributes = isEmpty ? FontAttributes.Italic : FontAttributes.None;
	}

	private void OnLabelTapped(object? sender, TappedEventArgs e)
	{
		LabelTapped?.Invoke(this, EventArgs.Empty);
		ActivateEditor();
	}

	private void OnPointerEntered(object? sender, PointerEventArgs e)
	{
		if (!_isEditing)
		{
			ApplyFocusGlow();
		}
	}

	private void OnPointerExited(object? sender, PointerEventArgs e)
	{
		if (!_isEditing)
		{
			ClearFocusGlow();
		}
	}

	private void OnInputFocused(object? sender, FocusEventArgs e)
	{
		BeginEditing();
		EditingStarted?.Invoke(this, EventArgs.Empty);
		ApplyFocusGlow();
	}

	// BeginEditing/EndEditing own the edit-session flag; ApplyFocusGlow/ClearFocusGlow own the
	// painting. The two concerns used to be fused into ClearFocusGlow, which cleared _isEditing
	// as its first statement while ApplyFocusGlow did not set it — an asymmetric pair in which a
	// method named for painting silently ended the edit session. Every caller that needs both
	// now asks for both, in the order the fused version used: state first, then paint.
	private void BeginEditing()
	{
		// Clearing the latch on focus means a swallowed Unfocused cannot leave it set and eat a
		// later genuine commit. This is the "cleared ... the next time the user starts editing"
		// half of the contract stated on _suppressUnfocusCommit.
		_isEditing = true;
		_suppressUnfocusCommit = false;
	}

	// The counterpart to BeginEditing. It does NOT touch _suppressUnfocusCommit: that latch is
	// consumed by TakeSuppressedCommit on the Unfocused it belongs to, and clearing it here would
	// race the unfocus handlers that read it after ending the session.
	private void EndEditing() =>
		_isEditing = false;

	// The hover glow and the focus glow are the same treatment; they were written out twice and
	// had to be kept in step by hand. ClearFocusGlow is the other half of this pair.
	private void ApplyFocusGlow()
	{
		var gold = DmTheme.Gold;
		FocusBorder.Stroke = gold;
		FocusBorder.BackgroundColor = DmTheme.EditorGlowTint;
#if !WINDOWS
		FocusBorder.Shadow = new Shadow
		{
			Brush = new SolidColorBrush(gold),
			Offset = new Point(0, 0),
			Radius = 10,
			Opacity = 0.4f
		};
#endif
	}

	// Entry.Completed fires when the user presses Enter — treat as commit.
	private void OnEntryCompleted(object? sender, EventArgs e) =>
		EditEntry.Unfocus();

	private void OnEntryUnfocused(object? sender, FocusEventArgs e)
	{
		// State first, then paint — the same order the fused ClearFocusGlow used, and it matters:
		// TakeSuppressedCommit below runs after the session has ended, exactly as before.
		EndEditing();
		ClearFocusGlow();

		if (!TakeSuppressedCommit())
		{
			Commit(EditEntry.Text);
		}

		HideInput(EditEntry);
		DisplayLabel.Opacity = 1;
		DisplayLabel.InputTransparent = false;
		EditingEnded?.Invoke(this, EventArgs.Empty);
	}

	private void OnEditorUnfocused(object? sender, FocusEventArgs e)
	{
		// State first, then paint, as in OnEntryUnfocused above.
		EndEditing();
		ClearFocusGlow();

		if (!TakeSuppressedCommit())
		{
			Commit(EditEditor.Text);
		}

		HideInput(EditEditor);
		DisplayLabel.Opacity = 1;
		DisplayLabel.InputTransparent = false;
		EditingEnded?.Invoke(this, EventArgs.Empty);
	}

	// Returns true when this unfocus belongs to a TryCommit that already committed, consuming
	// the flag so only the one pending Unfocused is skipped.
	private bool TakeSuppressedCommit()
	{
		if (!_suppressUnfocusCommit)
		{
			return false;
		}

		_suppressUnfocusCommit = false;
		return true;
	}

	private void Commit(string? value)
	{
		// Exactly one SyncDisplay runs per commit, on either branch. Assigning Text raises
		// TextProperty's propertyChanged, which syncs the display — but MAUI does not raise it for
		// an assignment equal to the current value, so a commit that changed nothing would leave
		// the display unsynced. That no-op case is the one the explicit call here covers; calling
		// it on both paths, as this method used to, syncs twice on every real edit.
		if (string.Equals(Text, value, StringComparison.Ordinal))
		{
			SyncDisplay(value);
		}
		else
		{
			Text = value;
		}

		TextCommitted?.Invoke(this, value);
	}

	// Painting only, and the exact counterpart of ApplyFocusGlow. Callers that also need the edit
	// session to end call EndEditing first; OnPointerExited deliberately does not, because losing
	// the pointer is not the end of an edit.
	private void ClearFocusGlow()
	{
		FocusBorder.Stroke = Colors.Transparent;
		FocusBorder.ClearValue(VisualElement.BackgroundColorProperty);
#if !WINDOWS
		FocusBorder.ClearValue(VisualElement.ShadowProperty);
#endif
	}
}
