
USE [QuanLySuKien];
GO

/* =========================================================
   NHOM 2: XAC THUC BAN TO CHUC VA QUAN LY LOAI SU KIEN
   ========================================================= */


/* 1. Gui yeu cau xac thuc ban to chuc */
CREATE OR ALTER PROCEDURE sp_GuiYeuCauXacThucToChuc
    @TaiKhoanID INT,
    @SoCCCD NVARCHAR(20),
    @NgaySinh DATE,
    @GioiTinh NVARCHAR(20),
    @DiaChiThuongTru NVARCHAR(500),
    @AnhCCCDMatTruoc NVARCHAR(500),
    @AnhCCCDMatSau NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan
        WHERE TaiKhoanID = @TaiKhoanID
          AND TrangThai = 1
    )
        THROW 50101, N'Tai khoan khong ton tai hoac da bi khoa.', 1;

    IF NULLIF(LTRIM(RTRIM(@SoCCCD)), N'') IS NULL
       OR @NgaySinh IS NULL
       OR NULLIF(LTRIM(RTRIM(@DiaChiThuongTru)), N'') IS NULL
       OR NULLIF(LTRIM(RTRIM(@AnhCCCDMatTruoc)), N'') IS NULL
       OR NULLIF(LTRIM(RTRIM(@AnhCCCDMatSau)), N'') IS NULL
        THROW 50102, N'Thong tin xac thuc khong duoc de trong.', 1;

    IF EXISTS (
        SELECT 1
        FROM BanToChuc
        WHERE TaiKhoanID = @TaiKhoanID
          AND TrangThai = 1
    )
        THROW 50103, N'Tai khoan da duoc xac thuc ban to chuc.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT 1
            FROM YeuCauXacThucToChuc WITH (UPDLOCK, HOLDLOCK)
            WHERE TaiKhoanID = @TaiKhoanID
              AND TrangThai = N'ChoDuyet'
        )
            THROW 50104, N'Tai khoan da co yeu cau dang cho duyet.', 1;

        /* Tai su dung yeu cau bi tu choi neu co */
        DECLARE @YeuCauID INT;

        SELECT TOP (1) @YeuCauID = YeuCauID
        FROM YeuCauXacThucToChuc WITH (UPDLOCK, HOLDLOCK)
        WHERE TaiKhoanID = @TaiKhoanID
          AND TrangThai = N'TuChoi'
        ORDER BY NgayTao DESC, YeuCauID DESC;

        IF @YeuCauID IS NOT NULL
        BEGIN
            UPDATE YeuCauXacThucToChuc
            SET
                SoCCCD = @SoCCCD,
                NgaySinh = @NgaySinh,
                GioiTinh = @GioiTinh,
                DiaChiThuongTru = @DiaChiThuongTru,
                AnhCCCDMatTruoc = @AnhCCCDMatTruoc,
                AnhCCCDMatSau = @AnhCCCDMatSau,
                TrangThai = N'ChoDuyet',
                NguoiDuyetID = NULL,
                NgayDuyet = NULL,
                LyDoTuChoi = NULL
            WHERE YeuCauID = @YeuCauID;
        END
        ELSE
        BEGIN
            INSERT INTO YeuCauXacThucToChuc (
                TaiKhoanID,
                SoCCCD,
                NgaySinh,
                GioiTinh,
                DiaChiThuongTru,
                AnhCCCDMatTruoc,
                AnhCCCDMatSau,
                TrangThai
            )
            VALUES (
                @TaiKhoanID,
                @SoCCCD,
                @NgaySinh,
                @GioiTinh,
                @DiaChiThuongTru,
                @AnhCCCDMatTruoc,
                @AnhCCCDMatSau,
                N'ChoDuyet'
            );

            SET @YeuCauID = CONVERT(INT, SCOPE_IDENTITY());
        END;

        COMMIT TRANSACTION;

        SELECT *
        FROM YeuCauXacThucToChuc
        WHERE YeuCauID = @YeuCauID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 2. Cap nhat yeu cau dang cho duyet */
