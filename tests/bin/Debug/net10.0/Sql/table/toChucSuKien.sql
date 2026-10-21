CREATE TABLE YeuCauXacThucToChuc (
    YeuCauID INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID INT NOT NULL,
    SoCCCD NVARCHAR(20) NOT NULL,
    NgaySinh DATE NOT NULL,
    GioiTinh NVARCHAR(10) NULL,
    DiaChiThuongTru NVARCHAR(300) NOT NULL,
    AnhCCCDMatTruoc NVARCHAR(500) NOT NULL,
    AnhCCCDMatSau NVARCHAR(500) NOT NULL,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'ChoDuyet',
    NguoiDuyetID INT NULL,
    NgayDuyet DATETIME2 NULL,
    LyDoTuChoi NVARCHAR(500) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_YeuCauXacThucToChuc_TaiKhoan
        FOREIGN KEY (TaiKhoanID)
        REFERENCES TaiKhoan(TaiKhoanID),

    CONSTRAINT FK_YeuCauXacThucToChuc_NguoiDuyet
        FOREIGN KEY (NguoiDuyetID)
        REFERENCES TaiKhoan(TaiKhoanID),

    CONSTRAINT CK_YeuCauXacThucToChuc_TrangThai
        CHECK (TrangThai IN (N'ChoDuyet', N'DaDuyet', N'TuChoi'))
);
GO

CREATE TABLE BanToChuc (
    BanToChucID INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID INT NOT NULL UNIQUE,
    SoCCCD NVARCHAR(20) NOT NULL,
    TenToChuc NVARCHAR(200) NULL,
    NgayXacThuc DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_BanToChuc_TaiKhoan
        FOREIGN KEY (TaiKhoanID)
        REFERENCES TaiKhoan(TaiKhoanID),

    CONSTRAINT UQ_BanToChuc_SoCCCD
        UNIQUE (SoCCCD)
);
GO

CREATE TABLE LoaiSuKien (
    LoaiSuKienID INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(500) NULL,
    TrangThai BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE SuKien (
    SuKienID INT IDENTITY(1,1) PRIMARY KEY,
    NguoiTaoID INT NOT NULL,
    LoaiSuKienID INT NOT NULL,
    TenSuKien NVARCHAR(200) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    AnhBia NVARCHAR(500) NULL,
    ThoiGianBatDau DATETIME2 NOT NULL,
    ThoiGianKetThuc DATETIME2 NOT NULL,
    SoLuongNguoiDuKien INT NOT NULL,
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'ChoDuyet',
    NguoiDuyetID INT NULL,
    NgayDuyet DATETIME2 NULL,
    LyDoTuChoi NVARCHAR(500) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_SuKien_NguoiTao
        FOREIGN KEY (NguoiTaoID)
        REFERENCES TaiKhoan(TaiKhoanID),

    CONSTRAINT FK_SuKien_LoaiSuKien
        FOREIGN KEY (LoaiSuKienID)
        REFERENCES LoaiSuKien(LoaiSuKienID),

    CONSTRAINT FK_SuKien_NguoiDuyet
        FOREIGN KEY (NguoiDuyetID)
        REFERENCES TaiKhoan(TaiKhoanID),

    CONSTRAINT CK_SuKien_SoLuongNguoiDuKien
        CHECK (SoLuongNguoiDuKien > 0),

    CONSTRAINT CK_SuKien_ThoiGian
        CHECK (ThoiGianKetThuc > ThoiGianBatDau),

    CONSTRAINT CK_SuKien_TrangThai
        CHECK (TrangThai IN (
            N'ChoDuyet',
            N'DaDuyet',
            N'TuChoi',
            N'DangDienRa',
            N'DaKetThuc',
            N'DaHuy'
        )),

    CONSTRAINT CK_SuKien_NguoiDuyet
        CHECK (NguoiDuyetID IS NULL OR NguoiDuyetID <> NguoiTaoID)
);
GO

CREATE INDEX IX_YeuCauXacThucToChuc_TaiKhoanID
    ON YeuCauXacThucToChuc(TaiKhoanID);

CREATE INDEX IX_YeuCauXacThucToChuc_TrangThai
    ON YeuCauXacThucToChuc(TrangThai);

CREATE INDEX IX_SuKien_NguoiTaoID
    ON SuKien(NguoiTaoID);

CREATE INDEX IX_SuKien_LoaiSuKienID
    ON SuKien(LoaiSuKienID);

CREATE INDEX IX_SuKien_TrangThai
    ON SuKien(TrangThai);
GO