using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LoginTamplate.Model;

public partial class CoinCheckContext : DbContext
{
    public CoinCheckContext()
    {
    }

    public CoinCheckContext(DbContextOptions<CoinCheckContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Abbonamenti> Abbonamentis { get; set; }

    public virtual DbSet<AcquistiAbbonamenti> AcquistiAbbonamentis { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Criptovalute> Criptovalutes { get; set; }

    public virtual DbSet<LogAttivitum> LogAttivita { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PreferenzeUtente> PreferenzeUtentes { get; set; }

    public virtual DbSet<Ruoli> Ruolis { get; set; }

    public virtual DbSet<Utenti> Utentis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=FRELSERPC\\SQLEXPRESS;Database=CoinCheck;TrustServerCertificate=true;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Abbonamenti>(entity =>
        {
            entity.HasKey(e => e.Idprodotto).HasName("PK__Abboname__EE66A6EE1D382A9F");

            entity.ToTable("Abbonamenti");

            entity.Property(e => e.Idprodotto).HasColumnName("IDProdotto");
            entity.Property(e => e.Descrizione).HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Prezzo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Quantita).HasDefaultValue(1);
            entity.Property(e => e.TipoAbbonamento)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AcquistiAbbonamenti>(entity =>
        {
            entity.HasKey(e => e.AcquistoId).HasName("PK__Acquisti__4F7119543471D2B3");

            entity.ToTable("AcquistiAbbonamenti");

            entity.Property(e => e.AcquistoId).HasColumnName("AcquistoID");
            entity.Property(e => e.DataAcquisto).HasColumnType("datetime");
            entity.Property(e => e.DataScadenza).HasColumnType("datetime");
            entity.Property(e => e.Idprodotto).HasColumnName("IDProdotto");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.IdprodottoNavigation).WithMany(p => p.AcquistiAbbonamentis)
                .HasForeignKey(d => d.Idprodotto)
                .HasConstraintName("FK__AcquistiA__IDPro__03F0984C");

            entity.HasOne(d => d.User).WithMany(p => p.AcquistiAbbonamentis)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__AcquistiA__UserI__02FC7413");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCADCDBF4D7");

            entity.Property(e => e.CommentDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__PostId__4F7CD00D");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__UserId__5070F446");
        });

        modelBuilder.Entity<Criptovalute>(entity =>
        {
            entity.HasKey(e => e.CriptoId).HasName("PK__Criptova__C68EEA5AD5894E37");

            entity.ToTable("Criptovalute");

            entity.Property(e => e.CriptoId).HasColumnName("CriptoID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MarketCap).HasColumnType("decimal(25, 2)");
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.Property(e => e.PrezzoUsd)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("PrezzoUSD");
            entity.Property(e => e.Simbolo).HasMaxLength(10);
            entity.Property(e => e.UltimoAggiornamento).HasColumnType("datetime");
            entity.Property(e => e.Variazione24h).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Volume24h).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<LogAttivitum>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__LogAttiv__5E5499A85D289924");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Timestamp).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.LogAttivita)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LogAttivi__UserI__47DBAE45");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Posts__AA1260185D73DA66");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PostDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Posts__UserId__4BAC3F29");
        });

        modelBuilder.Entity<PreferenzeUtente>(entity =>
        {
            entity.HasKey(e => e.PreferenzaId).HasName("PK__Preferen__C408672F1119D344");

            entity.ToTable("PreferenzeUtente");

            entity.Property(e => e.PreferenzaId).HasColumnName("PreferenzaID");
            entity.Property(e => e.CriptoId).HasColumnName("CriptoID");
            entity.Property(e => e.SogliaPrezzo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Cripto).WithMany(p => p.PreferenzeUtentes)
                .HasForeignKey(d => d.CriptoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PreferenzeUtente_Criptovalute");

            entity.HasOne(d => d.User).WithMany(p => p.PreferenzeUtentes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PreferenzeUtente_Utenti");
        });

        modelBuilder.Entity<Ruoli>(entity =>
        {
            entity.HasKey(e => e.RuoloId).HasName("PK__Ruoli__AD88FD7A79E1F3E8");

            entity.ToTable("Ruoli");

            entity.HasIndex(e => e.NomeRuolo, "UQ__Ruoli__5663F8FC3142396B").IsUnique();

            entity.Property(e => e.RuoloId).HasColumnName("RuoloID");
            entity.Property(e => e.NomeRuolo).HasMaxLength(50);
        });

        modelBuilder.Entity<Utenti>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Utenti__1788CCACD9F28BA6");

            entity.ToTable("Utenti");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Idabbonamento).HasColumnName("IDAbbonamento");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RuoloId).HasColumnName("RuoloID");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.IdabbonamentoNavigation).WithMany(p => p.Utentis)
                .HasForeignKey(d => d.Idabbonamento)
                .HasConstraintName("FK__Utenti__IDAbbona__75A278F5");

            entity.HasOne(d => d.Ruolo).WithMany(p => p.Utentis)
                .HasForeignKey(d => d.RuoloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Utenti__RuoloID__3E52440B");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
