using Microsoft.EntityFrameworkCore;
using sws.Models;


namespace WebApplicationSWEN3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();


            // Add services to the container.
            builder.Services.AddControllers();

            // Configure in-memory database
            builder.Services.AddDbContext<DocumentContext>(options =>
                options.UseInMemoryDatabase("MyInMemoryDatabase"));

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }
    }
}
