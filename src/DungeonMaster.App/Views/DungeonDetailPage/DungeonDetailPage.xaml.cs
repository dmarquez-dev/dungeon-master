using DungeonMaster.App.Controls;
using DungeonMaster.App.Extensions;
using DungeonMaster.App.Graphics;
using DungeonMaster.App.ViewModels;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using Microsoft.Maui.Controls.Shapes;

namespace DungeonMaster.App.Views;

/// <summary>
///		Displays the detail view for a single dungeon, including floors, areas, and the map placeholder.
///		Supports inline editing, floor reordering via drag-and-drop, and area management.
/// </summary>
public partial class DungeonDetailPage : ContentPage
{
	private readonly DungeonDetailViewModel _viewModel;
	private Guid? _selectedFloorId;
	private View? _dragPlaceholder;
	private FloorSidebarItem? _draggingItem;
	private FloorSidebarItem? _currentPlaceholderTarget;
	private bool _placeholderAfterTarget;

	/// <summary>
	///		Initializes a new instance of <see cref="DungeonDetailPage"/>, wires the map drawable,
	///		and subscribes to view model property and floor collection changes.
	/// </summary>
	/// <param name="viewModel">
	///		The view model that drives the dungeon detail view.
	/// </param>
	public DungeonDetailPage(DungeonDetailViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;

		// Wire map placeholder grid drawable
		if (this.FindByName<GraphicsView>("MapPlaceholderGraphics") is GraphicsView gv)
		{
			gv.Drawable = new MapGridDrawable();
		}

		viewModel.PropertyChanged += OnViewModelPropertyChanged;
		viewModel.Floors.CollectionChanged += OnFloorsChanged;
	}

