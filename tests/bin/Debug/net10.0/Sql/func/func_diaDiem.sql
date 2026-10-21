-- Nhom 3: Dia diem va dat dia diem
-- SQL Server

-- F01. Kiem tra dia diem ton tai
CREATE OR ALTER FUNCTION dbo.fn_KiemTraDiaDiemTonTai
(
    @DiaDiemID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.DiaDiem
        WHERE DiaDiemID = @DiaDiemID
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F02. Kiem tra dia diem dang hoat dong
CREATE OR ALTER FUNCTION dbo.fn_KiemTraDiaDiemHoatDong
(
    @DiaDiemID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.DiaDiem
        WHERE DiaDiemID = @DiaDiemID
          AND TrangThai = 1
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F03. Kiem tra suc chua dia diem
CREATE OR ALTER FUNCTION dbo.fn_KiemTraSucChuaDiaDiem
(
    @DiaDiemID INT,
    @SoLuongNguoiDuKien INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF @SoLuongNguoiDuKien > 0
       AND EXISTS
       (
           SELECT 1
           FROM dbo.DiaDiem
           WHERE DiaDiemID = @DiaDiemID
             AND TrangThai = 1
             AND SucChua >= @SoLuongNguoiDuKien
       )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F04. Tinh tong tien thue dia diem
CREATE OR ALTER FUNCTION dbo.fn_TinhTongTienThueDiaDiem
(
    @DiaDiemID INT,
    @SoGioThue DECIMAL(18,2)
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @GiaThue DECIMAL(18,2);
    DECLARE @TongTien DECIMAL(18,2);

    IF @SoGioThue <= 0
        RETURN NULL;

    SELECT @GiaThue = GiaThue
    FROM dbo.DiaDiem
    WHERE DiaDiemID = @DiaDiemID
      AND TrangThai = 1;

    IF @GiaThue IS NULL
        RETURN NULL;

    SET @TongTien = @GiaThue * @SoGioThue;

    RETURN @TongTien;
END;
GO

-- F05. Kiem tra khoang thoi gian dat dia diem hop le
CREATE OR ALTER FUNCTION dbo.fn_KiemTraThoiGianDatDiaDiem
(
    @ThoiGianBatDau DATETIME2,
    @ThoiGianKetThuc DATETIME2
)
RETURNS BIT
AS
BEGIN
    RETURN CASE
        WHEN @ThoiGianBatDau IS NOT NULL
         AND @ThoiGianKetThuc IS NOT NULL
         AND @ThoiGianKetThuc > @ThoiGianBatDau
        THEN 1
        ELSE 0
    END;
END;
GO

-- F06. Kiem tra dia diem co bi trung lich
CREATE OR ALTER FUNCTION dbo.fn_KiemTraTrungLichDiaDiem
(
    @DiaDiemID INT,
    @ThoiGianBatDau DATETIME2,
    @ThoiGianKetThuc DATETIME2,
    @BoQuaDatDiaDiemID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.DatDiaDiem
        WHERE DiaDiemID = @DiaDiemID
          AND TrangThai IN (N'ChoXacNhan', N'DaXacNhan')
          AND (@BoQuaDatDiaDiemID IS NULL
               OR DatDiaDiemID <> @BoQuaDatDiaDiemID)
          AND @ThoiGianBatDau < ThoiGianKetThuc
          AND @ThoiGianKetThuc > ThoiGianBatDau
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F07. Kiem tra tien coc khong vuot tong tien
CREATE OR ALTER FUNCTION dbo.fn_KiemTraTienCocDatDiaDiem
(
    @TongTien DECIMAL(18,2),
    @TienCoc DECIMAL(18,2)
)
RETURNS BIT
AS
BEGIN
    RETURN CASE
        WHEN @TongTien >= 0
         AND @TienCoc >= 0
         AND @TienCoc <= @TongTien
        THEN 1
        ELSE 0
    END;
END;
GO

-- F08. Dem luot dat dia diem theo trang thai
CREATE OR ALTER FUNCTION dbo.fn_DemDatDiaDiemTheoTrangThai
(
    @TrangThai NVARCHAR(30)
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.DatDiaDiem
    WHERE TrangThai = @TrangThai;

    RETURN @SoLuong;
END;
GO

-- F09. Dem luot dat cua mot dia diem
CREATE OR ALTER FUNCTION dbo.fn_DemLuotDatTheoDiaDiem
(
    @DiaDiemID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.DatDiaDiem
    WHERE DiaDiemID = @DiaDiemID;

    RETURN @SoLuong;
END;
GO

-- F10. Kiem tra su kien co the dat dia diem
CREATE OR ALTER FUNCTION dbo.fn_KiemTraSuKienCoTheDatDiaDiem
(
    @SuKienID INT,
    @TaiKhoanID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.SuKien AS SK
        INNER JOIN dbo.TaiKhoan AS TK
            ON TK.TaiKhoanID = SK.NguoiTaoID
        INNER JOIN dbo.BanToChuc AS BTC
            ON BTC.TaiKhoanID = TK.TaiKhoanID
        WHERE SK.SuKienID = @SuKienID
          AND SK.NguoiTaoID = @TaiKhoanID
          AND SK.TrangThai = N'DaDuyet'
          AND TK.TrangThai = 1
          AND BTC.TrangThai = 1
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F11. Dem dia diem theo trang thai hoat dong
CREATE OR ALTER FUNCTION dbo.fn_DemDiaDiemTheoTrangThai
(
    @TrangThai BIT
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.DiaDiem
    WHERE TrangThai = @TrangThai;

    RETURN @SoLuong;
END;
GO