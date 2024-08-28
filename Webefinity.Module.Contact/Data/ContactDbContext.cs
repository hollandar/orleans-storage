using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Module.Contact.Data
{
    public class ContactDbContext:DbContext
    {
        public ContactDbContext(DbContextOptions<ContactDbContext> options):base(options)
        {
            
        }

        public DbSet<ContactRecord> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ContactRecord>(contact =>
            {
                contact.HasKey(r => r.Id);
                contact.HasIndex(r => r.Created);
            });
        }
    }
}
