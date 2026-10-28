using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AmazonCopyDb.Data;

public class AmazonCopyDbContextFactory : IDesignTimeDbContextFactory<AmazonCopyDbContext>{

	public AmazonCopyDbContext CreateDbContext(string[] args){
		var cs = Environment.GetEnvironmentVariable("DB_CONNECTION")
			?? "Host=localhost;Port=5432;Database=amazon-copy;Username=postgres;Password=postgres";
		var options = new DbContextOptionsBuilder<AmazonCopyDbContext>().UseNpgsql(cs).Options;
		return new AmazonCopyDbContext(options);
	}

}
