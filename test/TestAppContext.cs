using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace test;

public partial class TestAppContext : DbContext
{
    public TestAppContext()
    {
    }

    public TestAppContext(DbContextOptions<TestAppContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DepartmentsName> DepartmentsNames { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Jobtitle> Jobtitles { get; set; }

   // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   // {

    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("departments_pkey");

            entity.ToTable("departments");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Managerid).HasColumnName("managerid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Parentid).HasColumnName("parentid");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.ManagerName).HasColumnName("manager_name");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Department)
                .HasForeignKey<Department>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_department");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.Parentid)
                .HasConstraintName("departments_parentid_fkey");
        });

        modelBuilder.Entity<DepartmentsName>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("uc_department_id");

            entity.ToTable("departments_names");

            entity.Property(e => e.DepartmentId)
                .ValueGeneratedNever()
                .HasColumnName("department_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("employees_pkey");

            entity.ToTable("employees");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Department).HasColumnName("department");
            entity.Property(e => e.Fullname).HasColumnName("fullname");
            entity.Property(e => e.Jobtitle).HasColumnName("jobtitle");
            entity.Property(e => e.Login).HasColumnName("login");
            entity.Property(e => e.Password).HasColumnName("password");

            entity.HasOne(d => d.DepartmentNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Department)
                .HasConstraintName("fk_department");

            entity.HasOne(d => d.JobtitleNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Jobtitle)
                .HasConstraintName("fk_jobtitle");
        });

        modelBuilder.Entity<Jobtitle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobtitle_pkey");

            entity.ToTable("jobtitle");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
