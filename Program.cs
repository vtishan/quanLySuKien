using System;
using Microsoft.Data.SqlClient;

try
{
    Console.WriteLine("Dang ket noi SQL Server...");

    Db.connect();

    Console.WriteLine("Ket noi thanh cong!");

    var bang = Db.suKien("thongKe");

    Console.WriteLine($"So dong: {bang.Rows.Count}");
    Console.WriteLine($"So cot: {bang.Columns.Count}");

    foreach (System.Data.DataColumn cot in bang.Columns)
        Console.Write($"{cot.ColumnName}\t");

    Console.WriteLine();

    foreach (System.Data.DataRow dong in bang.Rows)
        Console.WriteLine(string.Join("\t", dong.ItemArray));
}
catch (Exception loi)
{
    Console.WriteLine($"Loi: {loi.Message}");
    Environment.ExitCode = 1;
}
finally
{
    Db.disconnect();
}

Console.ReadLine();
