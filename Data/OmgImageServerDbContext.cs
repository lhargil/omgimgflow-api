using API.Data.Configurations;
using API.Models;
using API.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace API.Data
{
    public class OmgImageServerDbContext: DbContext
    {
        private readonly IDateTimeManager _dateTimeManager;

        public OmgImageServerDbContext(DbContextOptions<OmgImageServerDbContext> options)
        : base(options)
        {
            _dateTimeManager = this.GetService<IDateTimeManager>(); ;
        }
        public DbSet<OmgImage> OmgImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OmgImageConfiguration());
            modelBuilder.ApplyConfiguration(new OmgImageTagConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Added ||
        e.State == EntityState.Modified ||
        e.State == EntityState.Deleted))
            {
                var today = _dateTimeManager.Today;

                entry.Property("DateModified").CurrentValue = today;
                if (entry.State == EntityState.Added)
                {
                    entry.Property("DateCreated").CurrentValue = today;
                }

                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("IsDeleted").CurrentValue = true;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
