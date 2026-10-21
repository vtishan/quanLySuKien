namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class DbNghiepVuTests
{
    readonly DatabaseFixture _fixture;

    public DbNghiepVuTests(DatabaseFixture fixture) => _fixture = fixture;

    [SkippableFact]
    public void nghiep_vu_khong_ton_tai_nem_ArgumentException()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var loi = Assert.Throws<ArgumentException>(() => Db.taiKhoan("khongCo"));
        Assert.Contains("taiKhoan.khongCo", loi.Message);
    }

    [SkippableFact]
    public void nhom_khong_ton_tai_nem_ArgumentException()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.Throws<ArgumentException>(() => Db.suKien("khongCo"));
        Assert.Throws<ArgumentException>(() => Db.ve("khongCo"));
        Assert.Throws<ArgumentException>(() => Db.thanhToan("khongCo"));
    }

    [SkippableFact]
    public void ten_nghiep_vu_khong_phan_biet_hoa_thuong()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.Equal(4, Db.soDong(Db.taiKhoan("THONGKEVAITRO")));
    }

    [SkippableFact]
    public void thua_tham_so_nem_ArgumentException()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var loi = Assert.Throws<ArgumentException>(() => Db.taiKhoan("thongKeVaiTro", 1));
        Assert.Contains("Thua tham so", loi.Message);
    }

    [SkippableFact]
    public void tham_so_null_duoc_truyen_thanh_DBNull()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.True(Db.soDong(Db.taiKhoan("traCuu", null, null, null, 1, 100)) > 0);
    }

    [SkippableFact]
    public void SqlParameter_duoc_truyen_thang_vao_thu_tuc()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var thamSo = new Microsoft.Data.SqlClient.SqlParameter("@TaiKhoanID", _fixture.DuLieu.NguoiDungID);
        var bang = Db.taiKhoan("donVeTheoTaiKhoan", thamSo);
        Assert.True(Db.soDong(bang) >= 2);
    }

    [SkippableFact]
    public void taiKhoan_dangKy_tao_tai_khoan_vai_tro_nguoi_dung()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.taiKhoan("dangKy", "Le Van C", "le.van.c@test.local", "hash", "0901234567");
        Assert.Equal(1, Db.soDong(bang));
        Assert.Equal("NguoiDung", bang.Rows[0]["TenVaiTro"] as string);
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["TrangThai"]));
    }

    [SkippableFact]
    public void taiKhoan_dangKy_email_trung_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var loi = Record.Exception(() => Db.taiKhoan("dangKy", "Trung", "admin@test.local", "hash"));
        Assert.IsType<Microsoft.Data.SqlClient.SqlException>(loi);
    }

    [SkippableFact]
    public void taiKhoan_dangNhap_tra_ve_mat_khau_hash()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.taiKhoan("dangNhap", "ADMIN@TEST.LOCAL");
        Assert.Equal(1, Db.soDong(bang));
        Assert.Equal("hash", bang.Rows[0]["MatKhauHash"] as string);
    }

    [SkippableFact]
    public void taiKhoan_dangNhap_email_khong_ton_tai_tra_ve_rong()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.Equal(0, Db.soDong(Db.taiKhoan("dangNhap", "khongco@test.local")));
    }

    [SkippableFact]
    public void taiKhoan_capNhat_doi_ten_va_so_dien_thoai()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.taiKhoan("capNhat", _fixture.DuLieu.NguoiDungID, "Ten Moi", "nguoidung@test.local", "0912345678");
        Assert.Equal("Ten Moi", bang.Rows[0]["HoTen"] as string);
        Assert.Equal("0912345678", bang.Rows[0]["SoDienThoai"] as string);
    }

    [SkippableFact]
    public void taiKhoan_capNhat_email_trung_tai_khoan_khac_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var loi = Record.Exception(() => Db.taiKhoan("capNhat", _fixture.DuLieu.NguoiDungID, "Ten", "admin@test.local"));
        Assert.IsType<Microsoft.Data.SqlClient.SqlException>(loi);
    }

    [SkippableFact]
    public void taiKhoan_khoa_va_moKhoa_thay_doi_trang_thai()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var khoa = Db.taiKhoan("khoa", _fixture.DuLieu.AdminID, _fixture.DuLieu.NguoiDungID);
        Assert.Equal(0, Convert.ToInt32(khoa.Rows[0]["TrangThai"]));
        var mo = Db.taiKhoan("moKhoa", _fixture.DuLieu.AdminID, _fixture.DuLieu.NguoiDungID);
        Assert.Equal(1, Convert.ToInt32(mo.Rows[0]["TrangThai"]));
    }

    [SkippableFact]
    public void taiKhoan_khoa_boi_nguoi_khong_phai_quan_tri_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var loi = Record.Exception(() => Db.taiKhoan("khoa", _fixture.DuLieu.NguoiDungID, _fixture.DuLieu.ManagerID));
        Assert.IsType<Microsoft.Data.SqlClient.SqlException>(loi);
    }

    [SkippableFact]
    public void taiKhoan_doiVaiTro_tra_ve_ten_vai_tro_moi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var vaiTro = Db.thucThi("sp_ThongKeTaiKhoanTheoVaiTro");
        var banToChucID = Convert.ToInt32(vaiTro.Rows.Cast<System.Data.DataRow>().First(r => (string)r["TenVaiTro"] == "BanToChuc")["VaiTroID"]);
        var nguoiDungID = Convert.ToInt32(vaiTro.Rows.Cast<System.Data.DataRow>().First(r => (string)r["TenVaiTro"] == "NguoiDung")["VaiTroID"]);
        try
        {
            var bang = Db.taiKhoan("doiVaiTro", _fixture.DuLieu.AdminID, _fixture.DuLieu.NguoiDungID, banToChucID);
            Assert.Equal(banToChucID, Convert.ToInt32(bang.Rows[0]["VaiTroID"]));
            Assert.Equal("BanToChuc", bang.Rows[0]["TenVaiTro"] as string);
        }
        finally
        {
            Db.taiKhoan("doiVaiTro", _fixture.DuLieu.AdminID, _fixture.DuLieu.NguoiDungID, nguoiDungID);
        }
    }

    [SkippableFact]
    public void taiKhoan_traCuu_phan_trang_va_loc_theo_vai_tro()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var trang1 = Db.taiKhoan("traCuu", null, null, null, 1, 2);
        Assert.True(Db.soDong(trang1) is > 0 and <= 2);
        var admin = Db.taiKhoan("traCuu", "admin", null, null, 1, 20);
        Assert.Equal(1, Db.soDong(admin));
        Assert.Equal("admin@test.local", admin.Rows[0]["Email"] as string);
    }

    [SkippableFact]
    public void taiKhoan_traCuu_trang_khong_hop_le_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<Microsoft.Data.SqlClient.SqlException>(
            Record.Exception(() => Db.taiKhoan("traCuu", null, null, null, 0, 20)));
        Assert.IsType<Microsoft.Data.SqlClient.SqlException>(
            Record.Exception(() => Db.taiKhoan("traCuu", null, null, null, 1, 500)));
    }

    [SkippableFact]
    public void taiKhoan_thongKe_tra_ve_cot_mong_doi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.True(Db.soDong(Db.taiKhoan("thongKeTrangThai")) > 0);
        Assert.Equal(4, Db.soDong(Db.taiKhoan("thongKeVaiTro")));
    }

    [SkippableFact]
    public void taiKhoan_suKienVaDonVe_theo_tai_khoan()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.Equal(2, Db.soDong(Db.taiKhoan("suKienTheoTaiKhoan", _fixture.DuLieu.BanToChucID)));
        Assert.Equal(2, Db.soDong(Db.taiKhoan("donVeTheoTaiKhoan", _fixture.DuLieu.NguoiDungID)));
    }

    [SkippableFact]
    public void taiKhoan_suKien_theo_tai_khoan_khong_ton_tai_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<Microsoft.Data.SqlClient.SqlException>(
            Record.Exception(() => Db.taiKhoan("suKienTheoTaiKhoan", -1)));
    }
}
