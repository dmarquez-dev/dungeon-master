using MauiApplication = Microsoft.Maui.Controls.Application;

namespace DungeonMaster.App.Controls;

/// <summary>
///		Identifies which kind of entity a notebook is currently authoring metadata for.
/// </summary>
public enum NotebookKind
{
	/// <summary>The dungeon itself, which has no owning floor.</summary>
	Dungeon,

	/// <summary>A single floor of a dungeon.</summary>
	Floor,

	/// <summary>A single area on a floor.</summary>
	Area
}

/// <summary>
///		The entity a notebook has been opened on, together with the text it should display.
/// </summary>
/// <param name="Id">The identifier of the entity being authored.</param>
/// <param name="FloorId">
///		The owning floor identifier, or <see langword="null"/> for a dungeon-level notebook.
///		An area needs this to address the update command that saves it.
/// </param>
/// <param name="Kind">The kind of entity being authored.</param>
/// <param name="Name">The entity's display name.</param>
/// <param name="Description">The player-facing read-aloud text, or <see langword="null"/> when unset.</param>
/// <param name="Notes">The DM-only notes, or <see langword="null"/> when unset.</param>
public record NotebookTarget(
	Guid Id,
	Guid? FloorId,
	NotebookKind Kind,
	string Name,
	string? Description,
	string? Notes);

/// <summary>
///		The payload of a notebook autosave, describing which entity to update and with what text.
/// </summary>
/// <param name="Id">The identifier of the entity to update.</param>
/// <param name="FloorId">
///		The owning floor identifier, or <see langword="null"/> for a dungeon-level notebook.
/// </param>
/// <param name="Kind">The kind of entity to update.</param>
/// <param name="Name">The entity's name as edited.</param>
/// <param name="Description">The read-aloud text, or <see langword="null"/> when blank.</param>
/// <param name="Notes">The DM-only notes, or <see langword="null"/> when blank.</param>
public record NotebookSaveArgs(
	Guid Id,
	Guid? FloorId,
	NotebookKind Kind,
	string Name,
	string? Description,
	string? Notes);

/// <summary>
///		A draggable parchment notebook for authoring a dungeon, floor, or area's name,
///		read-aloud description, and DM-only notes.
/// </summary>
/// <remarks>
///		The notebook autosaves: typing marks it unsaved, a short pause commits, and unfocusing
///		an editor or closing the notebook commits immediately. Saving is therefore continuous
///		rather than an explicit action, and the footer stamp is the user's proof it happened.
/// </remarks>
public partial class NotebookView : ContentView
{
	/// <summary>Identifies which pane — and therefore which ribbon — is currently selected.</summary>
	private enum NotebookPane
	{
		Description,
		Notes
	}

	/// <summary>How long the user must pause before an edit is committed, in milliseconds.</summary>
	private const int DebounceMilliseconds = 900;

	/// <summary>Ribbon height when selected, in device-independent units.</summary>
	private const double SelectedRibbonHeight = 60;

	/// <summary>Ribbon height when not selected.</summary>
	private const double UnselectedRibbonHeight = 40;

	// The save stamp's two states. Green is not a themed token — it exists only here,
	// where "committed" needs to read as reassuring rather than as an action to take.
	private static readonly Color SavedColor = Color.FromArgb("#4A6A2A");
	private static readonly Color UnsavedColor = Color.FromArgb("#8B2020");

	/// <summary>
	///		Raised when the notebook has committed an edit and the host should persist it.
	/// </summary>
	public event EventHandler<NotebookSaveArgs>? SaveRequested;

	private NotebookTarget? _target;
	private CancellationTokenSource? _debounce;
	private bool _isDirty;
	private bool _isPopulating;
	private double _dragOriginX;
	private double _dragOriginY;

	/// <summary>
	///		Gets the identifier of the entity the notebook is currently open on,
	///		or <see langword="null"/> when the notebook is closed.
	/// </summary>
	/// <remarks>
	///		The host page uses this to close the notebook when its entity disappears after a reload.
	/// </remarks>
	public Guid? CurrentId => _target?.Id;

	/// <summary>
	///		Initializes a new instance of <see cref="NotebookView"/> and loads the XAML component tree.
	/// </summary>
	public NotebookView()
	{
		InitializeComponent();
		ApplyRibbonVisuals(NotebookPane.Description);
		SetSaveState(dirty: false);
	}

	/// <summary>
	///		Opens the notebook on the supplied target, replacing whatever it previously showed.
	/// </summary>
	/// <param name="target">The entity to author, and the text to seed the panes with.</param>
	/// <remarks>
	///		Any uncommitted edits to the previous target are committed first. Skipping that would
	///		write one entity's text into another the moment the user retargets mid-edit.
	/// </remarks>
	public void Open(NotebookTarget target)
	{
		ArgumentNullException.ThrowIfNull(target);

		// Order matters: flush the in-progress name edit, then commit the previous target,
		// and only then repoint at the new one.
		CancelPendingCommit();
		NameEditor.TryCommit();
		CommitIfDirty();

		_target = target;

		// Assigning Editor.Text raises TextChanged, which would otherwise mark the freshly
		// opened notebook dirty and schedule a save of text nobody has edited.
		_isPopulating = true;
		try
		{
			NameEditor.Text = target.Name;
			DescriptionPane.Text = target.Description ?? string.Empty;
			NotesPane.Text = target.Notes ?? string.Empty;
			KindLabel.Text = target.Kind.ToString().ToUpperInvariant();
		}
		finally
		{
			_isPopulating = false;
		}

		SelectRibbon(NotebookPane.Description);
		SetSaveState(dirty: false);
		IsVisible = true;
	}

