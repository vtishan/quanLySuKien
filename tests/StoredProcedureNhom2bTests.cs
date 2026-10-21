using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class StoredProcedureNhom2bTests
{
    readonly DatabaseFixture _fixture;

    public StoredProcedureNhom2bTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    static string Cccd() => "CCCD" + Guid.NewGuid().ToString("N")[..8];

    static (string, object?)[] YeuCau(int taiKhoan, string cccd) =>
    [
        ("@TaiKhoanID", taiKhoan), ("@SoCCCD", cccd), ("@NgaySinh", new DateTime(1990, 1, 1)),
        ("@GioiTinh", "Nam"), ("@DiaChiThuongTru", "HN"),
        ("@AnhCCCDMatTruoc", "/a.jpg"), ("@AnhCCCDMatSau", "/b.jpg")
    ];

    [SkippableFact]
    public void sp_DuyetYeuCauXacThucToChuc_tao_ho_so_ban_to_chuc()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.GoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.NguoiDungID, Cccd()));
        var yeuCauID = Sql.Nguyen(cn, "SELECT TOP 1 YeuCauID FROM YeuCauXacThucToChuc WHERE TaiKhoanID = @Id ORDER BY YeuCauID DESC",
            ("@Id", duLieu.NguoiDungID));

        var bang = Sql.GoiThuTuc(cn, "sp_DuyetYeuCauXacThucToChuc",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@YeuCauID", yeuCauID), ("@TenToChuc", "To chuc moi"));
        Assert.Equal("To chuc moi", bang.Rows[0]["TenToChuc"] as string);
        Assert.Equal(1, Convert.ToInt32(bang.Rows[0]["TrangThai"]));
        Assert.Equal("DaDuyet", Sql.Mot(cn, "SELECT TrangThai FROM YeuCauXacThucToChuc WHERE YeuCauID = @Id", ("@Id", yeuCauID)) as string);
    }

    [SkippableFact]
    public void sp_DuyetYeuCauXacThucToChuc_chan_sai_quyen_va_trang_thai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.GoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.NguoiDungID, Cccd()));
        var yeuCauID = Sql.Nguyen(cn, "SELECT TOP 1 YeuCauID FROM YeuCauXacThucToChuc WHERE TaiKhoanID = @Id ORDER BY YeuCauID DESC",
            ("@Id", duLieu.NguoiDungID));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DuyetYeuCauXacThucToChuc",
            ("@NguoiDuyetID", duLieu.NguoiDungID), ("@YeuCauID", yeuCauID), ("@TenToChuc", "T")), 50121);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DuyetYeuCauXacThucToChuc",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@YeuCauID", yeuCauID), ("@TenToChuc", "  ")), 50120);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DuyetYeuCauXacThucToChuc",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@YeuCauID", -1), ("@TenToChuc", "T")), 50122);
    }

    [SkippableFact]
    public void sp_TuChoiYeuCauXacThucToChuc_ghi_ly_do()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.GoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.NguoiDungID, Cccd()));
        var yeuCauID = Sql.Nguyen(cn, "SELECT TOP 1 YeuCauID FROM YeuCauXacThucToChuc WHERE TaiKhoanID = @Id ORDER BY YeuCauID DESC",
            ("@Id", duLieu.NguoiDungID));

        var bang = Sql.GoiThuTuc(cn, "sp_TuChoiYeuCauXacThucToChuc",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@YeuCauID", yeuCauID), ("@LyDoTuChoi", "Anh mo"));
        Assert.Equal("TuChoi", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal("Anh mo", bang.Rows[0]["LyDoTuChoi"] as string);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TuChoiYeuCauXacThucToChuc",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@YeuCauID", yeuCauID), ("@LyDoTuChoi", "x")), 50132);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TuChoiYeuCauXacThucToChuc",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@YeuCauID", yeuCauID), ("@LyDoTuChoi", "  ")), 50130);
    }

    [SkippableFact]
    public void sp_GuiLaiSauTuChoi_tai_su_dung_yeu_cau_cu()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.GoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.NguoiDungID, Cccd()));
        var yeuCauID = Sql.Nguyen(cn, "SELECT TOP 1 YeuCauID FROM YeuCauXacThucToChuc WHERE TaiKhoanID = @Id", ("@Id", duLieu.NguoiDungID));
        Sql.GoiThuTuc(cn, "sp_TuChoiYeuCauXacThucToChuc",
            ("@NguoiDuyetID", duLieu.ManagerID), ("@YeuCauID", yeuCauID), ("@LyDoTuChoi", "Thieu anh"));

        var bang = Sql.GoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.NguoiDungID, Cccd()));
        Assert.Equal(yeuCauID, Convert.ToInt32(bang.Rows[0]["YeuCauID"]));
        Assert.Equal("ChoDuyet", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal(1, Sql.Nguyen(cn, "SELECT COUNT(*) FROM YeuCauXacThucToChuc WHERE TaiKhoanID = @Id", ("@Id", duLieu.NguoiDungID)));
    }

    [SkippableFact]
    public void sp_ThongKeYeuCauXacThucToChuc_tra_ve_du_lieu()
    {
        using var cn = Mo();
        BoDuLieu.Tao(cn);
        Assert.True(Sql.GoiThuTuc(cn, "sp_ThongKeYeuCauXacThucToChuc").Rows.Count >= 1);
    }
}