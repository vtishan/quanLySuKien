using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class DbVeVaThanhToanTests
{
    readonly DatabaseFixture _fixture;

    public DbVeVaThanhToanTests(DatabaseFixture fixture) => _fixture = fixture;

    int TaoDonChoThanhToan(int soLuong = 2)
    {
        var bang = Db.donVe("tao", _fixture.DuLieu.NguoiDungID, _fixture.DuLieu.LoaiVeID, soLuong, 0);
        return Convert.ToInt32(bang.Rows[0]["DonVeID"]);
    }

    [SkippableFact]
    public void phatHanhVe_sinh_du_so_ve_va_ma_duy_nhat()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(3);
        Db.thanhToan("ghiNhan", donID, 300000m, "GD-PH-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong");

        var bang = Db.phatHanhVe(donID);
        Assert.Equal(3, Db.soDong(bang));
        Assert.Equal(3, bang.Rows.Cast<System.Data.DataRow>().Select(r => (string)r["MaVe"]).Distinct().Count());
        Assert.Equal(3, bang.Rows.Cast<System.Data.DataRow>().Select(r => (string)r["MaQR"]).Distinct().Count());
        Assert.All(bang.Rows.Cast<System.Data.DataRow>(), r => Assert.Equal("ChuaSuDung", r["TrangThai"] as string));
    }

    [SkippableFact]
    public void phatHanhVe_goi_lai_khong_sinh_them_ve()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(2);
        Db.thanhToan("ghiNhan", donID, 200000m, "GD-PH2-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong");
        var lan1 = Db.phatHanhVe(donID);
        var loi = Assert.Throws<SqlException>(() => Db.phatHanhVe(donID));
        Assert.Equal(50501, loi.Number);
        using var cn = _fixture.MoKetNoi();
        Assert.Equal(Db.soDong(lan1), Sql.Nguyen(cn, "SELECT COUNT(*) FROM Ve WHERE DonVeID = @Id", ("@Id", donID)));
    }

    [SkippableFact]
    public void phatHanhVe_phat_hanh_bu_phan_con_thieu()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(3);
        Db.thanhToan("ghiNhan", donID, 300000m, "GD-PHB-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong");
        using (var cn = _fixture.MoKetNoi())
        {
            Sql.ThucThi(cn, """
                INSERT INTO Ve (DonVeID, LoaiVeID, MaVe, MaQR, GiaThucTe, TrangThai)
                VALUES (@Don, @LoaiVe, N'VE-BU-1', N'QR-BU-1', 100000, N'ChuaSuDung');
                """, ("@Don", donID), ("@LoaiVe", _fixture.DuLieu.LoaiVeID));
        }

        var bang = Db.phatHanhVe(donID);
        Assert.Equal(2, Db.soDong(bang));
        using var cn2 = _fixture.MoKetNoi();
        Assert.Equal(3, Sql.Nguyen(cn2, "SELECT COUNT(*) FROM Ve WHERE DonVeID = @Id", ("@Id", donID)));
    }

    [SkippableFact]
    public void phatHanhVe_don_chua_thanh_toan_nem_loi_va_khong_ghi_du_lieu()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(1);
        var loi = Assert.Throws<SqlException>(() => Db.phatHanhVe(donID));
        Assert.Equal(50501, loi.Number);
        using var cn = _fixture.MoKetNoi();
        Assert.Equal(0, Sql.Nguyen(cn, "SELECT COUNT(*) FROM Ve WHERE DonVeID = @Id", ("@Id", donID)));
    }

    [SkippableFact]
    public void phatHanhVe_don_khong_ton_tai_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.Throws<SqlException>(() => Db.phatHanhVe(-999));
    }

    [SkippableFact]
    public void checkIn_ghi_nhan_nguoi_va_thoi_gian()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(1);
        Db.thanhToan("ghiNhan", donID, 100000m, "GD-CK-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong");
        var ve = Convert.ToInt32(Db.phatHanhVe(donID).Rows[0]["VeID"]);

        var bang = Db.ve("checkIn", ve, _fixture.DuLieu.ManagerID);
        Assert.Equal("DaCheckIn", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal(_fixture.DuLieu.ManagerID, Convert.ToInt32(bang.Rows[0]["NguoiCheckInID"]));
        Assert.IsType<DateTime>(bang.Rows[0]["ThoiGianCheckIn"]);
    }

    [SkippableFact]
    public void checkIn_lan_hai_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(1);
        Db.thanhToan("ghiNhan", donID, 100000m, "GD-CK2-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong");
        var ve = Convert.ToInt32(Db.phatHanhVe(donID).Rows[0]["VeID"]);
        Db.ve("checkIn", ve, _fixture.DuLieu.ManagerID);
        Assert.IsType<SqlException>(Record.Exception(() => Db.ve("checkIn", ve, _fixture.DuLieu.ManagerID)));
    }

    [SkippableFact]
    public void checkIn_nguoi_dung_thuong_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.ve("checkIn", _fixture.DuLieu.VeID, _fixture.DuLieu.NguoiDungID)));
    }

    [SkippableFact]
    public void theoDon_tra_ve_danh_sach_ve()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.True(Db.soDong(Db.ve("theoDon", _fixture.DuLieu.DonVeDaThanhToanID)) >= 1);
        Assert.Equal(0, Db.soDong(Db.ve("theoDon", -1)));
    }

    [SkippableFact]
    public void lichSuXuat_ghi_nhan_lan_xuat()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.ve("lichSuXuat", _fixture.DuLieu.VeID, "ExportPDF", _fixture.DuLieu.ManagerID);
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["LanXuat"]));
        Assert.Equal("ExportPDF", bang.Rows[0]["HinhThucXuat"] as string);
    }

    [SkippableFact]
    public void lichSuXuat_hinh_thuc_sai_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.ve("lichSuXuat", _fixture.DuLieu.VeID, "SaiHinhThuc", null)));
    }

    [SkippableFact]
    public void thongKeVe_tra_ve_cot_mong_doi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.True(Db.soDong(Db.ve("thongKeTrangThai")) > 0);
        Assert.True(Db.soDong(Db.ve("thongKeCheckIn", _fixture.DuLieu.SuKienID)) > 0);
    }

    [SkippableFact]
    public void thanhToan_thanh_cong_cap_nhat_don_va_so_luong()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(2);
        using var cn = _fixture.MoKetNoi();
        var daBanTruoc = Sql.Nguyen(cn, "SELECT SoLuongDaBan FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", _fixture.DuLieu.LoaiVeID));
        var giuTruoc = Sql.Nguyen(cn, "SELECT SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", _fixture.DuLieu.LoaiVeID));

        Db.thanhToan("ghiNhan", donID, 200000m, "GD-TT-" + Guid.NewGuid().ToString("N")[..8], "MoMo", "ThanhCong");

        Assert.Equal("ThanhCong", Sql.Mot(cn, "SELECT TrangThaiDon FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);
        Assert.Equal("DaThanhToan", Sql.Mot(cn, "SELECT TrangThaiThanhToan FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);
        Assert.Equal(daBanTruoc + 2, Sql.Nguyen(cn, "SELECT SoLuongDaBan FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", _fixture.DuLieu.LoaiVeID)));
        Assert.Equal(giuTruoc - 2, Sql.Nguyen(cn, "SELECT SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", _fixture.DuLieu.LoaiVeID)));
    }

    [SkippableFact]
    public void thanhToan_sai_so_tien_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(1);
        Assert.IsType<SqlException>(Record.Exception(() => Db.thanhToan("ghiNhan", donID, 999m,
            "GD-SAI-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong")));
    }

    [SkippableFact]
    public void thanhToan_phuong_thuc_sai_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(1);
        Assert.IsType<SqlException>(Record.Exception(() => Db.thanhToan("ghiNhan", donID, 100000m,
            "GD-PT-" + Guid.NewGuid().ToString("N")[..8], "Bitcoin", "ThanhCong")));
    }

    [SkippableFact]
    public void thanhToan_don_da_thanh_toan_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.thanhToan("ghiNhan",
            _fixture.DuLieu.DonVeDaThanhToanID, 200000m, "GD-LAP-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong")));
    }

    [SkippableFact]
    public void thanhToan_that_bai_khong_doi_trang_thai_don()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(1);
        var maGiaoDich = "GD-TB-" + Guid.NewGuid().ToString("N")[..8];
        Db.thanhToan("ghiNhan", donID, 100000m, maGiaoDich, "ZaloPay", "ThatBai");
        using var cn = _fixture.MoKetNoi();
        Assert.Equal("ThatBai", Sql.Mot(cn, "SELECT TrangThai FROM ThanhToan WHERE MaGiaoDichCongThanhToan = @Ma", ("@Ma", maGiaoDich)) as string);
        Assert.Equal("ChuaThanhToan", Sql.Mot(cn, "SELECT TrangThaiThanhToan FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);
        Assert.Equal("ChoThanhToan", Sql.Mot(cn, "SELECT TrangThaiDon FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);
    }

    [SkippableFact]
    public void hoanTien_mot_phan_va_toan_phan()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(2);
        Db.thanhToan("ghiNhan", donID, 200000m, "GD-HT-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong");

        var maMotPhan = "GD-HT1-" + Guid.NewGuid().ToString("N")[..8];
        Db.thanhToan("hoanTien", donID, 50000m, maMotPhan, "ThanhCong");
        using var cn = _fixture.MoKetNoi();
        Assert.Equal("HoanTien", Sql.Mot(cn, "SELECT LoaiGiaoDich FROM ThanhToan WHERE MaGiaoDichCongThanhToan = @Ma", ("@Ma", maMotPhan)) as string);
        Assert.Equal("HoanTienMotPhan", Sql.Mot(cn, "SELECT TrangThaiThanhToan FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);

        Db.thanhToan("hoanTien", donID, 150000m, "GD-HT2-" + Guid.NewGuid().ToString("N")[..8], "ThanhCong");
        Assert.Equal("DaHoanTien", Sql.Mot(cn, "SELECT TrangThaiThanhToan FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);
        Assert.Equal("DaHoanTien", Sql.Mot(cn, "SELECT TrangThaiDon FROM DonVe WHERE DonVeID = @Id", ("@Id", donID)) as string);
    }

    [SkippableFact]
    public void hoanTien_vuot_so_tien_da_tra_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(1);
        Db.thanhToan("ghiNhan", donID, 100000m, "GD-HTV-" + Guid.NewGuid().ToString("N")[..8], "VNPAY", "ThanhCong");
        Assert.IsType<SqlException>(Record.Exception(() => Db.thanhToan("hoanTien", donID, 150000m,
            "GD-HTV2-" + Guid.NewGuid().ToString("N")[..8], "ThanhCong")));
    }

    [SkippableFact]
    public void hoanTien_don_chua_thanh_toan_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var donID = TaoDonChoThanhToan(1);
        Assert.IsType<SqlException>(Record.Exception(() => Db.thanhToan("hoanTien", donID, 10000m,
            "GD-HTC-" + Guid.NewGuid().ToString("N")[..8], "ThanhCong")));
    }

    [SkippableFact]
    public void soNguyen_va_dongDau_tra_ve_dung()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.taiKhoan("thongKeTrangThai");
        Assert.NotNull(Db.dongDau(bang));
        Assert.True(Db.soNguyen(bang, "SoLuong") > 0);
        Assert.Null(Db.dongDau(new System.Data.DataTable()));
        Assert.Equal(0, Db.soNguyen(new System.Data.DataTable(), "SoLuong"));
    }
}
