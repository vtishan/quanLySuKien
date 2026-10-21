
USE [QuanLySuKien];
GO

/* =========================================================
   NHOM 3: QUAN LY DIA DIEM VA DAT DIA DIEM
   ========================================================= */


/* 1. Them dia diem */
CREATE OR ALTER PROCEDURE sp_ThemDiaDiem
    @NguoiThucHienID INT,
    @TenDiaDiem NVARCHAR(200),
    @DiaChi NVARCHAR(500),
    @SucChua INT,
    @GiaThue DECIMAL(18,2),
    @MoTa NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiThucHienID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro = N'QuanTriVien'
    )
        THROW 50301, N'Khong co quyen them dia diem.', 1;

    IF NULLIF(LTRIM(RTRIM(@TenDiaDiem)), N'') IS NULL
       OR NULLIF(LTRIM(RTRIM(@DiaChi)), N'') IS NULL
        THROW 50302, N'Ten dia diem va dia chi khong duoc de trong.', 1;

    IF @SucChua IS NULL OR @SucChua <= 0
        THROW 50303, N'Suc chua phai lon hon 0.', 1;

    IF @GiaThue IS NULL OR @GiaThue < 0
        THROW 50304, N'Gia thue khong hop le.', 1;

    INSERT INTO DiaDiem (
        TenDiaDiem,
        DiaChi,
        SucChua,
        GiaThue,
        MoTa,
        TrangThai
    )
    VALUES (
        @TenDiaDiem,
        @DiaChi,
        @SucChua,
        @GiaThue,
        @MoTa,
        1
    );

    SELECT *
    FROM DiaDiem
    WHERE DiaDiemID = CONVERT(INT, SCOPE_IDENTITY());
END;
GO


/* 2. Cap nhat dia diem */
CREATE OR ALTER PROCEDURE sp_CapNhatDiaDiem
    @NguoiThucHienID INT,
    @DiaDiemID INT,
    @TenDiaDiem NVARCHAR(200),
    @DiaChi NVARCHAR(500),
    @SucChua INT,
    @GiaThue DECIMAL(18,2),
    @MoTa NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiThucHienID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro = N'QuanTriVien'
    )
        THROW 50310, N'Khong co quyen cap nhat dia diem.', 1;

    IF NULLIF(LTRIM(RTRIM(@TenDiaDiem)), N'') IS NULL
       OR NULLIF(LTRIM(RTRIM(@DiaChi)), N'') IS NULL
        THROW 50311, N'Ten dia diem va dia chi khong duoc de trong.', 1;

    IF @SucChua IS NULL OR @SucChua <= 0
        THROW 50312, N'Suc chua phai lon hon 0.', 1;

    IF @GiaThue IS NULL OR @GiaThue < 0
        THROW 50313, N'Gia thue khong hop le.', 1;

    UPDATE DiaDiem
    SET
        TenDiaDiem = @TenDiaDiem,
        DiaChi = @DiaChi,
        SucChua = @SucChua,
        GiaThue = @GiaThue,
        MoTa = @MoTa
    WHERE DiaDiemID = @DiaDiemID;

    IF @@ROWCOUNT = 0
        THROW 50314, N'Dia diem khong ton tai.', 1;

    SELECT *
    FROM DiaDiem
    WHERE DiaDiemID = @DiaDiemID;
END;
GO


/* 3. Khoa hoac mo dia diem */
CREATE OR ALTER PROCEDURE sp_DoiTrangThaiDiaDiem
    @NguoiThucHienID INT,
    @DiaDiemID INT,
    @TrangThai BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiThucHienID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro = N'QuanTriVien'
    )
        THROW 50320, N'Khong co quyen thay doi trang thai dia diem.', 1;

    IF @TrangThai IS NULL
        THROW 50321, N'Trang thai khong hop le.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM DiaDiem
        WHERE DiaDiemID = @DiaDiemID
    )
        THROW 50322, N'Dia diem khong ton tai.', 1;

    IF @TrangThai = 0 AND EXISTS (
        SELECT 1
        FROM DatDiaDiem
        WHERE DiaDiemID = @DiaDiemID
          AND TrangThai IN (N'ChoXacNhan', N'DaXacNhan')
          AND ThoiGianKetThuc > SYSDATETIME()
    )
        THROW 50323, N'Khong the khoa dia diem dang co lich dat chua ket thuc.', 1;

    UPDATE DiaDiem
    SET TrangThai = @TrangThai
    WHERE DiaDiemID = @DiaDiemID;

    SELECT *
    FROM DiaDiem
    WHERE DiaDiemID = @DiaDiemID;