CREATE OR ALTER PROCEDURE sp_CapNhatYeuCauXacThucToChuc
    @TaiKhoanID INT,
    @YeuCauID INT,
    @SoCCCD NVARCHAR(20),
    @NgaySinh DATE,
    @GioiTinh NVARCHAR(20),
    @DiaChiThuongTru NVARCHAR(500),
    @AnhCCCDMatTruoc NVARCHAR(500),
    @AnhCCCDMatSau NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@SoCCCD)), N'') IS NULL
       OR @NgaySinh IS NULL
       OR NULLIF(LTRIM(RTRIM(@DiaChiThuongTru)), N'') IS NULL
       OR NULLIF(LTRIM(RTRIM(@AnhCCCDMatTruoc)), N'') IS NULL
       OR NULLIF(LTRIM(RTRIM(@AnhCCCDMatSau)), N'') IS NULL
        THROW 50110, N'Thong tin cap nhat khong hop le.', 1;

    UPDATE YeuCauXacThucToChuc
    SET
        SoCCCD = @SoCCCD,
        NgaySinh = @NgaySinh,
        GioiTinh = @GioiTinh,
        DiaChiThuongTru = @DiaChiThuongTru,
        AnhCCCDMatTruoc = @AnhCCCDMatTruoc,
        AnhCCCDMatSau = @AnhCCCDMatSau
    WHERE YeuCauID = @YeuCauID
      AND TaiKhoanID = @TaiKhoanID
      AND TrangThai = N'ChoDuyet';

    IF @@ROWCOUNT = 0
        THROW 50111, N'Khong tim thay yeu cau dang cho duyet cua tai khoan.', 1;

    SELECT *
    FROM YeuCauXacThucToChuc
    WHERE YeuCauID = @YeuCauID;
END;
GO


/* 3. Theo doi yeu cau xac thuc cua tai khoan */
CREATE OR ALTER PROCEDURE sp_TraCuuYeuCauXacThucToChuc
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        YeuCauID,
        TaiKhoanID,
        SoCCCD,
        NgaySinh,
        GioiTinh,
        DiaChiThuongTru,
        TrangThai,
        NguoiDuyetID,
        NgayDuyet,
        LyDoTuChoi,
        NgayTao
    FROM YeuCauXacThucToChuc
    WHERE TaiKhoanID = @TaiKhoanID
    ORDER BY NgayTao DESC, YeuCauID DESC;
END;
GO


/* 4. Duyet yeu cau va tao ho so ban to chuc */
CREATE OR ALTER PROCEDURE sp_DuyetYeuCauXacThucToChuc
    @NguoiDuyetID INT,
    @YeuCauID INT,
    @TenToChuc NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NULLIF(LTRIM(RTRIM(@TenToChuc)), N'') IS NULL
        THROW 50120, N'Ten to chuc khong duoc de trong.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM TaiKhoan TK
            INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
            WHERE TK.TaiKhoanID = @NguoiDuyetID
              AND TK.TrangThai = 1
              AND VT.TenVaiTro IN (N'QuanLySuKien', N'QuanTriVien')
        )
            THROW 50121, N'Khong co quyen duyet yeu cau.', 1;

        DECLARE @TaiKhoanID INT;
        DECLARE @SoCCCD NVARCHAR(20);
        DECLARE @TrangThai NVARCHAR(30);

        SELECT
            @TaiKhoanID = TaiKhoanID,
            @SoCCCD = SoCCCD,
            @TrangThai = TrangThai
        FROM YeuCauXacThucToChuc WITH (UPDLOCK, HOLDLOCK)
        WHERE YeuCauID = @YeuCauID;

        IF @TaiKhoanID IS NULL
            THROW 50122, N'Yeu cau xac thuc khong ton tai.', 1;

        IF @TrangThai <> N'ChoDuyet'
            THROW 50123, N'Yeu cau khong o trang thai cho duyet.', 1;

        IF EXISTS (
            SELECT 1
            FROM BanToChuc
            WHERE TaiKhoanID = @TaiKhoanID
              AND TrangThai = 1
        )
            THROW 50124, N'Tai khoan da co ho so ban to chuc hoat dong.', 1;

        UPDATE YeuCauXacThucToChuc
        SET
            TrangThai = N'DaDuyet',
            NguoiDuyetID = @NguoiDuyetID,
            NgayDuyet = SYSDATETIME(),
            LyDoTuChoi = NULL
        WHERE YeuCauID = @YeuCauID;

        IF EXISTS (
            SELECT 1
            FROM BanToChuc
            WHERE TaiKhoanID = @TaiKhoanID
        )
        BEGIN
            UPDATE BanToChuc
            SET
                SoCCCD = @SoCCCD,
                TenToChuc = @TenToChuc,
                NgayXacThuc = SYSDATETIME(),
                TrangThai = 1
            WHERE TaiKhoanID = @TaiKhoanID;
        END
        ELSE
        BEGIN
            INSERT INTO BanToChuc (
                TaiKhoanID,
                SoCCCD,
                TenToChuc,
                NgayXacThuc,
                TrangThai
            )
            VALUES (
                @TaiKhoanID,
                @SoCCCD,
                @TenToChuc,
                SYSDATETIME(),
                1
            );
        END;

        COMMIT TRANSACTION;

        SELECT *
        FROM BanToChuc
        WHERE TaiKhoanID = @TaiKhoanID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 5. Tu choi yeu cau xac thuc */
