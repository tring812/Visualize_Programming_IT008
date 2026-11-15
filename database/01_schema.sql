-- T05: Tao cau truc PharmacyDB theo ERD trong README.md.
-- Chay toan bo file bang SSMS hoac sqlcmd (co ho tro GO).
-- CANH BAO: moi lan chay se xoa toan bo du lieu PharmacyDB va tao lai.
-- Sau do chay 02_seed.sql khi script du lieu mau cua nhom da san sang.
USE master;
GO
IF DB_ID(N'PharmacyDB') IS NOT NULL
BEGIN
    ALTER DATABASE PharmacyDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PharmacyDB;
END;
GO
CREATE DATABASE PharmacyDB;
GO
USE PharmacyDB;
GO

CREATE TABLE dbo.LoaiThuoc
(
    MaLoai INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoaiThuoc PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.NhaCungCap
(
    MaNCC INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhaCungCap PRIMARY KEY,
    TenNCC NVARCHAR(200) NOT NULL,
    DiaChi NVARCHAR(255) NULL,
    SoDienThoai VARCHAR(20) NOT NULL
);

CREATE TABLE dbo.KhachHang
(
    MaKH INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KhachHang PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(20) NOT NULL,
    DiaChi NVARCHAR(255) NULL,
    CONSTRAINT UQ_KhachHang_SoDienThoai UNIQUE (SoDienThoai)
);

CREATE TABLE dbo.NhanVien
(
    MaNV INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(20) NOT NULL,
    TenDangNhap NVARCHAR(50) NOT NULL,
    MatKhauHash CHAR(64) NOT NULL,
    VaiTro NVARCHAR(20) NOT NULL CONSTRAINT DF_NhanVien_VaiTro DEFAULT N'NhanVien',
    TrangThai BIT NOT NULL CONSTRAINT DF_NhanVien_TrangThai DEFAULT 1,
    CONSTRAINT UQ_NhanVien_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT CK_NhanVien_VaiTro CHECK (VaiTro IN (N'QuanLy', N'NhanVien'))
);

CREATE TABLE dbo.Thuoc
(
    MaThuoc INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Thuoc PRIMARY KEY,
    TenThuoc NVARCHAR(200) NOT NULL,
    DonViTinh NVARCHAR(50) NOT NULL,
    GiaBan DECIMAL(18,0) NOT NULL,
    SoLuongTon INT NOT NULL CONSTRAINT DF_Thuoc_SoLuongTon DEFAULT 0,
    HanSuDung DATE NOT NULL,
    MaLoai INT NOT NULL,
    CONSTRAINT FK_Thuoc_LoaiThuoc FOREIGN KEY (MaLoai) REFERENCES dbo.LoaiThuoc(MaLoai),
    CONSTRAINT CK_Thuoc_GiaBan CHECK (GiaBan > 0),
    CONSTRAINT CK_Thuoc_SoLuongTon CHECK (SoLuongTon >= 0)
);

CREATE TABLE dbo.PhieuNhap
(
    MaPN INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhieuNhap PRIMARY KEY,
    NgayNhap DATETIME NOT NULL CONSTRAINT DF_PhieuNhap_NgayNhap DEFAULT GETDATE(),
    TongTien DECIMAL(18,0) NOT NULL,
    MaNCC INT NOT NULL,
    MaNV INT NOT NULL,
    CONSTRAINT FK_PhieuNhap_NhaCungCap FOREIGN KEY (MaNCC) REFERENCES dbo.NhaCungCap(MaNCC),
    CONSTRAINT FK_PhieuNhap_NhanVien FOREIGN KEY (MaNV) REFERENCES dbo.NhanVien(MaNV),
    CONSTRAINT CK_PhieuNhap_TongTien CHECK (TongTien >= 0)
);

CREATE TABLE dbo.HoaDon
(
    MaHD INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaDon PRIMARY KEY,
    NgayBan DATETIME NOT NULL CONSTRAINT DF_HoaDon_NgayBan DEFAULT GETDATE(),
    TongTien DECIMAL(18,0) NOT NULL,
    MaNV INT NOT NULL,
    MaKH INT NULL, -- NULL: khach le.
    CONSTRAINT FK_HoaDon_NhanVien FOREIGN KEY (MaNV) REFERENCES dbo.NhanVien(MaNV),
    CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH),
    CONSTRAINT CK_HoaDon_TongTien CHECK (TongTien >= 0)
);

CREATE TABLE dbo.CT_PhieuNhap
(
    MaPN INT NOT NULL,
    MaThuoc INT NOT NULL,
    SoLuong INT NOT NULL,
    GiaNhap DECIMAL(18,0) NOT NULL,
    CONSTRAINT PK_CT_PhieuNhap PRIMARY KEY (MaPN, MaThuoc),
    CONSTRAINT FK_CT_PhieuNhap_PhieuNhap FOREIGN KEY (MaPN) REFERENCES dbo.PhieuNhap(MaPN),
    CONSTRAINT FK_CT_PhieuNhap_Thuoc FOREIGN KEY (MaThuoc) REFERENCES dbo.Thuoc(MaThuoc),
    CONSTRAINT CK_CT_PhieuNhap_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_CT_PhieuNhap_GiaNhap CHECK (GiaNhap > 0)
);

CREATE TABLE dbo.CT_HoaDon
(
    MaHD INT NOT NULL,
    MaThuoc INT NOT NULL,
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,0) NOT NULL, -- Luu gia tai thoi diem ban.
    CONSTRAINT PK_CT_HoaDon PRIMARY KEY (MaHD, MaThuoc),
    CONSTRAINT FK_CT_HoaDon_HoaDon FOREIGN KEY (MaHD) REFERENCES dbo.HoaDon(MaHD),
    CONSTRAINT FK_CT_HoaDon_Thuoc FOREIGN KEY (MaThuoc) REFERENCES dbo.Thuoc(MaThuoc),
    CONSTRAINT CK_CT_HoaDon_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_CT_HoaDon_DonGia CHECK (DonGia > 0)
);
GO
