
/*
    WEBSITE QUAN LY SU KIEN
    NHOM 4: QUAN LY VE, DON VE VA THANH TOAN
    HE QUAN TRI CO SO DU LIEU: SQL SERVER
*/

-- ==================================================
-- TRIGGER 1: KIEM TRA LOAI VE
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_LoaiVe_KiemTraDuLieu
ON dbo.LoaiVe
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE LTRIM(RTRIM(TenLoai)) = N''
           OR GiaVe < 0
           OR SoLuongToiDa <= 0
           OR SoLuongDaBan < 0
           OR SoLuongDangGiu < 0
           OR SoLuongDaBan + SoLuongDangGiu > SoLuongToiDa
           OR SoLuongToiDaMoiDon <= 0
           OR ThoiGianDongBan <= ThoiGianMoBan
    )
    BEGIN
        THROW 50401,
              N'Du lieu loai ve khong hop le.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 2: KIEM TRA DON VE
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_DonVe_KiemTraDuLieu
ON dbo.DonVe
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TongTienGoc < 0
           OR TongTienGiam < 0
           OR TongTienThanhToan < 0
           OR TongTienGiam > TongTienGoc
           OR TongTienThanhToan <> TongTienGoc - TongTienGiam
    )
    BEGIN
        THROW 50402,
              N'Tong tien don ve khong hop le.',
              1;
    END;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThaiThanhToan NOT IN (
            N'ChuaThanhToan',
            N'DaThanhToan',
            N'HoanTienMotPhan',
            N'DaHoanTien'
        )
        OR TrangThaiDon NOT IN (
            N'ChoThanhToan',
            N'ThanhCong',
            N'BiHetHan',
            N'DaHuy',
            N'YeuCauHoanTien',
            N'DaHoanTien'
        )
    )
    BEGIN
        THROW 50403,
              N'Trang thai don ve hoac thanh toan khong hop le.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 3: KIEM TRA CHI TIET DON VE
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_ChiTietDonVe_KiemTraDuLieu
ON dbo.ChiTietDonVe
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Thanh tien phai bang so luong nhan don gia
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE SoLuong <= 0
           OR DonGia < 0
           OR ThanhTien < 0
           OR ThanhTien <> SoLuong * DonGia
    )
    BEGIN
        THROW 50404,
              N'So luong, don gia hoac thanh tien khong hop le.',
              1;
    END;

    -- Moi don chi duoc mua trong gioi han cua tung loai ve
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT DISTINCT DonVeID, LoaiVeID
            FROM inserted
        ) AS i
        INNER JOIN dbo.ChiTietDonVe AS ct
            ON ct.DonVeID = i.DonVeID
           AND ct.LoaiVeID = i.LoaiVeID
        INNER JOIN dbo.LoaiVe AS lv
            ON lv.LoaiVeID = i.LoaiVeID
        GROUP BY i.DonVeID, i.LoaiVeID,
                 lv.SoLuongToiDaMoiDon
        HAVING SUM(ct.SoLuong) > lv.SoLuongToiDaMoiDon
    )
    BEGIN
        THROW 50405,
              N'So luong ve trong don vuot gioi han mua.',
              1;
    END;

    -- Chi cho them chi tiet vao don dang cho thanh toan
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.DonVe AS dv
            ON dv.DonVeID = i.DonVeID
        WHERE dv.TrangThaiDon <> N'ChoThanhToan'
           OR dv.TrangThaiThanhToan <> N'ChuaThanhToan'
    )
    BEGIN
        THROW 50406,
              N'Chi duoc thay doi chi tiet cua don chua thanh toan.',
              1;
    END;

    -- Loai ve phai thuoc su kien da duoc phe duyet
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.LoaiVe AS lv
            ON lv.LoaiVeID = i.LoaiVeID
        INNER JOIN dbo.SuKien AS sk
            ON sk.SuKienID = lv.SuKienID
        WHERE lv.TrangThai = 0
           OR sk.TrangThai <> N'DaDuyet'
    )
    BEGIN
        THROW 50407,
              N'Loai ve khong hoat dong hoac su kien chua duoc duyet.',
              1;
    END;

    -- Cap nhat lai tong tien goc tu cac dong chi tiet
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT DISTINCT DonVeID
            FROM inserted
        ) AS i
        INNER JOIN dbo.DonVe AS dv
            ON dv.DonVeID = i.DonVeID
        WHERE dv.TongTienGiam > (
            SELECT COALESCE(SUM(ct.ThanhTien), 0)
            FROM dbo.ChiTietDonVe AS ct
            WHERE ct.DonVeID = i.DonVeID
        )
    )
    BEGIN
        THROW 50408,
              N'Tong tien giam khong duoc lon hon tong tien goc.',
              1;
    END;

    UPDATE dv
    SET TongTienGoc = tt.TongTienGoc,
        TongTienThanhToan = tt.TongTienGoc - dv.TongTienGiam
    FROM dbo.DonVe AS dv
    INNER JOIN (
        SELECT DISTINCT DonVeID
        FROM inserted
    ) AS i
        ON i.DonVeID = dv.DonVeID
    CROSS APPLY (
        SELECT COALESCE(SUM(ct.ThanhTien), 0) AS TongTienGoc
        FROM dbo.ChiTietDonVe AS ct
        WHERE ct.DonVeID = dv.DonVeID
    ) AS tt;