END;
GO


/* 4. Tra cuu dia diem */
CREATE OR ALTER PROCEDURE sp_TraCuuDiaDiem
    @TuKhoa NVARCHAR(200) = NULL,
    @SucChuaToiThieu INT = NULL,
    @GiaThueToiDa DECIMAL(18,2) = NULL,
    @TrangThai BIT = 1,
    @Trang INT = 1,
    @SoDong INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Trang < 1 OR @SoDong < 1 OR @SoDong > 200
        THROW 50330, N'Trang hoac so dong khong hop le.', 1;

    SELECT
        DiaDiemID,
        TenDiaDiem,
        DiaChi,
        SucChua,
        GiaThue,
        MoTa,
        TrangThai
    FROM DiaDiem
    WHERE
        (@TuKhoa IS NULL
            OR TenDiaDiem LIKE N'%' + @TuKhoa + N'%'
            OR DiaChi LIKE N'%' + @TuKhoa + N'%')
        AND (@SucChuaToiThieu IS NULL OR SucChua >= @SucChuaToiThieu)
        AND (@GiaThueToiDa IS NULL OR GiaThue <= @GiaThueToiDa)
        AND (@TrangThai IS NULL OR TrangThai = @TrangThai)
    ORDER BY DiaDiemID DESC
    OFFSET (@Trang - 1) * @SoDong ROWS
    FETCH NEXT @SoDong ROWS ONLY;
END;
GO


/* 5. Tao yeu cau dat dia diem */
CREATE OR ALTER PROCEDURE sp_TaoDatDiaDiem
    @TaiKhoanID INT,
    @SuKienID INT,
    @DiaDiemID INT,
    @ThoiGianBatDau DATETIME2,
    @ThoiGianKetThuc DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @ThoiGianBatDau IS NULL
       OR @ThoiGianKetThuc IS NULL
       OR @ThoiGianKetThuc <= @ThoiGianBatDau
        THROW 50340, N'Thoi gian dat dia diem khong hop le.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM TaiKhoan TK
            INNER JOIN BanToChuc BTC ON BTC.TaiKhoanID = TK.TaiKhoanID
            WHERE TK.TaiKhoanID = @TaiKhoanID
              AND TK.TrangThai = 1
              AND BTC.TrangThai = 1
        )
            THROW 50341, N'Tai khoan chua duoc xac thuc ban to chuc.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM SuKien
            WHERE SuKienID = @SuKienID
              AND NguoiTaoID = @TaiKhoanID
              AND TrangThai = N'DaDuyet'
        )
            THROW 50342, N'Su kien khong hop le, khong thuoc tai khoan hoac chua duoc duyet.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM DiaDiem WITH (UPDLOCK, HOLDLOCK)
            WHERE DiaDiemID = @DiaDiemID
              AND TrangThai = 1
        )
            THROW 50343, N'Dia diem khong ton tai hoac dang bi khoa.', 1;

        IF EXISTS (
            SELECT 1
            FROM DatDiaDiem WITH (UPDLOCK, HOLDLOCK)
            WHERE DiaDiemID = @DiaDiemID
              AND TrangThai IN (N'ChoXacNhan', N'DaXacNhan')
              AND ThoiGianBatDau < @ThoiGianKetThuc
              AND ThoiGianKetThuc > @ThoiGianBatDau
        )
            THROW 50344, N'Dia diem da co lich dat trung thoi gian.', 1;

        DECLARE @GiaThue DECIMAL(18,2);
        DECLARE @TongTien DECIMAL(18,2);
        DECLARE @TienCoc DECIMAL(18,2);

        SELECT @GiaThue = GiaThue
        FROM DiaDiem
        WHERE DiaDiemID = @DiaDiemID;

        /* GiaThue duoc tinh theo 24 gio.
           Tinh tien theo thoi luong thuc te, lam tron 2 chu so. */
        SET @TongTien = ROUND(
            @GiaThue *
            (DATEDIFF_BIG(SECOND, @ThoiGianBatDau, @ThoiGianKetThuc)
             / 86400.0),
            2
        );

        SET @TienCoc = ROUND(@TongTien * 0.5, 2);

        INSERT INTO DatDiaDiem (
            SuKienID,
            DiaDiemID,
            ThoiGianBatDau,
            ThoiGianKetThuc,
            TongTien,
            TienCoc,
            TrangThai
        )
        VALUES (
            @SuKienID,
            @DiaDiemID,
            @ThoiGianBatDau,
            @ThoiGianKetThuc,
            @TongTien,
            @TienCoc,
            N'ChoXacNhan'
        );

        DECLARE @DatDiaDiemID INT =
            CONVERT(INT, SCOPE_IDENTITY());

        COMMIT TRANSACTION;

        SELECT *
        FROM DatDiaDiem
        WHERE DatDiaDiemID = @DatDiaDiemID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 6. Xac nhan dat dia diem */