	private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(DungeonDetailViewModel.Dungeon) && _viewModel.Dungeon is not null)
		{
			TitleBanner.Title = _viewModel.DungeonName;
			DescriptionEditor.Text = _viewModel.DungeonDescription;
			NotesEditor.Text = _viewModel.DungeonNotes;
		}
	}

	private void OnFloorsChanged(
		object? sender,
		System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
		RebuildFloorList();

	private void RebuildFloorList()
	{
		FloorListContainer.Children.Clear();

		foreach (var floor in _viewModel.Floors)
		{
			var item = new FloorSidebarItem { Floor = floor };

			item.DeleteRequested += async (_, floorToDelete) =>
			{
				var confirmed = await Navigation.ShowConfirmPopupAsync(
					"Delete Floor",
					$"Delete Floor {floorToDelete.Number}? All areas on this floor will be removed.",
					"DELETE",
					"CANCEL",
					isDestructive: true);

				if (confirmed)
				{
					await _viewModel.RemoveFloorCommand.ExecuteAsync(floorToDelete.Id);
				}
			};

			item.SelectRequested += (_, _) =>
			{
				_selectedFloorId = floor.Id;
				CollapseAllExcept(item);
			};

			item.ReorderRequested += async (_, args) =>
			{
				_selectedFloorId = args.DraggedFloorId;
				await _viewModel.ReorderFloorCommand.ExecuteAsync((args.DraggedFloorId, args.TargetNumber));
			};

			item.DragStarted += (_, _) =>
			{
				_draggingItem = item;
				item.Opacity = 0;
			};

			item.DragEnded += (_, _) =>
			{
				_draggingItem = null;
				item.Opacity = 1.0;
				ClearDragPlaceholder();
			};

			item.PlaceholderRequested += (_, _) =>
				ShowDragPlaceholder(item);

			item.PlaceholderCleared += (_, _) =>
				ClearDragPlaceholder();

			item.NameCommitted += async (_, name) =>
			{
				if (name != floor.Name)
				{
					await _viewModel.UpdateFloorCommand.ExecuteAsync((floor.Id, name));
				}
			};

			item.AddAreaRequested += async (_, _) =>
				await _viewModel.AddAreaCommand.ExecuteAsync(floor.Id);

			item.AreaDeleteRequested += async (_, area) =>
				await _viewModel.RemoveAreaCommand.ExecuteAsync((floor.Id, area.Id));

			item.AreaNameCommitted += async (_, args) =>
			{
				if (args.Name != args.Area.Name)
				{
					await _viewModel.UpdateAreaNameCommand.ExecuteAsync((floor.Id, args.Area.Id, args.Name));
				}
			};

			FloorListContainer.Children.Add(item);

			// Restore expanded state after any list rebuild (add area, reorder, etc.)
			if (floor.Id == _selectedFloorId)
			{
				item.Select();
			}
		}
	}

	private void OnContentAreaTapped(object? sender, TappedEventArgs e)
	{
		TitleBanner.TryCommit();
		DescriptionEditor.TryCommit();
		NotesEditor.TryCommit();
		CommitAllFloorEditors();
	}

	private void OnSidebarTapped(object? sender, TappedEventArgs e) =>
		CommitAllFloorEditors();

	private void CommitAllFloorEditors()
	{
		foreach (var child in FloorListContainer.Children)
		{
			if (child is FloorSidebarItem item)
			{
				item.TryCommitActiveEditor();
			}
		}
	}

	private void CollapseAllExcept(FloorSidebarItem except)
	{
		foreach (var child in FloorListContainer.Children)
		{
			if (child is FloorSidebarItem other && !ReferenceEquals(other, except))
			{
				other.Deselect();
			}
		}
	}

	private void ShowDragPlaceholder(FloorSidebarItem target)
	{
		// The dragging item is invisible — hovering over it causes oscillation. Skip it.
		if (ReferenceEquals(target, _draggingItem))
		{
			return;
		}

		// Compute source and target indices, skipping the placeholder so it doesn't skew counts.
		int sourceIdx = -1, targetIdx = -1, n = 0;

		foreach (var child in FloorListContainer.Children)
		{
			if (ReferenceEquals(child, _dragPlaceholder))
			{
				continue;
			}

			if (ReferenceEquals(child, _draggingItem))
			{
				sourceIdx = n;
			}

			if (ReferenceEquals(child, target))
			{
				targetIdx = n;
			}

			n++;
		}

		// Dragging down → placeholder after target; dragging up → before target.
		bool afterTarget = sourceIdx >= 0 && targetIdx >= 0 && sourceIdx < targetIdx;

		// Bail out if nothing changed — prevents stutter from rapid DragOver events.
		if (ReferenceEquals(_currentPlaceholderTarget, target) && _placeholderAfterTarget == afterTarget)
		{
			return;
		}

		_currentPlaceholderTarget = target;
		_placeholderAfterTarget   = afterTarget;

		if (_dragPlaceholder is not null)
		{
			FloorListContainer.Children.Remove(_dragPlaceholder);
			_dragPlaceholder = null;
		}

		var insertIdx = FloorListContainer.Children.IndexOf(target);

		if (insertIdx < 0)
		{
			return;
		}

		if (afterTarget)
		{
			insertIdx++;
		}

		var capturedTarget = target;
		var dropGesture = new DropGestureRecognizer();

		dropGesture.DragOver += (_, dragArgs) =>
			dragArgs.AcceptedOperation = DataPackageOperation.Copy;

		dropGesture.Drop += async (_, dropArgs) =>
		{
			ClearDragPlaceholder();

			if (capturedTarget.Floor is null)
			{
				return;
			}

			if (!dropArgs.Data.Properties.TryGetValue("FloorId", out var obj))
			{
				return;
			}

			if (obj is not string idStr || !Guid.TryParse(idStr, out var draggedId))
			{
				return;
			}

			if (draggedId == capturedTarget.Floor.Id)
			{
				return;
			}

			_selectedFloorId = draggedId;
			await _viewModel.ReorderFloorCommand.ExecuteAsync((draggedId, capturedTarget.Floor.Number));
		};

		_dragPlaceholder = new Border
		{
			HeightRequest = 44,
			Stroke = new SolidColorBrush(Color.FromArgb("#C8A040")),
			StrokeThickness = 2,
			StrokeShape = new RoundRectangle { CornerRadius = 3 },
			BackgroundColor = Color.FromArgb("#20C8A040"),
			Margin = new Thickness(0, 0, 0, 4)
		};
		_dragPlaceholder.GestureRecognizers.Add(dropGesture);
		FloorListContainer.Children.Insert(insertIdx, _dragPlaceholder);
	}

	private void ClearDragPlaceholder()
	{
		_currentPlaceholderTarget = null;

		if (_dragPlaceholder is null)
		{
			return;
		}

		FloorListContainer.Children.Remove(_dragPlaceholder);
		_dragPlaceholder = null;
	}

	private void OnBackClicked(object? sender, EventArgs e) =>
		Shell.Current.GoToAsync("..");

	private async void OnAddFloorClicked(object? sender, EventArgs e)
	{
		await _viewModel.AddFloorCommand.ExecuteAsync(null);

		// Defer focus slightly to allow the floor list to rebuild before activating the editor
		Dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(80),
			() =>
			{
				if (FloorListContainer.Children.LastOrDefault() is FloorSidebarItem lastItem)
				{
					lastItem.FocusNameEditor();
				}
			});
	}

	private async void OnDungeonNameCommitted(object? sender, string? name)
	{
		if (!string.IsNullOrWhiteSpace(name) && name != _viewModel.DungeonName)
		{
			_viewModel.DungeonName = name;
			await _viewModel.SaveDungeonCommand.ExecuteAsync(null);
		}
	}

	private async void OnDescriptionCommitted(object? sender, string? value)
	{
		if (value != _viewModel.DungeonDescription)
		{
			_viewModel.DungeonDescription = value;
			await _viewModel.SaveDungeonCommand.ExecuteAsync(null);
		}
	}

	private async void OnNotesCommitted(object? sender, string? value)
	{
		if (value != _viewModel.DungeonNotes)
		{
			_viewModel.DungeonNotes = value;
			await _viewModel.SaveDungeonCommand.ExecuteAsync(null);
		}
	}
}
