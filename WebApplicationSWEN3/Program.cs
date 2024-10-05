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

            app.UseDefaultFiles(); // Add this line to serve index.html as the default file

            app.UseStaticFiles(); // Add this line to serve static files from wwwroot

            app.MapControllers();
            app.Use(async (context, next) =>
            {
                // If the request path is not for an API endpoint, redirect to index.html
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Request.Path = "/index.html";
                }

                await next();
            });

            app.Run();

        }
    }
}