CREATE OR ALTER PROCEDURE sp_XacNhanDatDiaDiem
    @NguoiThucHienID INT,
    @DatDiaDiemID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM TaiKhoan TK
            INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
            WHERE TK.TaiKhoanID = @NguoiThucHienID
              AND TK.TrangThai = 1
              AND VT.TenVaiTro IN (N'QuanLySuKien', N'QuanTriVien')
        )
            THROW 50350, N'Khong co quyen xac nhan dat dia diem.', 1;

        DECLARE @DiaDiemID INT;
        DECLARE @BatDau DATETIME2;
        DECLARE @KetThuc DATETIME2;
        DECLARE @TrangThai NVARCHAR(30);

        SELECT
            @DiaDiemID = DiaDiemID,
            @BatDau = ThoiGianBatDau,
            @KetThuc = ThoiGianKetThuc,
            @TrangThai = TrangThai
        FROM DatDiaDiem WITH (UPDLOCK, HOLDLOCK)
        WHERE DatDiaDiemID = @DatDiaDiemID;

        IF @DiaDiemID IS NULL
            THROW 50351, N'Yeu cau dat dia diem khong ton tai.', 1;

        IF @TrangThai <> N'ChoXacNhan'
            THROW 50352, N'Yeu cau khong o trang thai cho xac nhan.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM DiaDiem WITH (UPDLOCK, HOLDLOCK)
            WHERE DiaDiemID = @DiaDiemID
              AND TrangThai = 1
        )
            THROW 50353, N'Dia diem da bi khoa.', 1;

        IF EXISTS (
            SELECT 1
            FROM DatDiaDiem WITH (UPDLOCK, HOLDLOCK)
            WHERE DiaDiemID = @DiaDiemID
              AND DatDiaDiemID <> @DatDiaDiemID
              AND TrangThai IN (N'ChoXacNhan', N'DaXacNhan')
              AND ThoiGianBatDau < @KetThuc
              AND ThoiGianKetThuc > @BatDau
        )
            THROW 50354, N'Lich dat bi trung voi yeu cau khac.', 1;

        UPDATE DatDiaDiem
        SET TrangThai = N'DaXacNhan'
        WHERE DatDiaDiemID = @DatDiaDiemID;

        COMMIT TRANSACTION;

        SELECT *
        FROM DatDiaDiem
        WHERE DatDiaDiemID = @DatDiaDiemID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 7. Tu choi dat dia diem */
CREATE OR ALTER PROCEDURE sp_TuChoiDatDiaDiem
    @NguoiThucHienID INT,
    @DatDiaDiemID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiThucHienID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro IN (N'QuanLySuKien', N'QuanTriVien')
    )
        THROW 50360, N'Khong co quyen tu choi dat dia diem.', 1;

    UPDATE DatDiaDiem
    SET TrangThai = N'TuChoi'
    WHERE DatDiaDiemID = @DatDiaDiemID
      AND TrangThai = N'ChoXacNhan';

    IF @@ROWCOUNT = 0
        THROW 50361, N'Yeu cau khong ton tai hoac khong con cho xac nhan.', 1;

    SELECT *
    FROM DatDiaDiem
    WHERE DatDiaDiemID = @DatDiaDiemID;
