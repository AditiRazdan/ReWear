using LocalBakery.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

public partial class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite("Data Source=rewear.db")));
}
