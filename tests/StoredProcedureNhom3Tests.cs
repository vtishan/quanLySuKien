using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class StoredProcedureNhom3Tests
{
    readonly DatabaseFixture _fixture;

    public StoredProcedureNhom3Tests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    [SkippableFact]
    public void sp_ThemDiaDiem_tao_va_chan_sai_quyen()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.GoiThuTuc(cn, "sp_ThemDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TenDiaDiem", "San moi"),
            ("@DiaChi", "Q9"), ("@SucChua", 200), ("@GiaThue", 500000m), ("@MoTa", "mo ta"));
        Assert.Equal(200, Convert.ToInt32(bang.Rows[0]["SucChua"]));
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["TrangThai"]));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemDiaDiem",
            ("@NguoiThucHienID", duLieu.ManagerID), ("@TenDiaDiem", "X"),
            ("@DiaChi", "Y"), ("@SucChua", 10), ("@GiaThue", 0m)), 50301);
    }

    [SkippableFact]
    public void sp_ThemDiaDiem_chan_du_lieu_sai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TenDiaDiem", "  "),
            ("@DiaChi", "Y"), ("@SucChua", 10), ("@GiaThue", 0m)), 50302);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TenDiaDiem", "X"),
            ("@DiaChi", "Y"), ("@SucChua", 0), ("@GiaThue", 0m)), 50303);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_ThemDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TenDiaDiem", "X"),
            ("@DiaChi", "Y"), ("@SucChua", 10), ("@GiaThue", -1m)), 50304);
    }

    [SkippableFact]
    public void sp_CapNhatDiaDiem_cap_nhat_va_kiem_tra_ton_tai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var bang = Sql.GoiThuTuc(cn, "sp_CapNhatDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@DiaDiemID", duLieu.DiaDiemID),
            ("@TenDiaDiem", "Ten moi"), ("@DiaChi", "Q1"), ("@SucChua", 600), ("@GiaThue", 2000000m));
        Assert.Equal(600, Convert.ToInt32(bang.Rows[0]["SucChua"]));
        Assert.Equal(2000000m, Convert.ToDecimal(bang.Rows[0]["GiaThue"]));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CapNhatDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@DiaDiemID", -1),
            ("@TenDiaDiem", "X"), ("@DiaChi", "Y"), ("@SucChua", 1), ("@GiaThue", 0m)), 50314);
    }

    [SkippableFact]
    public void sp_DoiTrangThaiDiaDiem_chan_khi_con_lich_dat()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var tuongLai = DateTime.Now.AddDays(10);
        Sql.ThucThi(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 1000, 500);
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID),
            ("@A", tuongLai), ("@B", tuongLai.AddDays(1)));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DoiTrangThaiDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@DiaDiemID", duLieu.DiaDiemID), ("@TrangThai", false)), 50323);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DoiTrangThaiDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@DiaDiemID", -1), ("@TrangThai", false)), 50322);

        var khoa = Sql.Nguyen(cn, """
            INSERT INTO DiaDiem (TenDiaDiem, DiaChi, SucChua, GiaThue, TrangThai)
            VALUES (N'Khong lich', N'Q3', 10, 0, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """);
        Assert.Equal(0, Convert.ToInt32(Sql.GoiThuTuc(cn, "sp_DoiTrangThaiDiaDiem",
            ("@NguoiThucHienID", duLieu.AdminID), ("@DiaDiemID", khoa), ("@TrangThai", false)).Rows[0]["TrangThai"]));
    }

    [SkippableFact]
    public void sp_TraCuuDiaDiem_loc_phan_trang()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.True(Sql.GoiThuTuc(cn, "sp_TraCuuDiaDiem",
            ("@TrangThai", true), ("@Trang", 1), ("@SoDong", 50)).Rows.Count >= 1);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TraCuuDiaDiem", ("@Trang", -1), ("@SoDong", 20)), 50330);
    }

    [SkippableFact]
    public void sp_TaoDatDiaDiem_tao_va_chan_trung_lich()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var batDau = DateTime.Now.AddDays(40);
        var bang = Sql.GoiThuTuc(cn, "sp_TaoDatDiaDiem",
            ("@TaiKhoanID", duLieu.BanToChucID), ("@SuKienID", duLieu.SuKienID),
            ("@DiaDiemID", duLieu.DiaDiemID), ("@ThoiGianBatDau", batDau), ("@ThoiGianKetThuc", batDau.AddDays(1)));
        Assert.Equal("ChoXacNhan", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal(1000000m, Convert.ToDecimal(bang.Rows[0]["TongTien"]));
        Assert.Equal(500000m, Convert.ToDecimal(bang.Rows[0]["TienCoc"]));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDatDiaDiem",
            ("@TaiKhoanID", duLieu.BanToChucID), ("@SuKienID", duLieu.SuKienID),
            ("@DiaDiemID", duLieu.DiaDiemID), ("@ThoiGianBatDau", batDau.AddHours(2)), ("@ThoiGianKetThuc", batDau.AddDays(1))), 50344);
    }

    [SkippableFact]
    public void sp_TaoDatDiaDiem_chan_sai_quyen_va_thoi_gian()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var batDau = DateTime.Now.AddDays(50);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDatDiaDiem",
            ("@TaiKhoanID", duLieu.BanToChucID), ("@SuKienID", duLieu.SuKienID),
            ("@DiaDiemID", duLieu.DiaDiemID), ("@ThoiGianBatDau", batDau), ("@ThoiGianKetThuc", batDau)), 50340);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDatDiaDiem",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@SuKienID", duLieu.SuKienID),
            ("@DiaDiemID", duLieu.DiaDiemID), ("@ThoiGianBatDau", batDau), ("@ThoiGianKetThuc", batDau.AddDays(1))), 50341);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TaoDatDiaDiem",
            ("@TaiKhoanID", duLieu.BanToChucID), ("@SuKienID", duLieu.SuKienChoDuyetID),
            ("@DiaDiemID", duLieu.DiaDiemID), ("@ThoiGianBatDau", batDau), ("@ThoiGianKetThuc", batDau.AddDays(1))), 50342);
    }
}