	/// <summary>
	///		Commits any pending edit and hides the notebook.
	/// </summary>
	/// <remarks>
	///		Closing is one more save point, not the only one — the debounce and unfocus commits
	///		mean a crash or stray navigation costs the user nothing.
	/// </remarks>
	public void Close()
	{
		CancelPendingCommit();
		NameEditor.TryCommit();
		CommitIfDirty();

		_target = null;
		IsVisible = false;
	}

	private void OnNameCommitted(object? sender, string? e)
	{
		if (_isPopulating)
		{
			return;
		}

		// The inline editor only reports on commit, so there is nothing to debounce here.
		SetSaveState(dirty: true);
		CommitIfDirty();
	}

	private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
	{
		if (_isPopulating || _target is null)
		{
			return;
		}

		SetSaveState(dirty: true);

		var previous = _debounce;
		_debounce = new CancellationTokenSource();
		previous?.Cancel();
		previous?.Dispose();

		_ = CommitAfterDelayAsync(_debounce.Token);
	}

	private void OnEditorUnfocused(object? sender, FocusEventArgs e)
	{
		CancelPendingCommit();
		CommitIfDirty();
	}

	private async Task CommitAfterDelayAsync(CancellationToken ct)
	{
		try
		{
			await Task.Delay(DebounceMilliseconds, ct);
		}
		catch (TaskCanceledException)
		{
			return;
		}

		// The notebook may have been closed or retargeted during the delay.
		if (ct.IsCancellationRequested)
		{
			return;
		}

		CommitIfDirty();
	}

	private void CommitIfDirty()
	{
		if (!_isDirty || _target is null)
		{
			return;
		}

		SaveRequested?.Invoke(this, new NotebookSaveArgs(
			_target.Id,
			_target.FloorId,
			_target.Kind,
			NameEditor.Text ?? string.Empty,
			string.IsNullOrWhiteSpace(DescriptionPane.Text) ? null : DescriptionPane.Text,
			string.IsNullOrWhiteSpace(NotesPane.Text) ? null : NotesPane.Text));

		SetSaveState(dirty: false);
	}

	/// <summary>
	///		Cancels any scheduled commit so it cannot fire against a stale target.
	/// </summary>
	private void CancelPendingCommit()
	{
		_debounce?.Cancel();
		_debounce?.Dispose();
		_debounce = null;
	}

	private void SetSaveState(bool dirty)
	{
		_isDirty = dirty;
		SaveStateLabel.Text = dirty ? "UNSAVED" : "SAVED";
		SaveStateLabel.TextColor = dirty ? UnsavedColor : SavedColor;
	}

	private void OnReadRibbonTapped(object? sender, TappedEventArgs e) =>
		SelectRibbon(NotebookPane.Description);

	private void OnDmRibbonTapped(object? sender, TappedEventArgs e) =>
		SelectRibbon(NotebookPane.Notes);

	private void SelectRibbon(NotebookPane pane)
	{
		ApplyRibbonVisuals(pane);

		DescriptionPane.IsVisible = pane == NotebookPane.Description;
		NotesPane.IsVisible = pane == NotebookPane.Notes;
	}

	private void ApplyRibbonVisuals(NotebookPane pane)
	{
		var readSelected = pane == NotebookPane.Description;

		ReadRibbon.HeightRequest = readSelected ? SelectedRibbonHeight : UnselectedRibbonHeight;
		ReadRibbon.Background = RibbonBrush(readSelected ? "DMRibbonRead" : "DMRibbonReadDim");
		ReadRibbon.Opacity = readSelected ? 1 : 0.78;

		DmRibbon.HeightRequest = readSelected ? UnselectedRibbonHeight : SelectedRibbonHeight;
		DmRibbon.Background = RibbonBrush(readSelected ? "DMRibbonDmDim" : "DMRibbonDm");
		DmRibbon.Opacity = readSelected ? 0.78 : 1;
	}

	/// <summary>
	///		Builds the vertical fold gradient for a ribbon from its themed base colour.
	/// </summary>
	private static Brush RibbonBrush(string colorKey)
	{
		var top = (Color)MauiApplication.Current!.Resources[colorKey];

		return new LinearGradientBrush(
			[
				new GradientStop(top, 0f),
				new GradientStop(Darken(top, 0.72f), 1f)
			],
			new Point(0, 0),
			new Point(0, 1));
	}

	/// <summary>
	///		Mixes a colour toward black, matching the mock-up's <c>color-mix</c> fold shading.
	/// </summary>
	private static Color Darken(Color color, float factor) =>
		new(color.Red * factor, color.Green * factor, color.Blue * factor, color.Alpha);

	private void OnCloseClicked(object? sender, EventArgs e) =>
		Close();

	private void OnHeaderPanUpdated(object? sender, PanUpdatedEventArgs e)
	{
		switch (e.StatusType)
		{
			case GestureStatus.Started:
				_dragOriginX = TranslationX;
				_dragOriginY = TranslationY;
				break;

			case GestureStatus.Running:
				TranslationX = _dragOriginX + e.TotalX;
				TranslationY = _dragOriginY + e.TotalY;
				break;
		}
	}
}
