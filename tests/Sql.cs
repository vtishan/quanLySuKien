using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLySuKien.Tests;

public static class Sql
{
    public static object? Mot(SqlConnection cn, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        using var lenh = Lenh(cn, sql, thamSo);
        var giaTri = lenh.ExecuteScalar();
        return giaTri is DBNull ? null : giaTri;
    }

    public static async Task<object?> MotAsync(SqlConnection cn, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        using var lenh = Lenh(cn, sql, thamSo);
        var giaTri = await lenh.ExecuteScalarAsync();
        return giaTri is DBNull ? null : giaTri;
    }

    public static int Nguyen(SqlConnection cn, string sql, params (string Ten, object? GiaTri)[] thamSo) =>
        Convert.ToInt32(Mot(cn, sql, thamSo));

    public static decimal SoThuc(SqlConnection cn, string sql, params (string Ten, object? GiaTri)[] thamSo) =>
        Convert.ToDecimal(Mot(cn, sql, thamSo));

    public static DataTable Bang(SqlConnection cn, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        using var lenh = Lenh(cn, sql, thamSo);
        using var bo = new SqlDataAdapter(lenh);
        var bang = new DataTable();
        bo.Fill(bang);
        return bang;
    }

    public static int ThucThi(SqlConnection cn, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        using var lenh = Lenh(cn, sql, thamSo);
        return lenh.ExecuteNonQuery();
    }

    public static SqlCommand ThuTuc(SqlConnection cn, string ten, params (string Ten, object? GiaTri)[] thamSo)
    {
        var lenh = new SqlCommand(ten, cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 60 };
        foreach (var (tenThamSo, giaTri) in thamSo)
            lenh.Parameters.AddWithValue(tenThamSo, giaTri ?? DBNull.Value);
        return lenh;
    }

    public static DataTable GoiThuTuc(SqlConnection cn, string ten, params (string Ten, object? GiaTri)[] thamSo)
    {
        using var lenh = ThuTuc(cn, ten, thamSo);
        using var bo = new SqlDataAdapter(lenh);
        var bang = new DataTable();
        bo.Fill(bang);
        return bang;
    }

    public static SqlException Loi(Action hanhDong)
    {
        var loi = Record.Exception(hanhDong) as SqlException;
        Assert.NotNull(loi);
        return loi!;
    }

    public static SqlException LoiThuTuc(SqlConnection cn, string ten, params (string Ten, object? GiaTri)[] thamSo) =>
        Loi(() =>
        {
            using var lenh = ThuTuc(cn, ten, thamSo);
            lenh.ExecuteNonQuery();
        });

    public static SqlException LoiCauLenh(SqlConnection cn, string sql, params (string Ten, object? GiaTri)[] thamSo) =>
        Loi(() => ThucThi(cn, sql, thamSo));

    public static void ChacLoi(SqlException loi, int ma)
    {
        Assert.Equal(ma, loi.Number);
    }

    static SqlCommand Lenh(SqlConnection cn, string sql, (string Ten, object? GiaTri)[] thamSo)
    {
        var lenh = new SqlCommand(sql, cn) { CommandTimeout = 60 };
        foreach (var (ten, giaTri) in thamSo)
            lenh.Parameters.AddWithValue(ten, giaTri ?? DBNull.Value);
        return lenh;
    }
}