CREATE OR ALTER PROCEDURE sp_TuChoiYeuCauXacThucToChuc
    @NguoiDuyetID INT,
    @YeuCauID INT,
    @LyDoTuChoi NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@LyDoTuChoi)), N'') IS NULL
        THROW 50130, N'Phai nhap ly do tu choi.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiDuyetID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro IN (N'QuanLySuKien', N'QuanTriVien')
    )
        THROW 50131, N'Khong co quyen tu choi yeu cau.', 1;

    UPDATE YeuCauXacThucToChuc
    SET
        TrangThai = N'TuChoi',
        NguoiDuyetID = @NguoiDuyetID,
        NgayDuyet = SYSDATETIME(),
        LyDoTuChoi = @LyDoTuChoi
    WHERE YeuCauID = @YeuCauID
      AND TrangThai = N'ChoDuyet';

    IF @@ROWCOUNT = 0
        THROW 50132, N'Yeu cau khong ton tai hoac khong con cho duyet.', 1;

    SELECT *
    FROM YeuCauXacThucToChuc
    WHERE YeuCauID = @YeuCauID;
END;
GO


/* 6. Thong ke yeu cau xac thuc theo trang thai */
CREATE OR ALTER PROCEDURE sp_ThongKeYeuCauXacThucToChuc
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TrangThai,
        COUNT(*) AS SoLuong
    FROM YeuCauXacThucToChuc
    GROUP BY TrangThai
    ORDER BY TrangThai;
END;
GO


/* 7. Them loai su kien */
CREATE OR ALTER PROCEDURE sp_ThemLoaiSuKien
    @TenLoai NVARCHAR(100),
    @MoTa NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @TenLoai = LTRIM(RTRIM(@TenLoai));

    IF NULLIF(@TenLoai, N'') IS NULL
        THROW 50140, N'Ten loai su kien khong duoc de trong.', 1;

    IF EXISTS (
        SELECT 1
        FROM LoaiSuKien
        WHERE TenLoai = @TenLoai
    )
        THROW 50141, N'Ten loai su kien da ton tai.', 1;

    INSERT INTO LoaiSuKien (TenLoai, MoTa, TrangThai)
    VALUES (@TenLoai, @MoTa, 1);

    SELECT *
    FROM LoaiSuKien
    WHERE LoaiSuKienID = CONVERT(INT, SCOPE_IDENTITY());
END;
GO


/* 8. Cap nhat loai su kien */
CREATE OR ALTER PROCEDURE sp_CapNhatLoaiSuKien
    @LoaiSuKienID INT,
    @TenLoai NVARCHAR(100),
    @MoTa NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @TenLoai = LTRIM(RTRIM(@TenLoai));

    IF NULLIF(@TenLoai, N'') IS NULL
        THROW 50150, N'Ten loai su kien khong duoc de trong.', 1;

    IF EXISTS (
        SELECT 1
        FROM LoaiSuKien
        WHERE TenLoai = @TenLoai
          AND LoaiSuKienID <> @LoaiSuKienID
    )
        THROW 50151, N'Ten loai su kien da duoc su dung.', 1;

    UPDATE LoaiSuKien
    SET TenLoai = @TenLoai,
        MoTa = @MoTa
    WHERE LoaiSuKienID = @LoaiSuKienID;

    IF @@ROWCOUNT = 0
        THROW 50152, N'Loai su kien khong ton tai.', 1;

    SELECT *
    FROM LoaiSuKien
    WHERE LoaiSuKienID = @LoaiSuKienID;
END;
GO


