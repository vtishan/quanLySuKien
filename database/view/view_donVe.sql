-- Nhom 4: Quan ly loai ve, don ve, thanh toan, ve va xuat ve
-- SQL Server

-- V01. Danh sach loai ve kem thong tin su kien
CREATE OR ALTER VIEW dbo.vw_DanhSachLoaiVe
AS
SELECT
    LV.LoaiVeID,
    LV.SuKienID,
    SK.TenSuKien,
    SK.TrangThai AS TrangThaiSuKien,
    LV.TenLoai,
    LV.GiaVe,
    LV.SoLuongToiDa,
    LV.SoLuongDaBan,
    LV.SoLuongDangGiu,
    LV.SoLuongToiDa - LV.SoLuongDaBan - LV.SoLuongDangGiu
        AS SoLuongConLai,
    LV.SoLuongToiDaMoiDon,
    LV.ThoiGianMoBan,
    LV.ThoiGianDongBan,
    LV.TrangThai
FROM dbo.LoaiVe AS LV
INNER JOIN dbo.SuKien AS SK
    ON SK.SuKienID = LV.SuKienID;
GO

-- V02. Danh sach don ve kem thong tin nguoi mua
CREATE OR ALTER VIEW dbo.vw_DanhSachDonVe
AS
SELECT
    DV.DonVeID,
    DV.MaDonHang,
    DV.TaiKhoanID,
    TK.HoTen AS HoTenNguoiMua,
    TK.Email,
    DV.TongTienGoc,
    DV.MaGiamGia,
    DV.TongTienGiam,
    DV.TongTienThanhToan,
    DV.TrangThaiThanhToan,
    DV.TrangThaiDon,
    DV.ThoiGianHetHanThanhToan,
    DV.NgayDat
FROM dbo.DonVe AS DV
INNER JOIN dbo.TaiKhoan AS TK
    ON TK.TaiKhoanID = DV.TaiKhoanID;
GO

-- V03. Chi tiet don ve
CREATE OR ALTER VIEW dbo.vw_ChiTietDonVe
AS
SELECT
    CT.ChiTietDonVeID,
    CT.DonVeID,
    DV.MaDonHang,
    DV.TaiKhoanID,
    CT.LoaiVeID,
    LV.TenLoai AS TenLoaiVe,
    LV.SuKienID,
    SK.TenSuKien,
    CT.SoLuong,
    CT.DonGia,
    CT.ThanhTien
FROM dbo.ChiTietDonVe AS CT
INNER JOIN dbo.DonVe AS DV
    ON DV.DonVeID = CT.DonVeID
INNER JOIN dbo.LoaiVe AS LV
    ON LV.LoaiVeID = CT.LoaiVeID
INNER JOIN dbo.SuKien AS SK
    ON SK.SuKienID = LV.SuKienID;
GO

-- V04. Lich su giao dich thanh toan va hoan tien
CREATE OR ALTER VIEW dbo.vw_LichSuThanhToan
AS
SELECT
    TT.ThanhToanID,
    TT.DonVeID,
    DV.MaDonHang,
    DV.TaiKhoanID,
    TK.HoTen AS HoTenNguoiMua,
    TT.PhuongThucThanhToan,
    TT.MaGiaoDichCongThanhToan,
    TT.MaGiaoDichNganHang,
    TT.SoTien,
    TT.LoaiGiaoDich,
    TT.TrangThai,
    TT.NoiDungThanhToan,
    TT.NgayThanhToan
FROM dbo.ThanhToan AS TT
INNER JOIN dbo.DonVe AS DV
    ON DV.DonVeID = TT.DonVeID
INNER JOIN dbo.TaiKhoan AS TK
    ON TK.TaiKhoanID = DV.TaiKhoanID;
GO

-- V05. Thong ke thanh toan theo don ve
CREATE OR ALTER VIEW dbo.vw_ThongKeThanhToanTheoDonVe
AS
SELECT
    DV.DonVeID,
    DV.MaDonHang,
    DV.TaiKhoanID,
    DV.TongTienThanhToan,
    SUM(CASE
        WHEN TT.LoaiGiaoDich = N'ThanhToan'
             AND TT.TrangThai = N'ThanhCong'
        THEN TT.SoTien
        ELSE 0
    END) AS TongTienDaThanhToan,
    SUM(CASE
        WHEN TT.LoaiGiaoDich = N'HoanTien'
             AND TT.TrangThai = N'ThanhCong'
        THEN TT.SoTien
        ELSE 0
    END) AS TongTienDaHoan,
    SUM(CASE
        WHEN TT.LoaiGiaoDich = N'ThanhToan'
             AND TT.TrangThai = N'ThanhCong'
        THEN TT.SoTien
        ELSE 0
    END)
    -
    SUM(CASE
        WHEN TT.LoaiGiaoDich = N'HoanTien'
             AND TT.TrangThai = N'ThanhCong'
        THEN TT.SoTien
        ELSE 0
    END) AS SoTienThucThu
