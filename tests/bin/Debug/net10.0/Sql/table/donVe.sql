CREATE TABLE LoaiVe (
    LoaiVeID INT IDENTITY(1,1) PRIMARY KEY,
    SuKienID INT NOT NULL,
    TenLoai NVARCHAR(100) NOT NULL,
    GiaVe DECIMAL(18,2) NOT NULL,
    SoLuongToiDa INT NOT NULL,
    SoLuongDaBan INT NOT NULL DEFAULT 0,
    SoLuongDangGiu INT NOT NULL DEFAULT 0,
    SoLuongToiDaMoiDon INT NOT NULL DEFAULT 5,
    ThoiGianMoBan DATETIME2 NOT NULL,
    ThoiGianDongBan DATETIME2 NOT NULL,
    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_LoaiVe_SuKien
        FOREIGN KEY (SuKienID)
        REFERENCES SuKien(SuKienID),

    CONSTRAINT CK_LoaiVe_GiaVe
        CHECK (GiaVe >= 0),

    CONSTRAINT CK_LoaiVe_SoLuongToiDa
        CHECK (SoLuongToiDa > 0),

    CONSTRAINT CK_LoaiVe_SoLuongDaBan
        CHECK (SoLuongDaBan >= 0),

    CONSTRAINT CK_LoaiVe_SoLuongDangGiu
        CHECK (SoLuongDangGiu >= 0),

    CONSTRAINT CK_LoaiVe_SoLuong
        CHECK (SoLuongDaBan + SoLuongDangGiu <= SoLuongToiDa),

    CONSTRAINT CK_LoaiVe_SoLuongToiDaMoiDon
        CHECK (SoLuongToiDaMoiDon > 0),

    CONSTRAINT CK_LoaiVe_ThoiGian
        CHECK (ThoiGianDongBan > ThoiGianMoBan)
);
GO

CREATE TABLE DonVe (
    DonVeID INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID INT NOT NULL,
    MaDonHang NVARCHAR(50) NOT NULL UNIQUE,
    TongTienGoc DECIMAL(18,2) NOT NULL,
    MaGiamGia NVARCHAR(50) NULL,
    TongTienGiam DECIMAL(18,2) NOT NULL DEFAULT 0,
    TongTienThanhToan DECIMAL(18,2) NOT NULL,
    TrangThaiThanhToan NVARCHAR(30) NOT NULL DEFAULT N'ChuaThanhToan',
    TrangThaiDon NVARCHAR(30) NOT NULL DEFAULT N'ChoThanhToan',
    ThoiGianHetHanThanhToan DATETIME2 NOT NULL,
    NgayDat DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_DonVe_TaiKhoan
        FOREIGN KEY (TaiKhoanID)
        REFERENCES TaiKhoan(TaiKhoanID),

    CONSTRAINT CK_DonVe_TongTienGoc
        CHECK (TongTienGoc >= 0),

    CONSTRAINT CK_DonVe_TongTienGiam
        CHECK (TongTienGiam >= 0 AND TongTienGiam <= TongTienGoc),

    CONSTRAINT CK_DonVe_TongTienThanhToan
        CHECK (
            TongTienThanhToan >= 0
            AND TongTienThanhToan = TongTienGoc - TongTienGiam
        ),

    CONSTRAINT CK_DonVe_TrangThaiThanhToan
        CHECK (TrangThaiThanhToan IN (
            N'ChuaThanhToan',
            N'DaThanhToan',
            N'HoanTienMotPhan',
            N'DaHoanTien'
        )),

    CONSTRAINT CK_DonVe_TrangThaiDon
        CHECK (TrangThaiDon IN (
            N'ChoThanhToan',
            N'ThanhCong',
            N'BiHetHan',
            N'DaHuy',
            N'YeuCauHoanTien',
            N'DaHoanTien'
        ))
);
GO

CREATE TABLE ChiTietDonVe (
    ChiTietDonVeID INT IDENTITY(1,1) PRIMARY KEY,
    DonVeID INT NOT NULL,
    LoaiVeID INT NOT NULL,
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    ThanhTien DECIMAL(18,2) NOT NULL,

    CONSTRAINT FK_ChiTietDonVe_DonVe
        FOREIGN KEY (DonVeID)
        REFERENCES DonVe(DonVeID),

    CONSTRAINT FK_ChiTietDonVe_LoaiVe
        FOREIGN KEY (LoaiVeID)
        REFERENCES LoaiVe(LoaiVeID),

    CONSTRAINT UQ_ChiTietDonVe_DonVe_LoaiVe
        UNIQUE (DonVeID, LoaiVeID),

    CONSTRAINT CK_ChiTietDonVe_SoLuong
        CHECK (SoLuong > 0),

    CONSTRAINT CK_ChiTietDonVe_DonGia
        CHECK (DonGia >= 0),

    CONSTRAINT CK_ChiTietDonVe_ThanhTien
        CHECK (ThanhTien >= 0 AND ThanhTien = SoLuong * DonGia)
);
GO

