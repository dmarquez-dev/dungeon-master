# DungeonMaster.Tests

This README states the conventions a new test file in this project follows. They are rules, not a
description of the code as it stands.

## Test method naming (MASTER-144)

The naming scheme is chosen by the directory a file is created in. A new test file follows the scheme
of the directory it lives in, whatever the files beside it look like.

### `App/` -- sentence form

A test method under `App/` is named as a declarative sentence in PascalCase that states the condition
and the outcome, with **no underscore**. For example:

```csharp
public void NoRawColourLiteralsOutsideTheThemeDictionaries()
```

These facts assert over the app's source files and its theme, and usually have no single method under
test, so there is no member name to put first. The name reads as the property that holds. There is
no exception for a fact that exercises a helper method directly: it is a sentence too.

### `AppCore/`, `Application/`, `Domain/`, `Infrastructure/` -- `Method_Scenario_ExpectedBehaviour`

A test method in these directories is named `Method_Scenario_ExpectedBehaviour`, with the parts
separated by underscores:

- the first part names the member or type under test;
- the scenario part states the distinguishing condition, and is omitted when there is none
  (`Method_ExpectedBehaviour`);
- the last part states the expected outcome.

For example:

```csharp
public async Task HandleAsync_WithUnknownObjectId_ReturnsNotFoundAndDoesNotSave()
```

In either scheme, the name claims no more than the test asserts.

## XML documentation (MASTER-143)

`dm-conventions` `backend/dotnet-format.md` section 9 states:

> All public types, members, and parameters must have XML documentation comments.

**Test classes in this project are exempt from that rule.** A test class here is a class that
declares `[Fact]` or `[Theory]` methods; the exemption covers the class and its members. The decision
is recorded in register row `MASTER-143`.

Documentation that already exists on a test class is kept, and new documentation is welcome,
especially a comment that explains why a fact exists or what defect it guards against.

The exemption names test classes only. It does not extend to the non-test types in this project,
such as fixtures, scanners and readers.

## Arrange / Act / Assert (MASTER-137)

Test methods under `AppCore/`, `Application/`, `Domain/` and `Infrastructure/` are laid out in the
shape shown in `dm-conventions` `backend/dotnet-format.md` section 12:

- The body is divided by the comment markers `// Arrange`, `// Act` and `// Assert`, in that order.
- Each marker after the first in the body is preceded by a blank line.
- `// Arrange` is omitted when the test has no setup; the body then starts at `// Act`.
- A body that is a single statement needs no markers.

An illustrative sketch (the names are placeholders, not a test in this project):

```csharp
[Fact]
public async Task Handle_WithValidCommand_ReturnsSuccess()
{
	// Arrange
	var command = new SomeCommand("name");

	// Act
	var result = await _handler.Handle(command, CancellationToken.None);

	// Assert
	result.IsSuccess.Should().BeTrue();
}
```

The scan facts under `App/` are not bound by this section. They assert over source files rather than
arranging, acting on and asserting about a unit, so the markers would describe nothing.
