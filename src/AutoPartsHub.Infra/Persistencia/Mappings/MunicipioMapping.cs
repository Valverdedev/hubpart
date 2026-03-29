using AutoPartsHub.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoPartsHub.Infra.Persistencia.Mappings;

public sealed class MunicipioMapping : IEntityTypeConfiguration<Municipio>
{
    public void Configure(EntityTypeBuilder<Municipio> builder)
    {
        builder.ToTable("municipios");

        builder.HasKey(m => m.CodigoIbge);
        builder.Property(m => m.CodigoIbge).HasColumnName("codigo_ibge").ValueGeneratedNever();
        builder.Property(m => m.Nome).HasColumnName("nome").HasMaxLength(100);
        builder.Property(m => m.Latitude).HasColumnName("latitude");
        builder.Property(m => m.Longitude).HasColumnName("longitude");
        builder.Property(m => m.Capital).HasColumnName("capital");
        builder.Property(m => m.CodigoUf).HasColumnName("codigo_uf");
        builder.Property(m => m.SiafiId).HasColumnName("siafi_id").HasMaxLength(4);
        builder.Property(m => m.Ddd).HasColumnName("ddd");
        builder.Property(m => m.FusoHorario).HasColumnName("fuso_horario").HasMaxLength(32);

        builder.HasOne<Estado>()
            .WithMany()
            .HasForeignKey(m => m.CodigoUf)
            .HasPrincipalKey(e => e.CodigoUf)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