CREATE TABLE ThanhToan (
    ThanhToanID INT IDENTITY(1,1) PRIMARY KEY,
    DonVeID INT NOT NULL,
    PhuongThucThanhToan NVARCHAR(50) NOT NULL,
    MaGiaoDichCongThanhToan NVARCHAR(100) NOT NULL UNIQUE,
    MaGiaoDichNganHang NVARCHAR(100) NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    LoaiGiaoDich NVARCHAR(30) NOT NULL DEFAULT N'ThanhToan',
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'DangXuLy',
    NoiDungThanhToan NVARCHAR(255) NULL,
    NgayThanhToan DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_ThanhToan_DonVe
        FOREIGN KEY (DonVeID)
        REFERENCES DonVe(DonVeID),

    CONSTRAINT CK_ThanhToan_PhuongThuc
        CHECK (PhuongThucThanhToan IN (
            N'VNPAY',
            N'MoMo',
            N'ZaloPay',
            N'ChuyenKhoan'
        )),

    CONSTRAINT CK_ThanhToan_SoTien
        CHECK (SoTien > 0),

    CONSTRAINT CK_ThanhToan_LoaiGiaoDich
        CHECK (LoaiGiaoDich IN (
            N'ThanhToan',
            N'HoanTien'
        )),

    CONSTRAINT CK_ThanhToan_TrangThai
        CHECK (TrangThai IN (
            N'DangXuLy',
            N'ThanhCong',
            N'ThatBai',
            N'NghiVanLoi'
        ))
);
GO

CREATE TABLE Ve (
    VeID INT IDENTITY(1,1) PRIMARY KEY,
    DonVeID INT NOT NULL,
    LoaiVeID INT NOT NULL,
    MaVe NVARCHAR(100) NOT NULL UNIQUE,
    MaQR NVARCHAR(255) NOT NULL UNIQUE,
    HoTenNguoiSuDung NVARCHAR(100) NULL,
    EmailNguoiSuDung NVARCHAR(255) NULL,
    GiaThucTe DECIMAL(18,2) NOT NULL,
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'GiuCho',
    ThoiGianCheckIn DATETIME2 NULL,
    NguoiCheckInID INT NULL,
    NgayHuy DATETIME2 NULL,

    CONSTRAINT FK_Ve_DonVe
        FOREIGN KEY (DonVeID)
        REFERENCES DonVe(DonVeID),

    CONSTRAINT FK_Ve_LoaiVe
        FOREIGN KEY (LoaiVeID)
        REFERENCES LoaiVe(LoaiVeID),

    CONSTRAINT FK_Ve_NguoiCheckIn
        FOREIGN KEY (NguoiCheckInID)
        REFERENCES TaiKhoan(TaiKhoanID),

    CONSTRAINT CK_Ve_GiaThucTe
        CHECK (GiaThucTe >= 0),

    CONSTRAINT CK_Ve_TrangThai
        CHECK (TrangThai IN (
            N'GiuCho',
            N'ChuaSuDung',
            N'DaCheckIn',
            N'DaHuy',
            N'DaHoanTien'
        )),

    CONSTRAINT CK_Ve_CheckIn
        CHECK (
            (TrangThai = N'DaCheckIn'
             AND ThoiGianCheckIn IS NOT NULL
             AND NguoiCheckInID IS NOT NULL)
            OR
            (TrangThai <> N'DaCheckIn'
             AND ThoiGianCheckIn IS NULL
             AND NguoiCheckInID IS NULL)
        )
);
GO

CREATE TABLE XuatVe (
    XuatVeID INT IDENTITY(1,1) PRIMARY KEY,
    VeID INT NOT NULL,
    NguoiXuatID INT NULL,
    HinhThucXuat NVARCHAR(50) NOT NULL,
    MaSoPhoiVe NVARCHAR(100) NULL,
    FileVeURL NVARCHAR(500) NULL,
    LanXuat INT NOT NULL DEFAULT 1,
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'ThanhCong',
    ThoiGianXuat DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_XuatVe_Ve
        FOREIGN KEY (VeID)
        REFERENCES Ve(VeID),

    CONSTRAINT FK_XuatVe_NguoiXuat
        FOREIGN KEY (NguoiXuatID)
        REFERENCES TaiKhoan(TaiKhoanID),

    CONSTRAINT CK_XuatVe_HinhThucXuat
        CHECK (HinhThucXuat IN (
            N'ExportPDF',
            N'InVeGiayTaiQuay',
            N'GuiEmail',
            N'InQuaKiosk'
        )),

    CONSTRAINT CK_XuatVe_LanXuat
        CHECK (LanXuat > 0),

    CONSTRAINT CK_XuatVe_TrangThai
        CHECK (TrangThai IN (
            N'ThanhCong',
            N'ThatBai'
        ))
);
GO

CREATE INDEX IX_LoaiVe_SuKienID
    ON LoaiVe(SuKienID);
GO

CREATE INDEX IX_DonVe_TaiKhoanID
    ON DonVe(TaiKhoanID);
GO

CREATE INDEX IX_DonVe_TrangThai
    ON DonVe(TrangThaiDon, TrangThaiThanhToan);
GO

CREATE INDEX IX_ChiTietDonVe_LoaiVeID
    ON ChiTietDonVe(LoaiVeID);
GO

CREATE INDEX IX_ThanhToan_DonVeID
    ON ThanhToan(DonVeID);
GO

CREATE INDEX IX_ThanhToan_TrangThai
    ON ThanhToan(TrangThai, LoaiGiaoDich);
GO

CREATE INDEX IX_Ve_DonVeID
    ON Ve(DonVeID);
GO

CREATE INDEX IX_Ve_LoaiVeID
    ON Ve(LoaiVeID);
GO
CREATE INDEX IX_Ve_TrangThai
    ON Ve(TrangThai);
GO
CREATE INDEX IX_XuatVe_VeID
    ON XuatVe(VeID);
GO
