using Microsoft.Data.SqlClient;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace QuanLySuKien.Tests;

[Collection(DbStaticCollection.Ten)]
public sealed class DbConnectionTests
{
    readonly DatabaseFixture _fixture;

    public DbConnectionTests(DatabaseFixture fixture) => _fixture = fixture;

    [SkippableFact]
    public void connect_khong_co_cau_hinh_nem_InvalidOperationException()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var cu = Environment.GetEnvironmentVariable("SUKIEN_CONNECTION_STRING");
        var cuPhu = Environment.GetEnvironmentVariable("ConnectionStrings__QuanLySuKien");
        try
        {
            Environment.SetEnvironmentVariable("SUKIEN_CONNECTION_STRING", null);
            Environment.SetEnvironmentVariable("ConnectionStrings__QuanLySuKien", null);
            Db.disconnect();
            var loi = Assert.Throws<InvalidOperationException>(() => Db.connect());
            Assert.Contains("SUKIEN_CONNECTION_STRING", loi.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SUKIEN_CONNECTION_STRING", cu);
            Environment.SetEnvironmentVariable("ConnectionStrings__QuanLySuKien", cuPhu);
            Db.disconnect();
        }
    }

    [SkippableFact]
    public void connect_dung_bien_moi_truong_chinh()
    {
        _fixture.BoQuaNeuKhongCoSql();
        try
        {
            Environment.SetEnvironmentVariable("SUKIEN_CONNECTION_STRING", _fixture.ConnectionString);
            Environment.SetEnvironmentVariable("ConnectionStrings__QuanLySuKien", null);
            Assert.True(Db.connect());
            Assert.Equal(4, Db.soDong(Db.thucThi("sp_ThongKeTaiKhoanTheoVaiTro")));
        }
        finally
        {
            Db.disconnect();
        }
    }

    [SkippableFact]
    public void connect_dung_bien_moi_truong_du_phong()
    {
        _fixture.BoQuaNeuKhongCoSql();
        try
        {
            Environment.SetEnvironmentVariable("SUKIEN_CONNECTION_STRING", null);
            Environment.SetEnvironmentVariable("ConnectionStrings__QuanLySuKien", _fixture.ConnectionString);
            Assert.True(Db.connect());
            Assert.True(Db.soNguyen(Db.thucThi("sp_ThongKeTaiKhoanTheoTrangThai"), "SoLuong") > 0);
        }
        finally
        {
            Db.disconnect();
        }
    }

    [SkippableFact]
    public void connect_goi_lai_khi_dang_mo_van_thanh_cong()
    {
        _fixture.BoQuaNeuKhongCoSql();
        try
        {
            Environment.SetEnvironmentVariable("SUKIEN_CONNECTION_STRING", _fixture.ConnectionString);
            Assert.True(Db.connect());
            Assert.True(Db.connect());
            Assert.Equal(4, Db.soDong(Db.thucThi("sp_ThongKeTaiKhoanTheoVaiTro")));
        }
        finally
        {
            Db.disconnect();
        }
    }

    [SkippableFact]
    public void connect_chuoi_sai_nem_SqlException()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var chuoiSai = new SqlConnectionStringBuilder(_fixture.ConnectionString)
        {
            InitialCatalog = "KhongTonTaiDb_" + Guid.NewGuid().ToString("N")
        }.ConnectionString;
        try
        {
            Assert.Throws<SqlException>(() => Db.connect(chuoiSai));
        }
        finally
        {
            Db.disconnect();
        }
    }

    [SkippableFact]
    public void connect_chuoi_moi_thay_the_ket_noi_cu()
    {
        _fixture.BoQuaNeuKhongCoSql();
        var chuoiSai = new SqlConnectionStringBuilder(_fixture.ConnectionString)
        {
            InitialCatalog = "KhongTonTaiDb_" + Guid.NewGuid().ToString("N")
        }.ConnectionString;
        try
        {
            Assert.True(Db.connect(_fixture.ConnectionString));
            Assert.Throws<SqlException>(() => Db.connect(chuoiSai));
        }
        finally
        {
            Db.disconnect();
        }
    }

    [Fact]
    public void disconnect_khi_chua_ket_noi_khong_nem_loi()
    {
        Db.disconnect();
        Db.disconnect();
    }

    [SkippableFact]
    public void goi_sau_khi_disconnect_van_chay_duoc()
    {
        _fixture.BoQuaNeuKhongCoSql();
        try
        {
            Environment.SetEnvironmentVariable("SUKIEN_CONNECTION_STRING", _fixture.ConnectionString);
            Db.connect();
            Db.disconnect();
            Assert.Equal(4, Db.soDong(Db.thucThi("sp_ThongKeTaiKhoanTheoVaiTro")));
        }
        finally
        {
            Db.disconnect();
        }
    }
}
