using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Estoque.Models;

namespace CRMAudax;

public partial class MyDbContext : DbContext
{ 
    public DbSet<TableCodeBar> CodeBars { get; set; }

    #region DB_FACTORY
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

            string conn = configuration.GetSection("DatabaseData")["MySQL"];
            optionsBuilder.UseMySql(conn, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.17-mysql"));            
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    #endregion

}
