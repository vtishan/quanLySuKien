using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class StoredProcedureNhom4Tests
{
    readonly DatabaseFixture _fixture;

    public StoredProcedureNhom4Tests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    static string Ma(string tienTo) => tienTo + "-" + Guid.NewGuid().ToString("N")[..10];

    [SkippableFact]
    public void sp_ThemLoaiVe_tao_va_chan_du_lieu_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.GoiThuTuc(cn, "sp_ThemLoaiVe",
            ("@SuKienID", duLieu.SuKienID), ("@TenLoaiVe", "VIP"), ("@GiaVe", 300000m),
            ("@SoLuongToiDa", 20), ("@SoLuongToiDaMoiDon", 2),
            ("@ThoiGianMoBan", DateTime.Now.AddDays(-1)), ("@ThoiGianDongBan", DateTime.Now.AddDays(5)));
        Assert.Equal(20, Convert.ToInt32(bang.Rows[0]["SoLuongToiDa"]));
        Assert.Equal(0, Convert.ToInt32(bang.Rows[0]["SoLuongDaBan"]));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemLoaiVe",
            ("@SuKienID", duLieu.SuKienID), ("@TenLoaiVe", " "), ("@GiaVe", 1000m),
            ("@SoLuongToiDa", 5), ("@SoLuongToiDaMoiDon", 1),
            ("@ThoiGianMoBan", DateTime.Now), ("@ThoiGianDongBan", DateTime.Now.AddDays(1))), 50401);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemLoaiVe",
            ("@SuKienID", duLieu.SuKienID), ("@TenLoaiVe", "VIP"), ("@GiaVe", 1000m),
            ("@SoLuongToiDa", 5), ("@SoLuongToiDaMoiDon", 6),
            ("@ThoiGianMoBan", DateTime.Now), ("@ThoiGianDongBan", DateTime.Now.AddDays(1))), 50402);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemLoaiVe",
            ("@SuKienID", duLieu.SuKienChoDuyetID), ("@TenLoaiVe", "VIP"), ("@GiaVe", 1000m),
            ("@SoLuongToiDa", 5), ("@SoLuongToiDaMoiDon", 1),
            ("@ThoiGianMoBan", DateTime.Now), ("@ThoiGianDongBan", DateTime.Now.AddDays(1))), 50404);
    }

    [SkippableFact]
    public void sp_CapNhatLoaiVe_cap_nhat_va_chan_giam_so_luong()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.GoiThuTuc(cn, "sp_CapNhatLoaiVe",
            ("@LoaiVeID", duLieu.LoaiVeID), ("@TenLoaiVe", "Ve thuong"), ("@GiaVe", 120000m),
            ("@SoLuongToiDa", 120), ("@SoLuongToiDaMoiDon", 4),
            ("@ThoiGianMoBan", DateTime.Now.AddDays(-1)), ("@ThoiGianDongBan", DateTime.Now.AddDays(9)));
        Assert.Equal(120000m, Convert.ToDecimal(bang.Rows[0]["GiaVe"]));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CapNhatLoaiVe",
            ("@LoaiVeID", duLieu.LoaiVeID), ("@TenLoaiVe", "Ve thuong"), ("@GiaVe", 120000m),
            ("@SoLuongToiDa", 0), ("@SoLuongToiDaMoiDon", 4),
            ("@ThoiGianMoBan", DateTime.Now.AddDays(-1)), ("@ThoiGianDongBan", DateTime.Now.AddDays(9))), 50410);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CapNhatLoaiVe",
            ("@LoaiVeID", -1), ("@TenLoaiVe", "X"), ("@GiaVe", 1000m),
            ("@SoLuongToiDa", 5), ("@SoLuongToiDaMoiDon", 1),
            ("@ThoiGianMoBan", DateTime.Now), ("@ThoiGianDongBan", DateTime.Now.AddDays(1))), 50412);
    }

    [SkippableFact]
    public void sp_TaoDonVe_tao_va_giu_so_luong()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var giuTruoc = Sql.Nguyen(cn, "SELECT SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", duLieu.LoaiVeID));
        var bang = Sql.GoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", duLieu.LoaiVeID), ("@SoLuong", 3), ("@TongTienGiam", 0m));
        Assert.Equal(300000m, Convert.ToDecimal(bang.Rows[0]["TongTienGoc"]));
        Assert.Equal(300000m, Convert.ToDecimal(bang.Rows[0]["TongTienThanhToan"]));
        Assert.Equal("ChuaThanhToan", bang.Rows[0]["TrangThaiThanhToan"] as string);
        Assert.NotNull(bang.Rows[0]["ThoiGianHetHanThanhToan"]);
        Assert.Equal(giuTruoc + 3, Sql.Nguyen(cn, "SELECT SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", duLieu.LoaiVeID)));
    }

    [SkippableFact]
    public void sp_TaoDonVe_chan_du_lieu_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", duLieu.LoaiVeID), ("@SoLuong", 0)), 50420);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", duLieu.LoaiVeID), ("@SoLuong", 1), ("@TongTienGiam", -1m)), 50421);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", -1), ("@LoaiVeID", duLieu.LoaiVeID), ("@SoLuong", 1)), 50422);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", -1), ("@SoLuong", 1)), 50423);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", duLieu.LoaiVeID), ("@SoLuong", 6)), 50425);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", duLieu.LoaiVeID), ("@SoLuong", 1), ("@TongTienGiam", 200000m)), 50426);
    }

    [SkippableFact]
    public void sp_TaoDonVe_chan_khi_chua_mo_ban_hoac_da_dong_ban()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var chuaMo = Sql.Nguyen(cn, """
            INSERT INTO LoaiVe (SuKienID, TenLoai, GiaVe, SoLuongToiDa, SoLuongToiDaMoiDon,
                                ThoiGianMoBan, ThoiGianDongBan, TrangThai)
            VALUES (@Sk, N'Chua mo', 1000, 10, 2, DATEADD(DAY, 1, SYSDATETIME()), DATEADD(DAY, 2, SYSDATETIME()), 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", duLieu.SuKienID));
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", chuaMo), ("@SoLuong", 1)), 50424);

        var daDong = Sql.Nguyen(cn, """
            INSERT INTO LoaiVe (SuKienID, TenLoai, GiaVe, SoLuongToiDa, SoLuongToiDaMoiDon,
                                ThoiGianMoBan, ThoiGianDongBan, TrangThai)
            VALUES (@Sk, N'Da dong', 1000, 10, 2, DATEADD(DAY, -2, SYSDATETIME()), DATEADD(DAY, -1, SYSDATETIME()), 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", duLieu.SuKienID));
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", daDong), ("@SoLuong", 1)), 50424);
    }

    [SkippableFact]
    public void sp_GhiNhanThanhToan_cap_nhat_va_chan_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.GoiThuTuc(cn, "sp_GhiNhanThanhToan",
            ("@DonVeID", duLieu.DonVeID), ("@SoTien", 100000m), ("@MaGiaoDich", Ma("GD")),
            ("@PhuongThucThanhToan", "VNPAY"), ("@TrangThai", "ThanhCong"));
        Assert.Equal("DaThanhToan", Sql.Mot(cn, "SELECT TrangThaiThanhToan FROM DonVe WHERE DonVeID = @Id", ("@Id", duLieu.DonVeID)) as string);
        Assert.Equal(1, Sql.Nguyen(cn, "SELECT SoLuongDaBan FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", duLieu.LoaiVeID)));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanThanhToan",
            ("@DonVeID", duLieu.DonVeID), ("@SoTien", 100000m), ("@MaGiaoDich", Ma("GD")),
            ("@PhuongThucThanhToan", "VNPAY"), ("@TrangThai", "ThanhCong")), 50433);
    }

    [SkippableFact]
    public void sp_GhiNhanThanhToan_chan_tham_so_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanThanhToan",
            ("@DonVeID", duLieu.DonVeID), ("@SoTien", 0m), ("@MaGiaoDich", Ma("GD")),
            ("@PhuongThucThanhToan", "VNPAY"), ("@TrangThai", "ThanhCong")), 50430);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanThanhToan",
            ("@DonVeID", duLieu.DonVeID), ("@SoTien", 100000m), ("@MaGiaoDich", Ma("GD")),
            ("@PhuongThucThanhToan", "VNPAY"), ("@TrangThai", "Sai")), 50431);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanThanhToan",
            ("@DonVeID", duLieu.DonVeID), ("@SoTien", 100000m), ("@MaGiaoDich", Ma("GD")),
            ("@PhuongThucThanhToan", "Bitcoin"), ("@TrangThai", "ThanhCong")), 50435);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanThanhToan",
            ("@DonVeID", duLieu.DonVeID), ("@SoTien", 100000m), ("@MaGiaoDich", " "),
            ("@PhuongThucThanhToan", "VNPAY"), ("@TrangThai", "ThanhCong")), 50436);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanThanhToan",
            ("@DonVeID", -1), ("@SoTien", 100000m), ("@MaGiaoDich", Ma("GD")),
            ("@PhuongThucThanhToan", "VNPAY"), ("@TrangThai", "ThanhCong")), 50432);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanThanhToan",
            ("@DonVeID", duLieu.DonVeID), ("@SoTien", 99999m), ("@MaGiaoDich", Ma("GD")),
            ("@PhuongThucThanhToan", "VNPAY"), ("@TrangThai", "ThanhCong")), 50434);
    }
}