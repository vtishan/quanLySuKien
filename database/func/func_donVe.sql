-- Nhom 4: Quan ly ve, don ve va thanh toan
-- SQL Server
-- F01. Kiem tra loai ve dang hoat dong
CREATE OR ALTER FUNCTION dbo.fn_KiemTraLoaiVeHoatDong
(@LoaiVeID INT)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua AS BIT = 0;
    IF EXISTS (SELECT 1
               FROM   dbo.LoaiVe
               WHERE  LoaiVeID = @LoaiVeID
                      AND TrangThai = 1)
        SET @KetQua = 1;
    RETURN @KetQua;
END


GO
-- F02. Kiem tra thoi gian mo ban ve hop le
CREATE OR ALTER FUNCTION dbo.fn_KiemTraThoiGianBanVe
(@ThoiGianMoBan DATETIME2, @ThoiGianDongBan DATETIME2)
RETURNS BIT
AS
BEGIN
    RETURN CASE WHEN @ThoiGianMoBan IS NOT NULL
                     AND @ThoiGianDongBan IS NOT NULL
                     AND @ThoiGianDongBan > @ThoiGianMoBan THEN 1 ELSE 0 END;
END


GO
-- F03. Tinh so luong ve con lai
CREATE OR ALTER FUNCTION dbo.fn_LaySoLuongVeConLai
(@LoaiVeID INT)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuongConLai AS INT;
    SELECT @SoLuongConLai = SoLuongToiDa - SoLuongDaBan - SoLuongDangGiu
    FROM   dbo.LoaiVe
    WHERE  LoaiVeID = @LoaiVeID;
    RETURN @SoLuongConLai;
END


GO
-- F04. Kiem tra so luong ve co the mua
CREATE OR ALTER FUNCTION dbo.fn_KiemTraSoLuongVeCoTheMua
(@LoaiVeID INT, @SoLuongMua INT)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua AS BIT = 0;
    IF @SoLuongMua > 0
       AND EXISTS (SELECT 1
                   FROM   dbo.LoaiVe
                   WHERE  LoaiVeID = @LoaiVeID
                          AND TrangThai = 1
                          AND @SoLuongMua <= SoLuongToiDaMoiDon
                          AND @SoLuongMua <= SoLuongToiDa - SoLuongDaBan - SoLuongDangGiu)
        SET @KetQua = 1;
    RETURN @KetQua;
END


GO
-- F05. Tinh thanh tien cua chi tiet don ve
CREATE OR ALTER FUNCTION dbo.fn_TinhThanhTienVe
(@SoLuong INT, @DonGia DECIMAL (18, 2))
RETURNS DECIMAL (18, 2)
AS
BEGIN
    IF @SoLuong <= 0
       OR @DonGia < 0
        RETURN NULL;
    RETURN @SoLuong * @DonGia;
END


GO
-- F06. Tinh tong tien goc cua don ve
CREATE OR ALTER FUNCTION dbo.fn_TinhTongTienGocDonVe
(@DonVeID INT)
RETURNS DECIMAL (18, 2)
AS
BEGIN
    DECLARE @TongTien AS DECIMAL (18, 2);
    SELECT @TongTien = SUM(ThanhTien)
    FROM   dbo.ChiTietDonVe
    WHERE  DonVeID = @DonVeID;
    RETURN ISNULL(@TongTien, 0);
END


GO
-- F07. Tinh tong tien thanh toan sau giam gia
CREATE OR ALTER FUNCTION dbo.fn_TinhTongTienThanhToan
(@TongTienGoc DECIMAL (18, 2), @TongTienGiam DECIMAL (18, 2))
RETURNS DECIMAL (18, 2)
AS
BEGIN
    IF @TongTienGoc < 0
       OR @TongTienGiam < 0
       OR @TongTienGiam > @TongTienGoc
        RETURN NULL;
    RETURN @TongTienGoc - @TongTienGiam;
END


GO
-- F08. Kiem tra don ve da thanh toan thanh cong
CREATE OR ALTER FUNCTION dbo.fn_KiemTraDonVeDaThanhToan
(@DonVeID INT)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua AS BIT = 0;
    IF EXISTS (SELECT 1
               FROM   dbo.DonVe
               WHERE  DonVeID = @DonVeID
                      AND TrangThaiThanhToan = N'DaThanhToan'
                      AND TrangThaiDon = N'ThanhCong')
       AND EXISTS (SELECT 1
                   FROM   dbo.ThanhToan
                   WHERE  DonVeID = @DonVeID
                          AND LoaiGiaoDich = N'ThanhToan'
                          AND TrangThai = N'ThanhCong')
        SET @KetQua = 1;
    RETURN @KetQua;
END


