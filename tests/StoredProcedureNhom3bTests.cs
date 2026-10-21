using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class StoredProcedureNhom3bTests
{
    readonly DatabaseFixture _fixture;

    public StoredProcedureNhom3bTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    static int TaoDat(SqlConnection cn, BoDuLieu duLieu, DateTime batDau) =>
        Sql.Nguyen(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, @A, @B, 1000, 500);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID),
            ("@A", batDau), ("@B", batDau.AddDays(1)));

    [SkippableFact]
    public void sp_XacNhanDatDiaDiem_va_chan_sai_quyen()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var id = TaoDat(cn, duLieu, DateTime.Now.AddDays(60));
        var bang = Sql.GoiThuTuc(cn, "sp_XacNhanDatDiaDiem",
            ("@NguoiThucHienID", duLieu.ManagerID), ("@DatDiaDiemID", id));
        Assert.Equal("DaXacNhan", bang.Rows[0]["TrangThai"] as string);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_XacNhanDatDiaDiem",
            ("@NguoiThucHienID", duLieu.NguoiDungID), ("@DatDiaDiemID", id)), 50350);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_XacNhanDatDiaDiem",
            ("@NguoiThucHienID", duLieu.ManagerID), ("@DatDiaDiemID", -1)), 50351);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_XacNhanDatDiaDiem",
            ("@NguoiThucHienID", duLieu.ManagerID), ("@DatDiaDiemID", id)), 50352);
    }

    [SkippableFact]
    public void sp_TuChoiDatDiaDiem_va_HuyDatDiaDiem()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var tuChoiID = TaoDat(cn, duLieu, DateTime.Now.AddDays(100));
        Assert.Equal("TuChoi", Sql.GoiThuTuc(cn, "sp_TuChoiDatDiaDiem",
            ("@NguoiThucHienID", duLieu.ManagerID), ("@DatDiaDiemID", tuChoiID)).Rows[0]["TrangThai"] as string);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TuChoiDatDiaDiem",
            ("@NguoiThucHienID", duLieu.ManagerID), ("@DatDiaDiemID", tuChoiID)), 50361);

        var huyID = TaoDat(cn, duLieu, DateTime.Now.AddDays(110));
        Assert.Equal("DaHuy", Sql.GoiThuTuc(cn, "sp_HuyDatDiaDiem",
            ("@TaiKhoanID", duLieu.BanToChucID), ("@DatDiaDiemID", huyID)).Rows[0]["TrangThai"] as string);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_HuyDatDiaDiem",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@DatDiaDiemID", TaoDat(cn, duLieu, DateTime.Now.AddDays(120)))), 50370);
    }

    [SkippableFact]
    public void sp_TraCuuDatDiaDiem_phan_quyen_xem()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        TaoDat(cn, duLieu, DateTime.Now.AddDays(130));
        Assert.Equal(0, Sql.GoiThuTuc(cn, "sp_TraCuuDatDiaDiem", ("@TaiKhoanID", duLieu.NguoiDungID)).Rows.Count);
        Assert.True(Sql.GoiThuTuc(cn, "sp_TraCuuDatDiaDiem", ("@TaiKhoanID", duLieu.ManagerID)).Rows.Count >= 1);
        Assert.True(Sql.GoiThuTuc(cn, "sp_TraCuuDatDiaDiem", ("@TaiKhoanID", duLieu.BanToChucID)).Rows.Count >= 1);
    }

    [SkippableFact]
    public void sp_ThongKeDatDiaDiem_tra_ve_du_lieu()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        TaoDat(cn, duLieu, DateTime.Now.AddDays(140));
        Assert.True(Sql.GoiThuTuc(cn, "sp_ThongKeDatDiaDiemTheoTrangThai").Rows.Count >= 1);
        Assert.True(Sql.GoiThuTuc(cn, "sp_ThongKeDatDiaDiemTheoDiaDiem").Rows.Count >= 1);
    }
}