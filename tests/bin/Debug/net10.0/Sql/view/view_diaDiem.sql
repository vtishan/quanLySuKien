-- Nhom 3: Quan ly dia diem va dat dia diem
-- SQL Server

-- V01. Danh sach dia diem va thong tin suc chua
CREATE OR ALTER VIEW dbo.vw_DanhSachDiaDiem
AS
SELECT
    DD.DiaDiemID,
    DD.TenDiaDiem,
    DD.DiaChi,
    DD.SucChua,
    DD.GiaThue,
    DD.MoTa,
    DD.TrangThai,
    COUNT(DISTINCT DDD.DatDiaDiemID) AS TongLuotDat,
    COUNT(DISTINCT CASE
        WHEN DDD.TrangThai = N'DaXacNhan'
        THEN DDD.DatDiaDiemID
    END) AS TongLuotDatDaXacNhan
FROM dbo.DiaDiem AS DD
LEFT JOIN dbo.DatDiaDiem AS DDD
    ON DDD.DiaDiemID = DD.DiaDiemID
GROUP BY
    DD.DiaDiemID,
    DD.TenDiaDiem,
    DD.DiaChi,
    DD.SucChua,
    DD.GiaThue,
    DD.MoTa,
    DD.TrangThai;
GO

-- V02. Danh sach dia diem dang hoat dong
CREATE OR ALTER VIEW dbo.vw_DiaDiemDangHoatDong
AS
SELECT
    DiaDiemID,
    TenDiaDiem,
    DiaChi,
    SucChua,
    GiaThue,
    MoTa
FROM dbo.DiaDiem
WHERE TrangThai = 1;
GO

-- V03. Danh sach dat dia diem kem thong tin su kien
CREATE OR ALTER VIEW dbo.vw_DanhSachDatDiaDiem
AS
SELECT
    DDD.DatDiaDiemID,
    DDD.SuKienID,
    SK.TenSuKien,
    SK.NguoiTaoID,
    TK.HoTen AS HoTenNguoiDat,
    DDD.DiaDiemID,
    DD.TenDiaDiem,
    DD.DiaChi,
    DDD.ThoiGianBatDau,
    DDD.ThoiGianKetThuc,
    DDD.TongTien,
    DDD.TienCoc,
    DDD.TrangThai,
    DDD.NgayDat
FROM dbo.DatDiaDiem AS DDD
INNER JOIN dbo.SuKien AS SK
    ON SK.SuKienID = DDD.SuKienID
INNER JOIN dbo.TaiKhoan AS TK
    ON TK.TaiKhoanID = SK.NguoiTaoID
INNER JOIN dbo.DiaDiem AS DD
    ON DD.DiaDiemID = DDD.DiaDiemID;
GO

-- V04. Danh sach lich dat dia diem da xac nhan
CREATE OR ALTER VIEW dbo.vw_LichDatDiaDiemDaXacNhan
AS
SELECT
    DDD.DatDiaDiemID,
    DDD.DiaDiemID,
    DD.TenDiaDiem,
    DD.DiaChi,
    DDD.SuKienID,
    SK.TenSuKien,
    DDD.ThoiGianBatDau,
    DDD.ThoiGianKetThuc,
    DDD.TrangThai
FROM dbo.DatDiaDiem AS DDD
INNER JOIN dbo.DiaDiem AS DD
    ON DD.DiaDiemID = DDD.DiaDiemID
INNER JOIN dbo.SuKien AS SK
    ON SK.SuKienID = DDD.SuKienID
WHERE DDD.TrangThai = N'DaXacNhan';
GO

-- V05. Thong ke luot dat dia diem theo trang thai
CREATE OR ALTER VIEW dbo.vw_ThongKeDatDiaDiemTheoTrangThai
AS
SELECT
    TrangThai,
    COUNT(*) AS TongSoDat,
    SUM(TongTien) AS TongGiaTriDat,
    SUM(TienCoc) AS TongTienCoc
FROM dbo.DatDiaDiem
GROUP BY TrangThai;
GO

-- V06. Thong ke doanh thu va tien coc theo dia diem
CREATE OR ALTER VIEW dbo.vw_ThongKeDatDiaDiemTheoDiaDiem
AS
SELECT
    DD.DiaDiemID,
    DD.TenDiaDiem,
    COUNT(DDD.DatDiaDiemID) AS TongLuotDat,
    SUM(CASE
        WHEN DDD.TrangThai IN (N'DaXacNhan', N'HoanThanh')
        THEN DDD.TongTien
        ELSE 0
    END) AS TongGiaTriDatDaChapNhan,
    SUM(CASE
        WHEN DDD.TrangThai IN (N'DaXacNhan', N'HoanThanh')
        THEN DDD.TienCoc
        ELSE 0
    END) AS TongTienCocDaChapNhan
FROM dbo.DiaDiem AS DD
LEFT JOIN dbo.DatDiaDiem AS DDD
    ON DDD.DiaDiemID = DD.DiaDiemID
GROUP BY
    DD.DiaDiemID,
    DD.TenDiaDiem;
GO

-- V07. Danh sach dat dia diem cho xac nhan
CREATE OR ALTER VIEW dbo.vw_DatDiaDiemChoXacNhan
AS
SELECT
    DDD.DatDiaDiemID,
    DDD.SuKienID,
    SK.TenSuKien,
    SK.NguoiTaoID,
    TK.HoTen AS HoTenNguoiDat,
    DDD.DiaDiemID,
    DD.TenDiaDiem,
    DD.DiaChi,
    DDD.ThoiGianBatDau,
    DDD.ThoiGianKetThuc,
    DDD.TongTien,
    DDD.TienCoc,
    DDD.NgayDat
FROM dbo.DatDiaDiem AS DDD
INNER JOIN dbo.SuKien AS SK
    ON SK.SuKienID = DDD.SuKienID
INNER JOIN dbo.TaiKhoan AS TK
    ON TK.TaiKhoanID = SK.NguoiTaoID
INNER JOIN dbo.DiaDiem AS DD
    ON DD.DiaDiemID = DDD.DiaDiemID
WHERE DDD.TrangThai = N'ChoXacNhan';
GO