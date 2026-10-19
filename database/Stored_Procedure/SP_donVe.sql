
USE [QuanLySuKien];
GO

/* =========================================================
   NHOM 4: QUAN LY VE, DON VE VA THANH TOAN
   ========================================================= */


/* 1. Them loai ve */
CREATE OR ALTER PROCEDURE dbo.sp_ThemLoaiVe
    @SuKienID INT,
    @TenLoaiVe NVARCHAR(100),
    @GiaVe DECIMAL(18,2),
    @SoLuongToiDa INT,
    @SoLuongToiDaMoiDon INT,
    @ThoiGianMoBan DATETIME2,
    @ThoiGianDongBan DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@TenLoaiVe)), N'') IS NULL
        THROW 50401, N'Ten loai ve khong duoc de trong.', 1;

    IF @GiaVe < 0 OR @SoLuongToiDa <= 0
       OR @SoLuongToiDaMoiDon <= 0
       OR @SoLuongToiDaMoiDon > @SoLuongToiDa
        THROW 50402, N'Thong tin gia ve hoac so luong khong hop le.', 1;

    IF dbo.fn_KiemTraThoiGianBanVe(
        @ThoiGianMoBan, @ThoiGianDongBan
    ) = 0
        THROW 50403, N'Thoi gian mo ban va dong ban khong hop le.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.SuKien
        WHERE SuKienID = @SuKienID
          AND TrangThai = N'DaDuyet'
    )
        THROW 50404, N'Su kien chua duoc duyet hoac khong ton tai.', 1;

    INSERT INTO dbo.LoaiVe (
        SuKienID,
        TenLoaiVe,
        GiaVe,
        SoLuongToiDa,
        SoLuongDaBan,
        SoLuongDangGiu,
        SoLuongToiDaMoiDon,
        ThoiGianMoBan,
        ThoiGianDongBan,
        TrangThai
    )
    VALUES (
        @SuKienID,
        @TenLoaiVe,
        @GiaVe,
        @SoLuongToiDa,
        0,
        0,
        @SoLuongToiDaMoiDon,
        @ThoiGianMoBan,
        @ThoiGianDongBan,
        1
    );

    SELECT *
    FROM dbo.LoaiVe
    WHERE LoaiVeID = CONVERT(INT, SCOPE_IDENTITY());
END;
GO


/* 2. Cap nhat loai ve */
CREATE OR ALTER PROCEDURE dbo.sp_CapNhatLoaiVe
    @LoaiVeID INT,
    @TenLoaiVe NVARCHAR(100),
    @GiaVe DECIMAL(18,2),
    @SoLuongToiDa INT,
    @SoLuongToiDaMoiDon INT,
    @ThoiGianMoBan DATETIME2,
    @ThoiGianDongBan DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @GiaVe < 0 OR @SoLuongToiDa <= 0
       OR @SoLuongToiDaMoiDon <= 0
       OR @SoLuongToiDaMoiDon > @SoLuongToiDa
        THROW 50410, N'Gia ve hoac so luong khong hop le.', 1;

    IF dbo.fn_KiemTraThoiGianBanVe(
        @ThoiGianMoBan, @ThoiGianDongBan
    ) = 0
        THROW 50411, N'Thoi gian ban ve khong hop le.', 1;

    BEGIN TRANSACTION;

    DECLARE @DaBan INT;
    DECLARE @DangGiu INT;

    SELECT
        @DaBan = SoLuongDaBan,
        @DangGiu = SoLuongDangGiu
    FROM dbo.LoaiVe WITH (UPDLOCK, HOLDLOCK)
    WHERE LoaiVeID = @LoaiVeID;

    IF @DaBan IS NULL OR @DangGiu IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50412, N'Loai ve khong ton tai.', 1;
    END;

    IF @SoLuongToiDa < @DaBan + @DangGiu
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50413, N'So luong toi da khong duoc nho hon so ve da ban va dang giu.', 1;
    END;

    UPDATE dbo.LoaiVe
    SET
        TenLoaiVe = @TenLoaiVe,
        GiaVe = @GiaVe,
        SoLuongToiDa = @SoLuongToiDa,
        SoLuongToiDaMoiDon = @SoLuongToiDaMoiDon,
        ThoiGianMoBan = @ThoiGianMoBan,
        ThoiGianDongBan = @ThoiGianDongBan
    WHERE LoaiVeID = @LoaiVeID;

    COMMIT TRANSACTION;

    SELECT *
    FROM dbo.LoaiVe
    WHERE LoaiVeID = @LoaiVeID;
END;
GO