END;
GO


/* 8. Huy yeu cau dat dia diem */
CREATE OR ALTER PROCEDURE sp_HuyDatDiaDiem
    @TaiKhoanID INT,
    @DatDiaDiemID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DD
    SET TrangThai = N'DaHuy'
    FROM DatDiaDiem DD
    INNER JOIN SuKien SK ON SK.SuKienID = DD.SuKienID
    WHERE DD.DatDiaDiemID = @DatDiaDiemID
      AND SK.NguoiTaoID = @TaiKhoanID
      AND DD.TrangThai IN (N'ChoXacNhan', N'DaXacNhan')
      AND DD.ThoiGianBatDau > SYSDATETIME();

    IF @@ROWCOUNT = 0
        THROW 50370, N'Khong the huy yeu cau dat dia diem nay.', 1;

    SELECT *
    FROM DatDiaDiem
    WHERE DatDiaDiemID = @DatDiaDiemID;
END;
GO


/* 9. Tra cuu dat dia diem */
CREATE OR ALTER PROCEDURE sp_TraCuuDatDiaDiem
    @TaiKhoanID INT,
    @DiaDiemID INT = NULL,
    @TrangThai NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DD.DatDiaDiemID,
        DD.SuKienID,
        SK.TenSuKien,
        SK.NguoiTaoID,
        DD.DiaDiemID,
        D.TenDiaDiem,
        D.DiaChi,
        DD.ThoiGianBatDau,
        DD.ThoiGianKetThuc,
        DD.TongTien,
        DD.TienCoc,
        DD.TrangThai,
        DD.NgayDat
    FROM DatDiaDiem DD
    INNER JOIN SuKien SK ON SK.SuKienID = DD.SuKienID
    INNER JOIN DiaDiem D ON D.DiaDiemID = DD.DiaDiemID
    WHERE
        (
            SK.NguoiTaoID = @TaiKhoanID
            OR EXISTS (
                SELECT 1
                FROM TaiKhoan TK
                INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
                WHERE TK.TaiKhoanID = @TaiKhoanID
                  AND TK.TrangThai = 1
                  AND VT.TenVaiTro IN (N'QuanLySuKien', N'QuanTriVien')
            )
        )
        AND (@DiaDiemID IS NULL OR DD.DiaDiemID = @DiaDiemID)
        AND (@TrangThai IS NULL OR DD.TrangThai = @TrangThai)
    ORDER BY DD.NgayDat DESC, DD.DatDiaDiemID DESC;
END;
GO


/* 10. Thong ke dat dia diem theo trang thai */
CREATE OR ALTER PROCEDURE sp_ThongKeDatDiaDiemTheoTrangThai
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TrangThai,
        COUNT(*) AS SoLuotDat,
        SUM(TongTien) AS TongGiaTriDat,
        SUM(TienCoc) AS TongTienCoc
    FROM DatDiaDiem
    GROUP BY TrangThai
    ORDER BY TrangThai;
END;
GO


/* 11. Thong ke dat dia diem theo dia diem */
CREATE OR ALTER PROCEDURE sp_ThongKeDatDiaDiemTheoDiaDiem
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        D.DiaDiemID,
        D.TenDiaDiem,
        COUNT(DD.DatDiaDiemID) AS TongLuotDat,
        SUM(CASE
            WHEN DD.TrangThai IN (N'ChoXacNhan', N'DaXacNhan')
            THEN 1 ELSE 0
        END) AS SoLuotDatDangXuLy,
        SUM(CASE
            WHEN DD.TrangThai = N'DaXacNhan'
            THEN DD.TongTien ELSE 0
        END) AS TongGiaTriDatDaXacNhan,
        SUM(CASE
            WHEN DD.TrangThai = N'DaXacNhan'
            THEN DD.TienCoc ELSE 0
        END) AS TongTienCocDaXacNhan
    FROM DiaDiem D
    LEFT JOIN DatDiaDiem DD
        ON DD.DiaDiemID = D.DiaDiemID
    GROUP BY D.DiaDiemID, D.TenDiaDiem
    ORDER BY TongLuotDat DESC, D.DiaDiemID;
END;
GO
