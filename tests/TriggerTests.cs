using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class TriggerTests
{
    readonly DatabaseFixture _fixture;

    public TriggerTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    static string Ma(string tienTo) => tienTo + "-" + Guid.NewGuid().ToString("N")[..10];

    [SkippableFact]
    public void trg_VaiTro_chan_ten_rong()
    {
        using var cn = Mo();
        Sql.ChacLoi(Sql.LoiCauLenh(cn, "INSERT INTO VaiTro (TenVaiTro) VALUES (N'   ')"), 50001);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, "INSERT INTO VaiTro (TenVaiTro) VALUES (NULL)"), 50001);
    }

    [SkippableFact]
    public void trg_VaiTro_chan_xoa_vai_tro_dang_dung_nhung_cho_xoa_vai_tro_rong()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var dangDung = Sql.Nguyen(cn, "SELECT VaiTroID FROM TaiKhoan WHERE TaiKhoanID = @Id", ("@Id", duLieu.AdminID));
        Sql.ChacLoi(Sql.LoiCauLenh(cn, "DELETE FROM VaiTro WHERE VaiTroID = @Id", ("@Id", dangDung)), 50003);

        var rong = Sql.Nguyen(cn, """
            INSERT INTO VaiTro (TenVaiTro) VALUES (@Ten);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Ten", "VaiTro-" + Guid.NewGuid().ToString("N")[..8]));
        Assert.Equal(1, Sql.ThucThi(cn, "DELETE FROM VaiTro WHERE VaiTroID = @Id", ("@Id", rong)));
    }

    [SkippableFact]
    public void trg_TaiKhoan_chan_thong_tin_rong()
    {
        using var cn = Mo();
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash)
            SELECT VaiTroID, N'   ', @Email, N'hash' FROM VaiTro WHERE TenVaiTro = N'NguoiDung'
            """, ("@Email", "x-" + Guid.NewGuid().ToString("N")[..8] + "@t.local")), 50002);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash)
            SELECT VaiTroID, N'X', @Email, N'   ' FROM VaiTro WHERE TenVaiTro = N'NguoiDung'
            """, ("@Email", "y-" + Guid.NewGuid().ToString("N")[..8] + "@t.local")), 50002);
    }

    [SkippableFact]
    public void trg_TaiKhoan_chan_xoa_tai_khoan_co_du_lieu()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, "DELETE FROM TaiKhoan WHERE TaiKhoanID = @Id", ("@Id", duLieu.NguoiDungID)), 50004);

        var sach = BoDuLieu.TaiKhoan(cn, "NguoiDung", "sach-" + Guid.NewGuid().ToString("N")[..8] + "@t.local");
        Assert.Equal(1, Sql.ThucThi(cn, "DELETE FROM TaiKhoan WHERE TaiKhoanID = @Id", ("@Id", sach)));
    }

    [SkippableFact]
    public void trg_YeuCauXacThuc_chan_trang_thai_va_thieu_nguoi_duyet()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO YeuCauXacThucToChuc
                (TaiKhoanID, SoCCCD, NgaySinh, DiaChiThuongTru, AnhCCCDMatTruoc, AnhCCCDMatSau, TrangThai)
            VALUES (@Tk, @Cccd, '1990-01-01', N'HN', N'/a', N'/b', N'SaiTrangThai')
            """, ("@Tk", duLieu.NguoiDungID), ("@Cccd", Ma("CC"))), 50101);

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO YeuCauXacThucToChuc
                (TaiKhoanID, SoCCCD, NgaySinh, DiaChiThuongTru, AnhCCCDMatTruoc, AnhCCCDMatSau, TrangThai)
            VALUES (@Tk, @Cccd, '1990-01-01', N'HN', N'/a', N'/b', N'DaDuyet')
            """, ("@Tk", duLieu.NguoiDungID), ("@Cccd", Ma("CC"))), 50102);

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO YeuCauXacThucToChuc
                (TaiKhoanID, SoCCCD, NgaySinh, DiaChiThuongTru, AnhCCCDMatTruoc, AnhCCCDMatSau,
                 TrangThai, NguoiDuyetID, NgayDuyet, LyDoTuChoi)
            VALUES (@Tk, @Cccd, '1990-01-01', N'HN', N'/a', N'/b', N'TuChoi', @Duyet, SYSDATETIME(), N'   ')
            """, ("@Tk", duLieu.NguoiDungID), ("@Cccd", Ma("CC")), ("@Duyet", duLieu.ManagerID)), 50103);
    }

    [SkippableFact]
    public void trg_BanToChuc_chan_ho_so_khong_co_yeu_cau_da_duyet()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO BanToChuc (TaiKhoanID, SoCCCD, TenToChuc, TrangThai)
            VALUES (@Tk, @Cccd, N'To chuc', 1)
            """, ("@Tk", duLieu.NguoiDungID), ("@Cccd", Ma("CC"))), 50106);
    }

    [SkippableFact]
    public void trg_LoaiSuKien_chan_ten_rong()
    {
        using var cn = Mo();
        Sql.ChacLoi(Sql.LoiCauLenh(cn, "INSERT INTO LoaiSuKien (TenLoai) VALUES (N'  ')"), 50107);
    }

    [SkippableFact]
    public void trg_SuKien_chan_thoi_gian_va_trang_thai_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau,
                                ThoiGianKetThuc, SoLuongNguoiDuKien)
            VALUES (@Tk, @Loai, N'X', @A, @B, 10)
            """, ("@Tk", duLieu.BanToChucID), ("@Loai", duLieu.LoaiSuKienID),
            ("@A", DateTime.Now), ("@B", DateTime.Now.AddDays(-1))), 50108);

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau,
                                ThoiGianKetThuc, SoLuongNguoiDuKien, TrangThai)
            VALUES (@Tk, @Loai, N'X', SYSDATETIME(), DATEADD(DAY, 1, SYSDATETIME()), 10, N'SaiTrangThai')
            """, ("@Tk", duLieu.BanToChucID), ("@Loai", duLieu.LoaiSuKienID)), 50109);
    }

    [SkippableFact]
    public void trg_SuKien_chan_tu_duyet_va_nguoi_tao_khong_xac_thuc()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau, ThoiGianKetThuc,
                                SoLuongNguoiDuKien, TrangThai, NguoiDuyetID, NgayDuyet)
            VALUES (@Tk, @Loai, N'X', SYSDATETIME(), DATEADD(DAY, 1, SYSDATETIME()),
                    10, N'DaDuyet', @Tk, SYSDATETIME())
            """, ("@Tk", duLieu.BanToChucID), ("@Loai", duLieu.LoaiSuKienID)), 50110);

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau,
                                ThoiGianKetThuc, SoLuongNguoiDuKien)
            VALUES (@Tk, @Loai, N'X', SYSDATETIME(), DATEADD(DAY, 1, SYSDATETIME()), 10)
            """, ("@Tk", duLieu.NguoiDungID), ("@Loai", duLieu.LoaiSuKienID)), 50113);
    }

    [SkippableFact]
    public void trg_DiaDiem_va_DatDiaDiem_chan_du_lieu_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var loi = Sql.LoiCauLenh(cn, """
            INSERT INTO DiaDiem (TenDiaDiem, DiaChi, SucChua, GiaThue)
            VALUES (N'X', N'   ', 10, 0)
            """);
        Assert.Contains(loi.Number, new[] { 50301, 547 });

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @A, 100, 50)
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID), ("@A", DateTime.Now)), 50302);
    }

    [SkippableFact]
    public void trg_DatDiaDiem_chan_trung_lich()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var batDau = DateTime.Now.AddDays(20);
        Sql.ThucThi(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 100, 50)
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID),
            ("@A", batDau), ("@B", batDau.AddDays(1)));

        Sql.ChacLoi(Sql.LoiCauLenh(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 100, 50)
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID),
            ("@A", batDau.AddHours(1)), ("@B", batDau.AddDays(1))), 50306);

        Sql.ThucThi(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 100, 50)
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID),
            ("@A", batDau.AddDays(2)), ("@B", batDau.AddDays(3)));
    }

    [SkippableFact]
    public void trg_DiaDiem_chan_ngung_khi_con_lich()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var batDau = DateTime.Now.AddDays(30);
        Sql.ThucThi(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 100, 50)
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID),
            ("@A", batDau), ("@B", batDau.AddDays(1)));

        Sql.ChacLoi(Sql.LoiCauLenh(cn,
            "UPDATE DiaDiem SET TrangThai = 0 WHERE DiaDiemID = @Id", ("@Id", duLieu.DiaDiemID)), 50307);
    }
}