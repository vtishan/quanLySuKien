using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

public sealed class Seed
{
    public int AdminID { get; }
    public int ManagerID { get; }
    public int NguoiDungID { get; }
    public int BanToChucID { get; }
    public int BanToChucKhoaID { get; }
    public int LoaiSuKienID { get; }
    public int LoaiSuKienKhoaID { get; }
    public int SuKienID { get; }
    public int SuKienChoDuyetID { get; }
    public int DiaDiemID { get; }
    public int LoaiVeID { get; }
    public int DonVeID { get; }
    public int DonVeDaThanhToanID { get; }
    public int VeID { get; }

    Seed(int adminID, int managerID, int nguoiDungID, int banToChucID, int banToChucKhoaID,
         int loaiSuKienID, int loaiSuKienKhoaID, int suKienID, int suKienChoDuyetID,
         int diaDiemID, int loaiVeID, int donVeID, int donVeDaThanhToanID, int veID)
    {
        AdminID = adminID;
        ManagerID = managerID;
        NguoiDungID = nguoiDungID;
        BanToChucID = banToChucID;
        BanToChucKhoaID = banToChucKhoaID;
        LoaiSuKienID = loaiSuKienID;
        LoaiSuKienKhoaID = loaiSuKienKhoaID;
        SuKienID = suKienID;
        SuKienChoDuyetID = suKienChoDuyetID;
        DiaDiemID = diaDiemID;
        LoaiVeID = loaiVeID;
        DonVeID = donVeID;
        DonVeDaThanhToanID = donVeDaThanhToanID;
        VeID = veID;
    }

