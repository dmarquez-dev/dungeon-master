using DungeonMaster.App.Views.DungeonDetail;
using Plugin.Maui.KeyListener;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Executes <see cref="KeyChord"/> directly. The predicate derives from nothing and touches no MAUI type, so it runs without an application host, as <see cref="DrawSessionResultGuardTests"/> already does.
/// </summary>
/// <remarks>
///		The modifier members are enumerated with <see cref="Enum.GetValues{TEnum}()"/> rather than hand-listed, so a member the plugin adds later is covered without editing this file.
/// </remarks>
public class KeyChordTests
{
	/// <summary>
	///		Every declared modifier member other than <see cref="KeyboardModifiers.None"/> and <see cref="KeyboardModifiers.Control"/>.
	/// </summary>
	public static TheoryData<KeyboardModifiers> ModifiersOtherThanNoneAndControl()
	{
		var data = new TheoryData<KeyboardModifiers>();
		foreach (var modifier in Enum.GetValues<KeyboardModifiers>())
		{
			if (modifier != KeyboardModifiers.None && modifier != KeyboardModifiers.Control)
			{
				data.Add(modifier);
			}
		}

		return data;
	}

	/// <summary>
	///		Every declared modifier member other than <see cref="KeyboardModifiers.Control"/>, <see cref="KeyboardModifiers.None"/> included.
	/// </summary>
	public static TheoryData<KeyboardModifiers> ModifiersOtherThanControl()
	{
		var data = new TheoryData<KeyboardModifiers>();
		foreach (var modifier in Enum.GetValues<KeyboardModifiers>())
		{
			if (modifier != KeyboardModifiers.Control)
			{
				data.Add(modifier);
			}
		}

		return data;
	}

	/// <summary>
	///		Guards the combination theory below against passing vacuously on an empty member set.
	/// </summary>
	[Fact]
	public void DeclaredModifiers_ExcludingNoneAndControl_AreNotEmpty()
	{
		Assert.NotEmpty(ModifiersOtherThanNoneAndControl());
	}

	/// <summary>
	///		Control held together with any one other declared modifier is not the Control chord.
	/// </summary>
	[Theory]
	[MemberData(nameof(ModifiersOtherThanNoneAndControl))]
	public void IsControlOnly_WithControlCombinedWithAnotherModifier_ReturnsFalse(
		KeyboardModifiers other)
	{
		var accepted = KeyChord.IsControlOnly(KeyboardModifiers.Control | other);

		Assert.False(accepted);
	}

	/// <summary>
	///		Control held together with two other modifiers is not the Control chord.
	/// </summary>
	[Fact]
	public void IsControlOnly_WithControlShiftAndAlt_ReturnsFalse()
	{
		var accepted = KeyChord.IsControlOnly(KeyboardModifiers.Control | KeyboardModifiers.Shift | KeyboardModifiers.Alt);

		Assert.False(accepted);
	}

	/// <summary>
	///		Positive control: Control alone is the chord.
	/// </summary>
	[Fact]
	public void IsControlOnly_WithControlAlone_ReturnsTrue()
	{
		var accepted = KeyChord.IsControlOnly(KeyboardModifiers.Control);

		Assert.True(accepted);
	}

	/// <summary>
	///		Control: no modifier, or any single declared modifier other than Control, is not the chord.
	/// </summary>
	[Theory]
	[MemberData(nameof(ModifiersOtherThanControl))]
	public void IsControlOnly_WithASingleModifierOtherThanControl_ReturnsFalse(
		KeyboardModifiers modifier)
	{
		var accepted = KeyChord.IsControlOnly(modifier);

		Assert.False(accepted);
	}
}
