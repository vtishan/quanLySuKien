using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class FunctionTests
{
    readonly DatabaseFixture _fixture;

    public FunctionTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    static int Bit(SqlConnection cn, string sql, params (string Ten, object? GiaTri)[] thamSo) =>
        Convert.ToInt32(Sql.Mot(cn, sql, thamSo));

    [SkippableFact]
    public void fn_KiemTraEmailTonTai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var email = (string)Sql.Mot(cn, "SELECT Email FROM TaiKhoan WHERE TaiKhoanID = @Id", ("@Id", duLieu.AdminID))!;
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraEmailTonTai(@Email)", ("@Email", email)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraEmailTonTai(@Email)", ("@Email", "khong-co@test.local")));
    }

    [SkippableFact]
    public void fn_KiemTraTaiKhoanHoatDong()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var khoa = BoDuLieu.TaiKhoan(cn, "NguoiDung", "khoa-" + Guid.NewGuid().ToString("N") + "@test.local", 0);
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraTaiKhoanHoatDong(@Id)", ("@Id", duLieu.NguoiDungID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraTaiKhoanHoatDong(@Id)", ("@Id", khoa)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraTaiKhoanHoatDong(@Id)", ("@Id", -1)));
    }

    [SkippableFact]
    public void fn_LayVaiTroTaiKhoan_va_KiemTraVaiTro()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal("QuanTriVien", Sql.Mot(cn, "SELECT dbo.fn_LayVaiTroTaiKhoan(@Id)", ("@Id", duLieu.AdminID)) as string);
        Assert.Null(Sql.Mot(cn, "SELECT dbo.fn_LayVaiTroTaiKhoan(@Id)", ("@Id", -1)));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraVaiTro(@Id, N'BanToChuc')", ("@Id", duLieu.BanToChucID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraVaiTro(@Id, N'QuanTriVien')", ("@Id", duLieu.BanToChucID)));
    }

    [SkippableFact]
    public void fn_KiemTraBanToChucDaXacThuc()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraBanToChucDaXacThuc(@Id)", ("@Id", duLieu.BanToChucID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraBanToChucDaXacThuc(@Id)", ("@Id", duLieu.NguoiDungID)));
    }

    [SkippableFact]
    public void fn_DemTaiKhoanTheoTrangThai_va_VaiTro()
    {
        using var cn = Mo();
        BoDuLieu.Tao(cn);
        var hoatDong = Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemTaiKhoanTheoTrangThai(1)"));
        var tong = Sql.Nguyen(cn, "SELECT COUNT(*) FROM TaiKhoan");
        var biKhoa = Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemTaiKhoanTheoTrangThai(0)"));
        Assert.Equal(tong, hoatDong + biKhoa);
        var adminRole = Sql.Nguyen(cn, "SELECT VaiTroID FROM VaiTro WHERE TenVaiTro = N'QuanTriVien'");
        Assert.Equal(
            Sql.Nguyen(cn, "SELECT COUNT(*) FROM TaiKhoan WHERE VaiTroID = @Id", ("@Id", adminRole)),
            Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemTaiKhoanTheoVaiTro(@Id)", ("@Id", adminRole))));
    }

    [SkippableFact]
    public void fn_DemSuKien_va_DonVe_TheoTaiKhoan()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(2, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemSuKienTheoTaiKhoan(@Id)", ("@Id", duLieu.BanToChucID))));
        Assert.Equal(0, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemSuKienTheoTaiKhoan(@Id)", ("@Id", duLieu.NguoiDungID))));
        Assert.Equal(1, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemDonVeTheoTaiKhoan(@Id)", ("@Id", duLieu.NguoiDungID))));
    }

    [SkippableFact]
    public void fn_KiemTraYeuCauXacThuc()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var yeuCau = Sql.Nguyen(cn, "SELECT TOP 1 YeuCauID FROM YeuCauXacThucToChuc WHERE TaiKhoanID = @Id", ("@Id", duLieu.BanToChucID));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraYeuCauXacThucTonTai(@Id)", ("@Id", yeuCau)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraYeuCauXacThucTonTai(@Id)", ("@Id", -1)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraYeuCauChoDuyet(@Id)", ("@Id", duLieu.BanToChucID)));
    }

    [SkippableFact]
    public void fn_DemYeuCauXacThucTheoTrangThai()
    {
        using var cn = Mo();
        BoDuLieu.Tao(cn);
        var daDuyet = Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemYeuCauXacThucTheoTrangThai(N'DaDuyet')"));
        Assert.True(daDuyet >= 1);
        Assert.Equal(0, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemYeuCauXacThucTheoTrangThai(N'KhongCo')")));
    }

    [SkippableFact]
    public void fn_LoaiSuKien_va_SuKien()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraLoaiSuKienHoatDong(@Id)", ("@Id", duLieu.LoaiSuKienID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraLoaiSuKienHoatDong(@Id)", ("@Id", -1)));
        Assert.Equal(2, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemSuKienTheoLoai(@Id)", ("@Id", duLieu.LoaiSuKienID))));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraSuKienDaDuyet(@Id)", ("@Id", duLieu.SuKienID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraSuKienDaDuyet(@Id)", ("@Id", duLieu.SuKienChoDuyetID)));
        Assert.True(Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemSuKienTheoTrangThai(N'DaDuyet')")) >= 1);
    }

    [SkippableFact]
    public void fn_KiemTraThoiGianSuKien()
    {
        using var cn = Mo();
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraThoiGianSuKien(@A, @B)",
            ("@A", DateTime.Now), ("@B", DateTime.Now.AddHours(1))));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraThoiGianSuKien(@A, @B)",
            ("@A", DateTime.Now), ("@B", DateTime.Now.AddHours(-1))));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraThoiGianSuKien(@A, @B)",
            ("@A", DBNull.Value), ("@B", DateTime.Now)));
    }

    [SkippableFact]
    public void fn_KiemTraNguoiDuyetSuKien()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraNguoiDuyetSuKien(@Sk, @Nguoi)",
            ("@Sk", duLieu.SuKienID), ("@Nguoi", duLieu.ManagerID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraNguoiDuyetSuKien(@Sk, @Nguoi)",
            ("@Sk", duLieu.SuKienID), ("@Nguoi", duLieu.BanToChucID)));
    }

    [SkippableFact]
    public void fn_KiemTraQuyenTaoSuKien_va_SoLuongNguoiDuKien()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraQuyenTaoSuKien(@Id)", ("@Id", duLieu.BanToChucID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraQuyenTaoSuKien(@Id)", ("@Id", duLieu.ManagerID)));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraSoLuongNguoiDuKien(10)"));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraSoLuongNguoiDuKien(0)"));
    }

    [SkippableFact]
    public void fn_DiaDiem_ton_tai_hoat_dong_suc_chua()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraDiaDiemTonTai(@Id)", ("@Id", duLieu.DiaDiemID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraDiaDiemTonTai(@Id)", ("@Id", -1)));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraDiaDiemHoatDong(@Id)", ("@Id", duLieu.DiaDiemID)));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraSucChuaDiaDiem(@Id, 500)", ("@Id", duLieu.DiaDiemID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraSucChuaDiaDiem(@Id, 501)", ("@Id", duLieu.DiaDiemID)));
    }

    [SkippableFact]
    public void fn_TinhTongTienThueDiaDiem()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(2000000m, Convert.ToDecimal(Sql.Mot(cn, "SELECT dbo.fn_TinhTongTienThueDiaDiem(@Id, 2)", ("@Id", duLieu.DiaDiemID))));
        Assert.Null(Sql.Mot(cn, "SELECT dbo.fn_TinhTongTienThueDiaDiem(@Id, 0)", ("@Id", duLieu.DiaDiemID)));
        Assert.Null(Sql.Mot(cn, "SELECT dbo.fn_TinhTongTienThueDiaDiem(@Id, 1)", ("@Id", -1)));
    }

    [SkippableFact]
    public void fn_KiemTraThoiGianDatDiaDiem_va_TienCoc()
    {
        using var cn = Mo();
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraThoiGianDatDiaDiem(@A, @B)",
            ("@A", DateTime.Now), ("@B", DateTime.Now.AddHours(2))));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraThoiGianDatDiaDiem(@A, @B)",
            ("@A", DateTime.Now), ("@B", DateTime.Now)));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraTienCocDatDiaDiem(100, 50)"));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraTienCocDatDiaDiem(100, 101)"));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraTienCocDatDiaDiem(100, -1)"));
    }

    [SkippableFact]
    public void fn_KiemTraTrungLichDiaDiem()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var batDau = DateTime.Now.AddDays(200);
        var datID = Sql.Nguyen(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 1000, 500);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID), ("@A", batDau), ("@B", batDau.AddDays(1)));

        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraTrungLichDiaDiem(@Dd, @A, @B, NULL)",
            ("@Dd", duLieu.DiaDiemID), ("@A", batDau.AddHours(1)), ("@B", batDau.AddHours(2))));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraTrungLichDiaDiem(@Dd, @A, @B, @Bo)",
            ("@Dd", duLieu.DiaDiemID), ("@A", batDau.AddHours(1)), ("@B", batDau.AddHours(2)), ("@Bo", datID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraTrungLichDiaDiem(@Dd, @A, @B, NULL)",
            ("@Dd", duLieu.DiaDiemID), ("@A", batDau.AddDays(2)), ("@B", batDau.AddDays(3))));
    }

    [SkippableFact]
    public void fn_DemDatDiaDiem_va_DiaDiem()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var batDau = DateTime.Now.AddDays(300);
        Sql.ThucThi(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 1000, 500);
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID), ("@A", batDau), ("@B", batDau.AddDays(1)));

        Assert.True(Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemLuotDatTheoDiaDiem(@Id)", ("@Id", duLieu.DiaDiemID))) >= 1);
        Assert.Equal(1, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemDatDiaDiemTheoTrangThai(N'ChoXacNhan')")));
        Assert.True(Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemDiaDiemTheoTrangThai(1)")) >= 1);
    }

    [SkippableFact]
    public void fn_KiemTraSuKienCoTheDatDiaDiem()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraSuKienCoTheDatDiaDiem(@Sk, @Tk)",
            ("@Sk", duLieu.SuKienID), ("@Tk", duLieu.BanToChucID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraSuKienCoTheDatDiaDiem(@Sk, @Tk)",
            ("@Sk", duLieu.SuKienID), ("@Tk", duLieu.ManagerID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraSuKienCoTheDatDiaDiem(@Sk, @Tk)",
            ("@Sk", duLieu.SuKienChoDuyetID), ("@Tk", duLieu.BanToChucID)));
    }

    [SkippableFact]
    public void fn_LoaiVe_va_SoLuongVe()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraLoaiVeHoatDong(@Id)", ("@Id", duLieu.LoaiVeID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraLoaiVeHoatDong(@Id)", ("@Id", -1)));
        Assert.Equal(100, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_LaySoLuongVeConLai(@Id)", ("@Id", duLieu.LoaiVeID))));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraSoLuongVeCoTheMua(@Id, 5)", ("@Id", duLieu.LoaiVeID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraSoLuongVeCoTheMua(@Id, 6)", ("@Id", duLieu.LoaiVeID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraSoLuongVeCoTheMua(@Id, 0)", ("@Id", duLieu.LoaiVeID)));
        Assert.Null(Sql.Mot(cn, "SELECT dbo.fn_LaySoLuongVeConLai(@Id)", ("@Id", -1)));
    }

    [SkippableFact]
    public void fn_KiemTraThoiGianBanVe()
    {
        using var cn = Mo();
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraThoiGianBanVe(@A, @B)",
            ("@A", DateTime.Now), ("@B", DateTime.Now.AddHours(1))));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraThoiGianBanVe(@A, @B)",
            ("@A", DateTime.Now.AddHours(1)), ("@B", DateTime.Now)));
    }

    [SkippableFact]
    public void fn_TinhThanhTien_va_TongTien()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(300000m, Convert.ToDecimal(Sql.Mot(cn, "SELECT dbo.fn_TinhThanhTienVe(3, 100000)")));
        Assert.Null(Sql.Mot(cn, "SELECT dbo.fn_TinhThanhTienVe(0, 100000)"));
        Assert.Null(Sql.Mot(cn, "SELECT dbo.fn_TinhThanhTienVe(1, -1)"));
        Assert.Equal(100000m, Convert.ToDecimal(Sql.Mot(cn, "SELECT dbo.fn_TinhTongTienGocDonVe(@Id)", ("@Id", duLieu.DonVeID))));
        Assert.Equal(0m, Convert.ToDecimal(Sql.Mot(cn, "SELECT dbo.fn_TinhTongTienGocDonVe(@Id)", ("@Id", -1))));
        Assert.Equal(80000m, Convert.ToDecimal(Sql.Mot(cn, "SELECT dbo.fn_TinhTongTienThanhToan(100000, 20000)")));
        Assert.Null(Sql.Mot(cn, "SELECT dbo.fn_TinhTongTienThanhToan(100000, 200000)"));
        Assert.Null(Sql.Mot(cn, "SELECT dbo.fn_TinhTongTienThanhToan(-1, 0)"));
    }

    [SkippableFact]
    public void fn_ThanhToan_HoanTien_Va_Ve()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donDaTra = BoDuLieu.TaoDonDaThanhToan(cn, duLieu);
        var ve = BoDuLieu.PhatHanhVe(cn, donDaTra.DonVeID, donDaTra.LoaiVeID, "VE-FN-" + Guid.NewGuid().ToString("N")[..8]);

        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraDonVeDaThanhToan(@Id)", ("@Id", duLieu.DonVeID)));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraDonVeDaThanhToan(@Id)", ("@Id", donDaTra.DonVeID)));
        Assert.Equal(200000m, Convert.ToDecimal(Sql.Mot(cn, "SELECT dbo.fn_TinhTienDaThanhToan(@Id)", ("@Id", donDaTra.DonVeID))));
        Assert.Equal(0m, Convert.ToDecimal(Sql.Mot(cn, "SELECT dbo.fn_TinhTienDaHoan(@Id)", ("@Id", donDaTra.DonVeID))));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraSoTienHoanHopLe(@Id, 100000)", ("@Id", donDaTra.DonVeID)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraSoTienHoanHopLe(@Id, 200001)", ("@Id", donDaTra.DonVeID)));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraVeHopLe(@Id)", ("@Id", ve)));
        Assert.Equal(1, Bit(cn, "SELECT dbo.fn_KiemTraVeThuocDonHopLe(@Id)", ("@Id", ve)));
        Assert.Equal(0, Bit(cn, "SELECT dbo.fn_KiemTraVeHopLe(@Id)", ("@Id", -1)));
    }

    [SkippableFact]
    public void fn_DemVe_va_XuatVe()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donDaTra = BoDuLieu.TaoDonDaThanhToan(cn, duLieu);
        var ve = BoDuLieu.PhatHanhVe(cn, donDaTra.DonVeID, donDaTra.LoaiVeID, "VE-DEM-" + Guid.NewGuid().ToString("N")[..8]);
        Sql.ThucThi(cn, """
            INSERT INTO XuatVe (VeID, NguoiXuatID, HinhThucXuat) VALUES (@Ve, @Nguoi, N'ExportPDF');
            """, ("@Ve", ve), ("@Nguoi", duLieu.ManagerID));

        Assert.True(Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemVeTheoTrangThai(N'ChuaSuDung')")) >= 1);
        Assert.Equal(0, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemVeCheckInTheoSuKien(@Id)", ("@Id", duLieu.SuKienID))));
        Assert.Equal(1, Convert.ToInt32(Sql.Mot(cn, "SELECT dbo.fn_DemLanXuatVe(@Id)", ("@Id", ve))));
    }
}

