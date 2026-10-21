
/*
    WEBSITE QUAN LY SU KIEN
    NHOM 3: DIA DIEM VA DAT DIA DIEM
    HE QUAN TRI CO SO DU LIEU: SQL SERVER
*/

-- ==================================================
-- TRIGGER 1: KIEM TRA DU LIEU DIA DIEM
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_DiaDiem_KiemTraDuLieu
ON dbo.DiaDiem
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE LTRIM(RTRIM(TenDiaDiem)) = N''
           OR LTRIM(RTRIM(DiaChi)) = N''
           OR SucChua <= 0
           OR GiaThue < 0
    )
    BEGIN
        THROW 50301,
              N'Thong tin dia diem khong hop le.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 2: KIEM TRA DU LIEU DAT DIA DIEM
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_DatDiaDiem_KiemTraDuLieu
ON dbo.DatDiaDiem
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiem tra khoang thoi gian
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE ThoiGianKetThuc <= ThoiGianBatDau
    )
    BEGIN
        THROW 50302,
              N'Thoi gian ket thuc phai sau thoi gian bat dau.',
              1;
    END;

    -- Kiem tra tien coc
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TongTien < 0
           OR TienCoc < 0
           OR TienCoc > TongTien
    )
    BEGIN
        THROW 50303,
              N'Tien coc phai nam trong khoang tu 0 den tong tien.',
              1;
    END;

    -- Kiem tra trang thai dat dia diem
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai NOT IN (
            N'ChoXacNhan',
            N'DaXacNhan',
            N'TuChoi',
            N'DaHuy',
            N'HoanThanh'
        )
    )
    BEGIN
        THROW 50304,
              N'Trang thai dat dia diem khong hop le.',
              1;
    END;

    -- Khong cho dat dia diem dang ngung hoat dong
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.DiaDiem AS dd
            ON dd.DiaDiemID = i.DiaDiemID
        WHERE dd.TrangThai = 0
          AND i.TrangThai IN (
              N'ChoXacNhan',
              N'DaXacNhan'
          )
    )
    BEGIN
        THROW 50305,
              N'Khong the dat dia diem dang ngung hoat dong.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 3: NGAN TRUNG LICH DAT DIA DIEM
-- Hai khoang thoi gian trung nhau khi:
-- BatDauA < KetThucB va KetThucA > BatDauB
-- Chi xet cac don dat dang co hieu luc.
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_DatDiaDiem_NganTrungLich
ON dbo.DatDiaDiem
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.DatDiaDiem AS d
            ON d.DiaDiemID = i.DiaDiemID
           AND d.DatDiaDiemID <> i.DatDiaDiemID
           AND i.ThoiGianBatDau < d.ThoiGianKetThuc
           AND i.ThoiGianKetThuc > d.ThoiGianBatDau
        WHERE i.TrangThai IN (
                  N'ChoXacNhan',
                  N'DaXacNhan'
              )
          AND d.TrangThai IN (
                  N'ChoXacNhan',
                  N'DaXacNhan'
              )
    )
    BEGIN
        THROW 50306,
              N'Dia diem da co lich dat trung trong khoang thoi gian nay.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 4: KHONG CHO NGUNG DIA DIEM KHI CO LICH DAT
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_DiaDiem_KiemTraNgungHoatDong
ON dbo.DiaDiem
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(TrangThai)
       AND EXISTS (
            SELECT 1
            FROM inserted AS i
            INNER JOIN deleted AS old
                ON old.DiaDiemID = i.DiaDiemID
            INNER JOIN dbo.DatDiaDiem AS d
                ON d.DiaDiemID = i.DiaDiemID
            WHERE old.TrangThai = 1
              AND i.TrangThai = 0
              AND d.TrangThai IN (
                  N'ChoXacNhan',
                  N'DaXacNhan'
              )
       )
    BEGIN
        THROW 50307,
              N'Khong the ngung dia diem khi van con lich dat co hieu luc.',
              1;
    END;
END;
GO
