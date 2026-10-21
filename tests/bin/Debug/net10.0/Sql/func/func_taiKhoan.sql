-- Nhom 1: Quan ly tai khoan va phan quyen
-- SQL Server

CREATE OR ALTER FUNCTION dbo.fn_KiemTraEmailTonTai
(
    @Email NVARCHAR(255)
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.TaiKhoan
        WHERE Email = @Email
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_KiemTraTaiKhoanHoatDong
(
    @TaiKhoanID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.TaiKhoan
        WHERE TaiKhoanID = @TaiKhoanID
          AND TrangThai = 1
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_LayVaiTroTaiKhoan
(
    @TaiKhoanID INT
)
RETURNS NVARCHAR(50)
AS
BEGIN
    DECLARE @TenVaiTro NVARCHAR(50);

    SELECT @TenVaiTro = VT.TenVaiTro
    FROM dbo.TaiKhoan AS TK
    INNER JOIN dbo.VaiTro AS VT
        ON VT.VaiTroID = TK.VaiTroID
    WHERE TK.TaiKhoanID = @TaiKhoanID;

    RETURN @TenVaiTro;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_KiemTraVaiTro
(
    @TaiKhoanID INT,
    @TenVaiTro NVARCHAR(50)
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.TaiKhoan AS TK
        INNER JOIN dbo.VaiTro AS VT
            ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @TaiKhoanID
          AND VT.TenVaiTro = @TenVaiTro
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_KiemTraBanToChucDaXacThuc
(
    @TaiKhoanID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.BanToChuc
        WHERE TaiKhoanID = @TaiKhoanID
          AND TrangThai = 1
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_DemTaiKhoanTheoTrangThai
(
    @TrangThai BIT
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.TaiKhoan
    WHERE TrangThai = @TrangThai;

    RETURN @SoLuong;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_DemTaiKhoanTheoVaiTro
(
    @VaiTroID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.TaiKhoan
    WHERE VaiTroID = @VaiTroID;

    RETURN @SoLuong;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_DemSuKienTheoTaiKhoan
(
    @TaiKhoanID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.SuKien
    WHERE NguoiTaoID = @TaiKhoanID;

    RETURN @SoLuong;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_DemDonVeTheoTaiKhoan
(
    @TaiKhoanID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.DonVe
    WHERE TaiKhoanID = @TaiKhoanID;

    RETURN @SoLuong;
END;
GO