GO
-- F09. Tinh tong tien thanh toan thanh cong cua don
CREATE OR ALTER FUNCTION dbo.fn_TinhTienDaThanhToan
(@DonVeID INT)
RETURNS DECIMAL (18, 2)
AS
BEGIN
    DECLARE @SoTien AS DECIMAL (18, 2);
    SELECT @SoTien = SUM(SoTien)
    FROM   dbo.ThanhToan
    WHERE  DonVeID = @DonVeID
           AND LoaiGiaoDich = N'ThanhToan'
           AND TrangThai = N'ThanhCong';
    RETURN ISNULL(@SoTien, 0);
END


GO
-- F10. Tinh tong tien hoan thanh cong cua don
CREATE OR ALTER FUNCTION dbo.fn_TinhTienDaHoan
(@DonVeID INT)
RETURNS DECIMAL (18, 2)
AS
BEGIN
    DECLARE @SoTien AS DECIMAL (18, 2);
    SELECT @SoTien = SUM(SoTien)
    FROM   dbo.ThanhToan
    WHERE  DonVeID = @DonVeID
           AND LoaiGiaoDich = N'HoanTien'
           AND TrangThai = N'ThanhCong';
    RETURN ISNULL(@SoTien, 0);
END


GO
-- F11. Kiem tra so tien hoan khong vuot tien da thanh toan
CREATE OR ALTER FUNCTION dbo.fn_KiemTraSoTienHoanHopLe
(@DonVeID INT, @SoTienHoanMoi DECIMAL (18, 2))
RETURNS BIT
AS
BEGIN
    DECLARE @TienDaThanhToan AS DECIMAL (18, 2);
    DECLARE @TienDaHoan AS DECIMAL (18, 2);
    SET @TienDaThanhToan = dbo.fn_TinhTienDaThanhToan(@DonVeID);
    SET @TienDaHoan = dbo.fn_TinhTienDaHoan(@DonVeID);
    IF @SoTienHoanMoi > 0
       AND @SoTienHoanMoi <= @TienDaThanhToan - @TienDaHoan
        RETURN 1;
    RETURN 0;
END


GO
-- F12. Kiem tra ve co the su dung
CREATE OR ALTER FUNCTION dbo.fn_KiemTraVeHopLe
(@VeID INT)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua AS BIT = 0;
    IF EXISTS (SELECT 1
               FROM   dbo.Ve AS V
                      INNER JOIN
                      dbo.DonVe AS D
                      ON D.DonVeID = V.DonVeID
                      INNER JOIN
                      dbo.LoaiVe AS LV
                      ON LV.LoaiVeID = V.LoaiVeID
               WHERE  V.VeID = @VeID
                      AND V.TrangThai = N'ChuaSuDung'
                      AND D.TrangThaiThanhToan = N'DaThanhToan'
                      AND D.TrangThaiDon = N'ThanhCong'
                      AND LV.SuKienID IS NOT NULL)
        SET @KetQua = 1;
    RETURN @KetQua;
END


GO
-- F13. Kiem tra ve thuoc dung loai ve cua don
CREATE OR ALTER FUNCTION dbo.fn_KiemTraVeThuocDonHopLe
(@VeID INT)
RETURNS BIT
AS
BEGIN
    DECLARE @KetQua AS BIT = 0;
    IF EXISTS (SELECT 1
               FROM   dbo.Ve AS V
                      INNER JOIN
                      dbo.ChiTietDonVe AS CT
                      ON CT.DonVeID = V.DonVeID
                         AND CT.LoaiVeID = V.LoaiVeID
               WHERE  V.VeID = @VeID)
        SET @KetQua = 1;
    RETURN @KetQua;
END


GO
-- F14. Dem ve theo trang thai
CREATE OR ALTER FUNCTION dbo.fn_DemVeTheoTrangThai
(@TrangThai NVARCHAR (30))
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong AS INT;
    SELECT @SoLuong = COUNT(*)
    FROM   dbo.Ve
    WHERE  TrangThai = @TrangThai;
    RETURN @SoLuong;
END


GO
-- F15. Dem ve da check-in cua mot su kien
CREATE OR ALTER FUNCTION dbo.fn_DemVeCheckInTheoSuKien
(@SuKienID INT)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong AS INT;
    SELECT @SoLuong = COUNT(*)
    FROM   dbo.Ve AS V
           INNER JOIN
           dbo.LoaiVe AS LV
           ON LV.LoaiVeID = V.LoaiVeID
    WHERE  LV.SuKienID = @SuKienID
           AND V.TrangThai = N'DaCheckIn';
    RETURN @SoLuong;
END


GO
-- F16. Dem lich su xuat ve
CREATE OR ALTER FUNCTION dbo.fn_DemLanXuatVe
(@VeID INT)
RETURNS INT
AS
BEGIN
    DECLARE @SoLuong AS INT;
    SELECT @SoLuong = COUNT(*)
    FROM   dbo.XuatVe
    WHERE  VeID = @VeID;
    RETURN @SoLuong;
END