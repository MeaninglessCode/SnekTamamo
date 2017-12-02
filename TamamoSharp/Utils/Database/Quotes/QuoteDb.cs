using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace TamamoSharp.Database.Quotes
{
    public class QuoteDb : DbContext
    {
        public DbSet<Quote> Quotes { get; private set; }
        
        public QuoteDb() { Database.EnsureCreated(); }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {

        }
    }
}
