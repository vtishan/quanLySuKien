global using db = Db;

using System.Data;
using Microsoft.Data.SqlClient;

public static class Db
{
    const string BIEN_KET_NOI = "SUKIEN_CONNECTION_STRING";
    const string BIEN_KET_NOI_DU_PHONG = "ConnectionStrings__QuanLySuKien";
    const int THOI_GIAN_CHO = 30;

    static SqlConnection ketNoi;

    static string chuoiKetNoi() =>
        Environment.GetEnvironmentVariable(BIEN_KET_NOI)
        ?? Environment.GetEnvironmentVariable(BIEN_KET_NOI_DU_PHONG)
        ?? throw new InvalidOperationException($"Thieu cau hinh ket noi trong bien moi truong {BIEN_KET_NOI}.");

    public static bool connect()
    {
        ketNoi ??= new SqlConnection(chuoiKetNoi());
        if (ketNoi.State != ConnectionState.Open) ketNoi.Open();
        return true;
    }

    public static bool connect(string chuoi)
    {
        disconnect();
        ketNoi = new SqlConnection(chuoi);
        ketNoi.Open();
        return true;
    }

    public static void disconnect()
    {
        ketNoi?.Dispose();
        ketNoi = null;
    }

    static readonly Dictionary<string, (string sp, string[] ten)> NGHIEP_VU =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["taiKhoan.dangKy"] = ("sp_DangKyTaiKhoan", new[] { "@HoTen", "@Email", "@MatKhauHash", "@SoDienThoai" }),
            ["taiKhoan.dangNhap"] = ("sp_KiemTraDangNhap", new[] { "@Email" }),
            ["taiKhoan.capNhat"] = ("sp_CapNhatThongTinTaiKhoan", new[] { "@TaiKhoanID", "@HoTen", "@Email", "@SoDienThoai" }),
            ["taiKhoan.khoa"] = ("sp_KhoaTaiKhoan", new[] { "@NguoiThucHienID", "@TaiKhoanID" }),
            ["taiKhoan.moKhoa"] = ("sp_MoKhoaTaiKhoan", new[] { "@NguoiThucHienID", "@TaiKhoanID" }),
            ["taiKhoan.doiVaiTro"] = ("sp_DoiVaiTroTaiKhoan", new[] { "@NguoiThucHienID", "@TaiKhoanID", "@VaiTroID" }),
            ["taiKhoan.traCuu"] = ("sp_TraCuuTaiKhoan", new[] { "@TuKhoa", "@VaiTroID", "@TrangThai", "@Trang", "@SoDong" }),
            ["taiKhoan.thongKeTrangThai"] = ("sp_ThongKeTaiKhoanTheoTrangThai", Array.Empty<string>()),
            ["taiKhoan.thongKeVaiTro"] = ("sp_ThongKeTaiKhoanTheoVaiTro", Array.Empty<string>()),
            ["taiKhoan.suKienTheoTaiKhoan"] = ("sp_TraCuuSuKienTheoTaiKhoan", new[] { "@TaiKhoanID" }),
            ["taiKhoan.donVeTheoTaiKhoan"] = ("sp_TraCuuDonVeTheoTaiKhoan", new[] { "@TaiKhoanID" }),

