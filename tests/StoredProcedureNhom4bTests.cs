using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class StoredProcedureNhom4bTests
{
    readonly DatabaseFixture _fixture;

    public StoredProcedureNhom4bTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    static string Ma(string tienTo) => tienTo + "-" + Guid.NewGuid().ToString("N")[..10];

    static BoDuLieu TaoDonDaTra(SqlConnection cn) => BoDuLieu.Tao(cn);

    static int DonDaThanhToan(SqlConnection cn, BoDuLieu duLieu, int soLuong = 2)
    {
        var donID = Sql.Nguyen(cn, """
            INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienThanhToan,
                               TrangThaiThanhToan, TrangThaiDon, ThoiGianHetHanThanhToan)
            VALUES (@Tk, @Ma, @Tien, @Tien, N'DaThanhToan', N'ThanhCong', DATEADD(MINUTE, 15, SYSDATETIME()));
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", duLieu.NguoiDungID), ("@Ma", Ma("DH")), ("@Tien", 100000m * soLuong));

        Sql.ThucThi(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, @SoLuong, 100000, @Tien);
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan,
                                   SoTien, LoaiGiaoDich, TrangThai)
            VALUES (@Don, N'VNPAY', @Ma, @Tien, N'ThanhToan', N'ThanhCong');
            """, ("@Don", donID), ("@LoaiVe", duLieu.LoaiVeID), ("@SoLuong", soLuong),
            ("@Tien", 100000m * soLuong), ("@Ma", Ma("GD")));
        return donID;
    }

    [SkippableFact]
    public void sp_HuyDonVe_giai_phong_ve_dang_giu()
    {
        using var cn = Mo();
        var duLieu = TaoDonDaTra(cn);
        Sql.GoiThuTuc(cn, "sp_TaoDonVe",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@LoaiVeID", duLieu.LoaiVeID), ("@SoLuong", 2));
        var giuTruoc = Sql.Nguyen(cn, "SELECT SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", duLieu.LoaiVeID));

        var bang = Sql.GoiThuTuc(cn, "sp_HuyDonVe",
            ("@DonVeID", duLieu.DonVeID), ("@TaiKhoanID", duLieu.NguoiDungID));
        Assert.Equal("DaHuy", bang.Rows[0]["TrangThaiDon"] as string);
        Assert.Equal(giuTruoc - 1, Sql.Nguyen(cn, "SELECT SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", duLieu.LoaiVeID)));
    }

    [SkippableFact]
    public void sp_HuyDonVe_chan_sai_chu_so_huu_va_don_da_tra()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_HuyDonVe",
            ("@DonVeID", duLieu.DonVeID), ("@TaiKhoanID", duLieu.BanToChucID)), 50440);

        var daTra = DonDaThanhToan(cn, duLieu);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_HuyDonVe",
            ("@DonVeID", daTra), ("@TaiKhoanID", duLieu.NguoiDungID)), 50440);
    }

    [SkippableFact]
    public void sp_GhiNhanHoanTien_mot_phan_va_toan_phan()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu, 2);

        Sql.GoiThuTuc(cn, "sp_GhiNhanHoanTien",
            ("@DonVeID", donID), ("@SoTienHoan", 50000m), ("@MaGiaoDich", Ma("RF")), ("@TrangThai", "ThanhCong"));
        Assert.Equal("HoanTienMotPhan", Sql.Mot(cn, "SELECT TrangThaiThanhToan FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);

        Sql.GoiThuTuc(cn, "sp_GhiNhanHoanTien",
            ("@DonVeID", donID), ("@SoTienHoan", 150000m), ("@MaGiaoDich", Ma("RF")), ("@TrangThai", "ThanhCong"));
        Assert.Equal("DaHoanTien", Sql.Mot(cn, "SELECT TrangThaiDon FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);
    }

    [SkippableFact]
    public void sp_GhiNhanHoanTien_chan_vuot_so_tien_va_sai_tham_so()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu, 1);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanHoanTien",
            ("@DonVeID", donID), ("@SoTienHoan", 200000m), ("@MaGiaoDich", Ma("RF")), ("@TrangThai", "ThanhCong")), 50453);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanHoanTien",
            ("@DonVeID", donID), ("@SoTienHoan", 0m), ("@MaGiaoDich", Ma("RF")), ("@TrangThai", "ThanhCong")), 50451);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanHoanTien",
            ("@DonVeID", donID), ("@SoTienHoan", 10000m), ("@MaGiaoDich", Ma("RF")), ("@TrangThai", "Sai")), 50450);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanHoanTien",
            ("@DonVeID", donID), ("@SoTienHoan", 10000m), ("@MaGiaoDich", " "), ("@TrangThai", "ThanhCong")), 50454);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanHoanTien",
            ("@DonVeID", -1), ("@SoTienHoan", 10000m), ("@MaGiaoDich", Ma("RF")), ("@TrangThai", "ThanhCong")), 50452);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiNhanHoanTien",
            ("@DonVeID", duLieu.DonVeID), ("@SoTienHoan", 10000m), ("@MaGiaoDich", Ma("RF")), ("@TrangThai", "ThanhCong")), 50455);
    }

    [SkippableFact]
    public void sp_CheckInVe_ghi_nhan_va_chan_check_in_lai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu, 1);
        var ve = BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));

        var bang = Sql.GoiThuTuc(cn, "sp_CheckInVe", ("@VeID", ve), ("@NguoiCheckInID", duLieu.ManagerID));
        Assert.Equal("DaCheckIn", bang.Rows[0]["TrangThai"] as string);
        Assert.IsType<DateTime>(bang.Rows[0]["ThoiGianCheckIn"]);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CheckInVe", ("@VeID", ve), ("@NguoiCheckInID", duLieu.ManagerID)), 50461);
    }

    [SkippableFact]
    public void sp_CheckInVe_chan_sai_quyen_va_ve_chua_thanh_toan()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu, 1);
        var ve = BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CheckInVe",
            ("@VeID", ve), ("@NguoiCheckInID", duLieu.NguoiDungID)), 50460);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CheckInVe", ("@VeID", -1), ("@NguoiCheckInID", duLieu.ManagerID)), 50461);
    }

    [SkippableFact]
    public void sp_TraCuuVeTheoDon_va_GhiLichSuXuatVe()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu, 1);
        var ve = BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));

        Assert.Equal(1, Sql.GoiThuTuc(cn, "sp_TraCuuVeTheoDon", ("@DonVeID", donID)).Rows.Count);
        var bang = Sql.GoiThuTuc(cn, "sp_GhiLichSuXuatVe",
            ("@VeID", ve), ("@HinhThucXuat", "GuiEmail"), ("@NguoiThucHienID", duLieu.ManagerID));
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["LanXuat"]));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiLichSuXuatVe",
            ("@VeID", ve), ("@HinhThucXuat", "Sai"), ("@NguoiThucHienID", duLieu.ManagerID)), 50470);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GhiLichSuXuatVe",
            ("@VeID", -1), ("@HinhThucXuat", "ExportPDF"), ("@NguoiThucHienID", duLieu.ManagerID)), 50471);
    }

    [SkippableFact]
    public void sp_ThongKe_tra_ve_du_lieu()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu, 1);
        BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));

        Assert.True(Sql.GoiThuTuc(cn, "sp_ThongKeVeTheoTrangThai").Rows.Count >= 1);
        Assert.Equal(1, Sql.GoiThuTuc(cn, "sp_ThongKeCheckInTheoSuKien", ("@SuKienID", duLieu.SuKienID)).Rows.Count);
        Assert.True(Sql.GoiThuTuc(cn, "sp_ThongKeGiaoDich").Rows.Count >= 1);
        Assert.True(Sql.GoiThuTuc(cn, "sp_ThongKeGiaoDich", ("@DonVeID", donID)).Rows.Count >= 1);
    }
}