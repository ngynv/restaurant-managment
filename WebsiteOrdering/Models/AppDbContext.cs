using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteOrdering.ViewModels;

namespace WebsiteOrdering.Models
{
    public partial class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext()
        {

        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Monan> SanPhams { get; set; }
        public DbSet<Loaimonan> Category { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Listgiasize> ListGiaSizes { get; set; }
        public DbSet<Topping> Topping { get; set; }
        public DbSet<Chitietdonhang> ctdh { get; set; }
        public DbSet<Chitiettopping> cttopping { get; set; }
        public DbSet<Chinhanh> chinhanh { get; set; }
        public DbSet<Donhang> dhang { get; set; }
        public DbSet<Debanh> debanh { get; set; }
        public DbSet<Datban> Datbans { get; set; }
        public DbSet<Chitietdatban> chitietdatbans { get; set; }
        public DbSet<Ban> bans { get; set; }
        public DbSet<Banlock> Banlock { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public DbSet<MonAnGhepStats> MonAnGhepStats { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=pizza2;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Ban>(entity =>
            {
                entity.HasKey(e => e.Idban).HasName("PK__BAN__9367225E468C45A0");

                entity.ToTable("BAN");

                entity.Property(e => e.Idban)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDBAN");
                entity.Property(e => e.Idchinhanh)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDCHINHANH");
                entity.Property(e => e.Khuvuc)
                    .HasMaxLength(50)
                    .HasColumnName("KHUVUC");
                entity.Property(e => e.Songuoi).HasColumnName("SONGUOI");

                entity.Property(e => e.X).HasColumnName("X");
                entity.Property(e => e.Y).HasColumnName("Y");

                entity.Property(e => e.Tenban)
                    .HasMaxLength(500)
                    .HasColumnName("TENBAN");
                entity.Property(e => e.Trangthaiban)
                    .HasMaxLength(50)
                    .HasColumnName("TRANGTHAIBAN");

                entity.HasOne(d => d.IdchinhanhNavigation).WithMany(p => p.Bans)
                    .HasForeignKey(d => d.Idchinhanh)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__BAN__IDCHINHANH__267ABA7A");
            });
            modelBuilder.Entity<Banlock>(entity =>
            {
                entity.HasKey(e => e.IdBanLock).HasName("PK__IDBANLOCK__5F20FC40654C6C03");

                entity.ToTable("BANLOCK");

                entity.Property(e => e.IdBanLock)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDBANLOCK");

                entity.Property(e => e.IdBan)
                    .HasMaxLength(5)
                    .HasColumnName("IDBAN");

                entity.Property(e => e.Ngay)
                    .HasColumnName("NGAY");

                entity.Property(e => e.BatDau)
                    .HasColumnName("BATDAU");

                entity.Property(e => e.KetThuc)
                    .HasColumnName("KETTHUC");
                entity.HasOne(d => d.Ban).WithMany(p => p.Banlock)
                   .HasForeignKey(d => d.IdBan)
                   .OnDelete(DeleteBehavior.NoAction)
                   .HasConstraintName("FK__BANLOCK__IDBAN__267ABA7A");
            });
            modelBuilder.Entity<MonAnGhepStats>(entity =>
            {
                entity.HasKey(e => e.Idmonan);

                entity.ToTable("MONANGHEPSTATS");

                entity.Property(e => e.Idmonan)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDMONAN");

                entity.Property(e => e.SoLanDuocGhep)
                    .HasColumnName("SOLANDUOCGHEP");

                entity.HasOne(e => e.MonAn)
                    .WithMany()
                    .HasForeignKey(e => e.Idmonan)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_MONANGHEPSTATS_MONAN");
            });
            modelBuilder.Entity<Chinhanh>(entity =>
            {
                entity.HasKey(e => e.Idchinhanh).HasName("PK__CHINHANH__5F20FC40654C6C03");

                entity.ToTable("CHINHANH");

                entity.Property(e => e.Idchinhanh)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDCHINHANH");

                entity.Property(e => e.Diachicn)
                    .HasMaxLength(500)
                    .HasColumnName("DIACHICN");

                entity.Property(e => e.Tencnhanh)
                    .HasMaxLength(100)
                    .HasColumnName("TENCNHANH");

                // Cấu hình chính xác cho tọa độ
                entity.Property(e => e.Latitude)
                    .HasPrecision(9, 6)
                    .HasColumnName("LATITUDE");

                entity.Property(e => e.Longitude)
                    .HasPrecision(9, 6)
                    .HasColumnName("LONGITUDE");
            });

            modelBuilder.Entity<Chitietdatban>(entity =>
            {
                entity.HasKey(e => new { e.Iddatban, e.Idban }).HasName("PK__CHITIETD__27F2122C38F80AE0");

                entity.ToTable("CHITIETDATBAN");

                entity.Property(e => e.Iddatban)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDDATBAN");
                entity.Property(e => e.Idban)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDBAN");
                entity.Property(e => e.Giora).HasColumnName("GIORA");
                entity.Property(e => e.Giovao).HasColumnName("GIOVAO");

                entity.HasOne(d => d.IdbanNavigation).WithMany(p => p.Chitietdatbans)
                    .HasForeignKey(d => d.Idban)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__CHITIETDA__IDBAN__403A8C7D");

                entity.HasOne(d => d.IddatbanNavigation).WithMany(p => p.Chitietdatbans)
                    .HasForeignKey(d => d.Iddatban)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__CHITIETDA__IDDAT__3F466844");
            });

            modelBuilder.Entity<Chitietdonhang>(entity =>
            {
                entity.HasKey(e => e.IdChitiet).HasName("PK__CHITIETD__150E02391C3C3DE5");

                entity.ToTable("CHITIETDONHANG");
                entity.Property(e => e.IdChitiet)
                    .HasColumnName("IDCHITIET")
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Iddonhang)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDDONHANG");
                entity.Property(e => e.Idmonan)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDMONAN");
                entity.Property(e => e.Dongia).HasColumnName("DONGIA");
                entity.Property(e => e.Ghichu)
                    .HasMaxLength(500)
                    .HasColumnName("GHICHU");
                entity.Property(e => e.Iddebanh)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDDEBANH");
                entity.Property(e => e.Idmonan2)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDMONAN2");
                entity.Property(e => e.Idsize)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDSIZE");
                entity.Property(e => e.Kieupizza)
                    .HasMaxLength(50)
                    .HasColumnName("KIEUPIZZA");
                entity.Property(e => e.Soluong).HasColumnName("SOLUONG");
                entity.Property(e => e.Tongtiendh).HasColumnName("TONGTIENDH");

                entity.HasOne(d => d.IddebanhNavigation).WithMany(p => p.Chitietdonhangs)
                    .HasForeignKey(d => d.Iddebanh)
                    .HasConstraintName("FK__CHITIETDO__IDDEB__49C3F6B7");

                entity.HasOne(d => d.IddonhangNavigation).WithMany(p => p.Chitietdonhangs)
                    .HasForeignKey(d => d.Iddonhang)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__CHITIETDO__IDDON__47DBAE45");

                entity.HasOne(d => d.IdmonanNavigation).WithMany(p => p.Chitietdonhangs)
                    .HasForeignKey(d => d.Idmonan)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__CHITIETDO__IDMON__48CFD27E");
                entity.HasOne(d => d.Idmonan2Navigation)
                    .WithMany()
                    .HasForeignKey(d => d.Idmonan2)
                    .HasConstraintName("FK_CHITIETDONHANG_IDMONAN2")
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(d => d.IdsizeNavigation).WithMany(p => p.Chitietdonhangs)
                    .HasForeignKey(d => d.Idsize)
                    .HasConstraintName("FK__CHITIETDO__IDSIZ__4AB81AF0");
            });

            modelBuilder.Entity<Chitiettopping>(entity =>
            {
                entity.HasKey(e => new { e.IdChitiet, e.Idtopping })
                      .HasName("PK_CHITIETTOPPING");

                entity.ToTable("CHITIETTOPPING");

                entity.Property(e => e.Idtopping)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDTOPPING");
                entity.Property(e => e.IdChitiet)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDCHITIET");

                entity.HasOne(d => d.IdtoppingNavigation).WithMany(p => p.Chitiettoppings)
                    .HasForeignKey(d => d.Idtopping)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__CHITIETTO__IDTOP__4D94879B");

                entity.HasOne(d => d.Chitietdonhang).WithMany(p => p.Chitiettoppings)
                    .HasForeignKey(d => d.IdChitiet)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__CHITIETTOPPING__4E88ABD4");
            });

            modelBuilder.Entity<Datban>(entity =>
            {
                entity.HasKey(e => e.Iddatban).HasName("PK__DATBAN__DEC460099CB35DD0");

                entity.ToTable("DATBAN");

                entity.Property(e => e.Iddatban)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDDATBAN");
                entity.Property(e => e.Ghichu)
                    .HasMaxLength(500)
                    .HasColumnName("GHICHU");
                entity.Property(e => e.Giobatdau).HasColumnName("GIOBATDAU");
                entity.Property(e => e.Gioketthuc).HasColumnName("GIOKETTHUC");
                entity.Property(e => e.Idchinhanh)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDCHINHANH");
                entity.Property(e => e.Idngdung)
                .HasColumnName("IDNGDUNG");
                entity.Property(e => e.Ngaydat).HasColumnName("NGAYDAT");
                entity.Property(e => e.Songuoidat).HasColumnName("SONGUOIDAT");
                entity.Property(e => e.Trangthaidatban)
                    .IsUnicode(true)
                    .HasConversion<string>()
                    .HasColumnName("TRANGTHAIDATBAN");
                entity.Property(e => e.Lydo)
                    .HasColumnName("LYDO");                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  
                entity.Property(e => e.Tenngdat).HasColumnName("TENNGDAT");
                entity.Property(e => e.Sđtngdat).HasMaxLength(10).HasColumnName("SĐTNGDAT");
                entity.HasOne(d => d.IdchinhanhNavigation).WithMany(p => p.Datbans)
                    .HasForeignKey(d => d.Idchinhanh)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__DATBAN__IDCHINHA__3C69FB99");

                entity.HasOne(d => d.Nguoidung).WithMany(p => p.Datbans)
                    .HasForeignKey(d => d.Idngdung)
                    .HasConstraintName("FK__DATBAN__IDNGDUNG__3B75D760");
            });

            modelBuilder.Entity<Debanh>(entity =>
            {
                entity.HasKey(e => e.Iddebanh).HasName("PK__DEBANH__555F23FF6173600D");

                entity.ToTable("DEBANH");

                entity.Property(e => e.Iddebanh)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDDEBANH");
                entity.Property(e => e.Giadebanh).HasColumnName("GIADEBANH");
                entity.Property(e => e.Tendebanh)
                    .HasMaxLength(500)
                    .HasColumnName("TENDEBANH");
            });

            modelBuilder.Entity<Donhang>(entity =>
            {
                entity.HasKey(e => e.Iddonhang).HasName("PK__DONHANG__F59FA8B171F225E2");

                entity.ToTable("DONHANG");

                entity.Property(e => e.Iddonhang)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDDONHANG");
                entity.Property(e => e.Diachidh)
                    .HasMaxLength(500)
                    .HasColumnName("DIACHIDH");
                entity.Property(e => e.Idchinhanh)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDCHINHANH");
                entity.Property(e => e.Iddatban)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDDATBAN");
                entity.Property(e => e.Idngdung)
        .HasColumnName("IDNGDUNG");
                entity.Property(e => e.Ngaydat).HasColumnName("NGAYDAT");
                entity.Property(e => e.Ptttoan)
                    .HasMaxLength(50)
                    .HasColumnName("PTTTOAN");
                entity.Property(e => e.Songuoi).HasColumnName("SONGUOI");
                entity.Property(e => e.Tenkh)
                    .HasMaxLength(500)
                    .HasColumnName("TENKH");
                entity.Property(e => e.Tienship).HasColumnName("TIENSHIP");
                entity.Property(e => e.Tongtien).HasColumnName("TONGTIEN");
                entity.Property(e => e.Trangthai)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .HasColumnName("TRANGTHAI");

                entity.HasOne(d => d.IdchinhanhNavigation).WithMany(p => p.Donhangs)
                    .HasForeignKey(d => d.Idchinhanh)
                    .HasConstraintName("FK__DONHANG__IDCHINH__4316F928");

                entity.HasOne(d => d.IddatbanNavigation).WithMany(p => p.Donhangs)
                    .HasForeignKey(d => d.Iddatban)
                    .HasConstraintName("FK__DONHANG__IDDATBA__440B1D61");

                entity.HasOne(d => d.IdngdungNavigation)
                     .WithMany(p => p.Donhangs)
                     .HasForeignKey(d => d.Idngdung)
                     .HasConstraintName("FK__DONHANG__IDNGDUN__44FF419A");
            });


            modelBuilder.Entity<Listgiasize>(entity =>
            {
                entity.HasKey(e => new { e.Idloaimonan, e.Idsize }).HasName("PK__LISTGIAS__93A480290675CE8A");

                entity.ToTable("LISTGIASIZE");

                entity.Property(e => e.Idloaimonan)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDLOAIMONAN");
                entity.Property(e => e.Idsize)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDSIZE");
                entity.Property(e => e.Giasize).HasColumnName("GIASIZE");

                entity.HasOne(d => d.IdloaimonanNavigation).WithMany(p => p.Listgiasizes)
                    .HasForeignKey(d => d.Idloaimonan)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__LISTGIASI__IDLOA__31EC6D26");

                entity.HasOne(d => d.IdsizeNavigation).WithMany(p => p.Listgiasizes)
                    .HasForeignKey(d => d.Idsize)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__LISTGIASI__IDSIZ__32E0915F");
            });

            modelBuilder.Entity<Loaimonan>(entity =>
            {
                entity.HasKey(e => e.Idloaimonan).HasName("PK__LOAIMONA__6B7E94ED6B8131AD");

                entity.ToTable("LOAIMONAN");

                entity.Property(e => e.Idloaimonan)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDLOAIMONAN");
                entity.Property(e => e.IdloaimanCha)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDLOAIMAN_CHA");
                entity.Property(e => e.Tenloaimonan)
                    .HasMaxLength(500)
                    .HasColumnName("TENLOAIMONAN");
            });

            modelBuilder.Entity<Monan>(entity =>
            {
                entity.HasKey(e => e.Idmonan).HasName("PK__MONAN__091AA88F8490BF0E");

                entity.ToTable("MONAN");

                entity.Property(e => e.Idmonan)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDMONAN");
                entity.Property(e => e.Anhmonan)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("ANHMONAN");
                entity.Property(e => e.Giamonan).HasColumnName("GIAMONAN");
                entity.Property(e => e.Idloaimonan)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDLOAIMONAN");
                entity.Property(e => e.Mota)
                    .HasMaxLength(500)
                    .HasColumnName("MOTA");
                entity.Property(e => e.Tenmonan)
                    .HasMaxLength(500)
                    .HasColumnName("TENMONAN");
                entity.Property(e => e.Trangthaiman)
                    .HasMaxLength(50)
                    .HasColumnName("TRANGTHAIMAN");

                entity.HasOne(d => d.IdloaimonanNavigation).WithMany(p => p.Monans)
                    .HasForeignKey(d => d.Idloaimonan)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__MONAN__IDLOAIMON__38996AB5");
            });

            modelBuilder.Entity<Size>(entity =>
            {
                entity.HasKey(e => e.Idsize).HasName("PK__SIZE__8DA14C4EE47FAF46");

                entity.ToTable("SIZE");

                entity.Property(e => e.Idsize)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDSIZE");
                entity.Property(e => e.Tensize)
                    .HasMaxLength(500)
                    .HasColumnName("TENSIZE");
            });

            modelBuilder.Entity<Topping>(entity =>
            {
                entity.HasKey(e => e.Idtopping).HasName("PK__TOPPING__B17F5B453E08CBB9");

                entity.ToTable("TOPPING");

                entity.Property(e => e.Idtopping)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDTOPPING");
                entity.Property(e => e.Giatopping).HasColumnName("GIATOPPING");
                entity.Property(e => e.Idloaimonan)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength()
                    .HasColumnName("IDLOAIMONAN");
                entity.Property(e => e.Tentopping)
                    .HasMaxLength(500)
                    .HasColumnName("TENTOPPING");

                entity.HasOne(d => d.IdloaimonanNavigation).WithMany(p => p.Toppings)
                    .HasForeignKey(d => d.Idloaimonan)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK__TOPPING__IDLOAIM__2B3F6F97");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}