            ["suKien.guiYeuCau"] = ("sp_GuiYeuCauXacThucToChuc", new[] { "@TaiKhoanID", "@SoCCCD", "@NgaySinh", "@GioiTinh", "@DiaChiThuongTru", "@AnhCCCDMatTruoc", "@AnhCCCDMatSau" }),
            ["suKien.capNhatYeuCau"] = ("sp_CapNhatYeuCauXacThucToChuc", new[] { "@TaiKhoanID", "@YeuCauID", "@SoCCCD", "@NgaySinh", "@GioiTinh", "@DiaChiThuongTru", "@AnhCCCDMatTruoc", "@AnhCCCDMatSau" }),
            ["suKien.traCuuYeuCau"] = ("sp_TraCuuYeuCauXacThucToChuc", new[] { "@TaiKhoanID" }),
            ["suKien.duyetYeuCau"] = ("sp_DuyetYeuCauXacThucToChuc", new[] { "@NguoiDuyetID", "@YeuCauID", "@TenToChuc" }),
            ["suKien.tuChoiYeuCau"] = ("sp_TuChoiYeuCauXacThucToChuc", new[] { "@NguoiDuyetID", "@YeuCauID", "@LyDoTuChoi" }),
            ["suKien.thongKeYeuCau"] = ("sp_ThongKeYeuCauXacThucToChuc", Array.Empty<string>()),
            ["suKien.themLoai"] = ("sp_ThemLoaiSuKien", new[] { "@TenLoai", "@MoTa" }),
            ["suKien.capNhatLoai"] = ("sp_CapNhatLoaiSuKien", new[] { "@LoaiSuKienID", "@TenLoai", "@MoTa" }),
            ["suKien.doiTrangThaiLoai"] = ("sp_DoiTrangThaiLoaiSuKien", new[] { "@LoaiSuKienID", "@TrangThai" }),
            ["suKien.traCuuLoai"] = ("sp_TraCuuLoaiSuKien", new[] { "@TuKhoa", "@TrangThai" }),
            ["suKien.tao"] = ("sp_TaoSuKien", new[] { "@NguoiTaoID", "@LoaiSuKienID", "@TenSuKien", "@MoTa", "@AnhBia", "@ThoiGianBatDau", "@ThoiGianKetThuc", "@SoLuongNguoiDuKien" }),
            ["suKien.duyet"] = ("sp_DuyetSuKien", new[] { "@NguoiDuyetID", "@SuKienID" }),
            ["suKien.tuChoi"] = ("sp_TuChoiSuKien", new[] { "@NguoiDuyetID", "@SuKienID", "@LyDoTuChoi" }),
            ["suKien.traCuu"] = ("sp_TraCuuSuKien", new[] { "@TuKhoa", "@LoaiSuKienID", "@TrangThai", "@Trang", "@SoDong" }),
            ["suKien.thongKe"] = ("sp_ThongKeSuKien", Array.Empty<string>()),

            ["diaDiem.them"] = ("sp_ThemDiaDiem", new[] { "@NguoiThucHienID", "@TenDiaDiem", "@DiaChi", "@SucChua", "@GiaThue", "@MoTa" }),
            ["diaDiem.capNhat"] = ("sp_CapNhatDiaDiem", new[] { "@NguoiThucHienID", "@DiaDiemID", "@TenDiaDiem", "@DiaChi", "@SucChua", "@GiaThue", "@MoTa" }),
            ["diaDiem.doiTrangThai"] = ("sp_DoiTrangThaiDiaDiem", new[] { "@NguoiThucHienID", "@DiaDiemID", "@TrangThai" }),
            ["diaDiem.traCuu"] = ("sp_TraCuuDiaDiem", new[] { "@TuKhoa", "@SucChuaToiThieu", "@GiaThueToiDa", "@TrangThai", "@Trang", "@SoDong" }),
            ["diaDiem.dat"] = ("sp_TaoDatDiaDiem", new[] { "@TaiKhoanID", "@SuKienID", "@DiaDiemID", "@ThoiGianBatDau", "@ThoiGianKetThuc" }),
            ["diaDiem.xacNhanDat"] = ("sp_XacNhanDatDiaDiem", new[] { "@NguoiThucHienID", "@DatDiaDiemID" }),
            ["diaDiem.tuChoiDat"] = ("sp_TuChoiDatDiaDiem", new[] { "@NguoiThucHienID", "@DatDiaDiemID" }),
            ["diaDiem.huyDat"] = ("sp_HuyDatDiaDiem", new[] { "@TaiKhoanID", "@DatDiaDiemID" }),
            ["diaDiem.traCuuDat"] = ("sp_TraCuuDatDiaDiem", new[] { "@TaiKhoanID", "@DiaDiemID", "@TrangThai" }),
            ["diaDiem.thongKeTrangThai"] = ("sp_ThongKeDatDiaDiemTheoTrangThai", Array.Empty<string>()),
            ["diaDiem.thongKeDiaDiem"] = ("sp_ThongKeDatDiaDiemTheoDiaDiem", Array.Empty<string>()),