/* 9. Khoa hoac mo loai su kien */
CREATE OR ALTER PROCEDURE sp_DoiTrangThaiLoaiSuKien
    @LoaiSuKienID INT,
    @TrangThai BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE LoaiSuKien
    SET TrangThai = @TrangThai
    WHERE LoaiSuKienID = @LoaiSuKienID;

    IF @@ROWCOUNT = 0
        THROW 50160, N'Loai su kien khong ton tai.', 1;

    SELECT *
    FROM LoaiSuKien
    WHERE LoaiSuKienID = @LoaiSuKienID;
END;
GO


/* 10. Tra cuu loai su kien */
CREATE OR ALTER PROCEDURE sp_TraCuuLoaiSuKien
    @TuKhoa NVARCHAR(100) = NULL,
    @TrangThai BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        LoaiSuKienID,
        TenLoai,
        MoTa,
        TrangThai
    FROM LoaiSuKien
    WHERE
        (@TuKhoa IS NULL OR TenLoai LIKE N'%' + @TuKhoa + N'%')
        AND (@TrangThai IS NULL OR TrangThai = @TrangThai)
    ORDER BY TenLoai;
END;
GO


/* 11. Tao su kien */
CREATE OR ALTER PROCEDURE sp_TaoSuKien
    @NguoiTaoID INT,
    @LoaiSuKienID INT,
    @TenSuKien NVARCHAR(200),
    @MoTa NVARCHAR(MAX) = NULL,
    @AnhBia NVARCHAR(500) = NULL,
    @ThoiGianBatDau DATETIME2,
    @ThoiGianKetThuc DATETIME2,
    @SoLuongNguoiDuKien INT
AS
BEGIN
    SET NOCOUNT ON;

    SET @TenSuKien = LTRIM(RTRIM(@TenSuKien));

    IF NULLIF(@TenSuKien, N'') IS NULL
        THROW 50170, N'Ten su kien khong duoc de trong.', 1;

    IF @ThoiGianBatDau IS NULL
       OR @ThoiGianKetThuc IS NULL
       OR @ThoiGianKetThuc <= @ThoiGianBatDau
        THROW 50171, N'Thoi gian su kien khong hop le.', 1;

    IF @SoLuongNguoiDuKien IS NULL OR @SoLuongNguoiDuKien <= 0
        THROW 50172, N'So luong nguoi du kien phai lon hon 0.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN BanToChuc BTC ON BTC.TaiKhoanID = TK.TaiKhoanID
        WHERE TK.TaiKhoanID = @NguoiTaoID
          AND TK.TrangThai = 1
          AND BTC.TrangThai = 1
    )
        THROW 50173, N'Tai khoan chua duoc xac thuc ban to chuc.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM LoaiSuKien
        WHERE LoaiSuKienID = @LoaiSuKienID
          AND TrangThai = 1
    )
        THROW 50174, N'Loai su kien khong ton tai hoac da bi khoa.', 1;

    INSERT INTO SuKien (
        NguoiTaoID,
        LoaiSuKienID,
        TenSuKien,
        MoTa,
        AnhBia,
        ThoiGianBatDau,
        ThoiGianKetThuc,
        SoLuongNguoiDuKien,
        TrangThai
    )
    VALUES (
        @NguoiTaoID,
        @LoaiSuKienID,
        @TenSuKien,
        @MoTa,
        @AnhBia,
        @ThoiGianBatDau,
        @ThoiGianKetThuc,
        @SoLuongNguoiDuKien,
        N'ChoDuyet'
    );

    SELECT *
    FROM SuKien
    WHERE SuKienID = CONVERT(INT, SCOPE_IDENTITY());
END;
GO


/* 12. Duyet su kien */
CREATE OR ALTER PROCEDURE sp_DuyetSuKien
    @NguoiDuyetID INT,
    @SuKienID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiDuyetID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro IN (N'QuanLySuKien', N'QuanTriVien')
    )
        THROW 50180, N'Khong co quyen duyet su kien.', 1;

    IF EXISTS (
        SELECT 1
        FROM SuKien
        WHERE SuKienID = @SuKienID
          AND NguoiTaoID = @NguoiDuyetID
    )
        THROW 50181, N'Nguoi tao khong duoc tu duyet su kien.', 1;

    UPDATE SuKien
    SET
        TrangThai = N'DaDuyet',
        NguoiDuyetID = @NguoiDuyetID,
        NgayDuyet = SYSDATETIME(),
        LyDoTuChoi = NULL
    WHERE SuKienID = @SuKienID
      AND TrangThai = N'ChoDuyet';

    IF @@ROWCOUNT = 0
        THROW 50182, N'Su kien khong ton tai hoac khong o trang thai cho duyet.', 1;

    SELECT *
    FROM SuKien
    WHERE SuKienID = @SuKienID;
