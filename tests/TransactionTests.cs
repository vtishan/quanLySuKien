using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class TransactionTests
{
    readonly DatabaseFixture _fixture;

    public TransactionTests(DatabaseFixture fixture) => _fixture = fixture;

    SqlConnection Mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        return _fixture.MoKetNoi();
    }

    static string Ma(string tienTo) => tienTo + "-" + Guid.NewGuid().ToString("N")[..10];

    [SkippableFact]
    public void phatHanhVe_rollback_khi_don_chua_thanh_toan()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var loi = Assert.Throws<SqlException>(() => Db.phatHanhVe(duLieu.DonVeID));
        Assert.Equal(50501, loi.Number);
        Assert.Equal(0, Sql.Nguyen(cn, "SELECT COUNT(*) FROM Ve WHERE DonVeID = @Id", ("@Id", duLieu.DonVeID)));
    }

    [SkippableFact]
    public void phatHanhVe_khong_tao_ve_trung_ma_giua_cac_lan_goi()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var donID = DonDaThanhToan(cn, duLieu, 2);
        BoDuLieu.PhatHanhVe(cn, donID, duLieu.LoaiVeID, Ma("VE"));

        var truoc = Sql.Nguyen(cn, "SELECT COUNT(*) FROM Ve WHERE DonVeID = @Id", ("@Id", donID));
        var loi = Assert.Throws<SqlException>(() => Db.phatHanhVe(donID));
        Assert.Equal(50501, loi.Number);
        var sau = Sql.Nguyen(cn, "SELECT COUNT(*) FROM Ve WHERE DonVeID = @Id", ("@Id", donID));
        Assert.Equal(1, truoc);
        Assert.Equal(truoc, sau);
        Assert.Equal(sau, Sql.Nguyen(cn, "SELECT COUNT(DISTINCT MaVe) FROM Ve WHERE DonVeID = @Id", ("@Id", donID)));
    }

    [SkippableFact]
    public void giao_dich_tuong_minh_rollback_tra_lai_trang_thai_ban_dau()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var truoc = Sql.Nguyen(cn, "SELECT COUNT(*) FROM DonVe WHERE TaiKhoanID = @Id", ("@Id", duLieu.NguoiDungID));

        using (var giaoDich = cn.BeginTransaction())
        {
            Sql.ThucThi(cn, """
                INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienThanhToan, ThoiGianHetHanThanhToan)
                VALUES (@Tk, @Ma, 1000, 1000, DATEADD(MINUTE, 15, SYSDATETIME()))
                """, ("@Tk", duLieu.NguoiDungID), ("@Ma", Ma("DH")));
            giaoDich.Rollback();
        }

        Assert.Equal(truoc, Sql.Nguyen(cn, "SELECT COUNT(*) FROM DonVe WHERE TaiKhoanID = @Id", ("@Id", duLieu.NguoiDungID)));
    }

    [SkippableFact]
    public void giao_dich_tuong_minh_commit_luu_du_lieu()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var ma = Ma("DH");
        using (var giaoDich = cn.BeginTransaction())
        {
            Sql.ThucThi(cn, """
                INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienThanhToan, ThoiGianHetHanThanhToan)
                VALUES (@Tk, @Ma, 1000, 1000, DATEADD(MINUTE, 15, SYSDATETIME()))
                """, ("@Tk", duLieu.NguoiDungID), ("@Ma", ma));
            giaoDich.Commit();
        }

        Assert.Equal(1, Sql.Nguyen(cn, "SELECT COUNT(*) FROM DonVe WHERE MaDonHang = @Ma", ("@Ma", ma)));
    }

    [SkippableFact]
    public void transaction_khong_hoan_tat_thi_khong_giu_lock_vo_hinh()
    {
        using var cn1 = Mo();
        using var cn2 = Mo();
        var duLieu = BoDuLieu.Tao(cn1);

        using (var giaoDich = cn1.BeginTransaction())
        {
            Sql.ThucThi(cn1, """
                UPDATE DonVe SET TongTienGiam = 0 WHERE DonVeID = @Id
                """, ("@Id", duLieu.DonVeID));
            giaoDich.Rollback();
        }

        using var lenh = new SqlCommand("SELECT TongTienGiam FROM DonVe WHERE DonVeID = @Id", cn2)
        {
            CommandTimeout = 5
        };
        lenh.Parameters.AddWithValue("@Id", duLieu.DonVeID);
        Assert.Equal(0m, Convert.ToDecimal(lenh.ExecuteScalar()));
    }

    [SkippableFact]
    public void check_so_luong_ve_khong_vuot_gioi_han_qua_nhieu_lan_tao_don()
    {
        using var cn = Mo();
        var duLieu = BoDuLieu.Tao(cn);
        var loaiVeNho = Sql.Nguyen(cn, """
            INSERT INTO LoaiVe (SuKienID, TenLoai, GiaVe, SoLuongToiDa, SoLuongToiDaMoiDon,
                                ThoiGianMoBan, ThoiGianDongBan, TrangThai)
            VALUES (@Sk, N'Nho', 10000, 2, 2, DATEADD(DAY, -1, SYSDATETIME()), DATEADD(DAY, 5, SYSDATETIME()), 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Sk", duLieu.SuKienID));

        Db.donVe("tao", duLieu.NguoiDungID, loaiVeNho, 2);
        var loi = Record.Exception(() => Db.donVe("tao", duLieu.NguoiDungID, loaiVeNho, 1));
        Assert.IsType<SqlException>(loi);
        Assert.Equal(0, Sql.Nguyen(cn, "SELECT SoLuongToiDa - SoLuongDangGiu FROM LoaiVe WHERE LoaiVeID = @Id", ("@Id", loaiVeNho)));
    }

    [SkippableFact]
    public void connection_pool_khong_giu_transaction_mo()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using (var cn = Mo())
        {
            using var giaoDich = cn.BeginTransaction();
            Sql.ThucThi(cn, "SELECT 1");
        }

        using var cn2 = Mo();
        Assert.Equal("ChoThanhToan", Sql.Mot(cn2, """
            SELECT TOP 1 TrangThaiDon FROM DonVe ORDER BY DonVeID
            """) as string);
    }

    static int DonDaThanhToan(SqlConnection cn, BoDuLieu duLieu, int soLuong)
    {
        var donID = Sql.Nguyen(cn, """
            INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienThanhToan,
                               TrangThaiThanhToan, TrangThaiDon, ThoiGianHetHanThanhToan)
            VALUES (@Tk, @Ma, @Tien, @Tien, N'DaThanhToan', N'ThanhCong', DATEADD(MINUTE, 15, SYSDATETIME()));
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Tk", duLieu.NguoiDungID), ("@Ma", Ma("DH")), ("@Tien", 100000m * soLuong));

        Sql.ThucThi(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, @SoLuong, 100000, @Tien);
            INSERT INTO ThanhToan (DonVeID, PhuongThucThanhToan, MaGiaoDichCongThanhToan,
                                   SoTien, LoaiGiaoDich, TrangThai)
            VALUES (@Don, N'VNPAY', @Ma, @Tien, N'ThanhToan', N'ThanhCong');
            """, ("@Don", donID), ("@LoaiVe", duLieu.LoaiVeID), ("@SoLuong", soLuong),
            ("@Tien", 100000m * soLuong), ("@Ma", Ma("GD")));
        return donID;
    }
}