            ["donVe.themLoaiVe"] = ("sp_ThemLoaiVe", new[] { "@SuKienID", "@TenLoaiVe", "@GiaVe", "@SoLuongToiDa", "@SoLuongToiDaMoiDon", "@ThoiGianMoBan", "@ThoiGianDongBan" }),
            ["donVe.capNhatLoaiVe"] = ("sp_CapNhatLoaiVe", new[] { "@LoaiVeID", "@TenLoaiVe", "@GiaVe", "@SoLuongToiDa", "@SoLuongToiDaMoiDon", "@ThoiGianMoBan", "@ThoiGianDongBan" }),
            ["donVe.tao"] = ("sp_TaoDonVe", new[] { "@TaiKhoanID", "@LoaiVeID", "@SoLuong", "@TongTienGiam" }),
            ["donVe.huy"] = ("sp_HuyDonVe", new[] { "@DonVeID", "@TaiKhoanID" }),
            ["donVe.giaoDich"] = ("sp_ThongKeGiaoDich", new[] { "@DonVeID" }),

            ["ve.checkIn"] = ("sp_CheckInVe", new[] { "@VeID", "@NguoiCheckInID" }),
            ["ve.theoDon"] = ("sp_TraCuuVeTheoDon", new[] { "@DonVeID" }),
            ["ve.lichSuXuat"] = ("sp_GhiLichSuXuatVe", new[] { "@VeID", "@HinhThucXuat", "@NguoiThucHienID" }),
            ["ve.thongKeTrangThai"] = ("sp_ThongKeVeTheoTrangThai", Array.Empty<string>()),
            ["ve.thongKeCheckIn"] = ("sp_ThongKeCheckInTheoSuKien", new[] { "@SuKienID" }),