END;
GO


/* 13. Tu choi su kien */
CREATE OR ALTER PROCEDURE sp_TuChoiSuKien
    @NguoiDuyetID INT,
    @SuKienID INT,
    @LyDoTuChoi NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@LyDoTuChoi)), N'') IS NULL
        THROW 50190, N'Phai nhap ly do tu choi su kien.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM TaiKhoan TK
        INNER JOIN VaiTro VT ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiDuyetID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro IN (N'QuanLySuKien', N'QuanTriVien')
    )
        THROW 50191, N'Khong co quyen tu choi su kien.', 1;

    IF EXISTS (
        SELECT 1
        FROM SuKien
        WHERE SuKienID = @SuKienID
          AND NguoiTaoID = @NguoiDuyetID
    )
        THROW 50192, N'Nguoi tao khong duoc tu tu choi su kien cua minh.', 1;

    UPDATE SuKien
    SET
        TrangThai = N'TuChoi',
        NguoiDuyetID = @NguoiDuyetID,
        NgayDuyet = SYSDATETIME(),
        LyDoTuChoi = @LyDoTuChoi
    WHERE SuKienID = @SuKienID
      AND TrangThai = N'ChoDuyet';

    IF @@ROWCOUNT = 0
        THROW 50193, N'Su kien khong ton tai hoac khong o trang thai cho duyet.', 1;

    SELECT *
    FROM SuKien
    WHERE SuKienID = @SuKienID;
END;
GO


/* 14. Tra cuu su kien theo loai va trang thai */
CREATE OR ALTER PROCEDURE sp_TraCuuSuKien
    @TuKhoa NVARCHAR(200) = NULL,
    @LoaiSuKienID INT = NULL,
    @TrangThai NVARCHAR(30) = NULL,
    @Trang INT = 1,
    @SoDong INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Trang < 1 OR @SoDong < 1 OR @SoDong > 200
        THROW 50200, N'Trang hoac so dong khong hop le.', 1;

    SELECT
        SK.SuKienID,
        SK.NguoiTaoID,
        SK.LoaiSuKienID,
        LSK.TenLoai,
        SK.TenSuKien,
        SK.MoTa,
        SK.AnhBia,
        SK.ThoiGianBatDau,
        SK.ThoiGianKetThuc,
        SK.SoLuongNguoiDuKien,
        SK.TrangThai,
        SK.NguoiDuyetID,
        SK.NgayDuyet,
        SK.LyDoTuChoi,
        SK.NgayTao
    FROM SuKien SK
    INNER JOIN LoaiSuKien LSK
        ON LSK.LoaiSuKienID = SK.LoaiSuKienID
    WHERE
        (@TuKhoa IS NULL OR SK.TenSuKien LIKE N'%' + @TuKhoa + N'%')
        AND (@LoaiSuKienID IS NULL OR SK.LoaiSuKienID = @LoaiSuKienID)
        AND (@TrangThai IS NULL OR SK.TrangThai = @TrangThai)
    ORDER BY SK.NgayTao DESC, SK.SuKienID DESC
    OFFSET (@Trang - 1) * @SoDong ROWS
    FETCH NEXT @SoDong ROWS ONLY;
END;
GO


/* 15. Thong ke su kien theo loai va trang thai */
CREATE OR ALTER PROCEDURE sp_ThongKeSuKien
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        LSK.LoaiSuKienID,
        LSK.TenLoai,
        SK.TrangThai,
        COUNT(SK.SuKienID) AS SoLuongSuKien
    FROM LoaiSuKien LSK
    LEFT JOIN SuKien SK
        ON SK.LoaiSuKienID = LSK.LoaiSuKienID
    GROUP BY
        LSK.LoaiSuKienID,
        LSK.TenLoai,
        SK.TrangThai
    ORDER BY LSK.TenLoai, SK.TrangThai;
END;
GO
