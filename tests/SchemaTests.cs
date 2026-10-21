namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class SchemaTests
{
    readonly DatabaseFixture _fixture;

    public SchemaTests(DatabaseFixture fixture) => _fixture = fixture;

    static readonly string[] BangMongDoi =
    [
        "VaiTro", "TaiKhoan", "YeuCauXacThucToChuc", "BanToChuc", "LoaiSuKien", "SuKien",
        "DiaDiem", "DatDiaDiem", "LoaiVe", "DonVe", "ChiTietDonVe", "ThanhToan", "Ve", "XuatVe"
    ];

    [SkippableFact]
    public void du_14_bang_duoc_tao()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var bang = Sql.Bang(cn, "SELECT name FROM sys.tables WHERE is_ms_shipped = 0 ORDER BY name");
        var ten = bang.Rows.Cast<System.Data.DataRow>().Select(r => (string)r["name"]).ToArray();
        Assert.Equal(BangMongDoi.OrderBy(t => t), ten.OrderBy(t => t));
    }

    [SkippableFact]
    public void moi_bang_co_khoa_chinh()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        foreach (var bang in BangMongDoi)
        {
            var soPk = Sql.Nguyen(cn, """
                SELECT COUNT(*) FROM sys.key_constraints kc
                INNER JOIN sys.tables t ON t.object_id = kc.parent_object_id
                WHERE t.name = @Bang AND kc.type = 'PK'
                """, ("@Bang", bang));
            Assert.True(soPk == 1, $"Bang {bang} khong co dung 1 khoa chinh.");
        }
    }

    [SkippableFact]
    public void khoa_ngoai_tro_dung_bang_va_cot()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var kyVong = new (string Bang, string Cot, string BangCha, string CotCha)[]
        {
            ("TaiKhoan", "VaiTroID", "VaiTro", "VaiTroID"),
            ("YeuCauXacThucToChuc", "TaiKhoanID", "TaiKhoan", "TaiKhoanID"),
            ("YeuCauXacThucToChuc", "NguoiDuyetID", "TaiKhoan", "TaiKhoanID"),
            ("BanToChuc", "TaiKhoanID", "TaiKhoan", "TaiKhoanID"),
            ("SuKien", "NguoiTaoID", "TaiKhoan", "TaiKhoanID"),
            ("SuKien", "LoaiSuKienID", "LoaiSuKien", "LoaiSuKienID"),
            ("SuKien", "NguoiDuyetID", "TaiKhoan", "TaiKhoanID"),
            ("DatDiaDiem", "SuKienID", "SuKien", "SuKienID"),
            ("DatDiaDiem", "DiaDiemID", "DiaDiem", "DiaDiemID"),
            ("LoaiVe", "SuKienID", "SuKien", "SuKienID"),
            ("DonVe", "TaiKhoanID", "TaiKhoan", "TaiKhoanID"),
            ("ChiTietDonVe", "DonVeID", "DonVe", "DonVeID"),
            ("ChiTietDonVe", "LoaiVeID", "LoaiVe", "LoaiVeID"),
            ("ThanhToan", "DonVeID", "DonVe", "DonVeID"),
            ("Ve", "DonVeID", "DonVe", "DonVeID"),
            ("Ve", "LoaiVeID", "LoaiVe", "LoaiVeID"),
            ("Ve", "NguoiCheckInID", "TaiKhoan", "TaiKhoanID"),
            ("XuatVe", "VeID", "Ve", "VeID"),
            ("XuatVe", "NguoiXuatID", "TaiKhoan", "TaiKhoanID")
        };

        foreach (var (bang, cot, bangCha, cotCha) in kyVong)
        {
            var co = Sql.Nguyen(cn, """
                SELECT COUNT(*) FROM sys.foreign_key_columns fk
                INNER JOIN sys.tables t ON t.object_id = fk.parent_object_id
                INNER JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                INNER JOIN sys.columns c ON c.object_id = fk.parent_object_id AND c.column_id = fk.parent_column_id
                INNER JOIN sys.columns rc ON rc.object_id = fk.referenced_object_id AND rc.column_id = fk.referenced_column_id
                WHERE t.name = @Bang AND c.name = @Cot AND rt.name = @Cha AND rc.name = @CotCha
                """, ("@Bang", bang), ("@Cot", cot), ("@Cha", bangCha), ("@CotCha", cotCha));
            Assert.True(co == 1, $"Thieu FK {bang}.{cot} -> {bangCha}.{cotCha}");
        }
    }

    [SkippableFact]
    public void khoa_ngoai_chan_du_lieu_mo_coi()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var loi = Sql.LoiCauLenh(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash)
            VALUES (999999, N'X', @Email, N'hash')
            """, ("@Email", Email()));
        Assert.Equal(547, loi.Number);
    }

    [SkippableFact]
    public void khoa_ngoai_chan_xoa_ban_ghi_duoc_tham_chieu()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var vaiTroID = Sql.Nguyen(cn, """
            INSERT INTO VaiTro (TenVaiTro) VALUES (@Ten);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """, ("@Ten", "VaiTro-" + Guid.NewGuid().ToString("N")[..8]));
        Sql.ThucThi(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash)
            VALUES (@VaiTro, N'X', @Email, N'hash')
            """, ("@VaiTro", vaiTroID), ("@Email", Email()));
        Assert.Equal(547, Sql.LoiCauLenh(cn, "DELETE FROM VaiTro WHERE VaiTroID = @Id", ("@Id", vaiTroID)).Number);
    }

    [SkippableFact]
    public void rang_buoc_unique_duoc_ap_dung()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var email = Email();
        Sql.ThucThi(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash)
            SELECT VaiTroID, N'X', @Email, N'hash' FROM VaiTro WHERE TenVaiTro = N'NguoiDung'
            """, ("@Email", email));
        Assert.Equal(2627, Sql.LoiCauLenh(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash)
            SELECT VaiTroID, N'Y', @Email, N'hash' FROM VaiTro WHERE TenVaiTro = N'NguoiDung'
            """, ("@Email", email)).Number);
    }

    [SkippableFact]
    public void gia_tri_mac_dinh_duoc_ap_dung()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var email = Email();
        Sql.ThucThi(cn, """
            INSERT INTO TaiKhoan (VaiTroID, HoTen, Email, MatKhauHash)
            SELECT VaiTroID, N'X', @Email, N'hash' FROM VaiTro WHERE TenVaiTro = N'NguoiDung'
            """, ("@Email", email));
        var bang = Sql.Bang(cn, "SELECT TrangThai, NgayTao FROM TaiKhoan WHERE Email = @Email", ("@Email", email));
        Assert.True(Convert.ToBoolean(bang.Rows[0]["TrangThai"]));
        Assert.IsType<DateTime>(bang.Rows[0]["NgayTao"]);
    }

    [SkippableFact]
    public void check_constraint_chan_du_lieu_sai()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var loi = Sql.LoiCauLenh(cn, """
            INSERT INTO DiaDiem (TenDiaDiem, DiaChi, SucChua, GiaThue)
            VALUES (N'X', N'Y', 0, 100)
            """);
        Assert.Equal(547, loi.Number);
    }

    [SkippableFact]
    public void check_tien_coc_khong_vuot_tong_tien()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var duLieu = BoDuLieu.Tao(cn);
        var loi = Sql.LoiCauLenh(cn, """
            INSERT INTO DatDiaDiem (SuKienID, DiaDiemID, ThoiGianBatDau, ThoiGianKetThuc, TongTien, TienCoc)
            VALUES (@Sk, @Dd, SYSDATETIME(), DATEADD(DAY, 1, SYSDATETIME()), 100, 200)
            """, ("@Sk", duLieu.SuKienID), ("@Dd", duLieu.DiaDiemID));
        Assert.Equal(547, loi.Number);
    }

    [SkippableFact]
    public void check_thanh_tien_phai_bang_so_luong_nhan_don_gia()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var duLieu = BoDuLieu.Tao(cn);
        var loi = Sql.LoiCauLenh(cn, """
            INSERT INTO ChiTietDonVe (DonVeID, LoaiVeID, SoLuong, DonGia, ThanhTien)
            VALUES (@Don, @LoaiVe, 2, 100, 100)
            """, ("@Don", duLieu.DonVeID), ("@LoaiVe", duLieu.LoaiVeID));
        Assert.Equal(547, loi.Number);
    }

    [SkippableFact]
    public void check_trang_thai_khong_hop_le_bi_chan()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        var duLieu = BoDuLieu.Tao(cn);
        var loi = Sql.LoiCauLenh(cn, """
            INSERT INTO DonVe (TaiKhoanID, MaDonHang, TongTienGoc, TongTienThanhToan,
                               TrangThaiDon, ThoiGianHetHanThanhToan)
            VALUES (@Tk, @Ma, 100, 100, N'SaiTrangThai', SYSDATETIME())
            """, ("@Tk", duLieu.NguoiDungID), ("@Ma", "DH-" + Guid.NewGuid().ToString("N")[..8]));
        Assert.Equal(547, loi.Number);
    }

    [SkippableFact]
    public void co_du_cac_function_view_thu_tuc_va_trigger()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        Assert.Equal(48, Sql.Nguyen(cn, "SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN','IF','TF') AND is_ms_shipped = 0"));
        Assert.Equal(30, Sql.Nguyen(cn, "SELECT COUNT(*) FROM sys.views WHERE is_ms_shipped = 0"));
        Assert.Equal(49, Sql.Nguyen(cn, "SELECT COUNT(*) FROM sys.procedures WHERE is_ms_shipped = 0"));
        Assert.Equal(21, Sql.Nguyen(cn, "SELECT COUNT(*) FROM sys.triggers WHERE is_ms_shipped = 0"));
    }

    [SkippableFact]
    public void cac_bang_tra_cuu_chinh_co_index()
    {
        _fixture.BoQuaNeuKhongCoSql();
        using var cn = _fixture.MoKetNoi();
        foreach (var (bang, cot) in new[] { ("SuKien", "TrangThai"), ("DonVe", "TaiKhoanID"), ("Ve", "DonVeID"), ("DatDiaDiem", "DiaDiemID") })
        {
            var co = Sql.Nguyen(cn, """
                SELECT COUNT(*) FROM sys.index_columns ic
                INNER JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                INNER JOIN sys.tables t ON t.object_id = i.object_id
                INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                WHERE t.name = @Bang AND c.name = @Cot AND i.type > 0
                """, ("@Bang", bang), ("@Cot", cot));
            Assert.True(co > 0, $"Bang {bang} thieu index tren {cot}.");
        }
    }

    internal static string Email() => "tk-" + Guid.NewGuid().ToString("N")[..10] + "@test.local";
}