            ["thanhToan.ghiNhan"] = ("sp_GhiNhanThanhToan", new[] { "@DonVeID", "@SoTien", "@MaGiaoDich", "@PhuongThucThanhToan", "@TrangThai" }),
            ["thanhToan.hoanTien"] = ("sp_GhiNhanHoanTien", new[] { "@DonVeID", "@SoTienHoan", "@MaGiaoDich", "@TrangThai" })
        };

    public static DataTable taiKhoan(string nghiepVu, params object[] thamSo) => goi("taiKhoan", nghiepVu, thamSo);
    public static DataTable suKien(string nghiepVu, params object[] thamSo) => goi("suKien", nghiepVu, thamSo);
    public static DataTable diaDiem(string nghiepVu, params object[] thamSo) => goi("diaDiem", nghiepVu, thamSo);
    public static DataTable donVe(string nghiepVu, params object[] thamSo) => goi("donVe", nghiepVu, thamSo);
    public static DataTable ve(string nghiepVu, params object[] thamSo) => goi("ve", nghiepVu, thamSo);
    public static DataTable thanhToan(string nghiepVu, params object[] thamSo) => goi("thanhToan", nghiepVu, thamSo);
    public static DataTable thucThi(string thuTuc, params SqlParameter[] thamSo) => chay(thuTuc, thamSo);

    static DataTable goi(string nhom, string nghiepVu, object[] thamSo)
    {
        if (!NGHIEP_VU.TryGetValue(nhom + "." + nghiepVu, out var m))
            throw new ArgumentException($"Khong co nghiep vu {nhom}.{nghiepVu}.");
        return chay(m.sp, bienThamSo(m.ten, thamSo));
    }

    static SqlParameter[] bienThamSo(string[] ten, object[] giaTri)
    {
        var thamSo = new SqlParameter[giaTri.Length];
        for (var i = 0; i < giaTri.Length; i++)
            thamSo[i] = giaTri[i] as SqlParameter
                ?? new SqlParameter(i < ten.Length ? ten[i] : throw new ArgumentException("Thua tham so."), giaTri[i] ?? DBNull.Value);
        return thamSo;
    }

    static DataTable chay(string thuTuc, SqlParameter[] thamSo)
    {
        var dungChung = ketNoi is { State: ConnectionState.Open };
        var cn = dungChung ? ketNoi : new SqlConnection(chuoiKetNoi());
        if (!dungChung) cn.Open();
        try
        {
            using var lenh = new SqlCommand(thuTuc, cn)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = THOI_GIAN_CHO
            };
            if (thamSo.Length > 0) lenh.Parameters.AddRange(thamSo);
            using var bo = new SqlDataAdapter(lenh);
            var bang = new DataTable();
            bo.Fill(bang);
            return bang;
        }
        finally
        {
            if (!dungChung) cn.Dispose();
        }
    }

    public static DataTable phatHanhVe(int donVeID)
    {
        const string SINH_VE =
            @"DECLARE @thieu TABLE (
                  DonVeID INT, LoaiVeID INT, DonGia DECIMAL(18,2),
                  HoTen NVARCHAR(100), Email NVARCHAR(255), SoLuong INT);

              INSERT INTO @thieu (DonVeID, LoaiVeID, DonGia, HoTen, Email, SoLuong)
              SELECT CT.DonVeID, CT.LoaiVeID, CT.DonGia, TK.HoTen, TK.Email,
                     CT.SoLuong - (
                         SELECT COUNT(*) FROM dbo.Ve V
                         WHERE V.DonVeID = CT.DonVeID AND V.LoaiVeID = CT.LoaiVeID)
              FROM dbo.ChiTietDonVe CT
              INNER JOIN dbo.DonVe DV ON DV.DonVeID = CT.DonVeID
              INNER JOIN dbo.TaiKhoan TK ON TK.TaiKhoanID = DV.TaiKhoanID
              WHERE CT.DonVeID = @DonVeID
                AND DV.TrangThaiThanhToan IN (N'DaThanhToan', N'HoanTienMotPhan');

              IF NOT EXISTS (SELECT 1 FROM @thieu WHERE SoLuong > 0)
                  THROW 50501, N'Don ve chua thanh toan hoac da phat hanh du ve.', 1;

              WITH so AS (
                  SELECT 1 AS n
                  UNION ALL
                  SELECT n + 1 FROM so WHERE n < (SELECT MAX(SoLuong) FROM @thieu)
              )
              INSERT INTO dbo.Ve (
                  DonVeID, LoaiVeID, MaVe, MaQR,
                  HoTenNguoiSuDung, EmailNguoiSuDung, GiaThucTe, TrangThai)
              SELECT T.DonVeID, T.LoaiVeID,
                     N'VE' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), N'-', N''),
                     N'QR' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), N'-', N''),
                     T.HoTen, T.Email, T.DonGia, N'ChuaSuDung'
              FROM @thieu T
              INNER JOIN so ON so.n <= T.SoLuong
              OPTION (MAXRECURSION 0);";

        using var cn = new SqlConnection(chuoiKetNoi());
        cn.Open();
        using var giaoDich = cn.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            using (var lenh = new SqlCommand(SINH_VE, cn, giaoDich))
            {
                lenh.CommandTimeout = THOI_GIAN_CHO;
                lenh.Parameters.AddWithValue("@DonVeID", donVeID);
                lenh.ExecuteNonQuery();
            }

            var bang = new DataTable();
            using (var lenh = new SqlCommand(
                @"SELECT V.VeID, V.DonVeID, V.LoaiVeID, V.MaVe, V.MaQR, V.TrangThai, V.GiaThucTe
                  FROM dbo.Ve V WHERE V.DonVeID = @DonVeID ORDER BY V.VeID;", cn, giaoDich))
            {
                lenh.Parameters.AddWithValue("@DonVeID", donVeID);
                using var bo = new SqlDataAdapter(lenh);
                bo.Fill(bang);
            }

            giaoDich.Commit();
            return bang;
        }
        catch
        {
            giaoDich.Rollback();
            throw;
        }
    }

    public static DataRow? dongDau(DataTable bang) => bang.Rows.Count > 0 ? bang.Rows[0] : null;
    public static int soDong(DataTable bang) => bang.Rows.Count;
    public static int soNguyen(DataTable bang, string cot) =>
        bang.Rows.Count == 0 || bang.Rows[0][cot] is DBNull ? 0 : Convert.ToInt32(bang.Rows[0][cot]);
}