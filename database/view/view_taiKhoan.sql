-- Nhom 1: Quan ly tai khoan va phan quyen
-- SQL Server

-- V01. Danh sach tai khoan va vai tro
CREATE OR ALTER VIEW dbo.vw_DanhSachTaiKhoan
AS
SELECT
    TK.TaiKhoanID,
    TK.HoTen,
    TK.Email,
    TK.SoDienThoai,
    VT.VaiTroID,
    VT.TenVaiTro,
    TK.TrangThai,
    TK.NgayTao
FROM dbo.TaiKhoan AS TK
INNER JOIN dbo.VaiTro AS VT
    ON VT.VaiTroID = TK.VaiTroID;
GO

-- V02. Danh sach tai khoan ban to chuc
CREATE OR ALTER VIEW dbo.vw_DanhSachTaiKhoanBanToChuc
AS
SELECT
    TK.TaiKhoanID,
    TK.HoTen,
    TK.Email,
    TK.SoDienThoai,
    BTC.BanToChucID,
    BTC.TenToChuc,
    BTC.SoCCCD,
    BTC.NgayXacThuc,
    BTC.TrangThai AS TrangThaiBanToChuc,
    TK.TrangThai AS TrangThaiTaiKhoan
FROM dbo.TaiKhoan AS TK
INNER JOIN dbo.VaiTro AS VT
    ON VT.VaiTroID = TK.VaiTroID
INNER JOIN dbo.BanToChuc AS BTC
    ON BTC.TaiKhoanID = TK.TaiKhoanID
WHERE VT.TenVaiTro = N'BanToChuc';
GO

-- V03. Thong ke tai khoan theo vai tro
CREATE OR ALTER VIEW dbo.vw_ThongKeTaiKhoanTheoVaiTro
AS
SELECT
    VT.VaiTroID,
    VT.TenVaiTro,
    COUNT(TK.TaiKhoanID) AS TongSoTaiKhoan,
    SUM(CASE WHEN TK.TrangThai = 1 THEN 1 ELSE 0 END)
        AS SoTaiKhoanHoatDong,
    SUM(CASE WHEN TK.TrangThai = 0 THEN 1 ELSE 0 END)
        AS SoTaiKhoanBiKhoa
FROM dbo.VaiTro AS VT
LEFT JOIN dbo.TaiKhoan AS TK
    ON TK.VaiTroID = VT.VaiTroID
GROUP BY
    VT.VaiTroID,
    VT.TenVaiTro;
GO

-- V04. Danh sach su kien theo tai khoan tao
CREATE OR ALTER VIEW dbo.vw_SuKienTheoTaiKhoan
AS
SELECT
    TK.TaiKhoanID,
    TK.HoTen,
    TK.Email,
    SK.SuKienID,
    SK.TenSuKien,
    SK.LoaiSuKienID,
    SK.ThoiGianBatDau,
    SK.ThoiGianKetThuc,
    SK.TrangThai AS TrangThaiSuKien,
    SK.NgayTao
FROM dbo.TaiKhoan AS TK
INNER JOIN dbo.SuKien AS SK
    ON SK.NguoiTaoID = TK.TaiKhoanID;
GO

-- V05. Danh sach don ve theo tai khoan
CREATE OR ALTER VIEW dbo.vw_DonVeTheoTaiKhoan
AS
SELECT
    TK.TaiKhoanID,
    TK.HoTen,
    TK.Email,
    DV.DonVeID,
    DV.MaDonHang,
    DV.TongTienGoc,
    DV.TongTienGiam,
    DV.TongTienThanhToan,
    DV.TrangThaiThanhToan,
    DV.TrangThaiDon,
    DV.ThoiGianHetHanThanhToan,
    DV.NgayDat
FROM dbo.TaiKhoan AS TK
INNER JOIN dbo.DonVe AS DV
    ON DV.TaiKhoanID = TK.TaiKhoanID;
GO

-- V06. Danh sach yeu cau xac thuc ban to chuc
CREATE OR ALTER VIEW dbo.vw_YeuCauXacThucBanToChuc
AS
SELECT
    YC.YeuCauID,
    YC.TaiKhoanID,
    TK.HoTen AS HoTenNguoiDangKy,
    TK.Email,
    YC.TrangThai,
    YC.NguoiDuyetID,
    ND.HoTen AS HoTenNguoiDuyet,
    YC.NgayDuyet,
    YC.LyDoTuChoi,
    YC.NgayTao
FROM dbo.YeuCauXacThucToChuc AS YC
INNER JOIN dbo.TaiKhoan AS TK
    ON TK.TaiKhoanID = YC.TaiKhoanID
LEFT JOIN dbo.TaiKhoan AS ND
    ON ND.TaiKhoanID = YC.NguoiDuyetID;
GO