using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class StoredProcedureNhom2cTests
{
    readonly DatabaseFixture _fixture;

    public StoredProcedureNhom2cTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    [SkippableFact]
    public void sp_ThemLoaiSuKien_tao_va_chan_trung_ten()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var ten = "Loai " + Guid.NewGuid().ToString("N")[..6];
        var bang = Sql.GoiThuTuc(cn, "sp_ThemLoaiSuKien", ("@TenLoai", "  " + ten + "  "), ("@MoTa", "mo ta"));
        Assert.Equal(ten, bang.Rows[0]["TenLoai"] as string);
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["TrangThai"]));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemLoaiSuKien", ("@TenLoai", ten)), 50141);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemLoaiSuKien", ("@TenLoai", "   ")), 50140);
    }

    [SkippableFact]
    public void sp_CapNhatLoaiSuKien_cap_nhat_va_chan_trung()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.GoiThuTuc(cn, "sp_CapNhatLoaiSuKien",
            ("@LoaiSuKienID", duLieu.LoaiSuKienID), ("@TenLoai", "Hoi thao"), ("@MoTa", "moi"));
        Assert.Equal("moi", bang.Rows[0]["MoTa"] as string);

        var tenKhac = "Loai " + Guid.NewGuid().ToString("N")[..6];
        Sql.GoiThuTuc(cn, "sp_ThemLoaiSuKien", ("@TenLoai", tenKhac));
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CapNhatLoaiSuKien",
            ("@LoaiSuKienID", duLieu.LoaiSuKienID), ("@TenLoai", tenKhac)), 50151);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CapNhatLoaiSuKien",
            ("@LoaiSuKienID", -1), ("@TenLoai", "Ten moi")), 50152);
    }

    [SkippableFact]
    public void sp_DoiTrangThaiLoaiSuKien_khoa_va_mo()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        try
        {
            Assert.Equal(0, Convert.ToInt32(Sql.GoiThuTuc(cn, "sp_DoiTrangThaiLoaiSuKien",
                ("@LoaiSuKienID", duLieu.LoaiSuKienID), ("@TrangThai", false)).Rows[0]["TrangThai"]));
        }
        finally
        {
            Assert.Equal(1, Convert.ToInt32(Sql.GoiThuTuc(cn, "sp_DoiTrangThaiLoaiSuKien",
                ("@LoaiSuKienID", duLieu.LoaiSuKienID), ("@TrangThai", true)).Rows[0]["TrangThai"]));
        }
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DoiTrangThaiLoaiSuKien",
            ("@LoaiSuKienID", -1), ("@TrangThai", true)), 50160);
    }

    [SkippableFact]
    public void sp_TraCuuLoaiSuKien_loc_theo_tu_khoa_va_trang_thai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var ten = (string)Sql.Mot(cn, "SELECT TenLoai FROM LoaiSuKien WHERE LoaiSuKienID = @Id", ("@Id", duLieu.LoaiSuKienID))!;
        Assert.Equal(1, Sql.GoiThuTuc(cn, "sp_TraCuuLoaiSuKien", ("@TuKhoa", ten), ("@TrangThai", true)).Rows.Count);
        Assert.Equal(0, Sql.GoiThuTuc(cn, "sp_TraCuuLoaiSuKien", ("@TuKhoa", ten), ("@TrangThai", false)).Rows.Count);
    }

    [SkippableFact]
    public void sp_TaoSuKien_tao_cho_duyet_va_chan_sai_quyen()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.GoiThuTuc(cn, "sp_TaoSuKien",
            ("@NguoiTaoID", duLieu.BanToChucID), ("@LoaiSuKienID", duLieu.LoaiSuKienID),
            ("@TenSuKien", "  Su kien moi  "), ("@MoTa", DBNull.Value), ("@AnhBia", DBNull.Value),
            ("@ThoiGianBatDau", DateTime.Now.AddDays(30)), ("@ThoiGianKetThuc", DateTime.Now.AddDays(31)),
            ("@SoLuongNguoiDuKien", 100));
        Assert.Equal("Su kien moi", bang.Rows[0]["TenSuKien"] as string);
        Assert.Equal("ChoDuyet", bang.Rows[0]["TrangThai"] as string);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoSuKien",
            ("@NguoiTaoID", duLieu.NguoiDungID), ("@LoaiSuKienID", duLieu.LoaiSuKienID),
            ("@TenSuKien", "X"), ("@MoTa", DBNull.Value), ("@AnhBia", DBNull.Value),
            ("@ThoiGianBatDau", DateTime.Now.AddDays(1)), ("@ThoiGianKetThuc", DateTime.Now.AddDays(2)),
            ("@SoLuongNguoiDuKien", 10)), 50173);
    }

    [SkippableFact]
    public void sp_TaoSuKien_chan_du_lieu_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoSuKien",
            ("@NguoiTaoID", duLieu.BanToChucID), ("@LoaiSuKienID", duLieu.LoaiSuKienID),
            ("@TenSuKien", "  "), ("@MoTa", DBNull.Value), ("@AnhBia", DBNull.Value),
            ("@ThoiGianBatDau", DateTime.Now), ("@ThoiGianKetThuc", DateTime.Now.AddDays(1)),
            ("@SoLuongNguoiDuKien", 10)), 50170);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoSuKien",
            ("@NguoiTaoID", duLieu.BanToChucID), ("@LoaiSuKienID", duLieu.LoaiSuKienID),
            ("@TenSuKien", "X"), ("@MoTa", DBNull.Value), ("@AnhBia", DBNull.Value),
            ("@ThoiGianBatDau", DateTime.Now.AddDays(2)), ("@ThoiGianKetThuc", DateTime.Now.AddDays(1)),
            ("@SoLuongNguoiDuKien", 10)), 50171);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoSuKien",
            ("@NguoiTaoID", duLieu.BanToChucID), ("@LoaiSuKienID", duLieu.LoaiSuKienID),
            ("@TenSuKien", "X"), ("@MoTa", DBNull.Value), ("@AnhBia", DBNull.Value),
            ("@ThoiGianBatDau", DateTime.Now), ("@ThoiGianKetThuc", DateTime.Now.AddDays(1)),
            ("@SoLuongNguoiDuKien", 0)), 50172);
    }

    [SkippableFact]
    public void sp_DuyetSuKien_va_TuChoiSuKien()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.GoiThuTuc(cn, "sp_DuyetSuKien",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@SuKienID", duLieu.SuKienChoDuyetID));
        Assert.Equal("DaDuyet", bang.Rows[0]["TrangThai"] as string);

        var moi = Sql.Nguyen(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau,
                                ThoiGianKetThuc, SoLuongNguoiDuKien)
            VALUES (@Tk, @Loai, N'Tu choi', DATEADD(DAY, 5, SYSDATETIME()),
                    DATEADD(DAY, 6, SYSDATETIME()), 10);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", duLieu.BanToChucID), ("@Loai", duLieu.LoaiSuKienID));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TuChoiSuKien",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@SuKienID", moi), ("@LyDoTuChoi", "  ")), 50190);
        var tuChoi = Sql.GoiThuTuc(cn, "sp_TuChoiSuKien",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@SuKienID", moi), ("@LyDoTuChoi", "Khong hop le"));
        Assert.Equal("TuChoi", tuChoi.Rows[0]["TrangThai"] as string);
        Assert.Equal("Khong hop le", tuChoi.Rows[0]["LyDoTuChoi"] as string);
    }

    [SkippableFact]
    public void sp_DuyetSuKien_chan_tu_duyet_va_sai_quyen()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DuyetSuKien",
            ("@NguoiDuyetID", duLieu.BanToChucID), ("@SuKienID", duLieu.SuKienChoDuyetID)), 50181);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DuyetSuKien",
            ("@NguoiDuyetID", duLieu.NguoiDungID), ("@SuKienID", duLieu.SuKienChoDuyetID)), 50180);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DuyetSuKien",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@SuKienID", duLieu.SuKienID)), 50182);
    }

    [SkippableFact]
    public void sp_TraCuuSuKien_va_ThongKeSuKien()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var theoTrangThai = Sql.GoiThuTuc(cn, "sp_TraCuuSuKien", ("@TrangThai", "DaDuyet"), ("@Trang", 1), ("@SoDong", 50));
        Assert.All(theoTrangThai.Rows.Cast<System.Data.DataRow>(), r => Assert.Equal("DaDuyet", r["TrangThai"] as string));
        Assert.True(Sql.GoiThuTuc(cn, "sp_ThongKeSuKien").Rows.Count > 0);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TraCuuSuKien", ("@Trang", 1), ("@SoDong", 0)), 50200);
    }
}