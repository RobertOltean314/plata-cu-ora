using PlataCuOra.Server.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace PlataCuOra.Server.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions options) : base(options)
		{
			
		}
		public DbSet<User> Users { get; set; }
	}
}
