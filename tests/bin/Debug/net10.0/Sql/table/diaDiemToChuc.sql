CREATE TABLE DiaDiem (
    DiaDiemID INT IDENTITY(1,1) PRIMARY KEY,
    TenDiaDiem NVARCHAR(200) NOT NULL,
    DiaChi NVARCHAR(500) NOT NULL,
    SucChua INT NOT NULL,
    GiaThue DECIMAL(18,2) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_DiaDiem_SucChua
        CHECK (SucChua > 0),

    CONSTRAINT CK_DiaDiem_GiaThue
        CHECK (GiaThue >= 0)
);
GO

CREATE TABLE DatDiaDiem (
    DatDiaDiemID INT IDENTITY(1,1) PRIMARY KEY,
    SuKienID INT NOT NULL,
    DiaDiemID INT NOT NULL,
    ThoiGianBatDau DATETIME2 NOT NULL,
    ThoiGianKetThuc DATETIME2 NOT NULL,
    TongTien DECIMAL(18,2) NOT NULL,
    TienCoc DECIMAL(18,2) NOT NULL,
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'ChoXacNhan',
    NgayDat DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_DatDiaDiem_SuKien
        FOREIGN KEY (SuKienID)
        REFERENCES SuKien(SuKienID),

    CONSTRAINT FK_DatDiaDiem_DiaDiem
        FOREIGN KEY (DiaDiemID)
        REFERENCES DiaDiem(DiaDiemID),

    CONSTRAINT CK_DatDiaDiem_ThoiGian
        CHECK (ThoiGianKetThuc > ThoiGianBatDau),

    CONSTRAINT CK_DatDiaDiem_TongTien
        CHECK (TongTien >= 0),

    CONSTRAINT CK_DatDiaDiem_TienCoc
        CHECK (TienCoc >= 0 AND TienCoc <= TongTien),

    CONSTRAINT CK_DatDiaDiem_TrangThai
        CHECK (TrangThai IN (
            N'ChoXacNhan',
            N'DaXacNhan',
            N'TuChoi',
            N'DaHuy',
            N'HoanThanh'
        ))
);
GO

CREATE INDEX IX_DatDiaDiem_DiaDiem_ThoiGian
    ON DatDiaDiem (
        DiaDiemID,
        ThoiGianBatDau,
        ThoiGianKetThuc
    );
GO

CREATE INDEX IX_DatDiaDiem_SuKienID
    ON DatDiaDiem(SuKienID);
GO
