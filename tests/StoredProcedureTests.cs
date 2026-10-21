using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class StoredProcedureTests
{
    readonly DatabaseFixture _fixture;

    public StoredProcedureTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    [SkippableFact]
    public void sp_DangKyTaiKhoan_chuan_hoa_va_gan_vai_tro_nguoi_dung()
    {
        using var cn = Mo();
        var email = "dangky-" + Guid.NewGuid().ToString("N")[..8] + "@test.local";
        var bang = Sql.GoiThuTuc(cn, "sp_DangKyTaiKhoan",
            ("@HoTen", "  Nguyen Van A  "), ("@Email", "  " + email.ToUpperInvariant() + "  "),
            ("@MatKhauHash", "hash"), ("@SoDienThoai", DBNull.Value));
        Assert.Equal(1, bang.Rows.Count);
        Assert.Equal("Nguyen Van A", bang.Rows[0]["HoTen"] as string);
        Assert.Equal(email, bang.Rows[0]["Email"] as string);
        Assert.Equal("NguoiDung", bang.Rows[0]["TenVaiTro"] as string);
    }

    [SkippableFact]
    public void sp_DangKyTaiKhoan_thieu_du_lieu_nem_loi()
    {
        using var cn = Mo();
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DangKyTaiKhoan", ("@HoTen", " "), ("@Email", "a@b.c"), ("@MatKhauHash", "h")), 50001);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DangKyTaiKhoan", ("@HoTen", "A"), ("@Email", ""), ("@MatKhauHash", "h")), 50002);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DangKyTaiKhoan", ("@HoTen", "A"), ("@Email", "a@b.c"), ("@MatKhauHash", "")), 50003);
    }

    [SkippableFact]
    public void sp_DangKyTaiKhoan_email_trung_nem_loi_va_khong_tao_ban_ghi()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var email = (string)Sql.Mot(cn, "SELECT Email FROM TaiKhoan WHERE TaiKhoanID = @Id", ("@Id", duLieu.AdminID))!;
        var truoc = Sql.Nguyen(cn, "SELECT COUNT(*) FROM TaiKhoan");
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DangKyTaiKhoan",
            ("@HoTen", "X"), ("@Email", email), ("@MatKhauHash", "h")), 50004);
        Assert.Equal(truoc, Sql.Nguyen(cn, "SELECT COUNT(*) FROM TaiKhoan"));
    }

    [SkippableFact]
    public void sp_KiemTraDangNhap_tra_ve_ban_ghi_theo_email()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var email = (string)Sql.Mot(cn, "SELECT Email FROM TaiKhoan WHERE TaiKhoanID = @Id", ("@Id", duLieu.NguoiDungID))!;
        Assert.Equal(1, Sql.GoiThuTuc(cn, "sp_KiemTraDangNhap", ("@Email", email.ToUpperInvariant())).Rows.Count);
        Assert.Equal(0, Sql.GoiThuTuc(cn, "sp_KiemTraDangNhap", ("@Email", "khong-co@test.local")).Rows.Count);
    }

    [SkippableFact]
    public void sp_CapNhatThongTinTaiKhoan_cap_nhat_va_kiem_tra_trung()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var moi = "moi-" + Guid.NewGuid().ToString("N")[..8] + "@test.local";
        var bang = Sql.GoiThuTuc(cn, "sp_CapNhatThongTinTaiKhoan",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@HoTen", "Ten Moi"), ("@Email", moi), ("@SoDienThoai", "0900"));
        Assert.Equal("Ten Moi", bang.Rows[0]["HoTen"] as string);
        Assert.Equal(moi, bang.Rows[0]["Email"] as string);

        var adminEmail = (string)Sql.Mot(cn, "SELECT Email FROM TaiKhoan WHERE TaiKhoanID = @Id", ("@Id", duLieu.AdminID))!;
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CapNhatThongTinTaiKhoan",
            ("@TaiKhoanID", duLieu.NguoiDungID), ("@HoTen", "Ten"), ("@Email", adminEmail)), 50013);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_CapNhatThongTinTaiKhoan",
            ("@TaiKhoanID", -1), ("@HoTen", "Ten"), ("@Email", moi)), 50012);
    }

    [SkippableFact]
    public void sp_KhoaVaMoKhoaTaiKhoan_kiem_tra_quyen()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var khoa = Sql.GoiThuTuc(cn, "sp_KhoaTaiKhoan", ("@NguoiThucHienID", duLieu.AdminID), ("@TaiKhoanID", duLieu.NguoiDungID));
        Assert.Equal(0, Convert.ToInt32(khoa.Rows[0]["TrangThai"]));
        var mo = Sql.GoiThuTuc(cn, "sp_MoKhoaTaiKhoan", ("@NguoiThucHienID", duLieu.AdminID), ("@TaiKhoanID", duLieu.NguoiDungID));
        Assert.Equal(1, Convert.ToInt32(mo.Rows[0]["TrangThai"]));
    }

    [SkippableFact]
    public void sp_KhoaTaiKhoan_chan_tu_khoa_va_sai_quyen()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_KhoaTaiKhoan",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TaiKhoanID", duLieu.AdminID)), 50022);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_KhoaTaiKhoan",
            ("@NguoiThucHienID", duLieu.ManagerID), ("@TaiKhoanID", duLieu.NguoiDungID)), 50020);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_MoKhoaTaiKhoan",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TaiKhoanID", -1)), 50031);
    }

    [SkippableFact]
    public void sp_DoiVaiTroTaiKhoan_doi_duoc_va_chan_sai_quyen()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var vaiTro = Sql.Nguyen(cn, "SELECT VaiTroID FROM VaiTro WHERE TenVaiTro = N'QuanLySuKien'");
        var bang = Sql.GoiThuTuc(cn, "sp_DoiVaiTroTaiKhoan",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TaiKhoanID", duLieu.NguoiDungID), ("@VaiTroID", vaiTro));
        Assert.Equal(vaiTro, Convert.ToInt32(bang.Rows[0]["VaiTroID"]));

        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DoiVaiTroTaiKhoan",
            ("@NguoiThucHienID", duLieu.ManagerID), ("@TaiKhoanID", duLieu.NguoiDungID), ("@VaiTroID", vaiTro)), 50040);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DoiVaiTroTaiKhoan",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TaiKhoanID", duLieu.AdminID), ("@VaiTroID", vaiTro)), 50043);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_DoiVaiTroTaiKhoan",
            ("@NguoiThucHienID", duLieu.AdminID), ("@TaiKhoanID", duLieu.NguoiDungID), ("@VaiTroID", 999)), 50042);
    }

    [SkippableFact]
    public void sp_TraCuuTaiKhoan_loc_phan_trang_va_kiem_tra_tham_so()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var theoEmail = Sql.GoiThuTuc(cn, "sp_TraCuuTaiKhoan",
            ("@TuKhoa", (string)Sql.Mot(cn, "SELECT Email FROM TaiKhoan WHERE TaiKhoanID = @Id", ("@Id", duLieu.ManagerID))!));
        Assert.Equal(1, theoEmail.Rows.Count);
        Assert.True(Sql.GoiThuTuc(cn, "sp_TraCuuTaiKhoan", ("@Trang", 1), ("@SoDong", 2)).Rows.Count <= 2);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TraCuuTaiKhoan", ("@Trang", 0), ("@SoDong", 20)), 50050);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TraCuuTaiKhoan", ("@Trang", 1), ("@SoDong", 201)), 50050);
    }

    [SkippableFact]
    public void sp_ThongKeTaiKhoan_tra_ve_du_lieu()
    {
        using var cn = Mo();
        BoDuLieu.Tao(cn);
        Assert.True(Sql.GoiThuTuc(cn, "sp_ThongKeTaiKhoanTheoTrangThai").Rows.Count >= 1);
        Assert.Equal(4, Sql.GoiThuTuc(cn, "sp_ThongKeTaiKhoanTheoVaiTro").Rows.Count);
    }

    [SkippableFact]
    public void sp_TraCuuSuKienVaDonVeTheoTaiKhoan_kiem_tra_ton_tai()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        Assert.Equal(2, Sql.GoiThuTuc(cn, "sp_TraCuuSuKienTheoTaiKhoan", ("@TaiKhoanID", duLieu.BanToChucID)).Rows.Count);
        Assert.Equal(1, Sql.GoiThuTuc(cn, "sp_TraCuuDonVeTheoTaiKhoan", ("@TaiKhoanID", duLieu.NguoiDungID)).Rows.Count);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TraCuuSuKienTheoTaiKhoan", ("@TaiKhoanID", -1)), 50060);
        Sql.ChacLoi(Sql.LoiThuTuc(cn, "sp_TraCuuDonVeTheoTaiKhoan", ("@TaiKhoanID", -1)), 50070);
    }
}
