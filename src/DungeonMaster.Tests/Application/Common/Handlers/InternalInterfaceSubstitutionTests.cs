using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Common.Handlers;

/// <summary>
///		An internal interface that exists only so <see cref="InternalInterfaceSubstitutionTests"/> has an internal type to substitute. No product code declares one, so without it the guard below would have nothing to guard.
/// </summary>
internal interface IInternalSubstitutionProbe
{
	/// <summary>
	///		Gets a value the guard configures and reads back, so the substitute is exercised rather than merely constructed.
	/// </summary>
	int Value { get; }
}

/// <summary>
///		Guards <c>[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]</c> in <c>AssemblyInfo.cs</c>.
/// </summary>
public sealed class InternalInterfaceSubstitutionTests
{
	/// <summary>
	///		Verifies that NSubstitute can proxy an internal interface, which it can only do while Castle DynamicProxy's generated assembly is a friend of this one.
	/// </summary>
	[Fact]
	public void SubstituteFor_OnAnInternalInterface_SucceedsBecauseDynamicProxyGenAssembly2IsAFriendAssembly()
	{
		// Arrange
		var probe = Substitute.For<IInternalSubstitutionProbe>();

		// Act
		probe.Value.Returns(7);

		// Assert
		probe.Value.Should().Be(7);
	}
}