FROM dbo.DonVe AS DV
LEFT JOIN dbo.ThanhToan AS TT
    ON TT.DonVeID = DV.DonVeID
GROUP BY
    DV.DonVeID,
    DV.MaDonHang,
    DV.TaiKhoanID,
    DV.TongTienThanhToan;
GO

-- V06. Danh sach ve kem thong tin su kien va don hang
CREATE OR ALTER VIEW dbo.vw_DanhSachVe
AS
SELECT
    V.VeID,
    V.MaVe,
    V.MaQR,
    V.DonVeID,
    DV.MaDonHang,
    DV.TaiKhoanID,
    V.LoaiVeID,
    LV.TenLoai AS TenLoaiVe,
    LV.SuKienID,
    SK.TenSuKien,
    SK.ThoiGianBatDau,
    SK.ThoiGianKetThuc,
    V.HoTenNguoiSuDung,
    V.EmailNguoiSuDung,
    V.GiaThucTe,
    V.TrangThai,
    V.ThoiGianCheckIn,
    V.NguoiCheckInID,
    V.NgayHuy
FROM dbo.Ve AS V
INNER JOIN dbo.DonVe AS DV
    ON DV.DonVeID = V.DonVeID
INNER JOIN dbo.LoaiVe AS LV
    ON LV.LoaiVeID = V.LoaiVeID
INNER JOIN dbo.SuKien AS SK
    ON SK.SuKienID = LV.SuKienID;
GO

-- V07. Danh sach ve da check-in
CREATE OR ALTER VIEW dbo.vw_DanhSachVeDaCheckIn
AS
SELECT
    V.VeID,
    V.MaVe,
    V.HoTenNguoiSuDung,
    V.EmailNguoiSuDung,
    V.SuKienID,
    V.TenSuKien,
    V.ThoiGianCheckIn,
    V.NguoiCheckInID,
    ND.HoTen AS HoTenNguoiCheckIn
FROM dbo.vw_DanhSachVe AS V
LEFT JOIN dbo.TaiKhoan AS ND
    ON ND.TaiKhoanID = V.NguoiCheckInID
WHERE V.TrangThai = N'DaCheckIn';
GO

-- V08. Thong ke ve theo trang thai
CREATE OR ALTER VIEW dbo.vw_ThongKeVeTheoTrangThai
AS
SELECT
    TrangThai,
    COUNT(*) AS TongSoVe
FROM dbo.Ve
GROUP BY TrangThai;
GO

-- V09. Lich su xuat ve
CREATE OR ALTER VIEW dbo.vw_LichSuXuatVe
AS
SELECT
    XV.XuatVeID,
    XV.VeID,
    V.MaVe,
    V.MaQR,
    V.HoTenNguoiSuDung,
    XV.NguoiXuatID,
    TK.HoTen AS HoTenNguoiXuat,
    XV.HinhThucXuat,
    XV.MaSoPhoiVe,
    XV.FileVeURL,
    XV.LanXuat,
    XV.TrangThai,
    XV.ThoiGianXuat
FROM dbo.XuatVe AS XV
INNER JOIN dbo.Ve AS V
    ON V.VeID = XV.VeID
LEFT JOIN dbo.TaiKhoan AS TK
    ON TK.TaiKhoanID = XV.NguoiXuatID;
GO

-- V10. Thong ke ve theo su kien
CREATE OR ALTER VIEW dbo.vw_ThongKeVeTheoSuKien
AS
SELECT
    SK.SuKienID,
    SK.TenSuKien,
    COUNT(V.VeID) AS TongSoVe,
    SUM(CASE WHEN V.TrangThai = N'GiuCho' THEN 1 ELSE 0 END)
        AS SoVeGiuCho,
    SUM(CASE WHEN V.TrangThai = N'ChuaSuDung' THEN 1 ELSE 0 END)
        AS SoVeChuaSuDung,
    SUM(CASE WHEN V.TrangThai = N'DaCheckIn' THEN 1 ELSE 0 END)
        AS SoVeDaCheckIn,
    SUM(CASE WHEN V.TrangThai = N'DaHuy' THEN 1 ELSE 0 END)
        AS SoVeDaHuy,
    SUM(CASE WHEN V.TrangThai = N'DaHoanTien' THEN 1 ELSE 0 END)
        AS SoVeDaHoanTien
FROM dbo.SuKien AS SK
LEFT JOIN dbo.LoaiVe AS LV
    ON LV.SuKienID = SK.SuKienID
LEFT JOIN dbo.Ve AS V
    ON V.LoaiVeID = LV.LoaiVeID
GROUP BY
    SK.SuKienID,
    SK.TenSuKien;
GO