-- Nhom 2: Xac thuc ban to chuc va quan ly su kien
-- SQL Server

-- F01. Kiem tra yeu cau xac thuc ton tai
CREATE OR ALTER FUNCTION dbo.fn_KiemTraYeuCauXacThucTonTai
(
    @YeuCauID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.YeuCauXacThucToChuc
        WHERE YeuCauID = @YeuCauID
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F02. Kiem tra tai khoan co yeu cau dang cho duyet
CREATE OR ALTER FUNCTION dbo.fn_KiemTraYeuCauChoDuyet
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
        FROM dbo.YeuCauXacThucToChuc
        WHERE TaiKhoanID = @TaiKhoanID
          AND TrangThai = N'ChoDuyet'
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F03. Dem yeu cau xac thuc theo trang thai
CREATE OR ALTER FUNCTION dbo.fn_DemYeuCauXacThucTheoTrangThai
(
    @TrangThai NVARCHAR(20)
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.YeuCauXacThucToChuc
    WHERE TrangThai = @TrangThai;

    RETURN @SoLuong;
END;
GO

-- F04. Kiem tra loai su kien dang hoat dong
CREATE OR ALTER FUNCTION dbo.fn_KiemTraLoaiSuKienHoatDong
(
    @LoaiSuKienID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.LoaiSuKien
        WHERE LoaiSuKienID = @LoaiSuKienID
          AND TrangThai = 1
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F05. Dem su kien theo loai su kien
CREATE OR ALTER FUNCTION dbo.fn_DemSuKienTheoLoai
(
    @LoaiSuKienID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.SuKien
    WHERE LoaiSuKienID = @LoaiSuKienID;

    RETURN @SoLuong;
END;
GO

-- F06. Kiem tra khoang thoi gian su kien hop le
CREATE OR ALTER FUNCTION dbo.fn_KiemTraThoiGianSuKien
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

-- F07. Kiem tra nguoi tao khong tu duyet su kien
CREATE OR ALTER FUNCTION dbo.fn_KiemTraNguoiDuyetSuKien
(
    @SuKienID INT,
    @NguoiDuyetID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.SuKien
        WHERE SuKienID = @SuKienID
          AND NguoiTaoID <> @NguoiDuyetID
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F08. Kiem tra su kien da duoc duyet
CREATE OR ALTER FUNCTION dbo.fn_KiemTraSuKienDaDuyet
(
    @SuKienID INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua BIT = 0;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.SuKien
        WHERE SuKienID = @SuKienID
          AND TrangThai = N'DaDuyet'
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F09. Dem su kien theo trang thai
CREATE OR ALTER FUNCTION dbo.fn_DemSuKienTheoTrangThai
(
    @TrangThai NVARCHAR(30)
)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong INT;

    SELECT @SoLuong = COUNT(*)
    FROM dbo.SuKien
    WHERE TrangThai = @TrangThai;

    RETURN @SoLuong;
END;
GO

-- F10. Kiem tra ban to chuc co duoc phep tao su kien
CREATE OR ALTER FUNCTION dbo.fn_KiemTraQuyenTaoSuKien
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
        FROM dbo.TaiKhoan AS TK
        INNER JOIN dbo.BanToChuc AS BTC
            ON BTC.TaiKhoanID = TK.TaiKhoanID
        INNER JOIN dbo.VaiTro AS VT
            ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @TaiKhoanID
          AND TK.TrangThai = 1
          AND BTC.TrangThai = 1
          AND VT.TenVaiTro = N'BanToChuc'
    )
        SET @KetQua = 1;

    RETURN @KetQua;
END;
GO

-- F11. Kiem tra so luong nguoi du kien hop le
CREATE OR ALTER FUNCTION dbo.fn_KiemTraSoLuongNguoiDuKien
(
    @SoLuongNguoiDuKien INT
)
RETURNS BIT
AS
BEGIN
    RETURN CASE
        WHEN @SoLuongNguoiDuKien > 0 THEN 1
        ELSE 0
    END;
END;
GO