using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DAL.Entities;
using DAL.Interfaces;

namespace DAL.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customers> Customers { get; set; } = null!;
        public virtual DbSet<Invoices> Invoices { get; set; } = null!;
        public virtual DbSet<ItemsDetails> ItemsDetails { get; set; } = null!;
        public virtual DbSet<UserAccount> UserAccount { get; set; } = null!;
        public virtual DbSet<ErrorLog> ErroLog { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    optionsBuilder.UseSqlServer("Server=DESKTOP-KVLQN0J\\SQLEXPRESS;Database=InvoiceMgmt;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
            //}
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-KVLQN0J\\SQLEXPRESS;Database=InvoiceMgmt;Trusted_Connection=True;Encrypt=False;", sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5, // The maximum number of retry attempts
                        maxRetryDelay: TimeSpan.FromSeconds(30), // The maximum delay between retries
                        errorNumbersToAdd: null // Additional error numbers to consider for retrying
                    );
                });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customers>(entity =>
            {
                entity.HasKey(e => e.CustomerId)
                    .HasName("PK__Customer__B611CB7D164452B1");

                entity.Property(e => e.CustomerId)
                    .UseIdentityColumn()
                    .HasColumnName("customerId");

                entity.Property(e => e.AddressLine)
                    .HasMaxLength(100)
                    .HasColumnName("addressLine");

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .HasColumnName("city");

                entity.Property(e => e.CompanyId).HasColumnName("companyId");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("createdOn")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EmailId)
                    .HasMaxLength(50)
                    .HasColumnName("email_id");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(500)
                    .HasColumnName("firstName");

                entity.Property(e => e.LastName).HasMaxLength(500);

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .HasColumnName("phone");

                entity.Property(e => e.Pincode)
                    .HasMaxLength(30)
                    .HasColumnName("pincode");

                entity.Property(e => e.States)
                    .HasMaxLength(50)
                    .HasColumnName("states");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK__Customers__compa__1920BF5C");
            });

            modelBuilder.Entity<Invoices>(entity =>
            {
                entity.HasKey(e => e.InvoiceNo)
                    .HasName("PK__Invoices__1253964607020F21");

                entity.Property(e => e.InvoiceNo).HasColumnName("invoiceNo");

                entity.Property(e => e.CompanyId).HasColumnName("companyId");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(500)
                    .HasColumnName("customerName");

                entity.Property(e => e.InvoiceDate)
                    .HasColumnType("datetime")
                    .HasColumnName("invoiceDate")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK__Invoices__compan__09DE7BCC");

                entity.HasMany(e => e.Details)
                    .WithOne(d => d.InvoiceNoNavigation)
                    .HasForeignKey(d => d.InvoiceNo)
                    .HasConstraintName("FK__ItemsDetails__invoiceNo__0BC6C43E")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ItemsDetails>(entity =>
            {
                entity.HasKey(e => new { e.InvoiceNo, e.ItemName });

                entity.Property(e => e.InvoiceNo).HasColumnName("invoiceNo");

                entity.Property(e => e.ItemName)
                    .HasMaxLength(500)
                    .HasColumnName("itemName");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.GrossAmt)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("GrossAmt");

                entity.Property(e => e.GSTRate)
                   .HasColumnType("decimal(18, 2)")
                   .HasColumnName("GSTRate");

                entity.Property(e => e.NetAmt)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("NetAmt");

                entity.HasOne(d => d.InvoiceNoNavigation)
                    .WithMany(p => p.Details)
                    .HasForeignKey(d => d.InvoiceNo)
                    .HasConstraintName("FK__ItemsDetails__invoiceNo__0BC6C43E")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(e => e.CompanyId)
                    .HasName("PK__UserAcco__AD5459907F60ED59");

                entity.Property(e => e.CompanyId)
                    .ValueGeneratedNever()
                    .HasColumnName("companyId");

                entity.Property(e => e.AddressLine)
                    .HasMaxLength(500)
                    .HasColumnName("addressLine");

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .HasColumnName("city");

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(100)
                    .HasColumnName("companyName");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("createdOn")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EmailId)
                    .HasMaxLength(100)
                    .HasColumnName("email_id");

                entity.Property(e => e.GstNo)
                    .HasMaxLength(20)
                    .HasColumnName("gstNo");

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .HasColumnName("phone");

                entity.Property(e => e.Pincode)
                    .HasMaxLength(30)
                    .HasColumnName("pincode");

                entity.Property(e => e.States)
                    .HasMaxLength(50)
                    .HasColumnName("states");
            });
            modelBuilder.Entity<ErrorLog>(entity =>
            {

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.methodName)
                   .HasMaxLength(200)
                   .HasColumnName("methodName");

                entity.Property(e => e.errorMsg)
                  .HasMaxLength(200)
                  .HasColumnName("errorMsg");

                entity.Property(e => e.methodName)
                  .HasColumnName("methodName");

                entity.Property(e => e.loggedDate)
                  .HasColumnType("datetime")
                  .HasColumnName("loggedDate")
                  .HasDefaultValueSql("(getdate())");

            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
