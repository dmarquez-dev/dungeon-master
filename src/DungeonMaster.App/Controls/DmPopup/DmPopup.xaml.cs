using DungeonMaster.App.Theme;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed popup chrome control that wraps arbitrary body content inside a styled frame with a title header.
/// </summary>
/// <remarks>
///		FORMATTING IS OWNED HERE, not by each popup. Body inset, the gap between body sections and
///		the gap across a button row are three decisions every popup would otherwise re-make, and
///		two hosts had already drifted apart on the second of them. A popup author who sets none of
///		the three gets the shared values.
///		<para>
///		<see cref="ContentSpacing"/> is applied to the body only when the body is a stack layout
///		that has NOT set <c>Spacing</c> itself. An author who does set it keeps it — deliberately,
///		because silently overriding an attribute that is written in the markup would make the file
///		lie about what renders. An override is therefore visible in the diff and can be removed to
///		return the popup to the shared value.
///		</para>
///		<para>
///		<see cref="ButtonRowSpacing"/> cannot be applied from here: the button row sits at an
///		arbitrary depth inside content this control does not own. It is offered for the host to
///		bind, e.g. <c>ColumnSpacing="{Binding ButtonRowSpacing, Source={x:Reference Chrome}}"</c>.
///		</para>
/// </remarks>
public partial class DmPopup : ContentView
{
	/// <summary>
	///		Identifies the <see cref="Title"/> bindable property.
	/// </summary>
	public static readonly BindableProperty TitleProperty =
		BindableProperty.Create(
			nameof(Title),
			typeof(string),
			typeof(DmPopup),
			string.Empty);

	/// <summary>
	///		Identifies the <see cref="Body"/> bindable property.
	/// </summary>
	public static readonly BindableProperty BodyProperty =
		BindableProperty.Create(
			nameof(Body),
			typeof(View),
			typeof(DmPopup),
			null,
			propertyChanged: (bindable, _, newValue) =>
			{
				if (bindable is DmPopup popup && newValue is View view)
				{
					popup.BodyPresenter.Content = view;
					popup.AdoptBodySpacing(view);
				}
			});

	/// <summary>
	///		Identifies the <see cref="BodyPadding"/> bindable property.
	///		The default arrives through <c>defaultValueCreator</c> rather than as a constant
	///		because a constant is evaluated in this static field initialiser, which can run before
	///		<c>Application.Current.Resources</c> exists; the creator runs on first read of an
	///		instance. See the remarks on <see cref="DmTheme"/>.
	/// </summary>
	public static readonly BindableProperty BodyPaddingProperty =
		BindableProperty.Create(
			nameof(BodyPadding),
			typeof(Thickness),
			typeof(DmPopup),
			defaultValueCreator: _ =>
				DmTheme.PaddingPanel);

	/// <summary>
	///		Identifies the <see cref="ContentSpacing"/> bindable property.
	///		Resolved lazily for the reason given on <see cref="BodyPaddingProperty"/>.
	/// </summary>
	public static readonly BindableProperty ContentSpacingProperty =
		BindableProperty.Create(
			nameof(ContentSpacing),
			typeof(double),
			typeof(DmPopup),
			propertyChanged: (bindable, _, _) =>
				((DmPopup)bindable).ApplyContentSpacing(),
			defaultValueCreator: _ =>
				DmTheme.SpaceXL);

	/// <summary>
	///		Identifies the <see cref="ButtonRowSpacing"/> bindable property.
	///		Resolved lazily for the reason given on <see cref="BodyPaddingProperty"/>.
	/// </summary>
	public static readonly BindableProperty ButtonRowSpacingProperty =
		BindableProperty.Create(
			nameof(ButtonRowSpacing),
			typeof(double),
			typeof(DmPopup),
			defaultValueCreator: _ =>
				DmTheme.SpaceLarge);

	// True while the body's Spacing is this control's to set. Cleared when the body sets its own,
	// so a later ContentSpacing change cannot overwrite an author's explicit value.
	private bool _ownsBodySpacing;

	/// <summary>
	///		Gets or sets the heading text displayed in the popup title bar.
	/// </summary>
	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	/// <summary>
	///		Gets or sets the <see cref="View"/> rendered inside the popup body area.
	/// </summary>
	public View Body
	{
		get => (View)GetValue(BodyProperty);
		set => SetValue(BodyProperty, value);
	}

	/// <summary>
	///		Gets or sets the inset around the popup body. Defaults to the panel padding role,
	///		which is the same inset the title header uses.
	/// </summary>
	public Thickness BodyPadding
	{
		get => (Thickness)GetValue(BodyPaddingProperty);
		set => SetValue(BodyPaddingProperty, value);
	}

	/// <summary>
	///		Gets or sets the gap between the sections of the popup body.
	///		Applied to <see cref="Body"/> when it is a stack layout that has not set its own
	///		<c>Spacing</c>; see the remarks on this class.
	/// </summary>
	public double ContentSpacing
	{
		get => (double)GetValue(ContentSpacingProperty);
		set => SetValue(ContentSpacingProperty, value);
	}

	/// <summary>
	///		Gets or sets the gap between the buttons of the popup's action row.
	///		Offered for the host to bind; see the remarks on this class for why it cannot be
	///		applied from here.
	/// </summary>
	public double ButtonRowSpacing
	{
		get => (double)GetValue(ButtonRowSpacingProperty);
		set => SetValue(ButtonRowSpacingProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="DmPopup"/> and loads the XAML component tree.
	/// </summary>
	public DmPopup()
	{
		InitializeComponent();
	}

	private void AdoptBodySpacing(View body)
	{
		// IsSet answers "did anything assign this property", which for a body built in XAML is
		// exactly "did the author write a Spacing attribute" — the body is fully initialised
		// before it is handed to this property.
		_ownsBodySpacing =
			body is StackBase stack
			&& !stack.IsSet(StackBase.SpacingProperty);

		ApplyContentSpacing();
	}

	private void ApplyContentSpacing()
	{
		if (_ownsBodySpacing && BodyPresenter.Content is StackBase stack)
		{
			stack.Spacing = ContentSpacing;
		}
	}
}
