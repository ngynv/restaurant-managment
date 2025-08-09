using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteOrdering.Migrations
{
    /// <inheritdoc />
    public partial class AddBanLockTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CHINHANH",
                columns: table => new
                {
                    IDCHINHANH = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    TENCNHANH = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DIACHICN = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LATITUDE = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    LONGITUDE = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CHINHANH__5F20FC40654C6C03", x => x.IDCHINHANH);
                });

            migrationBuilder.CreateTable(
                name: "DEBANH",
                columns: table => new
                {
                    IDDEBANH = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    TENDEBANH = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GIADEBANH = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DEBANH__555F23FF6173600D", x => x.IDDEBANH);
                });

            migrationBuilder.CreateTable(
                name: "LOAIMONAN",
                columns: table => new
                {
                    IDLOAIMONAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    TENLOAIMONAN = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IDLOAIMAN_CHA = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LOAIMONA__6B7E94ED6B8131AD", x => x.IDLOAIMONAN);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SIZE",
                columns: table => new
                {
                    IDSIZE = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    TENSIZE = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SIZE__8DA14C4EE47FAF46", x => x.IDSIZE);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HOTEN = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NGAYSINH = table.Column<DateOnly>(type: "date", nullable: true),
                    GIOITINH = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CHINHANH = table.Column<string>(type: "char(5)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_CHINHANH_CHINHANH",
                        column: x => x.CHINHANH,
                        principalTable: "CHINHANH",
                        principalColumn: "IDCHINHANH");
                });

            migrationBuilder.CreateTable(
                name: "BAN",
                columns: table => new
                {
                    IDBAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    TENBAN = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SONGUOI = table.Column<int>(type: "int", nullable: false),
                    TRANGTHAIBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KHUVUC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    X = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Y = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDCHINHANH = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__BAN__9367225E468C45A0", x => x.IDBAN);
                    table.ForeignKey(
                        name: "FK__BAN__IDCHINHANH__267ABA7A",
                        column: x => x.IDCHINHANH,
                        principalTable: "CHINHANH",
                        principalColumn: "IDCHINHANH");
                });

            migrationBuilder.CreateTable(
                name: "MONAN",
                columns: table => new
                {
                    IDMONAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    TENMONAN = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GIAMONAN = table.Column<int>(type: "int", nullable: false),
                    SoLuongBan = table.Column<int>(type: "int", nullable: false),
                    ANHMONAN = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    MOTA = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TRANGTHAIMAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IDLOAIMONAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MONAN__091AA88F8490BF0E", x => x.IDMONAN);
                    table.ForeignKey(
                        name: "FK__MONAN__IDLOAIMON__38996AB5",
                        column: x => x.IDLOAIMONAN,
                        principalTable: "LOAIMONAN",
                        principalColumn: "IDLOAIMONAN");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DATBAN",
                columns: table => new
                {
                    IDDATBAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    NGAYDAT = table.Column<DateOnly>(type: "date", nullable: false),
                    GIOBATDAU = table.Column<TimeOnly>(type: "time", nullable: false),
                    GIOKETTHUC = table.Column<TimeOnly>(type: "time", nullable: false),
                    SONGUOIDAT = table.Column<int>(type: "int", nullable: false),
                    GHICHU = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TRANGTHAIDATBAN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDNGDUNG = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TENNGDAT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SĐTNGDAT = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IDCHINHANH = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    LYDO = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DATBAN__DEC460099CB35DD0", x => x.IDDATBAN);
                    table.ForeignKey(
                        name: "FK__DATBAN__IDCHINHA__3C69FB99",
                        column: x => x.IDCHINHANH,
                        principalTable: "CHINHANH",
                        principalColumn: "IDCHINHANH");
                    table.ForeignKey(
                        name: "FK__DATBAN__IDNGDUNG__3B75D760",
                        column: x => x.IDNGDUNG,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BANLOCK",
                columns: table => new
                {
                    IDBANLOCK = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    IDBAN = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    BATDAU = table.Column<TimeOnly>(type: "time", nullable: false),
                    KETTHUC = table.Column<TimeOnly>(type: "time", nullable: false),
                    NGAY = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__IDBANLOCK__5F20FC40654C6C03", x => x.IDBANLOCK);
                    table.ForeignKey(
                        name: "FK__BANLOCK__IDBAN__267ABA7A",
                        column: x => x.IDBAN,
                        principalTable: "BAN",
                        principalColumn: "IDBAN");
                });

            migrationBuilder.CreateTable(
                name: "LISTGIASIZE",
                columns: table => new
                {
                    IDLOAIMONAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    IDSIZE = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    GIASIZE = table.Column<int>(type: "int", nullable: false),
                    MonanIdmonan = table.Column<string>(type: "char(5)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LISTGIAS__93A480290675CE8A", x => new { x.IDLOAIMONAN, x.IDSIZE });
                    table.ForeignKey(
                        name: "FK_LISTGIASIZE_MONAN_MonanIdmonan",
                        column: x => x.MonanIdmonan,
                        principalTable: "MONAN",
                        principalColumn: "IDMONAN");
                    table.ForeignKey(
                        name: "FK__LISTGIASI__IDLOA__31EC6D26",
                        column: x => x.IDLOAIMONAN,
                        principalTable: "LOAIMONAN",
                        principalColumn: "IDLOAIMONAN");
                    table.ForeignKey(
                        name: "FK__LISTGIASI__IDSIZ__32E0915F",
                        column: x => x.IDSIZE,
                        principalTable: "SIZE",
                        principalColumn: "IDSIZE");
                });

            migrationBuilder.CreateTable(
                name: "MONANGHEPSTATS",
                columns: table => new
                {
                    IDMONAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    SOLANDUOCGHEP = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MONANGHEPSTATS", x => x.IDMONAN);
                    table.ForeignKey(
                        name: "FK_MONANGHEPSTATS_MONAN",
                        column: x => x.IDMONAN,
                        principalTable: "MONAN",
                        principalColumn: "IDMONAN",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TOPPING",
                columns: table => new
                {
                    IDTOPPING = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    TENTOPPING = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GIATOPPING = table.Column<int>(type: "int", nullable: false),
                    IDLOAIMONAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    MonanIdmonan = table.Column<string>(type: "char(5)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TOPPING__B17F5B453E08CBB9", x => x.IDTOPPING);
                    table.ForeignKey(
                        name: "FK_TOPPING_MONAN_MonanIdmonan",
                        column: x => x.MonanIdmonan,
                        principalTable: "MONAN",
                        principalColumn: "IDMONAN");
                    table.ForeignKey(
                        name: "FK__TOPPING__IDLOAIM__2B3F6F97",
                        column: x => x.IDLOAIMONAN,
                        principalTable: "LOAIMONAN",
                        principalColumn: "IDLOAIMONAN");
                });

            migrationBuilder.CreateTable(
                name: "CHITIETDATBAN",
                columns: table => new
                {
                    IDDATBAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    IDBAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    GIOVAO = table.Column<TimeOnly>(type: "time", nullable: false),
                    GIORA = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CHITIETD__27F2122C38F80AE0", x => new { x.IDDATBAN, x.IDBAN });
                    table.ForeignKey(
                        name: "FK__CHITIETDA__IDBAN__403A8C7D",
                        column: x => x.IDBAN,
                        principalTable: "BAN",
                        principalColumn: "IDBAN");
                    table.ForeignKey(
                        name: "FK__CHITIETDA__IDDAT__3F466844",
                        column: x => x.IDDATBAN,
                        principalTable: "DATBAN",
                        principalColumn: "IDDATBAN");
                });

            migrationBuilder.CreateTable(
                name: "DONHANG",
                columns: table => new
                {
                    IDDONHANG = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    DIACHIDH = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NGAYDAT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TRANGTHAI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TONGTIEN = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PTTTOAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SONGUOI = table.Column<int>(type: "int", nullable: true),
                    Magiaodich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TENKH = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sdtkh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TIENSHIP = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeliveryMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Khoangcachship = table.Column<double>(type: "float", nullable: true),
                    IDCHINHANH = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true),
                    IDDATBAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true),
                    IDNGDUNG = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DONHANG__F59FA8B171F225E2", x => x.IDDONHANG);
                    table.ForeignKey(
                        name: "FK__DONHANG__IDCHINH__4316F928",
                        column: x => x.IDCHINHANH,
                        principalTable: "CHINHANH",
                        principalColumn: "IDCHINHANH");
                    table.ForeignKey(
                        name: "FK__DONHANG__IDDATBA__440B1D61",
                        column: x => x.IDDATBAN,
                        principalTable: "DATBAN",
                        principalColumn: "IDDATBAN");
                    table.ForeignKey(
                        name: "FK__DONHANG__IDNGDUN__44FF419A",
                        column: x => x.IDNGDUNG,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CHITIETDONHANG",
                columns: table => new
                {
                    IDCHITIET = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    GHICHU = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SOLUONG = table.Column<int>(type: "int", nullable: false),
                    DONGIA = table.Column<int>(type: "int", nullable: false),
                    TONGTIENDH = table.Column<int>(type: "int", nullable: false),
                    KIEUPIZZA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IDMONAN2 = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true),
                    IDDONHANG = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    IDMONAN = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    IDDEBANH = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true),
                    IDSIZE = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CHITIETD__150E02391C3C3DE5", x => x.IDCHITIET);
                    table.ForeignKey(
                        name: "FK_CHITIETDONHANG_IDMONAN2",
                        column: x => x.IDMONAN2,
                        principalTable: "MONAN",
                        principalColumn: "IDMONAN");
                    table.ForeignKey(
                        name: "FK__CHITIETDO__IDDEB__49C3F6B7",
                        column: x => x.IDDEBANH,
                        principalTable: "DEBANH",
                        principalColumn: "IDDEBANH");
                    table.ForeignKey(
                        name: "FK__CHITIETDO__IDDON__47DBAE45",
                        column: x => x.IDDONHANG,
                        principalTable: "DONHANG",
                        principalColumn: "IDDONHANG");
                    table.ForeignKey(
                        name: "FK__CHITIETDO__IDMON__48CFD27E",
                        column: x => x.IDMONAN,
                        principalTable: "MONAN",
                        principalColumn: "IDMONAN");
                    table.ForeignKey(
                        name: "FK__CHITIETDO__IDSIZ__4AB81AF0",
                        column: x => x.IDSIZE,
                        principalTable: "SIZE",
                        principalColumn: "IDSIZE");
                });

            migrationBuilder.CreateTable(
                name: "CHITIETTOPPING",
                columns: table => new
                {
                    IDTOPPING = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false),
                    IDCHITIET = table.Column<string>(type: "char(5)", unicode: false, fixedLength: true, maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHITIETTOPPING", x => new { x.IDCHITIET, x.IDTOPPING });
                    table.ForeignKey(
                        name: "FK__CHITIETTOPPING__4E88ABD4",
                        column: x => x.IDCHITIET,
                        principalTable: "CHITIETDONHANG",
                        principalColumn: "IDCHITIET");
                    table.ForeignKey(
                        name: "FK__CHITIETTO__IDTOP__4D94879B",
                        column: x => x.IDTOPPING,
                        principalTable: "TOPPING",
                        principalColumn: "IDTOPPING");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CHINHANH",
                table: "AspNetUsers",
                column: "CHINHANH");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BAN_IDCHINHANH",
                table: "BAN",
                column: "IDCHINHANH");

            migrationBuilder.CreateIndex(
                name: "IX_BANLOCK_IDBAN",
                table: "BANLOCK",
                column: "IDBAN");

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETDATBAN_IDBAN",
                table: "CHITIETDATBAN",
                column: "IDBAN");

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETDONHANG_IDDEBANH",
                table: "CHITIETDONHANG",
                column: "IDDEBANH");

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETDONHANG_IDDONHANG",
                table: "CHITIETDONHANG",
                column: "IDDONHANG");

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETDONHANG_IDMONAN",
                table: "CHITIETDONHANG",
                column: "IDMONAN");

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETDONHANG_IDMONAN2",
                table: "CHITIETDONHANG",
                column: "IDMONAN2");

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETDONHANG_IDSIZE",
                table: "CHITIETDONHANG",
                column: "IDSIZE");

            migrationBuilder.CreateIndex(
                name: "IX_CHITIETTOPPING_IDTOPPING",
                table: "CHITIETTOPPING",
                column: "IDTOPPING");

            migrationBuilder.CreateIndex(
                name: "IX_DATBAN_IDCHINHANH",
                table: "DATBAN",
                column: "IDCHINHANH");

            migrationBuilder.CreateIndex(
                name: "IX_DATBAN_IDNGDUNG",
                table: "DATBAN",
                column: "IDNGDUNG");

            migrationBuilder.CreateIndex(
                name: "IX_DONHANG_IDCHINHANH",
                table: "DONHANG",
                column: "IDCHINHANH");

            migrationBuilder.CreateIndex(
                name: "IX_DONHANG_IDDATBAN",
                table: "DONHANG",
                column: "IDDATBAN");

            migrationBuilder.CreateIndex(
                name: "IX_DONHANG_IDNGDUNG",
                table: "DONHANG",
                column: "IDNGDUNG");

            migrationBuilder.CreateIndex(
                name: "IX_LISTGIASIZE_IDSIZE",
                table: "LISTGIASIZE",
                column: "IDSIZE");

            migrationBuilder.CreateIndex(
                name: "IX_LISTGIASIZE_MonanIdmonan",
                table: "LISTGIASIZE",
                column: "MonanIdmonan");

            migrationBuilder.CreateIndex(
                name: "IX_MONAN_IDLOAIMONAN",
                table: "MONAN",
                column: "IDLOAIMONAN");

            migrationBuilder.CreateIndex(
                name: "IX_TOPPING_IDLOAIMONAN",
                table: "TOPPING",
                column: "IDLOAIMONAN");

            migrationBuilder.CreateIndex(
                name: "IX_TOPPING_MonanIdmonan",
                table: "TOPPING",
                column: "MonanIdmonan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BANLOCK");

            migrationBuilder.DropTable(
                name: "CHITIETDATBAN");

            migrationBuilder.DropTable(
                name: "CHITIETTOPPING");

            migrationBuilder.DropTable(
                name: "LISTGIASIZE");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "MONANGHEPSTATS");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BAN");

            migrationBuilder.DropTable(
                name: "CHITIETDONHANG");

            migrationBuilder.DropTable(
                name: "TOPPING");

            migrationBuilder.DropTable(
                name: "DEBANH");

            migrationBuilder.DropTable(
                name: "DONHANG");

            migrationBuilder.DropTable(
                name: "SIZE");

            migrationBuilder.DropTable(
                name: "MONAN");

            migrationBuilder.DropTable(
                name: "DATBAN");

            migrationBuilder.DropTable(
                name: "LOAIMONAN");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CHINHANH");
        }
    }
}
