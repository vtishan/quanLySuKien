
/*
    WEBSITE QUAN LY SU KIEN
    NHOM 2: XAC THUC BAN TO CHUC VA QUAN LY SU KIEN
    HE QUAN TRI CO SO DU LIEU: SQL SERVER
*/

-- ==================================================
-- TRIGGER 1: KIEM TRA YEU CAU XAC THUC TO CHUC
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_YeuCauXacThucToChuc_KiemTraDuLieu
ON dbo.YeuCauXacThucToChuc
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai NOT IN (N'ChoDuyet', N'DaDuyet', N'TuChoi')
    )
    BEGIN
        THROW 50101, N'Trang thai yeu cau xac thuc khong hop le.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai = N'DaDuyet'
          AND (
              NguoiDuyetID IS NULL
              OR NgayDuyet IS NULL
          )
    )
    BEGIN
        THROW 50102,
              N'Yeu cau da duyet phai co nguoi duyet va ngay duyet.',
              1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai = N'TuChoi'
          AND (
              NguoiDuyetID IS NULL
              OR NgayDuyet IS NULL
              OR LyDoTuChoi IS NULL
              OR LTRIM(RTRIM(LyDoTuChoi)) = N''
          )
    )
    BEGIN
        THROW 50103,
              N'Yeu cau tu choi phai co nguoi duyet, ngay duyet va ly do.',
              1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.TaiKhoan AS tk
            ON tk.TaiKhoanID = i.NguoiDuyetID
        WHERE i.NguoiDuyetID IS NOT NULL
          AND tk.TrangThai = 0
    )
    BEGIN
        THROW 50104, N'Nguoi duyet dang bi khoa.', 1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 2: KIEM TRA HO SO BAN TO CHUC
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_BanToChuc_KiemTraHoSo
ON dbo.BanToChuc
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.TaiKhoan AS tk
            ON tk.TaiKhoanID = i.TaiKhoanID
        WHERE tk.TrangThai = 0
    )
    BEGIN
        THROW 50105,
              N'Tai khoan bi khoa khong the kich hoat ho so ban to chuc.',
              1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        WHERE i.TrangThai = 1
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.YeuCauXacThucToChuc AS yc
              WHERE yc.TaiKhoanID = i.TaiKhoanID
                AND yc.TrangThai = N'DaDuyet'
                AND yc.SoCCCD = i.SoCCCD
          )
    )
    BEGIN
        THROW 50106,
              N'Ho so ban to chuc hoat dong phai co yeu cau xac thuc da duyet tuong ung.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 3: KIEM TRA LOAI SU KIEN
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_LoaiSuKien_KiemTraTenLoai
ON dbo.LoaiSuKien
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TenLoai IS NULL
           OR LTRIM(RTRIM(TenLoai)) = N''
    )
    BEGIN
        THROW 50107, N'Ten loai su kien khong duoc de trong.', 1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 4: KIEM TRA SU KIEN
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_SuKien_KiemTraDuLieu
ON dbo.SuKien
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiem tra thoi gian
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE ThoiGianKetThuc <= ThoiGianBatDau
    )
    BEGIN
        THROW 50108,
              N'Thoi gian ket thuc su kien phai sau thoi gian bat dau.',
              1;
    END;

    -- Kiem tra trang thai su kien
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai NOT IN (
            N'ChoDuyet',
            N'DaDuyet',
            N'TuChoi',
            N'DangDienRa',
            N'DaKetThuc',
            N'DaHuy'
        )
    )
    BEGIN
        THROW 50109, N'Trang thai su kien khong hop le.', 1;
    END;

    -- Khong cho nguoi tao tu duyet su kien cua minh
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai = N'DaDuyet'
          AND NguoiDuyetID = NguoiTaoID
    )
    BEGIN
        THROW 50110,
              N'Nguoi tao khong duoc tu phe duyet su kien cua minh.',
              1;
    END;

    -- Su kien da duyet phai co nguoi duyet va ngay duyet
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai = N'DaDuyet'
          AND (
              NguoiDuyetID IS NULL
              OR NgayDuyet IS NULL
          )
    )
    BEGIN
        THROW 50111,
              N'Su kien da duyet phai co nguoi duyet va ngay duyet.',
              1;
    END;

    -- Nguoi duyet khong duoc la tai khoan bi khoa
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.TaiKhoan AS tk
            ON tk.TaiKhoanID = i.NguoiDuyetID
        WHERE i.NguoiDuyetID IS NOT NULL
          AND tk.TrangThai = 0
    )
    BEGIN
        THROW 50112, N'Nguoi duyet su kien dang bi khoa.', 1;
    END;

    -- Chi ban to chuc da xac thuc moi duoc tao su kien
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.BanToChuc AS btc
            WHERE btc.TaiKhoanID = i.NguoiTaoID
              AND btc.TrangThai = 1
        )
    )
    BEGIN
        THROW 50113,
              N'Chi ban to chuc da xac thuc moi duoc tao su kien.',
              1;
    END;

    -- Tai khoan nguoi tao phai dang hoat dong
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.TaiKhoan AS tk
            ON tk.TaiKhoanID = i.NguoiTaoID
        WHERE tk.TrangThai = 0
    )
    BEGIN
        THROW 50114,
              N'Tai khoan nguoi tao su kien dang bi khoa.',
              1;
    END;
END;
GO
