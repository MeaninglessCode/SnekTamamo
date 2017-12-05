﻿using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TamamoSharp.Database.Quotes
{
    public class QuoteDb : DbContext
    {
        public DbSet<Quote> Quotes { get; private set; }
        
        public QuoteDb() { Database.EnsureCreated(); }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            string dir = Path.Combine(AppContext.BaseDirectory, "data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string dataDir = Path.Combine(dir, "quotes.db");
            builder.UseSqlite($"Filename={dataDir}");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Quote>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.Name)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.Content)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.OwnerId)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.GuildId)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.CreatedAt)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.UpdatedAt)
                .IsRequired();
            builder.Entity<Quote>()
                .Property(x => x.Uses)
                .IsRequired();
        }

        public async Task AddQuoteAsync(Quote q)
        {
            await Quotes.AddAsync(q);
            await SaveChangesAsync();
        }

        public async Task DeleteQuoteAsync(Quote q)
        {
            Remove(q);
            await SaveChangesAsync();
        }

        public async Task AddUseAsync(Quote q)
        {
            q.Uses += 1;
            Update(q);
            await SaveChangesAsync();
        }

        public async Task<Quote> GetQuoteAsync(ulong guildId, string name)
            => await Quotes.FirstOrDefaultAsync(x => x.GuildId == guildId && x.Name == name);

        public async Task<Quote[]> GetQuotesAsync(ulong guildId)
            => await Quotes.Where(x => x.GuildId == guildId).ToArrayAsync();
    }
}