END;
GO


-- ==================================================
-- TRIGGER 4: KIEM TRA THANH TOAN VA HOAN TIEN
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_ThanhToan_KiemTraGiaoDich
ON dbo.ThanhToan
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE SoTien <= 0
           OR LoaiGiaoDich NOT IN (N'ThanhToan', N'HoanTien')
           OR TrangThai NOT IN (
               N'DangXuLy',
               N'ThanhCong',
               N'ThatBai',
               N'NghiVanLoi'
           )
           OR LTRIM(RTRIM(PhuongThucThanhToan)) = N''
           OR LTRIM(RTRIM(MaGiaoDichCongThanhToan)) = N''
    )
    BEGIN
        THROW 50409,
              N'Thong tin giao dich thanh toan khong hop le.',
              1;
    END;

    /*
        Kiem tra tong tien hoan thanh cong khong vuot
        tong tien thanh toan thanh cong cua tung don.
    */
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT DISTINCT DonVeID
            FROM inserted
            WHERE LoaiGiaoDich = N'HoanTien'
              AND TrangThai = N'ThanhCong'
        ) AS i
        INNER JOIN dbo.ThanhToan AS tt
            ON tt.DonVeID = i.DonVeID
        GROUP BY i.DonVeID
        HAVING
            SUM(
                CASE
                    WHEN tt.LoaiGiaoDich = N'HoanTien'
                     AND tt.TrangThai = N'ThanhCong'
                    THEN tt.SoTien
                    ELSE 0
                END
            )
            >
            SUM(
                CASE
                    WHEN tt.LoaiGiaoDich = N'ThanhToan'
                     AND tt.TrangThai = N'ThanhCong'
                    THEN tt.SoTien
                    ELSE 0
                END
            )
    )
    BEGIN
        THROW 50410,
              N'Tong tien hoan vuot qua tong tien da thanh toan thanh cong.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 5: KIEM TRA VE VA CHECK-IN
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_Ve_KiemTraDuLieu
ON dbo.Ve
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai NOT IN (
            N'GiuCho',
            N'ChuaSuDung',
            N'DaCheckIn',
            N'DaHuy',
            N'DaHoanTien'
        )
    )
    BEGIN
        THROW 50411,
              N'Trang thai ve khong hop le.',
              1;
    END;

    -- Ve da check-in phai co thoi gian va nguoi check-in
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE TrangThai = N'DaCheckIn'
          AND (
              ThoiGianCheckIn IS NULL
              OR NguoiCheckInID IS NULL
          )
    )
    BEGIN
        THROW 50412,
              N'Ve da check-in phai co thoi gian va nguoi check-in.',
              1;
    END;

    -- Khong cho check-in ve da huy hoac hoan tien
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN deleted AS d
            ON d.VeID = i.VeID
        WHERE i.TrangThai = N'DaCheckIn'
          AND d.TrangThai IN (N'DaHuy', N'DaHoanTien')
    )
    BEGIN
        THROW 50413,
              N'Khong the check-in ve da huy hoac da hoan tien.',
              1;
    END;

    -- Ve su dung duoc phai thuoc don da thanh toan
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.DonVe AS dv
            ON dv.DonVeID = i.DonVeID
        WHERE i.TrangThai IN (N'ChuaSuDung', N'DaCheckIn')
          AND dv.TrangThaiThanhToan NOT IN (
              N'DaThanhToan',
              N'HoanTienMotPhan'
          )
    )
    BEGIN
        THROW 50414,
              N'Chi phat hanh hoac check-in ve khi don da thanh toan.',
              1;
    END;

    -- Loai ve phai nam trong chi tiet cua don tuong ung
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        WHERE NOT EXISTS (
            SELECT 1
            FROM dbo.ChiTietDonVe AS ct
            WHERE ct.DonVeID = i.DonVeID
              AND ct.LoaiVeID = i.LoaiVeID
        )
    )
    BEGIN
        THROW 50415,
              N'Loai ve khong thuoc don ve tuong ung.',
              1;
    END;

    -- Tong so ve da tao khong duoc vuot so luong da mua
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT DISTINCT DonVeID, LoaiVeID
            FROM inserted
        ) AS i
        INNER JOIN dbo.Ve AS v
            ON v.DonVeID = i.DonVeID
           AND v.LoaiVeID = i.LoaiVeID
        INNER JOIN dbo.ChiTietDonVe AS ct
            ON ct.DonVeID = i.DonVeID
           AND ct.LoaiVeID = i.LoaiVeID
        GROUP BY i.DonVeID, i.LoaiVeID, ct.SoLuong
        HAVING COUNT_BIG(v.VeID) > ct.SoLuong
    )
    BEGIN
        THROW 50416,
              N'So ve da tao vuot so luong ve trong don.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 6: KIEM TRA LICH SU XUAT VE
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_XuatVe_KiemTraDuLieu
ON dbo.XuatVe
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE HinhThucXuat NOT IN (
            N'ExportPDF',
            N'InVeGiayTaiQuay',
            N'GuiEmail',
            N'InQuaKiosk'
        )
        OR LanXuat <= 0
    )
    BEGIN
        THROW 50417,
              N'Hinh thuc xuat hoac lan xuat khong hop le.',
              1;
    END;

    -- Chi xuat ve da phat hanh va chua bi huy/hoan tien
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.Ve AS v
            ON v.VeID = i.VeID
        WHERE i.TrangThai = N'ThanhCong'
          AND v.TrangThai NOT IN (
              N'ChuaSuDung',
              N'DaCheckIn'
          )
    )
    BEGIN
        THROW 50418,
              N'Chi xuat ve hop le, chua bi huy hoac hoan tien.',
              1;
    END;
END;
GO


-- ==================================================
-- TRIGGER 7: BAO VE LICH SU GIAO DICH
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_ThanhToan_NganXoaGiaoDich
ON dbo.ThanhToan
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    THROW 50419,
          N'Khong duoc xoa lich su thanh toan hoac hoan tien.',
          1;
END;
GO


-- ==================================================
-- TRIGGER 8: BAO VE LICH SU VE DA PHAT HANH
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_Ve_NganXoaVe
ON dbo.Ve
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    THROW 50420,
          N'Khong duoc xoa ve; hay cap nhat trang thai ve theo nghiep vu.',
          1;
END;
GO


-- ==================================================
-- TRIGGER 9: BAO VE LICH SU XUAT VE
-- ==================================================

CREATE OR ALTER TRIGGER dbo.trg_XuatVe_NganXoaLichSu
ON dbo.XuatVe
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    THROW 50421,
          N'Khong duoc xoa lich su xuat ve.',
          1;
END;
GO
