
USE [QuanLySuKien];
GO

/* =========================================================
   NHOM 1: TAI KHOAN VA PHAN QUYEN
   ========================================================= */


/* 1. Dang ky tai khoan nguoi dung */
CREATE OR ALTER PROCEDURE sp_DangKyTaiKhoan
    @HoTen NVARCHAR(100),
    @Email NVARCHAR(255),
    @MatKhauHash NVARCHAR(255),
    @SoDienThoai NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @HoTen = LTRIM(RTRIM(@HoTen));
    SET @Email = LOWER(LTRIM(RTRIM(@Email)));

    IF NULLIF(@HoTen, N'') IS NULL
        THROW 50001, N'Ho ten khong duoc de trong.', 1;

    IF NULLIF(@Email, N'') IS NULL
        THROW 50002, N'Email khong duoc de trong.', 1;

    IF NULLIF(@MatKhauHash, N'') IS NULL
        THROW 50003, N'Mat khau hash khong duoc de trong.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1
            FROM TaiKhoan WITH (UPDLOCK, HOLDLOCK)
            WHERE Email = @Email
        )
            THROW 50004, N'Email da duoc su dung.', 1;

        DECLARE @VaiTroID INT;

        SELECT @VaiTroID = VaiTroID
        FROM VaiTro
        WHERE TenVaiTro = N'NguoiDung';

        IF @VaiTroID IS NULL
            THROW 50005, N'Chua cau hinh vai tro NguoiDung.', 1;

        INSERT INTO TaiKhoan (
            VaiTroID,
            HoTen,
            Email,
            MatKhauHash,
            SoDienThoai
        )
        VALUES (
            @VaiTroID,
            @HoTen,
            @Email,
            @MatKhauHash,
            @SoDienThoai
        );

        DECLARE @TaiKhoanID INT = CONVERT(INT, SCOPE_IDENTITY());

        COMMIT TRANSACTION;

        SELECT
            TK.TaiKhoanID,
            TK.HoTen,
            TK.Email,
            TK.SoDienThoai,
            TK.VaiTroID,
            VT.TenVaiTro,
            TK.TrangThai,
            TK.NgayTao
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT
            ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @TaiKhoanID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO


/* 2. Kiem tra thong tin tai khoan khi dang nhap */
CREATE OR ALTER PROCEDURE sp_KiemTraDangNhap
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Email = LOWER(LTRIM(RTRIM(@Email)));

    SELECT
        TK.TaiKhoanID,
        TK.HoTen,
        TK.Email,
        TK.MatKhauHash,
        TK.VaiTroID,
        VT.TenVaiTro,
        TK.TrangThai
    FROM TaiKhoan TK
    INNER JOIN VaiTro VT
        ON VT.VaiTroID = TK.VaiTroID
    WHERE TK.Email = @Email;
END;
GO


/* 3. Cap nhat thong tin ca nhan */
CREATE OR ALTER PROCEDURE sp_CapNhatThongTinTaiKhoan
    @TaiKhoanID INT,
    @HoTen NVARCHAR(100),
    @Email NVARCHAR(255),
    @SoDienThoai NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @HoTen = LTRIM(RTRIM(@HoTen));
    SET @Email = LOWER(LTRIM(RTRIM(@Email)));

    IF NULLIF(@HoTen, N'') IS NULL
        THROW 50010, N'Ho ten khong duoc de trong.', 1;

    IF NULLIF(@Email, N'') IS NULL
        THROW 50011, N'Email khong duoc de trong.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan
        WHERE TaiKhoanID = @TaiKhoanID
    )
        THROW 50012, N'Tai khoan khong ton tai.', 1;

    IF EXISTS (
        SELECT 1
        FROM TaiKhoan
        WHERE Email = @Email
          AND TaiKhoanID <> @TaiKhoanID
    )
        THROW 50013, N'Email da duoc tai khoan khac su dung.', 1;

    UPDATE TaiKhoan
    SET
        HoTen = @HoTen,
        Email = @Email,
        SoDienThoai = @SoDienThoai
    WHERE TaiKhoanID = @TaiKhoanID;

    SELECT
        TaiKhoanID,
        HoTen,
        Email,
        SoDienThoai,
        VaiTroID,
        TrangThai,
        NgayTao
    FROM TaiKhoan
    WHERE TaiKhoanID = @TaiKhoanID;
END;
GO


/* 4. Khoa tai khoan */
CREATE OR ALTER PROCEDURE sp_KhoaTaiKhoan
    @NguoiThucHienID INT,
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT
            ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiThucHienID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro = N'QuanTriVien'
    )
        THROW 50020, N'Khong co quyen khoa tai khoan.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan
        WHERE TaiKhoanID = @TaiKhoanID
    )
        THROW 50021, N'Tai khoan can khoa khong ton tai.', 1;

    IF @NguoiThucHienID = @TaiKhoanID
        THROW 50022, N'Khong the tu khoa tai khoan cua minh.', 1;

    UPDATE TaiKhoan
    SET TrangThai = 0
    WHERE TaiKhoanID = @TaiKhoanID;

    SELECT TaiKhoanID, HoTen, Email, TrangThai
    FROM TaiKhoan
    WHERE TaiKhoanID = @TaiKhoanID;
END;
GO