/* 3. Tao don ve va giu so luong ve */
CREATE OR ALTER PROCEDURE dbo.sp_TaoDonVe
    @TaiKhoanID INT,
    @LoaiVeID INT,
    @SoLuong INT,
    @TongTienGiam DECIMAL(18,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @SoLuong IS NULL OR @SoLuong <= 0
        THROW 50420, N'So luong ve phai lon hon 0.', 1;

    IF @TongTienGiam IS NULL OR @TongTienGiam < 0
        THROW 50421, N'Tien giam khong hop le.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.TaiKhoan
            WHERE TaiKhoanID = @TaiKhoanID
              AND TrangThai = 1
        )
            THROW 50422, N'Tai khoan khong ton tai hoac da bi khoa.', 1;

        DECLARE @GiaVe DECIMAL(18,2);
        DECLARE @TongTienGoc DECIMAL(18,2);
        DECLARE @TongTienThanhToan DECIMAL(18,2);
        DECLARE @SoLuongConLai INT;
        DECLARE @SoLuongDaBan INT;
        DECLARE @SoLuongDangGiu INT;
        DECLARE @SoLuongToiDa INT;
        DECLARE @SoLuongToiDaMoiDon INT;
        DECLARE @ThoiGianMoBan DATETIME2;
        DECLARE @ThoiGianDongBan DATETIME2;

        SELECT
            @GiaVe = GiaVe,
            @SoLuongDaBan = SoLuongDaBan,
            @SoLuongDangGiu = SoLuongDangGiu,
            @SoLuongToiDa = SoLuongToiDa,
            @SoLuongToiDaMoiDon = SoLuongToiDaMoiDon,
            @ThoiGianMoBan = ThoiGianMoBan,
            @ThoiGianDongBan = ThoiGianDongBan
        FROM dbo.LoaiVe WITH (UPDLOCK, HOLDLOCK)
        WHERE LoaiVeID = @LoaiVeID
          AND TrangThai = 1;

        IF @GiaVe IS NULL
            THROW 50423, N'Loai ve khong ton tai hoac da bi khoa.', 1;

        IF SYSDATETIME() < @ThoiGianMoBan
           OR SYSDATETIME() >= @ThoiGianDongBan
            THROW 50424, N'Loai ve chua mo ban hoac da dong ban.', 1;

        SET @SoLuongConLai =
            @SoLuongToiDa - @SoLuongDaBan - @SoLuongDangGiu;

        IF @SoLuong > @SoLuongToiDaMoiDon
           OR @SoLuong > @SoLuongConLai
            THROW 50425, N'So luong ve vuot gioi han cho phep.', 1;

        SET @TongTienGoc = @GiaVe * @SoLuong;

        IF @TongTienGiam > @TongTienGoc
            THROW 50426, N'Tien giam vuot qua tong tien goc.', 1;

        SET @TongTienThanhToan =
            dbo.fn_TinhTongTienThanhToan(
                @TongTienGoc, @TongTienGiam
            );

        IF @TongTienThanhToan IS NULL
            THROW 50427, N'Khong tinh duoc tong tien thanh toan.', 1;

        INSERT INTO dbo.DonVe (
            TaiKhoanID,
            TongTienGoc,
            TongTienGiam,
            TongTienThanhToan,
            TrangThaiThanhToan,
            TrangThaiDon
        )
        VALUES (
            @TaiKhoanID,
            @TongTienGoc,
            @TongTienGiam,
            @TongTienThanhToan,
            N'ChoThanhToan',
            N'ChoThanhToan'
        );

        DECLARE @DonVeID INT =
            CONVERT(INT, SCOPE_IDENTITY());

        INSERT INTO dbo.ChiTietDonVe (
            DonVeID,
            LoaiVeID,
            SoLuong,
            DonGia,
            ThanhTien
        )
        VALUES (
            @DonVeID,
            @LoaiVeID,
            @SoLuong,
            @GiaVe,
            dbo.fn_TinhThanhTienVe(@SoLuong, @GiaVe)
        );

        UPDATE dbo.LoaiVe
        SET SoLuongDangGiu = SoLuongDangGiu + @SoLuong
        WHERE LoaiVeID = @LoaiVeID;

        COMMIT TRANSACTION;

        SELECT *
        FROM dbo.DonVe
        WHERE DonVeID = @DonVeID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 4. Ghi nhan ket qua thanh toan */
