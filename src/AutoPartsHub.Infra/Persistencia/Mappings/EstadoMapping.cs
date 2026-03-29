using AutoPartsHub.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsHub.Infra.Persistencia.Mappings;

public sealed class EstadoMapping : IEntityTypeConfiguration<Estado>
{
    public void Configure(EntityTypeBuilder<Estado> builder)
    {
        builder.ToTable("estados");

        builder.HasKey(e => e.CodigoUf);
        builder.Property(e => e.CodigoUf).HasColumnName("codigo_uf").ValueGeneratedNever();
        builder.Property(e => e.Uf).HasColumnName("uf").HasMaxLength(2);
        builder.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100);
        builder.Property(e => e.Latitude).HasColumnName("latitude");
        builder.Property(e => e.Longitude).HasColumnName("longitude");
        builder.Property(e => e.Regiao).HasColumnName("regiao").HasMaxLength(12);
    }
}
