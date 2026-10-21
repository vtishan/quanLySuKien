using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class DbDiaDiemTests
{
    readonly DatabaseFixture _fixture;

    public DbDiaDiemTests(DatabaseFixture fixture) => _fixture = fixture;

    [SkippableFact]
    public void them_tao_dia_diem_hoat_dong()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.diaDiem("them", _fixture.DuLieu.AdminID, "San van dong", "Quan 7", 1000, 5000000, "mo ta");
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["TrangThai"]));
        Assert.Equal(1000, Convert.ToInt32(bang.Rows[0]["SucChua"]));
    }

    [SkippableFact]
    public void them_boi_nguoi_khong_phai_quan_tri_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() =>
            Db.diaDiem("them", _fixture.DuLieu.ManagerID, "X", "Y", 10, 0)));
    }

    [SkippableFact]
    public void them_suc_chua_khong_duong_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() =>
            Db.diaDiem("them", _fixture.DuLieu.AdminID, "X", "Y", 0, 0)));
    }

    [SkippableFact]
    public void capNhat_doi_suc_chua_va_gia_thue()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var bang = Db.diaDiem("capNhat", _fixture.DuLieu.AdminID, _fixture.DuLieu.DiaDiemID,
            "Trung tam hoi nghi", "Quan 1", 600, 1200000, null);
        Assert.Equal(600, Convert.ToInt32(bang.Rows[0]["SucChua"]));
        Assert.Equal(1200000m, Convert.ToDecimal(bang.Rows[0]["GiaThue"]));
        Db.diaDiem("capNhat", _fixture.DuLieu.AdminID, _fixture.DuLieu.DiaDiemID,
            "Trung tam hoi nghi", "Quan 1", 500, 1000000, null);
    }

    [SkippableFact]
    public void capNhat_dia_diem_khong_ton_tai_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() =>
            Db.diaDiem("capNhat", _fixture.DuLieu.AdminID, -1, "X", "Y", 10, 0, null)));
    }

    [SkippableFact]
    public void doiTrangThai_khoa_va_mo_lai()
    {
        _fixture.BoQuaNeuKhongCoSql();
        try
        {
            var khoa = Db.diaDiem("doiTrangThai", _fixture.DuLieu.AdminID, _fixture.DuLieu.DiaDiemID, false);
            Assert.Equal(0, Convert.ToInt32(khoa.Rows[0]["TrangThai"]));
        }
        finally
        {
            var mo = Db.diaDiem("doiTrangThai", _fixture.DuLieu.AdminID, _fixture.DuLieu.DiaDiemID, true);
            Assert.Equal(1, Convert.ToInt32(mo.Rows[0]["TrangThai"]));
        }
    }

    [SkippableFact]
    public void traCuu_loc_theo_suc_chua_va_gia()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.True(Db.soDong(Db.diaDiem("traCuu", "Trung tam", 400, null, null, 1, 20)) >= 1);
        Assert.Equal(0, Db.soDong(Db.diaDiem("traCuu", null, 100000, null, null, 1, 20)));
        Assert.Equal(0, Db.soDong(Db.diaDiem("traCuu", null, null, 1000, null, 1, 20)));
    }

    [SkippableFact]
    public void dat_tao_lich_cho_xac_nhan_va_tinh_tien()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var batDau = DateTime.Now.AddDays(40);
        var bang = Db.diaDiem("dat", _fixture.DuLieu.BanToChucID, _fixture.DuLieu.SuKienID,
            _fixture.DuLieu.DiaDiemID, batDau, batDau.AddDays(1));
        Assert.Equal("ChoXacNhan", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal(1000000m, Convert.ToDecimal(bang.Rows[0]["TongTien"]));
        Assert.Equal(500000m, Convert.ToDecimal(bang.Rows[0]["TienCoc"]));
    }

    [SkippableFact]
    public void dat_trung_lich_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var batDau = DateTime.Now.AddDays(50);
        Db.diaDiem("dat", _fixture.DuLieu.BanToChucID, _fixture.DuLieu.SuKienID,
            _fixture.DuLieu.DiaDiemID, batDau, batDau.AddDays(1));
        Assert.IsType<SqlException>(Record.Exception(() => Db.diaDiem("dat", _fixture.DuLieu.BanToChucID,
            _fixture.DuLieu.SuKienID, _fixture.DuLieu.DiaDiemID, batDau.AddHours(1), batDau.AddDays(1))));
    }

    [SkippableFact]
    public void dat_su_kien_chua_duyet_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var batDau = DateTime.Now.AddDays(60);
        Assert.IsType<SqlException>(Record.Exception(() => Db.diaDiem("dat", _fixture.DuLieu.BanToChucID,
            _fixture.DuLieu.SuKienChoDuyetID, _fixture.DuLieu.DiaDiemID, batDau, batDau.AddDays(1))));
    }

    [SkippableFact]
    public void dat_thoi_gian_khong_hop_le_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var batDau = DateTime.Now.AddDays(70);
        Assert.IsType<SqlException>(Record.Exception(() => Db.diaDiem("dat", _fixture.DuLieu.BanToChucID,
            _fixture.DuLieu.SuKienID, _fixture.DuLieu.DiaDiemID, batDau, batDau)));
    }

    [SkippableFact]
    public void xacNhanDat_chuyen_sang_da_xac_nhan()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var batDau = DateTime.Now.AddDays(80);
        var dat = Db.diaDiem("dat", _fixture.DuLieu.BanToChucID, _fixture.DuLieu.SuKienID,
            _fixture.DuLieu.DiaDiemID, batDau, batDau.AddDays(1));
        var id = Convert.ToInt32(dat.Rows[0]["DatDiaDiemID"]);
        var bang = Db.diaDiem("xacNhanDat", _fixture.DuLieu.ManagerID, id);
        Assert.Equal("DaXacNhan", bang.Rows[0]["TrangThai"] as string);
    }

    [SkippableFact]
    public void xacNhanDat_khong_co_quyen_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.IsType<SqlException>(Record.Exception(() => Db.diaDiem("xacNhanDat", _fixture.DuLieu.NguoiDungID, 1)));
    }

    [SkippableFact]
    public void tuChoiDat_chuyen_sang_tu_choi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var batDau = DateTime.Now.AddDays(90);
        var dat = Db.diaDiem("dat", _fixture.DuLieu.BanToChucID, _fixture.DuLieu.SuKienID,
            _fixture.DuLieu.DiaDiemID, batDau, batDau.AddDays(1));
        var id = Convert.ToInt32(dat.Rows[0]["DatDiaDiemID"]);
        var bang = Db.diaDiem("tuChoiDat", _fixture.DuLieu.ManagerID, id);
        Assert.Equal("TuChoi", bang.Rows[0]["TrangThai"] as string);
    }

    [SkippableFact]
    public void huyDat_chuyen_sang_da_huy()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var batDau = DateTime.Now.AddDays(100);
        var dat = Db.diaDiem("dat", _fixture.DuLieu.BanToChucID, _fixture.DuLieu.SuKienID,
            _fixture.DuLieu.DiaDiemID, batDau, batDau.AddDays(1));
        var id = Convert.ToInt32(dat.Rows[0]["DatDiaDiemID"]);
        var bang = Db.diaDiem("huyDat", _fixture.DuLieu.BanToChucID, id);
        Assert.Equal("DaHuy", bang.Rows[0]["TrangThai"] as string);
    }

    [SkippableFact]
    public void huyDat_khong_phai_chu_su_kien_nem_loi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var batDau = DateTime.Now.AddDays(110);
        var dat = Db.diaDiem("dat", _fixture.DuLieu.BanToChucID, _fixture.DuLieu.SuKienID,
            _fixture.DuLieu.DiaDiemID, batDau, batDau.AddDays(1));
        var id = Convert.ToInt32(dat.Rows[0]["DatDiaDiemID"]);
        Assert.IsType<SqlException>(Record.Exception(() => Db.diaDiem("huyDat", _fixture.DuLieu.NguoiDungID, id)));
    }

    [SkippableFact]
    public void traCuuDat_chu_su_kien_chi_thay_cua_minh()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.Equal(0, Db.soDong(Db.diaDiem("traCuuDat", _fixture.DuLieu.NguoiDungID, null, null)));
        Assert.True(Db.soDong(Db.diaDiem("traCuuDat", _fixture.DuLieu.ManagerID, null, null)) > 0);
    }

    [SkippableFact]
    public void thongKe_tra_ve_du_lieu()
    {
        _fixture.BoQuaNeuKhongCoSql();
        Assert.True(Db.soDong(Db.diaDiem("thongKeDiaDiem")) > 0);
        Assert.True(Db.soDong(Db.diaDiem("thongKeTrangThai")) > 0);
    }
}
