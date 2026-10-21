using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    public const string TienTo = "QuanLySuKien_Test_";

    public string Server { get; } = TestEnv.Server;
    public string Database { get; } = TienTo + Guid.NewGuid().ToString("N")[..10];
    public string ConnectionString { get; }
    public bool SanSang { get; private set; }
    public string? Loi { get; private set; }

    public Seed DuLieu { get; private set; } = null!;

    public DatabaseFixture()
    {
        ConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = Server,
            InitialCatalog = Database,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            Encrypt = false,
            Pooling = false,
            ConnectTimeout = 5
        }.ConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using (var master = await MoMaster())
                await Chay(master, $"CREATE DATABASE [{Database}]");

            await using var cn = new SqlConnection(ConnectionString);
            await cn.OpenAsync();

            foreach (var tep in TestEnv.TepSql())
            {
                var noiDung = (await File.ReadAllTextAsync(tep))
                    .Replace("USE [QuanLySuKien];", $"USE [{Database}];", StringComparison.OrdinalIgnoreCase);
                foreach (var lo in TachLo(noiDung))
                    await Chay(cn, lo);
            }

            SanSang = true;
            DuLieu = Seed.Tao(cn);
        }
        catch (Exception ex)
        {
            Loi = ex.Message;
        }
    }

    public async Task DisposeAsync()
    {
        if (!SanSang) return;
        try
        {
            await using var master = await MoMaster();
            await Chay(master, $"""
                ALTER DATABASE [{Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{Database}];
                """);
        }
        catch
        {
        }
    }

    public void BoQuaNeuKhongCoSql() => Skip.IfNot(SanSang, Loi ?? "Khong ket noi duoc SQL Server.");

    public SqlConnection MoKetNoi()
    {
        var cn = new SqlConnection(ConnectionString);
        cn.Open();
        return cn;
    }

    public async Task<SqlConnection> MoKetNoiAsync()
    {
        var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();
        return cn;
    }

    async Task<SqlConnection> MoMaster()
    {
        var cn = new SqlConnection(new SqlConnectionStringBuilder
        {
            DataSource = Server,
            InitialCatalog = "master",
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            Encrypt = false,
            Pooling = false,
            ConnectTimeout = 5
        }.ConnectionString);
        await cn.OpenAsync();
        return cn;
    }

    static async Task Chay(SqlConnection cn, string sql)
    {
        await using var lenh = new SqlCommand(sql, cn) { CommandTimeout = 60 };
        await lenh.ExecuteNonQueryAsync();
    }

    static IEnumerable<string> TachLo(string noiDung) =>
        System.Text.RegularExpressions.Regex
            .Split(noiDung, @"^\s*GO\s*(?:--.*)?$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .Where(lo => !string.IsNullOrWhiteSpace(lo));
}

[CollectionDefinition(DbStaticCollection.Ten, DisableParallelization = true)]
public sealed class DbStaticCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Ten = "db-static";
}

public static class TestEnv
{
    public static string Server =>
        Environment.GetEnvironmentVariable("SUKIEN_TEST_SERVER") ?? @".\SQL2022EXP";

    public static string ThuMucGoc()
    {
        var thuMuc = new DirectoryInfo(AppContext.BaseDirectory);
        while (thuMuc is not null && !Directory.Exists(Path.Combine(thuMuc.FullName, "database")))
            thuMuc = thuMuc.Parent;
        return thuMuc?.FullName ?? Directory.GetCurrentDirectory();
    }

    public static IEnumerable<string> TepSql()
    {
        var saoChep = Path.Combine(AppContext.BaseDirectory, "Sql");
        var goc = Directory.Exists(saoChep) ? saoChep : Path.Combine(ThuMucGoc(), "database");
        string[] thuTuNhom = ["table", "func", "view", "Stored_Procedure", "trigger"];
        string[] thuTuBang =
        [
            "taiKhoan_PhanQuyen.sql", "toChucSuKien.sql", "diaDiemToChuc.sql", "donVe.sql"
        ];

        return thuTuNhom.SelectMany(nhom =>
        {
            var tep = Directory.GetFiles(Path.Combine(goc, nhom), "*.sql");
            return nhom == "table"
                ? thuTuBang.Select(t => Path.Combine(goc, nhom, t))
                : tep.OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
        }).ToList();
    }
}