CREATE OR ALTER PROCEDURE dbo.sp_GhiNhanThanhToan
    @DonVeID INT,
    @SoTien DECIMAL(18,2),
    @MaGiaoDich NVARCHAR(200),
    @TrangThai NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @SoTien <= 0
        THROW 50430, N'So tien thanh toan phai lon hon 0.', 1;

    IF @TrangThai NOT IN (N'ThanhCong', N'ThatBai', N'ChoXuLy')
        THROW 50431, N'Trang thai thanh toan khong hop le.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @TongTien DECIMAL(18,2);
        DECLARE @TrangThaiDon NVARCHAR(30);

        SELECT
            @TongTien = TongTienThanhToan,
            @TrangThaiDon = TrangThaiDon
        FROM dbo.DonVe WITH (UPDLOCK, HOLDLOCK)
        WHERE DonVeID = @DonVeID;

        IF @TongTien IS NULL
            THROW 50432, N'Don ve khong ton tai.', 1;

        IF @TrangThaiDon <> N'ChoThanhToan'
            THROW 50433, N'Don ve khong con cho thanh toan.', 1;

        IF @TrangThai = N'ThanhCong'
           AND @SoTien <> @TongTien
            THROW 50434, N'So tien thanh toan khong khop tong tien don.', 1;

        INSERT INTO dbo.ThanhToan (
            DonVeID,
            LoaiGiaoDich,
            SoTien,
            MaGiaoDich,
            TrangThai
        )
        VALUES (
            @DonVeID,
            N'ThanhToan',
            @SoTien,
            @MaGiaoDich,
            @TrangThai
        );

        IF @TrangThai = N'ThanhCong'
        BEGIN
            UPDATE dbo.DonVe
            SET
                TrangThaiThanhToan = N'DaThanhToan',
                TrangThaiDon = N'ThanhCong'
            WHERE DonVeID = @DonVeID;

            UPDATE LV
            SET
                SoLuongDangGiu = SoLuongDangGiu - CT.SoLuong,
                SoLuongDaBan = SoLuongDaBan + CT.SoLuong
            FROM dbo.LoaiVe LV
            INNER JOIN dbo.ChiTietDonVe CT
                ON CT.LoaiVeID = LV.LoaiVeID
            WHERE CT.DonVeID = @DonVeID;
        END;

        COMMIT TRANSACTION;

        SELECT *
        FROM dbo.ThanhToan
        WHERE DonVeID = @DonVeID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 5. Huy don ve chua thanh toan va giai phong ve giu */
CREATE OR ALTER PROCEDURE dbo.sp_HuyDonVe
    @DonVeID INT,
    @TaiKhoanID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.DonVe WITH (UPDLOCK, HOLDLOCK)
            WHERE DonVeID = @DonVeID
              AND TaiKhoanID = @TaiKhoanID
              AND TrangThaiDon = N'ChoThanhToan'
        )
            THROW 50440, N'Don ve khong ton tai, khong thuoc tai khoan hoac khong the huy.', 1;

        UPDATE LV
        SET SoLuongDangGiu = SoLuongDangGiu - CT.SoLuong
        FROM dbo.LoaiVe LV
        INNER JOIN dbo.ChiTietDonVe CT
            ON CT.LoaiVeID = LV.LoaiVeID
        WHERE CT.DonVeID = @DonVeID;

        UPDATE dbo.DonVe
        SET TrangThaiDon = N'DaHuy'
        WHERE DonVeID = @DonVeID;

        COMMIT TRANSACTION;

        SELECT *
        FROM dbo.DonVe
        WHERE DonVeID = @DonVeID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 6. Ghi nhan hoan tien */
CREATE OR ALTER PROCEDURE dbo.sp_GhiNhanHoanTien
    @DonVeID INT,
    @SoTienHoan DECIMAL(18,2),
    @MaGiaoDich NVARCHAR(200),
    @TrangThai NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @TrangThai NOT IN (N'ThanhCong', N'ThatBai', N'ChoXuLy')
        THROW 50450, N'Trang thai hoan tien khong hop le.', 1;

    IF @SoTienHoan <= 0
        THROW 50451, N'So tien hoan phai lon hon 0.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.DonVe WITH (UPDLOCK, HOLDLOCK)
            WHERE DonVeID = @DonVeID
        )
            THROW 50452, N'Don ve khong ton tai.', 1;

        IF @TrangThai = N'ThanhCong'
           AND dbo.fn_KiemTraSoTienHoanHopLe(
               @DonVeID, @SoTienHoan
           ) = 0
            THROW 50453, N'So tien hoan vuot so tien thanh toan con lai.', 1;

        INSERT INTO dbo.ThanhToan (
            DonVeID,
            LoaiGiaoDich,
            SoTien,
            MaGiaoDich,
            TrangThai
        )
        VALUES (
            @DonVeID,
            N'HoanTien',
            @SoTienHoan,
            @MaGiaoDich,
            @TrangThai
        );

        IF @TrangThai = N'ThanhCong'
           AND dbo.fn_TinhTienDaHoan(@DonVeID) >=
               dbo.fn_TinhTienDaThanhToan(@DonVeID)
        BEGIN
            UPDATE dbo.DonVe
            SET TrangThaiDon = N'DaHuy'
            WHERE DonVeID = @DonVeID;
        END;

        COMMIT TRANSACTION;

        SELECT *
        FROM dbo.ThanhToan
        WHERE DonVeID = @DonVeID
          AND LoaiGiaoDich = N'HoanTien';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 7. Check-in ve */
