/*
    WEBSITE QUAN LY SU KIEN
    NHOM 1: TAI KHOAN VA PHAN QUYEN
    HE QUAN TRI CO SO DU LIEU: SQL SERVER
*/

-- ==================================================
-- TRIGGER 1: KIEM TRA TEN VAI TRO
-- Khong cho phep ten vai tro rong hoac chi co khoang trang.
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_VaiTro_KiemTraTenVaiTro
ON dbo.VaiTro
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TenVaiTro IS NULL
           OR LTRIM(RTRIM(TenVaiTro)) = N''
    )
    BEGIN
        THROW 50001, N'Ten vai tro khong duoc de trong.', 1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 2: KIEM TRA THONG TIN TAI KHOAN
-- Khong cho phep ho ten hoac email rong.
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_TaiKhoan_KiemTraThongTin
ON dbo.TaiKhoan
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE HoTen IS NULL
           OR LTRIM(RTRIM(HoTen)) = N''
           OR Email IS NULL
           OR LTRIM(RTRIM(Email)) = N''
           OR MatKhauHash IS NULL
           OR LTRIM(RTRIM(MatKhauHash)) = N''
    )
    BEGIN
        THROW 50002,
              N'Ho ten, email va mat khau bam khong duoc de trong.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 3: KHONG CHO XOA VAI TRO DANG DUOC SU DUNG
-- Bao ve quan he giua VaiTro va TaiKhoan.
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_VaiTro_NganXoaVaiTroDangSuDung
ON dbo.VaiTro
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM deleted AS d
        INNER JOIN dbo.TaiKhoan AS tk
            ON tk.VaiTroID = d.VaiTroID
    )
    BEGIN
        THROW 50003,
              N'Khong the xoa vai tro dang duoc tai khoan su dung.',
              1;
    END;

    DELETE vt
    FROM dbo.VaiTro AS vt
    INNER JOIN deleted AS d
        ON d.VaiTroID = vt.VaiTroID;
END;
GO


-- ==================================================
-- TRIGGER 4: KHONG CHO XOA TAI KHOAN CO DU LIEU LIEN QUAN
-- Bao ve lich su yeu cau, su kien, don ve va thao tac.
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_TaiKhoan_NganXoaTaiKhoanCoLienQuan
ON dbo.TaiKhoan
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM deleted AS d
        WHERE EXISTS (
            SELECT 1
            FROM dbo.YeuCauXacThucToChuc AS yc
            WHERE yc.TaiKhoanID = d.TaiKhoanID
               OR yc.NguoiDuyetID = d.TaiKhoanID
        )
        OR EXISTS (
            SELECT 1
            FROM dbo.BanToChuc AS btc
            WHERE btc.TaiKhoanID = d.TaiKhoanID
        )
        OR EXISTS (
            SELECT 1
            FROM dbo.SuKien AS sk
            WHERE sk.NguoiTaoID = d.TaiKhoanID
               OR sk.NguoiDuyetID = d.TaiKhoanID
        )
        OR EXISTS (
            SELECT 1
            FROM dbo.DonVe AS dv
            WHERE dv.TaiKhoanID = d.TaiKhoanID
        )
        OR EXISTS (
            SELECT 1
            FROM dbo.Ve AS v
            WHERE v.NguoiCheckInID = d.TaiKhoanID
        )
        OR EXISTS (
            SELECT 1
            FROM dbo.XuatVe AS xv
            WHERE xv.NguoiXuatID = d.TaiKhoanID
        )
    )
    BEGIN
        THROW 50004,
              N'Khong the xoa tai khoan co du lieu nghiep vu lien quan. Hay khoa tai khoan thay vi xoa.',
              1;
    END;

    DELETE tk
    FROM dbo.TaiKhoan AS tk
    INNER JOIN deleted AS d
        ON d.TaiKhoanID = tk.TaiKhoanID;
END;
GO