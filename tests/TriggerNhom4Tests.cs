using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class TriggerNhom4Tests
{
    readonly DatabaseFixture _fixture;

    public TriggerNhom4Tests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    static string Ma(string tienTo) => tienTo + "-" + Guid.NewGuid().ToString("N")[..10];

    static int DonDaThanhToan(SqlConnection cn, BoDuLieu duLieu, int soLuong = 1)
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
    public void trg_LoaiVe_chan_du_lieu_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO LoaiVe (SuKienID, TenLoai, GiaVe, SoLuongToiDa, SoLuongToiDaMoiDon,
                                ThoiGianMoBan, ThoiGianDongBan)
            VALUES (@Sk, N'   ', 1000, 10, 1, SYSDATETIME(), DATEADD(DAY, 1, SYSDATETIME()))
            """, ("@Sk", duLieu.SuKienID)), 50401);
    }

    [SkippableFact]
    public void trg_DonVe_chan_tong_tien_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienThanhToan, ThoiGianHetHanThanhToan)
            VALUES (@Tk, @Ma, 100000, 90000, DATEADD(MINUTE, 15, SYSDATETIME()))
            """, ("@Tk", duLieu.NguoiDungID), ("@Ma", Ma("DH"))), 547);
    }

    [SkippableFact]
    public void trg_ChiTietDonVe_chan_thanh_tien_sai_va_don_da_tra()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, 2, 100000, 100000)
            """, ("@Don", duLieu.DonVeID), ("@LoaiVe", duLieu.LoaiVeID)), 547);

        var daTra = DonDaThanhToan(cn, duLieu);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, 1, 100000, 100000)
            """, ("@Don", daTra), ("@LoaiVe", duLieu.LoaiVeID)), 50406);
    }

    [SkippableFact]
    public void trg_ChiTietDonVe_chan_vuot_gioi_han_mua()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, 6, 100000, 600000)
            """, ("@Don", duLieu.DonVeID), ("@LoaiVe", duLieu.LoaiVeID)), 50405);
    }

    [SkippableFact]
    public void trg_ThanhToan_chan_giao_dich_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan, SoTien)
            VALUES (@Don, N'   ', @Ma, 100000)
            """, ("@Don", duLieu.DonVeID), ("@Ma", Ma("GD"))), 50409);

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan, SoTien)
            VALUES (@Don, N'VNPAY', @Ma, 0)
            """, ("@Don", duLieu.DonVeID), ("@Ma", Ma("GD"))), 547);
    }

    [SkippableFact]
    public void trg_ThanhToan_chan_hoan_tien_vuot_so_da_tra()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan,
                                   SoTien, LoaiGiaoDich, TrangThai)
            VALUES (@Don, N'VNPAY', @Ma, 200000, N'HoanTien', N'ThanhCong')
            """, ("@Don", donID), ("@Ma", Ma("RF"))), 50410);
    }

    [SkippableFact]
    public void trg_ThanhToan_chan_xoa_lich_su()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, "DELETE FROM ThanhToan WHERE DonVeID = @Id", ("@Id", duLieu.DonVeID)), 50419);
    }

    [SkippableFact]
    public void trg_Ve_chan_trang_thai_sai_va_check_in_khong_hop_le()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, @Ma, @Qr, 100000, N'SaiTrangThai')
            """, ("@Don", donID), ("@LoaiVe", duLieu.LoaiVeID), ("@Ma", Ma("VE")), ("@Qr", Ma("QR"))), 50411);

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, @Ma, @Qr, 100000, N'DaCheckIn')
            """, ("@Don", donID), ("@LoaiVe", duLieu.LoaiVeID), ("@Ma", Ma("VE")), ("@Qr", Ma("QR"))), 50412);
    }

    [SkippableFact]
    public void trg_Ve_chan_phat_hanh_khi_chua_thanh_toan()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, @Ma, @Qr, 100000, N'ChuaSuDung')
            """, ("@Don", duLieu.DonVeID), ("@LoaiVe", duLieu.LoaiVeID), ("@Ma", Ma("VE")), ("@Qr", Ma("QR"))), 50414);
    }

    [SkippableFact]
    public void trg_Ve_chan_loai_ve_khong_thuoc_don()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu);
        var loaiKhac = Sql.Nguyen(cn, """
            INSERT INTO LoaiVe (SuKienID, TenLoai, GiaVe, SoLuongToiDa, SoLuongToiDaMoiDon,
                                ThoiGianMoBan, ThoiGianDongBan, TrangThai)
            VALUES (@Sk, N'Khac', 100000, 10, 1, DATEADD(DAY, -1, SYSDATETIME()), DATEADD(DAY, 5, SYSDATETIME()), 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", duLieu.SuKienID));

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, @Ma, @Qr, 100000, N'ChuaSuDung')
            """, ("@Don", donID), ("@LoaiVe", loaiKhac), ("@Ma", Ma("VE")), ("@Qr", Ma("QR"))), 50415);
    }

    [SkippableFact]
    public void trg_Ve_chan_phat_hanh_vuot_so_luong_da_mua()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu, 1);
        BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, @Ma, @Qr, 100000, N'ChuaSuDung')
            """, ("@Don", donID), ("@LoaiVe", duLieu.LoaiVeID), ("@Ma", Ma("VE")), ("@Qr", Ma("QR"))), 50416);
    }

    [SkippableFact]
    public void trg_Ve_chan_xoa_ve()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu);
        var ve = BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));
        Sql.ChacLoi(Sql.LoiCauLenh(cn, "DELETE FROM Ve WHERE VeID = @Id", ("@Id", ve)), 50420);
    }

    [SkippableFact]
    public void trg_XuatVe_chan_hinh_thuc_sai_va_ve_khong_hop_le()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu);
        var ve = BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO XuatVe (VeID, HinhThucXuat) VALUES (@Ve, N'SaiHinhThuc')
            """, ("@Ve", ve)), 50417);

        var veGiuCho = Sql.Nguyen(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, @Ma, @Qr, 100000, N'GiuCho');
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Don", donID), ("@LoaiVe", duLieu.LoaiVeID), ("@Ma", Ma("VE")), ("@Qr", Ma("QR")));
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO XuatVe (VeID, HinhThucXuat) VALUES (@Ve, N'ExportPDF')
            """, ("@Ve", veGiuCho)), 50418);
    }

    [SkippableFact]
    public void trg_XuatVe_chan_xoa_lich_su()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu);
        var ve = BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));
        var xuatID = Sql.Nguyen(cn, """
            INSERT INTO XuatVe (VeID, HinhThucXuat) VALUES (@Ve, N'ExportPDF');
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Ve", ve));
        Sql.ChacLoi(Sql.LoiCauLenh(cn, "DELETE FROM XuatVe WHERE XuatVeID = @Id", ("@Id", xuatID)), 50421);
    }
}