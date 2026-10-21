using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

public sealed class BoDuLieu
{
    public int AdminID { get; private init; }
    public int ManagerID { get; private init; }
    public int NguoiDungID { get; private init; }
    public int BanToChucID { get; private init; }
    public int LoaiSuKienID { get; private init; }
    public int SuKienID { get; private init; }
    public int SuKienChoDuyetID { get; private init; }
    public int DiaDiemID { get; private init; }
    public int LoaiVeID { get; private init; }
    public int DonVeID { get; private init; }

    public static BoDuLieu Tao(SqlConnection cn)
    {
        var duoi = Guid.NewGuid().ToString("N")[..8];

        var admin = TaiKhoan(cn, "QuanTriVien", $"admin-{duoi}@test.local", 1);
        var manager = TaiKhoan(cn, "QuanLySuKien", $"manager-{duoi}@test.local", 1);
        var nguoiDung = TaiKhoan(cn, "NguoiDung", $"nguoidung-{duoi}@test.local", 1);
        var banToChuc = TaiKhoan(cn, "BanToChuc", $"btc-{duoi}@test.local", 1);

        Sql.ThucThi(cn, """
            INSERT INTO YeuCauXacThucToChuc
                (TaiKhoanID, SoCCCD, NgaySinh, DiaChiThuongTru, AnhCCCDMatTruoc,
                 AnhCCCDMatSau, TrangThai, NguoiDuyetID, NgayDuyet)
            VALUES (@Tk, @Cccd, '1990-01-01', N'Ha Noi', N'/a.jpg', N'/b.jpg',
                    N'DaDuyet', @Duyet, SYSDATETIME());
            INSERT INTO BanToChuc (TaiKhoanID, SoCCCD, TenToChuc, TrangThai)
            VALUES (@Tk, @Cccd, @Ten, 1);
            """, ("@Tk", banToChuc), ("@Cccd", "CCCD" + duoi), ("@Duyet", manager), ("@Ten", "To chuc " + duoi));

        var loai = Sql.Nguyen(cn, """
            INSERT INTO LoaiSuKien (TenLoai, TrangThai) VALUES (@Ten, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Ten", "Loai " + duoi));

        var suKien = Sql.Nguyen(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau,
                                ThoiGianKetThuc, SoLuongNguoiDuKien, TrangThai, NguoiDuyetID, NgayDuyet)
            VALUES (@Tk, @Loai, N'Da duyet', DATEADD(DAY, 10, SYSDATETIME()),
                    DATEADD(DAY, 11, SYSDATETIME()), 100, N'DaDuyet', @Duyet, SYSDATETIME());
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", banToChuc), ("@Loai", loai), ("@Duyet", manager));

        var suKienCho = Sql.Nguyen(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau,
                                ThoiGianKetThuc, SoLuongNguoiDuKien)
            VALUES (@Tk, @Loai, N'Cho duyet', DATEADD(DAY, 20, SYSDATETIME()),
                    DATEADD(DAY, 21, SYSDATETIME()), 50);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", banToChuc), ("@Loai", loai));

        var diaDiem = Sql.Nguyen(cn, """
            INSERT INTO DiaDiem (TenDiaDiem, DiaChi, SucChua, GiaThue, TrangThai)
            VALUES (@Ten, N'Quan 1', 500, 1000000, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Ten", "Dia diem " + duoi));

        var loaiVe = Sql.Nguyen(cn, """
            INSERT INTO LoaiVe (SuKienID, TenLoai, GiaVe, SoLuongToiDa, SoLuongToiDaMoiDon,
                                ThoiGianMoBan, ThoiGianDongBan, TrangThai)
            VALUES (@Sk, N'Ve thuong', 100000, 100, 5,
                    DATEADD(DAY, -1, SYSDATETIME()), DATEADD(DAY, 9, SYSDATETIME()), 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", suKien));

        var donVe = Sql.Nguyen(cn, """
            INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienThanhToan, ThoiGianHetHanThanhToan)
            VALUES (@Tk, @Ma, 100000, 100000, DATEADD(MINUTE, 15, SYSDATETIME()));
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", nguoiDung), ("@Ma", "DH-" + duoi));

        Sql.ThucThi(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, 1, 100000, 100000);
            """, ("@Don", donVe), ("@LoaiVe", loaiVe));

        return new BoDuLieu
        {
            AdminID = admin,
            ManagerID = manager,
            NguoiDungID = nguoiDung,
            BanToChucID = banToChuc,
            LoaiSuKienID = loai,
            SuKienID = suKien,
            SuKienChoDuyetID = suKienCho,
            DiaDiemID = diaDiem,
            LoaiVeID = loaiVe,
            DonVeID = donVe
        };
    }

    public static int TaiKhoan(SqlConnection cn, string vaiTro, string email, int trangThai = 1) =>
        Sql.Nguyen(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash, TrangThai)
            SELECT VaiTroID, @Email, @Email, N'hash', @TrangThai FROM VaiTro WHERE TenVaiTro = @VaiTro;
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@VaiTro", vaiTro), ("@Email", email), ("@TrangThai", trangThai));

    public int ThanhToanDon(SqlConnection cn, decimal soTien)
    {
        var ma = "GD-" + Guid.NewGuid().ToString("N")[..10];
        Sql.ThucThi(cn, """
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan,
                                   SoTien, LoaiGiaoDich, TrangThai)
            VALUES (@Don, N'VNPAY', @Ma, @Tien, N'ThanhToan', N'ThanhCong');
            UPDATE DonVe SET TrangThaiThanhToan = N'DaThanhToan', TrangThaiDon = N'ThanhCong'
            WHERE DonVeID = @Don;
            """, ("@Don", DonVeID), ("@Ma", ma), ("@Tien", soTien));
        return DonVeID;
    }

    public int PhatHanhVe(SqlConnection cn, string maVe)
    {
        return Sql.Nguyen(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, @Ma, @Qr, 100000, N'ChuaSuDung');
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Don", DonVeID), ("@LoaiVe", LoaiVeID), ("@Ma", maVe), ("@Qr", "QR-" + maVe));
    }

    public static (int DonVeID, int LoaiVeID) TaoDonDaThanhToan(SqlConnection cn, BoDuLieu duLieu)
    {
        var donID = Sql.Nguyen(cn, """
            INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienThanhToan,
                               TrangThaiThanhToan, TrangThaiDon, ThoiGianHetHanThanhToan)
            VALUES (@Tk, @Ma, 200000, 200000, N'DaThanhToan', N'ThanhCong', DATEADD(MINUTE, 15, SYSDATETIME()));
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", duLieu.NguoiDungID), ("@Ma", "DH-TT-" + Guid.NewGuid().ToString("N")[..8]));

        Sql.ThucThi(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, 2, 100000, 200000);
            """, ("@Don", donID), ("@LoaiVe", duLieu.LoaiVeID));

        Sql.ThucThi(cn, """
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan,
                                   SoTien, LoaiGiaoDich, TrangThai)
            VALUES (@Don, N'VNPAY', @Ma, 200000, N'ThanhToan', N'ThanhCong');
            """, ("@Don", donID), ("@Ma", "GD-" + Guid.NewGuid().ToString("N")[..10]));

        return (donID, duLieu.LoaiVeID);
    }

    public static int PhatHanhVe(SqlConnection cn, int donVeID, int loaiVeID, string maVe) =>
        Sql.Nguyen(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, @Ma, @Qr, 100000, N'ChuaSuDung');
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Don", donVeID), ("@LoaiVe", loaiVeID), ("@Ma", maVe), ("@Qr", "QR-" + maVe));
}
