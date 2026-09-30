namespace DungeonMaster.Domain.ValueObjects;

/// <summary>
///     Represents a position in canvas-space pixels. Used for placed object positions,
///     stored relative to the containing area's origin vertex.
/// </summary>
/// <remarks>
///		<para>
///			Both coordinates must be finite (MASTER-081 / CORE-016). A <see cref="float.NaN"/> or an infinity here is a
///			defect, not an expected failure: no user action produces one and no caller has a sensible recovery, so the
///			type throws rather than returning a result value the caller would have to handle. That follows
///			<c>backend.md</c> section Error handling — "Distinguish expected failure from defect ... a defect
///			terminates the operation".
///		</para>
///		<para>
///			The guard exists because the store is not one. <c>CanvasPointNonFinitePersistenceTests.cs</c> (T1) pinned
///			what a <c>REAL NOT NULL</c> SQLite column actually does with each of the three non-finite values, and they
///			do not behave alike: a <see cref="float.NaN"/> throws on save — a <c>DbUpdateException</c> wrapping
///			<c>InvalidOperationException("Cannot store 'NaN' values.")</c> — while <see cref="float.PositiveInfinity"/>
///			and <see cref="float.NegativeInfinity"/> round-trip intact, saved and reloaded unchanged. The
///			<see cref="float.NaN"/> rejection is the ADO provider (<c>Microsoft.Data.Sqlite</c>) refusing to bind a
///			<see cref="float.NaN"/> parameter: it is not a SQLite constraint and not the <c>IsRequired()</c> mapping.
///			The <c>REAL</c> column type is nonetheless why EF attempts a floating-point parameter bind at all, so it is
///			not irrelevant — a TEXT mapping would store <see cref="float.NaN"/> without complaint. So the store stops
///			one of three cases, at commit time, downstream of the boundary that accepted the value.
///		</para>
///		<para>
///			One consequence of that asymmetry bounds what a materialization test can cover:
///			<see cref="float.NaN"/> specifically cannot reach materialization, because it cannot be written in the
///			first place. <c>CanvasPointGuardMaterializationTests.cs</c> therefore writes an infinity through raw SQL,
///			not a <see cref="float.NaN"/>, as <c>CanvasPointNonFinitePersistenceTests.cs</c> requires.
///		</para>
/// </remarks>
/// <param name="X">Horizontal pixel offset from the area origin. Must be finite.</param>
/// <param name="Y">Vertical pixel offset from the area origin. Must be finite.</param>
/// <exception cref="ArgumentOutOfRangeException">
///		<paramref name="X"/> or <paramref name="Y"/> is <see cref="float.NaN"/>,
///		<see cref="float.PositiveInfinity"/> or <see cref="float.NegativeInfinity"/>. The
///		<see cref="ArgumentException.ParamName"/> is the offending coordinate and the
///		<see cref="ArgumentOutOfRangeException.ActualValue"/> is the value that was rejected.
/// </exception>
public record CanvasPoint(float X, float Y)
{
	// The initializers are what the primary constructor runs, and they are not optional: declaring X and Y
	// explicitly below stops the compiler from emitting the positional assignment, so without these two lines the
	// parameters would be dropped on the floor (CS8907) and every point would read back as 0. The copy constructor
	// a with-expression uses copies these fields directly and runs neither initializer, which is why the guard also
	// lives in the init accessors.
	private readonly float x = Finite(X, nameof(X));
	private readonly float y = Finite(Y, nameof(Y));

	/// <summary>
	///		Horizontal pixel offset from the area origin.
	/// </summary>
	/// <remarks>
	///		Declared with an explicit <see langword="init"/> accessor rather than as a property initializer of the shape
	///		<c>PathSegment.cs:35</c> uses. That distinction is the whole point: a record's compiler-generated copy
	///		constructor assigns the backing fields directly and never re-runs a property initializer, so an initializer
	///		guards the constructor and leaves <c>with</c>-expressions open — the carried MASTER-170 gap that
	///		<c>AreaOutlineGeometry.Translate</c> walks through. An <see langword="init"/> accessor runs on both paths,
	///		because a <c>with</c>-expression assigns each named property through its accessor after the copy.
	/// </remarks>
	public float X
	{
		get => x;
		init => x = Finite(value, nameof(X));
	}

	/// <summary>
	///		Vertical pixel offset from the area origin.
	/// </summary>
	/// <remarks>
	///		Guarded through an <see langword="init"/> accessor for the reason given on <see cref="X"/>.
	/// </remarks>
	public float Y
	{
		get => y;
		init => y = Finite(value, nameof(Y));
	}

	/// <summary>
	///		Returns <paramref name="value"/> when it is finite, and throws otherwise.
	/// </summary>
	/// <param name="value">The coordinate being assigned.</param>
	/// <param name="paramName">
	///		The positional name of the coordinate, so that the reported
	///		<see cref="ArgumentException.ParamName"/> is the name a caller wrote.
	/// </param>
	/// <returns>
	///		<paramref name="value"/>, unchanged.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	///		<paramref name="value"/> is not finite.
	/// </exception>
	private static float Finite(float value, string paramName)
	{
		// float.IsFinite is the whole predicate: it is false for NaN and both infinities and true for every other
		// float, float.MaxValue and float.MinValue included. A magnitude test would reject those two, which are
		// legitimate coordinates.
		if (!float.IsFinite(value))
		{
			throw new ArgumentOutOfRangeException(
				paramName,
				value,
				"A canvas coordinate must be finite.");
		}

		return value;
	}
}
