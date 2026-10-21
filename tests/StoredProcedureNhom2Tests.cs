using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class StoredProcedureNhom2Tests
{
    readonly DatabaseFixture _fixture;

    public StoredProcedureNhom2Tests(DatabaseFixture fixture) => _fixture = fixture;

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
    public void sp_GuiYeuCauXacThucToChuc_tao_va_chan_gui_trung()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var cccd = Cccd();
        var bang = Sql.GoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.NguoiDungID, cccd));
        Assert.Equal("ChoDuyet", bang.Rows[0]["TrangThai"] as string);
        Assert.Equal(cccd, bang.Rows[0]["SoCCCD"] as string);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.NguoiDungID, cccd)), 50104);
    }

    [SkippableFact]
    public void sp_GuiYeuCauXacThucToChuc_chan_tai_khoan_khoa_va_da_xac_thuc()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var khoa = BoDuLieu.TaiKhoan(cn, "NguoiDung", "k-" + Guid.NewGuid().ToString("N") + "@test.local", 0);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(khoa, Cccd())), 50101);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.BanToChucID, Cccd())), 50103);
    }

    [SkippableFact]
    public void sp_GuiYeuCauXacThucToChuc_chan_thong_tin_rong()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@SoCCCD", " "), ("@NgaySinh", new DateTime(1990, 1, 1)),
            ("@GioiTinh", "Nam"), ("@DiaChiThuongTru", "HN"), ("@AnhCCCDMatTruoc", "/a"), ("@AnhCCCDMatSau", "/b")), 50102);
    }

    [SkippableFact]
    public void sp_CapNhatYeuCauXacThucToChuc_cap_nhat_khi_cho_duyet()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var cccd = Cccd();
        Sql.GoiThuTuc(cn, "sp_GuiYeuCauXacThucToChuc", YeuCau(duLieu.NguoiDungID, cccd));
        var yeuCauID = Sql.Nguyen(cn, "SELECT TOP 1 YeuCauID FROM YeuCauXacThucToChuc WHERE TaiKhoanID = @Id ORDER BY YeuCauID DESC",
            ("@Id", duLieu.NguoiDungID));

        var moi = Cccd();
        var bang = Sql.GoiThuTuc(cn, "sp_CapNhatYeuCauXacThucToChuc",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@YeuCauID", yeuCauID), ("@SoCCCD", moi),
            ("@NgaySinh", new DateTime(1991, 2, 2)), ("@GioiTinh", "Nu"),
            ("@DiaChiThuongTru", "DN"), ("@AnhCCCDMatTruoc", "/c.jpg"), ("@AnhCCCDMatSau", "/d.jpg"));
        Assert.Equal(moi, bang.Rows[0]["SoCCCD"] as string);
        Assert.Equal("/c.jpg", bang.Rows[0]["AnhCCCDMatTruoc"] as string);

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CapNhatYeuCauXacThucToChuc",
            ("@TaiKhoanID", duLieu.BanToChucID), ("@YeuCauID", yeuCauID), ("@SoCCCD", moi),
            ("@NgaySinh", new DateTime(1991, 2, 2)), ("@GioiTinh", "Nu"),
            ("@DiaChiThuongTru", "DN"), ("@AnhCCCDMatTruoc", "/c.jpg"), ("@AnhCCCDMatSau", "/d.jpg")), 50111);
    }

    [SkippableFact]
    public void sp_TraCuuYeuCauXacThucToChuc_tra_ve_lich_su()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.True(Sql.GoiThuTuc(cn, "sp_TraCuuYeuCauXacThucToChuc", ("@TaiKhoanID", duLieu.BanToChucID)).Rows.Count >= 1);
        Assert.Equal(0, Sql.GoiThuTuc(cn, "sp_TraCuuYeuCauXacThucToChuc", ("@TaiKhoanID", -1)).Rows.Count);
    }
}
