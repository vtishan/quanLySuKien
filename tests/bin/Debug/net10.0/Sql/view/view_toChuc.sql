-- Nhom 2: Xac thuc ban to chuc va quan ly loai su kien
-- SQL Server

-- V01. Danh sach yeu cau xac thuc ban to chuc
CREATE OR ALTER VIEW dbo.vw_DanhSachYeuCauXacThuc
AS
SELECT
    YC.YeuCauID,
    YC.TaiKhoanID,
    TK.HoTen AS HoTenNguoiDangKy,
    TK.Email,
    TK.SoDienThoai,
    YC.NgaySinh,
    YC.GioiTinh,
    YC.DiaChiThuongTru,
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

-- V02. Danh sach yeu cau dang cho duyet
CREATE OR ALTER VIEW dbo.vw_YeuCauXacThucChoDuyet
AS
SELECT
    YC.YeuCauID,
    YC.TaiKhoanID,
    TK.HoTen,
    TK.Email,
    YC.TrangThai,
    YC.NgayTao
FROM dbo.YeuCauXacThucToChuc AS YC
INNER JOIN dbo.TaiKhoan AS TK
    ON TK.TaiKhoanID = YC.TaiKhoanID
WHERE YC.TrangThai = N'ChoDuyet';
GO

-- V03. Thong ke yeu cau xac thuc theo trang thai
CREATE OR ALTER VIEW dbo.vw_ThongKeYeuCauXacThuc
AS
SELECT
    TrangThai,
    COUNT(*) AS TongSoYeuCau
FROM dbo.YeuCauXacThucToChuc
GROUP BY TrangThai;
GO

-- V04. Danh sach ban to chuc da duoc xac thuc
CREATE OR ALTER VIEW dbo.vw_DanhSachBanToChucDaXacThuc
AS
SELECT
    BTC.BanToChucID,
    BTC.TaiKhoanID,
    TK.HoTen,
    TK.Email,
    TK.SoDienThoai,
    BTC.SoCCCD,
    BTC.TenToChuc,
    BTC.NgayXacThuc,
    BTC.TrangThai AS TrangThaiBanToChuc,
    TK.TrangThai AS TrangThaiTaiKhoan
FROM dbo.BanToChuc AS BTC
INNER JOIN dbo.TaiKhoan AS TK
    ON TK.TaiKhoanID = BTC.TaiKhoanID;
GO

-- V05. Danh sach loai su kien
CREATE OR ALTER VIEW dbo.vw_DanhSachLoaiSuKien
AS
SELECT
    LSK.LoaiSuKienID,
    LSK.TenLoai,
    LSK.MoTa,
    LSK.TrangThai,
    COUNT(SK.SuKienID) AS TongSoSuKien
FROM dbo.LoaiSuKien AS LSK
LEFT JOIN dbo.SuKien AS SK
    ON SK.LoaiSuKienID = LSK.LoaiSuKienID
GROUP BY
    LSK.LoaiSuKienID,
    LSK.TenLoai,
    LSK.MoTa,
    LSK.TrangThai;
GO

-- V06. Thong ke su kien theo loai
CREATE OR ALTER VIEW dbo.vw_ThongKeSuKienTheoLoai
AS
SELECT
    LSK.LoaiSuKienID,
    LSK.TenLoai,
    COUNT(SK.SuKienID) AS TongSoSuKien,
    SUM(CASE WHEN SK.TrangThai = N'ChoDuyet' THEN 1 ELSE 0 END)
        AS SoSuKienChoDuyet,
    SUM(CASE WHEN SK.TrangThai = N'DaDuyet' THEN 1 ELSE 0 END)
        AS SoSuKienDaDuyet,
    SUM(CASE WHEN SK.TrangThai = N'TuChoi' THEN 1 ELSE 0 END)
        AS SoSuKienTuChoi,
    SUM(CASE WHEN SK.TrangThai = N'DangDienRa' THEN 1 ELSE 0 END)
        AS SoSuKienDangDienRa,
    SUM(CASE WHEN SK.TrangThai = N'DaKetThuc' THEN 1 ELSE 0 END)
        AS SoSuKienDaKetThuc,
    SUM(CASE WHEN SK.TrangThai = N'DaHuy' THEN 1 ELSE 0 END)
        AS SoSuKienDaHuy
FROM dbo.LoaiSuKien AS LSK
LEFT JOIN dbo.SuKien AS SK
    ON SK.LoaiSuKienID = LSK.LoaiSuKienID
GROUP BY
    LSK.LoaiSuKienID,
    LSK.TenLoai;
GO

-- V07. Danh sach su kien kem thong tin nguoi tao va nguoi duyet
CREATE OR ALTER VIEW dbo.vw_ThongTinDuyetSuKien
AS
SELECT
    SK.SuKienID,
    SK.TenSuKien,
    LSK.TenLoai,
    SK.NguoiTaoID,
    NT.HoTen AS HoTenNguoiTao,
    SK.NguoiDuyetID,
    ND.HoTen AS HoTenNguoiDuyet,
    SK.ThoiGianBatDau,
    SK.ThoiGianKetThuc,
    SK.TrangThai,
    SK.NgayDuyet,
    SK.LyDoTuChoi,
    SK.NgayTao
FROM dbo.SuKien AS SK
INNER JOIN dbo.LoaiSuKien AS LSK
    ON LSK.LoaiSuKienID = SK.LoaiSuKienID
INNER JOIN dbo.TaiKhoan AS NT
    ON NT.TaiKhoanID = SK.NguoiTaoID
LEFT JOIN dbo.TaiKhoan AS ND
    ON ND.TaiKhoanID = SK.NguoiDuyetID;
GO