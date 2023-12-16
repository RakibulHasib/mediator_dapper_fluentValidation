using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Test_project.Entity;

namespace Test_project.Context;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employee { get; set; }

    public virtual DbSet<RoleAssignTbl> RoleAssignTbl { get; set; }

    public virtual DbSet<RoleMasterTbl> RoleMasterTbl { get; set; }

    public virtual DbSet<UserInfoTbl> UserInfoTbl { get; set; }

    public virtual DbSet<UserLogInInfoTbl> UserLogInInfoTbl { get; set; }

    public SqlConnection CreateConnection() => (SqlConnection)Database.GetDbConnection();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF1BC0C0F22");

            entity.ToTable("Employee");

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Dob)
                .HasColumnType("date")
                .HasColumnName("DOB");
            entity.Property(e => e.EmployeeName).HasMaxLength(100);
        });

        modelBuilder.Entity<RoleAssignTbl>(entity =>
        {
            entity.HasKey(e => e.RoleAssignId).HasName("PK__RoleAssi__F5E90E9AA0266F91");

            entity.ToTable("RoleAssign_tbl");

            entity.Property(e => e.RoleAssignId).HasColumnName("RoleAssignID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<RoleMasterTbl>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__RoleMast__8AFACE3AB6F487F5");

            entity.ToTable("RoleMaster_tbl");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<UserInfoTbl>(entity =>
        {
            entity.HasKey(e => e.UserInfoId).HasName("PK__User_Inf__D07EF2C404081423");

            entity.ToTable("User_Info_tbl");

            entity.Property(e => e.UserInfoId).HasColumnName("UserInfoID");
            entity.Property(e => e.Email).HasMaxLength(30);
            entity.Property(e => e.FirstName).HasMaxLength(30);
            entity.Property(e => e.LastName).HasMaxLength(30);
        });

        modelBuilder.Entity<UserLogInInfoTbl>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Log__1788CCAC0F0E68CE");

            entity.ToTable("User_LogInInfo_tbl");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.LoginId)
                .HasMaxLength(50)
                .HasColumnName("LoginID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.TokenSecretKey).HasMaxLength(100);
            entity.Property(e => e.UserInfoId).HasColumnName("UserInfoID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