/* 5. Mo khoa tai khoan */
CREATE OR ALTER PROCEDURE sp_MoKhoaTaiKhoan
    @NguoiThucHienID INT,
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT
            ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiThucHienID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro = N'QuanTriVien'
    )
        THROW 50030, N'Khong co quyen mo khoa tai khoan.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan
        WHERE TaiKhoanID = @TaiKhoanID
    )
        THROW 50031, N'Tai khoan can mo khoa khong ton tai.', 1;

    UPDATE TaiKhoan
    SET TrangThai = 1
    WHERE TaiKhoanID = @TaiKhoanID;

    SELECT TaiKhoanID, HoTen, Email, TrangThai
    FROM TaiKhoan
    WHERE TaiKhoanID = @TaiKhoanID;
END;
GO


/* 6. Thay doi vai tro tai khoan */
CREATE OR ALTER PROCEDURE sp_DoiVaiTroTaiKhoan
    @NguoiThucHienID INT,
    @TaiKhoanID INT,
    @VaiTroID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT
            ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiThucHienID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro = N'QuanTriVien'
    )
        THROW 50040, N'Khong co quyen thay doi vai tro.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan
        WHERE TaiKhoanID = @TaiKhoanID
    )
        THROW 50041, N'Tai khoan khong ton tai.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM VaiTro
        WHERE VaiTroID = @VaiTroID
    )
        THROW 50042, N'Vai tro khong ton tai.', 1;

    IF @NguoiThucHienID = @TaiKhoanID
        THROW 50043, N'Khong the tu thay doi vai tro cua minh.', 1;

    UPDATE TaiKhoan
    SET VaiTroID = @VaiTroID
    WHERE TaiKhoanID = @TaiKhoanID;

    SELECT
        TK.TaiKhoanID,
        TK.HoTen,
        TK.Email,
        TK.VaiTroID,
        VT.TenVaiTro
    FROM TaiKhoan TK
    INNER JOIN VaiTro VT
        ON VT.VaiTroID = TK.VaiTroID
    WHERE TK.TaiKhoanID = @TaiKhoanID;
END;
GO


/* 7. Tra cuu tai khoan theo tu khoa va trang thai */
CREATE OR ALTER PROCEDURE sp_TraCuuTaiKhoan
    @TuKhoa NVARCHAR(255) = NULL,
    @VaiTroID INT = NULL,
    @TrangThai BIT = NULL,
    @Trang INT = 1,
    @SoDong INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Trang < 1 OR @SoDong < 1 OR @SoDong > 200
        THROW 50050, N'Trang hoac so dong khong hop le.', 1;

    SELECT
        TK.TaiKhoanID,
        TK.HoTen,
        TK.Email,
        TK.SoDienThoai,
        TK.VaiTroID,
        VT.TenVaiTro,
        TK.TrangThai,
        TK.NgayTao
    FROM TaiKhoan TK
    INNER JOIN VaiTro VT
        ON VT.VaiTroID = TK.VaiTroID
    WHERE
        (
            @TuKhoa IS NULL
            OR TK.HoTen LIKE N'%' + @TuKhoa + N'%'
            OR TK.Email LIKE N'%' + @TuKhoa + N'%'
        )
        AND (@VaiTroID IS NULL OR TK.VaiTroID = @VaiTroID)
        AND (@TrangThai IS NULL OR TK.TrangThai = @TrangThai)
    ORDER BY TK.TaiKhoanID DESC
    OFFSET (@Trang - 1) * @SoDong ROWS
    FETCH NEXT @SoDong ROWS ONLY;
END;
GO


/* 8. Thong ke tai khoan theo trang thai */
CREATE OR ALTER PROCEDURE sp_ThongKeTaiKhoanTheoTrangThai
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TrangThai,
        CASE
            WHEN TrangThai = 1 THEN N'Hoat dong'
            ELSE N'Bi khoa'
        END AS TenTrangThai,
        COUNT(*) AS SoLuong
    FROM TaiKhoan
    GROUP BY TrangThai
    ORDER BY TrangThai DESC;
END;
GO


/* 9. Thong ke tai khoan theo vai tro */
CREATE OR ALTER PROCEDURE sp_ThongKeTaiKhoanTheoVaiTro
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        VT.VaiTroID,
        VT.TenVaiTro,
        COUNT(TK.TaiKhoanID) AS TongTaiKhoan,
        SUM(CASE WHEN TK.TrangThai = 1 THEN 1 ELSE 0 END)
            AS SoTaiKhoanHoatDong,
        SUM(CASE WHEN TK.TrangThai = 0 THEN 1 ELSE 0 END)
            AS SoTaiKhoanBiKhoa
    FROM VaiTro VT
    LEFT JOIN TaiKhoan TK
        ON TK.VaiTroID = VT.VaiTroID
    GROUP BY VT.VaiTroID, VT.TenVaiTro
    ORDER BY VT.VaiTroID;
END;
GO


/* 10. Tra cuu su kien do tai khoan tao */
CREATE OR ALTER PROCEDURE sp_TraCuuSuKienTheoTaiKhoan
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan
        WHERE TaiKhoanID = @TaiKhoanID
    )
        THROW 50060, N'Tai khoan khong ton tai.', 1;

    SELECT *
    FROM SuKien
    WHERE NguoiTaoID = @TaiKhoanID
    ORDER BY NgayTao DESC;
END;
GO


/* 11. Tra cuu don ve cua tai khoan */
CREATE OR ALTER PROCEDURE sp_TraCuuDonVeTheoTaiKhoan
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan
        WHERE TaiKhoanID = @TaiKhoanID
    )
        THROW 50070, N'Tai khoan khong ton tai.', 1;

    SELECT *
    FROM DonVe
    WHERE TaiKhoanID = @TaiKhoanID
    ORDER BY DonVeID DESC;
END;
GO
