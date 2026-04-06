using Microsoft.Data.Sqlite;
using Web.Data;
using Web.Repositories;
using Web.Services;
namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = "Data Source=bikesharing.db";

            // Služby
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton(connectionString);
            builder.Services.AddSingleton(new DatabaseInitializer(connectionString));
            builder.Services.AddScoped<StationRepository>();
            builder.Services.AddScoped<BikeRepository>();
            builder.Services.AddScoped<RentalRepository>();
            builder.Services.AddScoped<BikeStatusHistoryRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddDistributedMemoryCache();// cache na drzeni sesion
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddSingleton<TokenService>();


            var app = builder.Build();

            // Inicializace DB při startu
            app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

            // Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
