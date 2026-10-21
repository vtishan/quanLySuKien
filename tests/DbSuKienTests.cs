using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class DbSuKienTests
{
    readonly DatabaseFixture _fixture;

    public DbSuKienTests(DatabaseFixture fixture) => _fixture = fixture;

    [SkippableFact]
    public void themLoai_tra_ve_loai_moi_hoat_dong()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var ten = "Loai " + Guid.NewGuid().ToString("N")[..6];
        var bang = Db.suKien("themLoai", ten, "mo ta");
        Assert.Equal(ten, bang.Rows[0]["TenLoai"] as string);
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["TrangThai"]));
    }

    [SkippableFact]
    public void themLoai_trung_ten_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("themLoai", "Hoi thao", null)));
    }

    [SkippableFact]
    public void themLoai_ten_rong_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("themLoai", "   ", null)));
    }

    [SkippableFact]
    public void capNhatLoai_doi_mo_ta()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.suKien("capNhatLoai", _fixture.DuLieu.LoaiSuKienID, "Hoi thao", "mo ta moi");
        Assert.Equal("mo ta moi", bang.Rows[0]["MoTa"] as string);
    }

    [SkippableFact]
    public void capNhatLoai_khong_ton_tai_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("capNhatLoai", -1, "Ten", null)));
    }

    [SkippableFact]
    public void doiTrangThaiLoai_khoa_va_mo_lai()
    {
        _fixture.BoQuaNeuKhongCoSql();
        try
        {
            Assert.Equal(0, Convert.ToInt32(Db.suKien("doiTrangThaiLoai", _fixture.DuLieu.LoaiSuKienKhoaID, true).Rows[0]["TrangThai"]));
            Assert.Equal(0, Convert.ToInt32(Db.suKien("doiTrangThaiLoai", _fixture.DuLieu.LoaiSuKienID, false).Rows[0]["TrangThai"]));
        }
        finally
        {
            Db.suKien("doiTrangThaiLoai", _fixture.DuLieu.LoaiSuKienID, true);
            Db.suKien("doiTrangThaiLoai", _fixture.DuLieu.LoaiSuKienKhoaID, false);
        }
    }

    [SkippableFact]
    public void traCuuLoai_loc_theo_tu_khoa_va_trang_thai()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.Equal(1, Db.soDong(Db.suKien("traCuuLoai", "Hoi thao", null)));
        Assert.Equal(1, Db.soDong(Db.suKien("traCuuLoai", "Hoi thao", false)));
        Assert.Equal(0, Db.soDong(Db.suKien("traCuuLoai", "Khong ton tai", null)));
    }

    [SkippableFact]
    public void tao_tao_su_kien_cho_duyet()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.suKien("tao", _fixture.DuLieu.BanToChucID, _fixture.DuLieu.LoaiSuKienID,
            "Su kien moi", "mo ta", null,
            DateTime.Now.AddDays(30), DateTime.Now.AddDays(31), 200);
        Assert.Equal("ChoDuyet", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal(200, Convert.ToInt32(bang.Rows[0]["SoLuongNguoiDuKien"]));
    }

    [SkippableFact]
    public void tao_boi_nguoi_dung_thuong_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("tao", _fixture.DuLieu.NguoiDungID,
            _fixture.DuLieu.LoaiSuKienID, "Su kien", null, null,
            DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), 10)));
    }

    [SkippableFact]
    public void tao_thoi_gian_ket_thuc_truoc_bat_dau_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("tao", _fixture.DuLieu.BanToChucID,
            _fixture.DuLieu.LoaiSuKienID, "Su kien", null, null,
            DateTime.Now.AddDays(5), DateTime.Now.AddDays(1), 10)));
    }

    [SkippableFact]
    public void tao_loai_su_kien_da_khoa_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("tao", _fixture.DuLieu.BanToChucID,
            _fixture.DuLieu.LoaiSuKienKhoaID, "Su kien", null, null,
            DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), 10)));
    }

    [SkippableFact]
    public void duyet_chuyen_trang_thai_va_ghi_nguoi_duyet()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.suKien("duyet", _fixture.DuLieu.ManagerID, _fixture.DuLieu.SuKienChoDuyetID);
        Assert.Equal("DaDuyet", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal(_fixture.DuLieu.ManagerID, Convert.ToInt32(bang.Rows[0]["NguoiDuyetID"]));
        Assert.IsType<DateTime>(bang.Rows[0]["NgayDuyet"]);
    }

    [SkippableFact]
    public void duyet_nguoi_tao_tu_duyet_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("duyet", _fixture.DuLieu.BanToChucID, _fixture.DuLieu.SuKienChoDuyetID)));
    }

    [SkippableFact]
    public void duyet_lai_su_kien_da_duyet_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("duyet", _fixture.DuLieu.ManagerID, _fixture.DuLieu.SuKienID)));
    }

    [SkippableFact]
    public void tuChoi_can_ly_do_va_ghi_nhan()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("tuChoi", _fixture.DuLieu.ManagerID, _fixture.DuLieu.SuKienChoDuyetID, "  ")));
    }

    [SkippableFact]
    public void traCuu_loc_theo_trang_thai_va_loai()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var daDuyet = Db.suKien("traCuu", null, null, "DaDuyet", 1, 100);
        Assert.True(Db.soDong(daDuyet) >= 1);
        Assert.All(daDuyet.Rows.Cast<System.Data.DataRow>(), r => Assert.Equal("DaDuyet", r["TrangThai"] as string));
        Assert.Equal(0, Db.soDong(Db.suKien("traCuu", "khong co su kien nay", null, null, 1, 20)));
    }

    [SkippableFact]
    public void traCuu_trang_khong_hop_le_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("traCuu", null, null, null, 1, 201)));
    }

    [SkippableFact]
    public void thongKe_tra_ve_theo_loai_va_trang_thai()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.True(Db.soDong(Db.suKien("thongKe")) > 0);
    }

    [SkippableFact]
    public void traCuuYeuCau_tra_ve_lich_su_cua_tai_khoan()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.Equal(1, Db.soDong(Db.suKien("traCuuYeuCau", _fixture.DuLieu.BanToChucID)));
        Assert.Equal(0, Db.soDong(Db.suKien("traCuuYeuCau", _fixture.DuLieu.NguoiDungID)));
    }

    [SkippableFact]
    public void thongKeYeuCau_tra_ve_cot_so_luong()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.suKien("thongKeYeuCau");
        Assert.True(Db.soNguyen(bang, "SoLuong") > 0);
    }

    [SkippableFact]
    public void guiYeuCau_tao_yeu_cau_cho_duyet()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.suKien("guiYeuCau", _fixture.DuLieu.NguoiDungID, "098765432109", new DateTime(1995, 5, 5),
            "Nu", "Da Nang", "/a.jpg", "/b.jpg");
        Assert.Equal("ChoDuyet", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal("098765432109", bang.Rows[0]["SoCCCD"] as string);
    }

    [SkippableFact]
    public void guiYeuCau_bo_qua_du_lieu_dinh_danh_rong_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("guiYeuCau", _fixture.DuLieu.NguoiDungID,
            "0123", new DateTime(1995, 5, 5), null, "  ", "/a.jpg", "/b.jpg")));
    }

    [SkippableFact]
    public void guiYeuCau_tai_khoan_da_xac_thuc_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("guiYeuCau", _fixture.DuLieu.BanToChucID,
            "0123", new DateTime(1995, 5, 5), null, "HN", "/a.jpg", "/b.jpg")));
    }

    [SkippableFact]
    public void capNhatYeuCau_doi_so_cccd()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var yeuCau = Db.suKien("guiYeuCau", _fixture.DuLieu.NguoiDungID, "098765432109",
            new DateTime(1995, 5, 5), null, "HN", "/a.jpg", "/b.jpg");
        var yeuCauID = Convert.ToInt32(yeuCau.Rows[0]["YeuCauID"]);
        var bang = Db.suKien("capNhatYeuCau", _fixture.DuLieu.NguoiDungID, yeuCauID, "098765432110",
            new DateTime(1995, 5, 5), null, "HN", "/c.jpg", "/d.jpg");
        Assert.Equal("098765432110", bang.Rows[0]["SoCCCD"] as string);
        Assert.Equal("/c.jpg", bang.Rows[0]["AnhCCCDMatTruoc"] as string);
    }

    [SkippableFact]
    public void capNhatYeuCau_khong_phai_chu_so_huu_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var yeuCau = Db.suKien("guiYeuCau", _fixture.DuLieu.NguoiDungID, "098765432111",
            new DateTime(1995, 5, 5), null, "HN", "/a.jpg", "/b.jpg");
        var yeuCauID = Convert.ToInt32(yeuCau.Rows[0]["YeuCauID"]);
        Assert.IsType<SqlException>(Record.Exception(() => Db.suKien("capNhatYeuCau",
            _fixture.DuLieu.BanToChucID, yeuCauID, "1", new DateTime(1995, 5, 5), null, "HN", "/a.jpg", "/b.jpg")));
    }
}
