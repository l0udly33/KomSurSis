using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ComputerBuildSystem.Models;

public partial class PsaContext : DbContext
{
    public PsaContext()
    {
    }

    public PsaContext(DbContextOptions<PsaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartElement> CartElements { get; set; }

    public virtual DbSet<Case> Cases { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ComputerPart> ComputerParts { get; set; }

    public virtual DbSet<Cpu> Cpus { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameSpecification> GameSpecifications { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Gpu> Gpus { get; set; }

    public virtual DbSet<HardDisk> HardDisks { get; set; }

    public virtual DbSet<Motherboard> Motherboards { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Psu> Psus { get; set; }

    public virtual DbSet<Ram> Rams { get; set; }

    public virtual DbSet<Specification> Specifications { get; set; }

    public virtual DbSet<SpecificationType> SpecificationTypes { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("server=localhost;port=3306;user=root;password=root;database=psa");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("admins");

            entity.Property(e => e.IdUser).HasColumnName("id_User");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("admins_ibfk_1");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("carts");

            entity.HasIndex(e => e.FkClient, "fk_Client").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.FkClient).HasColumnName("fk_Client");
            entity.Property(e => e.Sum).HasColumnName("sum");

            entity.HasOne(d => d.FkClientNavigation).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.FkClient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("carts_ibfk_1");
        });

        modelBuilder.Entity<CartElement>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.FkCart }).HasName("PRIMARY");

            entity.ToTable("cart_elements");

            entity.HasIndex(e => e.FkCart, "fk_Cart");

            entity.HasIndex(e => e.FkComputerPart, "fk_Computer_part");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.FkCart).HasColumnName("fk_Cart");
            entity.Property(e => e.FkComputerPart).HasColumnName("fk_Computer_part");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.FkCartNavigation).WithMany(p => p.CartElements)
                .HasForeignKey(d => d.FkCart)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cart_elements_ibfk_1");

            entity.HasOne(d => d.FkComputerPartNavigation).WithMany(p => p.CartElements)
                .HasForeignKey(d => d.FkComputerPart)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cart_elements_ibfk_2");
        });

        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cases");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(255)
                .HasColumnName("color");
            entity.Property(e => e.Dimensions)
                .HasMaxLength(255)
                .HasColumnName("dimensions");
            entity.Property(e => e.Standarts)
                .HasMaxLength(255)
                .HasColumnName("standarts");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Case)
                .HasForeignKey<Case>(d => d.Id)
                .HasConstraintName("cases_ibfk_1");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PRIMARY");

            entity.ToTable("clients");

            entity.Property(e => e.IdUser).HasColumnName("id_User");
            entity.Property(e => e.LoyaltyPoints).HasColumnName("loyalty_points");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Client)
                .HasForeignKey<Client>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clients_ibfk_1");
        });

        modelBuilder.Entity<ComputerPart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("computer_parts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Benchmark)
                .HasMaxLength(255)
                .HasColumnName("benchmark");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(255)
                .HasColumnName("manufacturer");
            entity.Property(e => e.Model)
                .HasMaxLength(255)
                .HasColumnName("model");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Cpu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cpu");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Connection)
                .HasMaxLength(255)
                .HasColumnName("connection");
            entity.Property(e => e.Cores).HasColumnName("cores");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.Memory)
                .HasMaxLength(255)
                .HasColumnName("memory");
            entity.Property(e => e.Power).HasColumnName("power");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Cpu)
                .HasForeignKey<Cpu>(d => d.Id)
                .HasConstraintName("cpu_ibfk_1");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("feedbacks");

            entity.HasIndex(e => e.FkOrder, "fk_Order").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.FkOrder).HasColumnName("fk_Order");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Text)
                .HasMaxLength(255)
                .HasColumnName("text");

            entity.HasOne(d => d.FkOrderNavigation).WithOne(p => p.Feedback)
                .HasForeignKey<Feedback>(d => d.FkOrder)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feedbacks_ibfk_1");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("games");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Developer)
                .HasMaxLength(255)
                .HasColumnName("developer");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ReleaseDate)
                .HasColumnType("date")
                .HasColumnName("release_date");
        });

        modelBuilder.Entity<GameSpecification>(entity =>
        {
            entity.HasKey(e => new { e.FkSpecifications, e.FkGame }).HasName("PRIMARY");

            entity.ToTable("game_specifications");

            entity.Property(e => e.FkSpecifications).HasColumnName("fk_Specifications");
            entity.Property(e => e.FkGame).HasColumnName("fk_Game");

            entity.HasOne(d => d.FkSpecificationsNavigation).WithMany(p => p.GameSpecifications)
                .HasForeignKey(d => d.FkSpecifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_specifications_ibfk_1");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("genres");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasMany(d => d.FkGames).WithMany(p => p.FkGenres)
                .UsingEntity<Dictionary<string, object>>(
                    "GenreGame",
                    r => r.HasOne<Game>().WithMany()
                        .HasForeignKey("FkGame")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("genre_games_ibfk_2"),
                    l => l.HasOne<Genre>().WithMany()
                        .HasForeignKey("FkGenre")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("genre_games_ibfk_1"),
                    j =>
                    {
                        j.HasKey("FkGenre", "FkGame").HasName("PRIMARY");
                        j.ToTable("genre_games");
                        j.HasIndex(new[] { "FkGame" }, "fk_Game");
                        j.IndexerProperty<int>("FkGenre").HasColumnName("fk_Genre");
                        j.IndexerProperty<int>("FkGame").HasColumnName("fk_Game");
                    });
        });

        modelBuilder.Entity<Gpu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("gpu");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Connection)
                .HasMaxLength(255)
                .HasColumnName("connection");
            entity.Property(e => e.Dimensions)
                .HasMaxLength(45)
                .HasDefaultValueSql("'300 x 120 x 60'")
                .HasColumnName("dimensions");
            entity.Property(e => e.Memory).HasColumnName("memory");
            entity.Property(e => e.MemoryFrequency).HasColumnName("memory_frequency");
            entity.Property(e => e.MemoryType)
                .HasMaxLength(255)
                .HasColumnName("memory_type");
            entity.Property(e => e.Power).HasColumnName("power");
            entity.Property(e => e.RamQuantity).HasColumnName("RAM_quantity");
            entity.Property(e => e.RamType)
                .HasMaxLength(255)
                .HasColumnName("RAM_type");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Gpu)
                .HasForeignKey<Gpu>(d => d.Id)
                .HasConstraintName("gpu_ibfk_1");
        });

        modelBuilder.Entity<HardDisk>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hard_disks");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Capacity)
                .HasMaxLength(255)
                .HasColumnName("capacity");
            entity.Property(e => e.Connection)
                .HasMaxLength(255)
                .HasColumnName("connection");
            entity.Property(e => e.ReadingSpeed).HasColumnName("reading_speed");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.WritingSpeed).HasColumnName("writing_speed");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.HardDisk)
                .HasForeignKey<HardDisk>(d => d.Id)
                .HasConstraintName("hard_disks_ibfk_1");
        });

        modelBuilder.Entity<Motherboard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("motherboards");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CpuSocket)
                .HasMaxLength(255)
                .HasColumnName("CPU_socket");
            entity.Property(e => e.GpuSocket)
                .HasMaxLength(255)
                .HasColumnName("GPU_socket");
            entity.Property(e => e.MaximumAmountOfMemory).HasColumnName("maximum_amount_of_memory");
            entity.Property(e => e.MaximumMemoryFrequency).HasColumnName("maximum_memory_frequency");
            entity.Property(e => e.MemoryConnection)
                .HasMaxLength(255)
                .HasColumnName("memory_connection");
            entity.Property(e => e.MemoryStandart)
                .HasMaxLength(255)
                .HasColumnName("memory_standart");
            entity.Property(e => e.SizeStandart)
                .HasMaxLength(255)
                .HasDefaultValueSql("'ATX'")
                .HasColumnName("size_standart");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Motherboard)
                .HasForeignKey<Motherboard>(d => d.Id)
                .HasConstraintName("motherboards_ibfk_1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.FkCart, "fk_Cart").IsUnique();

            entity.HasIndex(e => e.FkClient, "fk_Client");

            entity.HasIndex(e => e.FkState, "fk_State");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.FkCart).HasColumnName("fk_Cart");
            entity.Property(e => e.FkClient).HasColumnName("fk_Client");
            entity.Property(e => e.FkState).HasColumnName("fk_State");
            entity.Property(e => e.Sum).HasColumnName("sum");

            entity.HasOne(d => d.FkCartNavigation).WithOne(p => p.Order)
                .HasForeignKey<Order>(d => d.FkCart)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_ibfk_2");

            entity.HasOne(d => d.FkClientNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.FkClient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_ibfk_3");

            entity.HasOne(d => d.FkStateNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.FkState)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_ibfk_1");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payments");

            entity.HasIndex(e => e.FkOrder, "fk_Order").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.FkOrder).HasColumnName("fk_Order");
            entity.Property(e => e.Iban)
                .HasMaxLength(255)
                .HasColumnName("IBAN");
            entity.Property(e => e.Sum).HasColumnName("sum");

            entity.HasOne(d => d.FkOrderNavigation).WithOne(p => p.Payment)
                .HasForeignKey<Payment>(d => d.FkOrder)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payments_ibfk_1");
        });

        modelBuilder.Entity<Psu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("psu");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Power).HasColumnName("power");
            entity.Property(e => e.SizeStandart)
                .HasMaxLength(255)
                .HasColumnName("size_standart");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Psu)
                .HasForeignKey<Psu>(d => d.Id)
                .HasConstraintName("psu_ibfk_1");
        });

        modelBuilder.Entity<Ram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ram");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasDefaultValueSql("'16'")
                .HasColumnName("amount");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
            entity.Property(e => e.Voltage).HasColumnName("voltage");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Ram)
                .HasForeignKey<Ram>(d => d.Id)
                .HasConstraintName("ram_ibfk_1");
        });

        modelBuilder.Entity<Specification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("specifications");

            entity.HasIndex(e => e.FkPsu, "fk_PSU");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FkCpu).HasColumnName("fk_CPU");
            entity.Property(e => e.FkGpu).HasColumnName("fk_GPU");
            entity.Property(e => e.FkHardDisk).HasColumnName("fk_Hard_disk");
            entity.Property(e => e.FkPsu).HasColumnName("fk_PSU");
            entity.Property(e => e.FkSpecificationType).HasColumnName("fk_specification_type");

            entity.HasOne(d => d.FkPsuNavigation).WithMany(p => p.Specifications)
                .HasForeignKey(d => d.FkPsu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("specifications_ibfk_1");
        });

        modelBuilder.Entity<SpecificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("specification_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("states");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Surname)
                .HasMaxLength(255)
                .HasColumnName("surname");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
