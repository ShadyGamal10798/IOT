using IOT.TCPListner.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teltonika.Codec.Model;

namespace IOT.TCPListner.Data
{
    public class ListenerDbContext : DbContext
    {
        public ListenerDbContext(DbContextOptions<ListenerDbContext> options) :base(options)
        {
                
        }
        public DbSet<AVLData> AVLsData {  get; set; }
        public DbSet<CommandTransaction> CommandTransactions { get; set; }
    }
}