    public static Seed Tao(SqlConnection cn)
    {
        var admin = ThemTaiKhoan(cn, "QuanTriVien", "admin@test.local", 1);
        var manager = ThemTaiKhoan(cn, "QuanLySuKien", "manager@test.local", 1);
        var nguoiDung = ThemTaiKhoan(cn, "NguoiDung", "nguoidung@test.local", 1);
        var btc = ThemTaiKhoan(cn, "BanToChuc", "btc@test.local", 1);
        var btcKhoa = ThemTaiKhoan(cn, "BanToChuc", "btckhoa@test.local", 0);

        foreach (var (taiKhoan, cccd, ten) in new[]
                 {
                     (btc, "012345678901", "To chuc A"),
                     (btcKhoa, "012345678902", "To chuc B")
                 })
        {
            Sql.ThucThi(cn, """
                INSERT INTO YeuCauXacThucToChuc
                    (TaiKhoanID, SoCCCD, NgaySinh, GioiTinh, DiaChiThuongTru,
                     AnhCCCDMatTruoc, AnhCCCDMatSau, TrangThai, NguoiDuyetID, NgayDuyet)
                VALUES (@Tk, @Cccd, '1990-01-01', N'Nam', N'Ha Noi',
                        N'/anh/truoc.jpg', N'/anh/sau.jpg', N'DaDuyet', @Duyet, SYSDATETIME());
                """, ("@Tk", taiKhoan), ("@Cccd", cccd), ("@Duyet", manager));

            Sql.ThucThi(cn, """
                INSERT INTO BanToChuc (TaiKhoanID, SoCCCD, TenToChuc, TrangThai)
                VALUES (@Tk, @Cccd, @Ten, 1);
                """, ("@Tk", taiKhoan), ("@Cccd", cccd), ("@Ten", ten));
        }

        var loai = Sql.Nguyen(cn, """
            INSERT INTO LoaiSuKien (TenLoai, MoTa, TrangThai)
            VALUES (N'Hoi thao', N'Loai dang hoat dong', 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """);

        var loaiKhoa = Sql.Nguyen(cn, """
            INSERT INTO LoaiSuKien (TenLoai, MoTa, TrangThai)
            VALUES (N'Hoi thao da khoa', NULL, 0);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """);

        var suKien = Sql.Nguyen(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau,
                                ThoiGianKetThuc, SoLuongNguoiDuKien, TrangThai,
                                NguoiDuyetID, NgayDuyet)
            VALUES (@Tk, @Loai, N'Su kien da duyet', DATEADD(DAY, 10, SYSDATETIME()),
                    DATEADD(DAY, 11, SYSDATETIME()), 100, N'DaDuyet', @Duyet, SYSDATETIME());
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", btc), ("@Loai", loai), ("@Duyet", manager));

        var suKienCho = Sql.Nguyen(cn, """
            INSERT INTO SuKien (NguoiTaoID, LoaiSuKienID, TenSuKien, ThoiGianBatDau,
                                ThoiGianKetThuc, SoLuongNguoiDuKien)
            VALUES (@Tk, @Loai, N'Su kien cho duyet', DATEADD(DAY, 20, SYSDATETIME()),
                    DATEADD(DAY, 21, SYSDATETIME()), 50);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", btc), ("@Loai", loai));

        var diaDiem = Sql.Nguyen(cn, """
            INSERT INTO DiaDiem (TenDiaDiem, DiaChi, SucChua, GiaThue, TrangThai)
            VALUES (N'Trung tam hoi nghi', N'Quan 1', 500, 1000000, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """);

        var loaiVe = Sql.Nguyen(cn, """
            INSERT INTO LoaiVe (SuKienID, TenLoai, GiaVe, SoLuongToiDa, SoLuongToiDaMoiDon,
                                ThoiGianMoBan, ThoiGianDongBan, TrangThai)
            VALUES (@Sk, N'Ve thuong', 100000, 100, 5,
                    DATEADD(DAY, -1, SYSDATETIME()), DATEADD(DAY, 9, SYSDATETIME()), 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", suKien));

        var donVe = Sql.Nguyen(cn, """
            INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienGiam,
                               TongTienThanhToan, ThoiGianHetHanThanhToan)
            VALUES (@Tk, N'DH-CHO-THANH-TOAN', 100000, 0, 100000, DATEADD(MINUTE, 15, SYSDATETIME()));
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", nguoiDung));

        Sql.ThucThi(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, 1, 100000, 100000);
            """, ("@Don", donVe), ("@LoaiVe", loaiVe));

        var donVeDaTra = Sql.Nguyen(cn, """
            INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienGiam,
                               TongTienThanhToan, TrangThaiThanhToan, TrangThaiDon,
                               ThoiGianHetHanThanhToan)
            VALUES (@Tk, N'DH-DA-THANH-TOAN', 200000, 0, 200000,
                    N'DaThanhToan', N'ThanhCong', DATEADD(MINUTE, 15, SYSDATETIME()));
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", nguoiDung));

        Sql.ThucThi(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, 2, 100000, 200000);
            """, ("@Don", donVeDaTra), ("@LoaiVe", loaiVe));

        Sql.ThucThi(cn, """
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan,
                                   SoTien, LoaiGiaoDich, TrangThai)
            VALUES (@Don, N'VNPAY', N'GD-DA-THANH-TOAN', 200000, N'ThanhToan', N'ThanhCong');
            """, ("@Don", donVeDaTra));

        var ve = Sql.Nguyen(cn, """
            INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
            VALUES (@Don, @LoaiVe, N'VE-TEST-1', N'QR-TEST-1', 100000, N'ChuaSuDung');
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Don", donVeDaTra), ("@LoaiVe", loaiVe));

        return new Seed(admin, manager, nguoiDung, btc, btcKhoa, loai, loaiKhoa,
            suKien, suKienCho, diaDiem, loaiVe, donVe, donVeDaTra, ve);
    }

    static int ThemTaiKhoan(SqlConnection cn, string vaiTro, string email, int trangThai) =>
        Sql.Nguyen(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash, TrangThai)
            SELECT VaiTroID, @Ten, @Email, N'hash', @TrangThai FROM VaiTro WHERE TenVaiTro = @VaiTro;
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@VaiTro", vaiTro), ("@Ten", email), ("@Email", email), ("@TrangThai", trangThai));
}
