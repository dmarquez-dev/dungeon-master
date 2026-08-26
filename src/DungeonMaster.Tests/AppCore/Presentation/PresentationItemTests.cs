using DungeonMaster.App.Core.Presentation;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Presentation;

public class PresentationItemTests
{
	private static AreaItem Area(string? description, string? notes) =>
		new(Guid.NewGuid(), "Area", description, notes, []);

	private static FloorItem Floor(string? description, string? notes) =>
		new(Guid.NewGuid(), 1, "Floor", description, notes, [], []);

	[Fact]
	public void AreaItem_HasNotebookContent_IsFalseWhenDescriptionAndNotesAreBothNull()
	{
		Area(null, null).HasNotebookContent.Should().BeFalse();
	}

	[Fact]
	public void AreaItem_HasNotebookContent_IsFalseWhenDescriptionAndNotesAreBothWhitespace()
	{
		Area("   ", "\t").HasNotebookContent.Should().BeFalse();
	}

	[Fact]
	public void AreaItem_HasNotebookContent_IsTrueWhenOnlyDescriptionHasText()
	{
		Area("a description", null).HasNotebookContent.Should().BeTrue();
	}

	[Fact]
	public void AreaItem_HasNotebookContent_IsTrueWhenOnlyNotesHaveText()
	{
		Area(null, "some notes").HasNotebookContent.Should().BeTrue();
	}

	[Fact]
	public void AreaItem_HasNotebookContent_IsNotAConstructorParameter()
	{
		typeof(AreaItem)
			.GetConstructors()
			.SelectMany(constructor => constructor.GetParameters())
			.Select(parameter => parameter.Name)
			.Should().NotContain("HasNotebookContent");
	}

	[Fact]
	public void AreaItem_HasNotebookContent_TracksALaterEditRatherThanHoldingTheValueItWasBuiltWith()
	{
		var built = Area(null, null);
		var edited = built with { Description = "added later" };

		built.HasNotebookContent.Should().BeFalse();
		edited.HasNotebookContent.Should().BeTrue();
	}

	[Fact]
	public void FloorItem_HasNotebookContent_IsFalseWhenDescriptionAndNotesAreBothNull()
	{
		Floor(null, null).HasNotebookContent.Should().BeFalse();
	}

	[Fact]
	public void FloorItem_HasNotebookContent_IsFalseWhenDescriptionAndNotesAreBothWhitespace()
	{
		Floor(" ", "  ").HasNotebookContent.Should().BeFalse();
	}

	[Fact]
	public void FloorItem_HasNotebookContent_IsTrueWhenOnlyDescriptionHasText()
	{
		Floor("a description", null).HasNotebookContent.Should().BeTrue();
	}

	[Fact]
	public void FloorItem_HasNotebookContent_IsTrueWhenOnlyNotesHaveText()
	{
		Floor(null, "some notes").HasNotebookContent.Should().BeTrue();
	}

	[Fact]
	public void FloorItem_HasNotebookContent_IsNotAConstructorParameter()
	{
		typeof(FloorItem)
			.GetConstructors()
			.SelectMany(constructor => constructor.GetParameters())
			.Select(parameter => parameter.Name)
			.Should().NotContain("HasNotebookContent");
	}

	[Fact]
	public void FloorItem_HasNotebookContent_TracksALaterEditRatherThanHoldingTheValueItWasBuiltWith()
	{
		var built = Floor(null, null);
		var edited = built with { Notes = "added later" };

		built.HasNotebookContent.Should().BeFalse();
		edited.HasNotebookContent.Should().BeTrue();
	}

	[Fact]
	public void AreaItem_CarriesTheOutlineItWasBuiltWith()
	{
		var outline = new PathSegment[]
		{
			new(PathSegmentKind.Line, new TilePoint(1, 2), null, null),
		};

		var item = new AreaItem(Guid.NewGuid(), "Area", null, null, outline);

		item.Outline.Should().Equal(outline);
	}
}
