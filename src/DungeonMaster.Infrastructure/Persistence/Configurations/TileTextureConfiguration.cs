using DungeonMaster.Domain.Entities.Area;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DungeonMaster.Infrastructure.Persistence.Configurations;

/// <summary>
///		Configures the EF Core entity mapping for <see cref="TileTexture"/>.
/// </summary>
public sealed class TileTextureConfiguration
	: IEntityTypeConfiguration<TileTexture>
{
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<TileTexture> builder)
	{
		builder.ToTable("TileTextures");

		builder.HasKey(t => t.Id);

		builder.Property(t => t.Id)
			.ValueGeneratedNever();

		builder.Property(t => t.AreaId)
			.IsRequired();

		builder.Property(t => t.Kind)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(t => t.CreatedAt)
			.IsRequired();

		builder.Property(t => t.LastModified)
			.IsRequired();

		builder.OwnsOne(t => t.Position, position =>
		{
			position.Property(p => p.Col)
				.HasColumnName("Position_Col")
				.IsRequired();

			position.Property(p => p.Row)
				.HasColumnName("Position_Row")
				.IsRequired();
		});
	}
}