CREATE OR ALTER PROCEDURE dbo.sp_CheckInVe
    @VeID INT,
    @NguoiCheckInID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.TaiKhoan TK
        INNER JOIN dbo.VaiTro VT
            ON VT.VaiTroID = TK.VaiTroID
        WHERE TK.TaiKhoanID = @NguoiCheckInID
          AND TK.TrangThai = 1
          AND VT.TenVaiTro IN (N'BanToChuc', N'QuanLySuKien', N'QuanTriVien')
    )
        THROW 50460, N'Khong co quyen check-in ve.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF dbo.fn_KiemTraVeHopLe(@VeID) = 0
            THROW 50461, N'Ve khong hop le hoac khong the su dung.', 1;

        UPDATE dbo.Ve
        SET
            TrangThai = N'DaCheckIn',
            NguoiCheckInID = @NguoiCheckInID,
            ThoiGianCheckIn = SYSDATETIME()
        WHERE VeID = @VeID
          AND TrangThai = N'ChuaSuDung';

        IF @@ROWCOUNT = 0
            THROW 50462, N'Ve da duoc check-in hoac khong the su dung.', 1;

        COMMIT TRANSACTION;

        SELECT *
        FROM dbo.Ve
        WHERE VeID = @VeID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO


/* 8. Tra cuu ve theo don */
CREATE OR ALTER PROCEDURE dbo.sp_TraCuuVeTheoDon
    @DonVeID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.VeID,
        V.DonVeID,
        V.LoaiVeID,
        V.MaVe,
        V.MaQR,
        V.TrangThai,
        V.NguoiCheckInID,
        V.ThoiGianCheckIn
    FROM dbo.Ve V
    WHERE V.DonVeID = @DonVeID
    ORDER BY V.VeID;
END;
GO


/* 9. Ghi lich su xuat ve */
CREATE OR ALTER PROCEDURE dbo.sp_GhiLichSuXuatVe
    @VeID INT,
    @HinhThucXuat NVARCHAR(30),
    @NguoiThucHienID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @HinhThucXuat NOT IN (
        N'PDF', N'Email', N'InTaiQuay', N'Kiosk'
    )
        THROW 50470, N'Hinh thuc xuat ve khong hop le.', 1;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Ve
        WHERE VeID = @VeID
          AND dbo.fn_KiemTraVeThuocDonHopLe(VeID) = 1
          AND dbo.fn_KiemTraDonVeDaThanhToan(DonVeID) = 1
    )
        THROW 50471, N'Ve khong hop le hoac don ve chua thanh toan.', 1;

    INSERT INTO dbo.XuatVe (
        VeID,
        HinhThucXuat,
        NguoiThucHienID,
        NgayXuat
    )
    VALUES (
        @VeID,
        @HinhThucXuat,
        @NguoiThucHienID,
        SYSDATETIME()
    );

    SELECT *
    FROM dbo.XuatVe
    WHERE XuatVeID = CONVERT(INT, SCOPE_IDENTITY());
END;
GO


/* 10. Thong ke ve theo trang thai */
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeVeTheoTrangThai
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TrangThai,
        COUNT(*) AS SoLuongVe
    FROM dbo.Ve
    GROUP BY TrangThai
    ORDER BY TrangThai;
END;
GO


/* 11. Thong ke check-in theo su kien */
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeCheckInTheoSuKien
    @SuKienID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        @SuKienID AS SuKienID,
        COUNT(*) AS TongVeDaPhatHanh,
        SUM(CASE
            WHEN V.TrangThai = N'DaCheckIn' THEN 1
            ELSE 0
        END) AS TongVeDaCheckIn,
        SUM(CASE
            WHEN V.TrangThai = N'ChuaSuDung' THEN 1
            ELSE 0
        END) AS TongVeChuaCheckIn
    FROM dbo.Ve V
    INNER JOIN dbo.LoaiVe LV
        ON LV.LoaiVeID = V.LoaiVeID
    WHERE LV.SuKienID = @SuKienID;
END;
GO


/* 12. Thong ke giao dich theo loai va trang thai */
CREATE OR ALTER PROCEDURE dbo.sp_ThongKeGiaoDich
    @DonVeID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        LoaiGiaoDich,
        TrangThai,
        COUNT(*) AS SoGiaoDich,
        SUM(SoTien) AS TongSoTien
    FROM dbo.ThanhToan
    WHERE @DonVeID IS NULL OR DonVeID = @DonVeID
    GROUP BY LoaiGiaoDich, TrangThai
    ORDER BY LoaiGiaoDich, TrangThai;
END;
GO
