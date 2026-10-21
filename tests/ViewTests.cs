using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class ViewTests
{
    readonly DatabaseFixture _fixture;

    public ViewTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    [SkippableFact]
    public void tat_ca_view_deu_truy_van_duoc()
    {
        using var cn = Mo();
        BoDuLieu.Tao(cn);
        var view = Sql.Bang(cn, "SELECT name FROM sys.views WHERE is_ms_shipped = 0 ORDER BY name");
        foreach (System.Data.DataRow row in view.Rows)
        {
            var ten = (string)row["name"];
            var loi = Record.Exception(() => Sql.Bang(cn, $"SELECT TOP 5 * FROM dbo.[{ten}]"));
            Assert.True(loi is null, $"View {ten} loi: {loi?.Message}");
        }
    }

    [SkippableFact]
    public void vw_DanhSachTaiKhoan_join_vai_tro()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.Bang(cn, "SELECT * FROM vw_DanhSachTaiKhoan WHERE TaiKhoanID = @Id", ("@Id", duLieu.AdminID));
        Assert.Equal(1, bang.Rows.Count);
        Assert.Equal("QuanTriVien", bang.Rows[0]["TenVaiTro"] as string);
    }

    [SkippableFact]
    public void vw_DanhSachTaiKhoanBanToChuc_chi_lay_ban_to_chuc()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.Bang(cn, "SELECT * FROM vw_DanhSachTaiKhoanBanToChuc");
        Assert.Contains(bang.Rows.Cast<System.Data.DataRow>(), r => (int)r["TaiKhoanID"] == duLieu.BanToChucID);
        Assert.DoesNotContain(bang.Rows.Cast<System.Data.DataRow>(), r => (int)r["TaiKhoanID"] == duLieu.NguoiDungID);
    }

    [SkippableFact]
    public void vw_ThongKeTaiKhoanTheoVaiTro_co_du_4_vai_tro()
    {
        using var cn = Mo();
        var bang = Sql.Bang(cn, "SELECT * FROM vw_ThongKeTaiKhoanTheoVaiTro");
        Assert.Equal(4, bang.Rows.Count);
        Assert.All(bang.Rows.Cast<System.Data.DataRow>(), r => Assert.NotNull(r["TongSoTaiKhoan"]));
    }

    [SkippableFact]
    public void vw_YeuCauXacThuc_loc_theo_trang_thai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ThucThi(cn, """
            INSERT INTO YeuCauXacThucToChuc
                (TaiKhoanID, SoCCCD, NgaySinh, DiaChiThuongTru, AnhCCCDMatTruoc, AnhCCCDMatSau)
            VALUES (@Tk, @Cccd, '1995-01-01', N'HN', N'/a.jpg', N'/b.jpg');
            """, ("@Tk", duLieu.NguoiDungID), ("@Cccd", "CCCD" + Guid.NewGuid().ToString("N")[..6]));

        var choDuyet = Sql.Bang(cn, "SELECT * FROM vw_YeuCauXacThucChoDuyet");
        Assert.Contains(choDuyet.Rows.Cast<System.Data.DataRow>(), r => (int)r["TaiKhoanID"] == duLieu.NguoiDungID);
        var tatCa = Sql.Bang(cn, "SELECT * FROM vw_DanhSachYeuCauXacThuc WHERE TaiKhoanID = @Id", ("@Id", duLieu.NguoiDungID));
        Assert.Equal(1, tatCa.Rows.Count);
        Assert.Null(tatCa.Rows[0]["HoTenNguoiDuyet"]);
    }

    [SkippableFact]
    public void vw_ThongKeSuKienTheoLoai_dem_theo_trang_thai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.Bang(cn, "SELECT * FROM vw_ThongKeSuKienTheoLoai WHERE LoaiSuKienID = @Id", ("@Id", duLieu.LoaiSuKienID));
        Assert.Equal(1, bang.Rows.Count);
        Assert.Equal(2, Convert.ToInt32(bang.Rows[0]["TongSoSuKien"]));
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["SoSuKienDaDuyet"]));
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["SoSuKienChoDuyet"]));
    }

    [SkippableFact]
    public void vw_ThongTinDuyetSuKien_co_ten_nguoi_tao_va_nguoi_duyet()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.Bang(cn, "SELECT * FROM vw_ThongTinDuyetSuKien WHERE SuKienID = @Id", ("@Id", duLieu.SuKienID));
        Assert.Equal(1, bang.Rows.Count);
        Assert.NotNull(bang.Rows[0]["HoTenNguoiTao"]);
        Assert.NotNull(bang.Rows[0]["HoTenNguoiDuyet"]);
        Assert.Equal("DaDuyet", bang.Rows[0]["TrangThai"] as string);
    }

    [SkippableFact]
    public void vw_DanhSachDiaDiem_dem_luot_dat()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var batDau = DateTime.Now.AddDays(400);
        Sql.ThucThi(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc, TrangThai)
            VALUES (@Sk, @Dd, @A, @B, 1000, 500, N'DaXacNhan');
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID), ("@A", batDau), ("@B", batDau.AddDays(1)));

        var bang = Sql.Bang(cn, "SELECT * FROM vw_DanhSachDiaDiem WHERE DiaDiemID = @Id", ("@Id", duLieu.DiaDiemID));
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["TongLuotDat"]));
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["TongLuotDatDaXacNhan"]));
    }

    [SkippableFact]
    public void vw_DiaDiemDangHoatDong_chi_lay_dang_hoat_dong()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var khoa = Sql.Nguyen(cn, """
            INSERT INTO DiaDiem (TenDiaDiem, DiaChi, SucChua, GiaThue, TrangThai)
            VALUES (N'Khoa', N'Q2', 10, 0, 0);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """);
        var bang = Sql.Bang(cn, "SELECT * FROM vw_DiaDiemDangHoatDong");
        Assert.Contains(bang.Rows.Cast<System.Data.DataRow>(), r => (int)r["DiaDiemID"] == duLieu.DiaDiemID);
        Assert.DoesNotContain(bang.Rows.Cast<System.Data.DataRow>(), r => (int)r["DiaDiemID"] == khoa);
    }

    [SkippableFact]
    public void vw_DanhSachLoaiVe_tinh_so_luong_con_lai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.Bang(cn, "SELECT * FROM vw_DanhSachLoaiVe WHERE LoaiVeID = @Id", ("@Id", duLieu.LoaiVeID));
        Assert.Equal(100, Convert.ToInt32(bang.Rows[0]["SoLuongConLai"]));
    }

    [SkippableFact]
    public void vw_ChiTietDonVe_join_du_thong_tin()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.Bang(cn, "SELECT * FROM vw_ChiTietDonVe WHERE DonVeID = @Id", ("@Id", duLieu.DonVeID));
        Assert.Equal(1, bang.Rows.Count);
        Assert.Equal("Ve thuong", bang.Rows[0]["TenLoaiVe"] as string);
        Assert.Equal(100000m, Convert.ToDecimal(bang.Rows[0]["ThanhTien"]));
    }

    [SkippableFact]
    public void vw_ThongKeThanhToanTheoDonVe_tinh_thuc_thu()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var don = BoDuLieu.TaoDonDaThanhToan(cn, duLieu);
        Sql.ThucThi(cn, """
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan,
                                   SoTien, LoaiGiaoDich, TrangThai)
            VALUES (@Don, N'VNPAY', @Ma, 50000, N'HoanTien', N'ThanhCong');
            """, ("@Don", don.DonVeID), ("@Ma", "GD-RF-" + Guid.NewGuid().ToString("N")[..8]));

        var bang = Sql.Bang(cn, "SELECT * FROM vw_ThongKeThanhToanTheoDonVe WHERE DonVeID = @Id", ("@Id", don.DonVeID));
        Assert.Equal(200000m, Convert.ToDecimal(bang.Rows[0]["TongTienDaThanhToan"]));
        Assert.Equal(50000m, Convert.ToDecimal(bang.Rows[0]["TongTienDaHoan"]));
        Assert.Equal(150000m, Convert.ToDecimal(bang.Rows[0]["SoTienThucThu"]));
    }

    [SkippableFact]
    public void vw_DanhSachVe_va_VeDaCheckIn()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var don = BoDuLieu.TaoDonDaThanhToan(cn, duLieu);
        var ve = BoDuLieu.PhatHanhVe(cn, don.DonVeID, don.LoaiVeID, "VE-VW-" + Guid.NewGuid().ToString("N")[..8]);

        Assert.Equal(1, Sql.Bang(cn, "SELECT * FROM vw_DanhSachVe WHERE VeID = @Id", ("@Id", ve)).Rows.Count);
        Assert.Equal(0, Sql.Bang(cn, "SELECT * FROM vw_DanhSachVeDaCheckIn WHERE VeID = @Id", ("@Id", ve)).Rows.Count);

        Sql.ThucThi(cn, """
            UPDATE Ve SET TrangThai = N'DaCheckIn', ThoiGianCheckIn = SYSDATETIME(), NguoiCheckInID = @Nguoi
            WHERE VeID = @Ve;
            """, ("@Ve", ve), ("@Nguoi", duLieu.ManagerID));

        var daCheckIn = Sql.Bang(cn, "SELECT * FROM vw_DanhSachVeDaCheckIn WHERE VeID = @Id", ("@Id", ve));
        Assert.Equal(1, daCheckIn.Rows.Count);
        Assert.NotNull(daCheckIn.Rows[0]["HoTenNguoiCheckIn"]);
    }

    [SkippableFact]
    public void vw_LichSuXuatVe_va_ThongKeVeTheoSuKien()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var don = BoDuLieu.TaoDonDaThanhToan(cn, duLieu);
        var ve = BoDuLieu.PhatHanhVe(cn, don.DonVeID, don.LoaiVeID, "VE-XV-" + Guid.NewGuid().ToString("N")[..8]);
        Sql.ThucThi(cn, """
            INSERT INTO XuatVe (VeID, NguoiXuatID, HinhThucXuat) VALUES (@Ve, @Nguoi, N'GuiEmail');
            """, ("@Ve", ve), ("@Nguoi", duLieu.ManagerID));

        Assert.Equal(1, Sql.Bang(cn, "SELECT * FROM vw_LichSuXuatVe WHERE VeID = @Id", ("@Id", ve)).Rows.Count);
        var thongKe = Sql.Bang(cn, "SELECT * FROM vw_ThongKeVeTheoSuKien WHERE SuKienID = @Id", ("@Id", duLieu.SuKienID));
        Assert.True(Convert.ToInt32(thongKe.Rows[0]["TongSoVe"]) >= 1);
    }

    [SkippableFact]
    public void vw_DatDiaDiemChoXacNhan_va_LichDatDaXacNhan()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var batDau = DateTime.Now.AddDays(500);
        var id = Sql.Nguyen(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 1000, 500);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID), ("@A", batDau), ("@B", batDau.AddDays(1)));

        Assert.Equal(1, Sql.Bang(cn, "SELECT * FROM vw_DatDiaDiemChoXacNhan WHERE DatDiaDiemID = @Id", ("@Id", id)).Rows.Count);
        Assert.Equal(0, Sql.Bang(cn, "SELECT * FROM vw_LichDatDiaDiemDaXacNhan WHERE DatDiaDiemID = @Id", ("@Id", id)).Rows.Count);
    }
}
