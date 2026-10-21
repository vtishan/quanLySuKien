using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class DbDonVeTests
{
    readonly DatabaseFixture _fixture;

    public DbDonVeTests(DatabaseFixture fixture) => _fixture = fixture;

    [SkippableFact]
    public void themLoaiVe_tao_loai_ve_moi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.donVe("themLoaiVe", _fixture.DuLieu.SuKienID, "VIP " + Guid.NewGuid().ToString("N")[..5],
            300000, 20, 2, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(5));
        Assert.Equal(0, Convert.ToInt32(bang.Rows[0]["SoLuongDaBan"]));
        Assert.Equal(2, Convert.ToInt32(bang.Rows[0]["SoLuongToiDaMoiDon"]));
    }

    [SkippableFact]
    public void themLoaiVe_su_kien_chua_duyet_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("themLoaiVe", _fixture.DuLieu.SuKienChoDuyetID,
            "VIP", 1000, 10, 1, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1))));
    }

    [SkippableFact]
    public void themLoaiVe_gioi_han_moi_don_vuot_so_luong_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("themLoaiVe", _fixture.DuLieu.SuKienID,
            "VIP", 1000, 5, 6, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1))));
    }

    [SkippableFact]
    public void themLoaiVe_thoi_gian_dong_ban_truoc_mo_ban_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("themLoaiVe", _fixture.DuLieu.SuKienID,
            "VIP", 1000, 5, 1, DateTime.Now.AddDays(2), DateTime.Now.AddDays(1))));
    }

    [SkippableFact]
    public void capNhatLoaiVe_doi_gia()
    {
        _fixture.BoQuaNeuKhongCoSql();
        try
        {
            var bang = Db.donVe("capNhatLoaiVe", _fixture.DuLieu.LoaiVeID, "Ve thuong", 150000, 100, 5,
                DateTime.Now.AddDays(-1), DateTime.Now.AddDays(9));
            Assert.Equal(150000m, Convert.ToDecimal(bang.Rows[0]["GiaVe"]));
        }
        finally
        {
            Db.donVe("capNhatLoaiVe", _fixture.DuLieu.LoaiVeID, "Ve thuong", 100000, 100, 5,
                DateTime.Now.AddDays(-1), DateTime.Now.AddDays(9));
        }
    }

    [SkippableFact]
    public void capNhatLoaiVe_so_luong_toi_da_nho_hon_da_ban_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("capNhatLoaiVe", _fixture.DuLieu.LoaiVeID,
            "Ve thuong", 100000, 1, 1, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(9))));
    }

    [SkippableFact]
    public void tao_tao_don_va_giu_cho_ve()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var truoc = Db.soNguyen(Db.thucThi("sp_ThongKeVeTheoTrangThai"), "SoLuongVe");
        var bang = Db.donVe("tao", _fixture.DuLieu.NguoiDungID, _fixture.DuLieu.LoaiVeID, 2, 0);
        Assert.Equal(200000m, Convert.ToDecimal(bang.Rows[0]["TongTienGoc"]));
        Assert.Equal(200000m, Convert.ToDecimal(bang.Rows[0]["TongTienThanhToan"]));
        Assert.Equal("ChuaThanhToan", bang.Rows[0]["TrangThaiThanhToan"] as string);
        Assert.Equal(truoc, Db.soNguyen(Db.thucThi("sp_ThongKeVeTheoTrangThai"), "SoLuongVe"));
    }

    [SkippableFact]
    public void tao_ap_dung_giam_gia()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.donVe("tao", _fixture.DuLieu.NguoiDungID, _fixture.DuLieu.LoaiVeID, 1, 20000);
        Assert.Equal(100000m, Convert.ToDecimal(bang.Rows[0]["TongTienGoc"]));
        Assert.Equal(20000m, Convert.ToDecimal(bang.Rows[0]["TongTienGiam"]));
        Assert.Equal(80000m, Convert.ToDecimal(bang.Rows[0]["TongTienThanhToan"]));
    }

    [SkippableFact]
    public void tao_giam_gia_vuot_tong_tien_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("tao", _fixture.DuLieu.NguoiDungID,
            _fixture.DuLieu.LoaiVeID, 1, 200000)));
    }

    [SkippableFact]
    public void tao_vuot_gioi_han_moi_don_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("tao", _fixture.DuLieu.NguoiDungID,
            _fixture.DuLieu.LoaiVeID, 6, 0)));
    }

    [SkippableFact]
    public void tao_so_luong_khong_duong_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("tao", _fixture.DuLieu.NguoiDungID,
            _fixture.DuLieu.LoaiVeID, 0, 0)));
    }

    [SkippableFact]
    public void tao_tai_khoan_bi_khoa_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("tao", _fixture.DuLieu.BanToChucKhoaID,
            _fixture.DuLieu.LoaiVeID, 1, 0)));
    }

    [SkippableFact]
    public void huy_giai_phong_ve_dang_giu()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var don = Db.donVe("tao", _fixture.DuLieu.NguoiDungID, _fixture.DuLieu.LoaiVeID, 2, 0);
        var id = Convert.ToInt32(don.Rows[0]["DonVeID"]);
        var giuTruoc = Sql.Nguyen(_fixture.MoKetNoi(),
            "SELECT SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", _fixture.DuLieu.LoaiVeID));
        var bang = Db.donVe("huy", id, _fixture.DuLieu.NguoiDungID);
        Assert.Equal("DaHuy", bang.Rows[0]["TrangThaiDon"] as string);
        using var cn = _fixture.MoKetNoi();
        var giuSau = Sql.Nguyen(cn, "SELECT SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", _fixture.DuLieu.LoaiVeID));
        Assert.Equal(giuTruoc - 2, giuSau);
    }

    [SkippableFact]
    public void huy_don_cua_nguoi_khac_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var don = Db.donVe("tao", _fixture.DuLieu.NguoiDungID, _fixture.DuLieu.LoaiVeID, 1, 0);
        var id = Convert.ToInt32(don.Rows[0]["DonVeID"]);
        Assert.IsType<SqlException>(Record.Exception(() => Db.donVe("huy", id, _fixture.DuLieu.BanToChucID)));
    }

    [SkippableFact]
    public void giaoDich_thong_ke_theo_don()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.donVe("giaoDich", _fixture.DuLieu.DonVeDaThanhToanID);
        Assert.True(Db.soDong(bang) >= 1);
        Assert.True(Db.soDong(Db.donVe("giaoDich", DBNull.Value)) >= Db.soDong(bang));
    }
}
