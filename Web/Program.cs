using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using Web.Data;
using Web.Repositories;
using Web.Services;
namespace Web
{
    public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }

        public override DateTime Parse(object value)
        {
            // 1. Zkontrolujeme, jestli nám SQLite neposlala datum jako text (String)
            if (value is string stringValue)
            {
                // Přeložíme text na C# datum
                var parsedDate = DateTime.Parse(stringValue);
                // Až teď mu řekneme, že to bylo UTC a chceme lokální čas
                return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc).ToLocalTime();
            }

            // 2. Pro jistotu: Pokud by to náhodou už jako datum přišlo
            if (value is DateTime dt)
            {
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
            }

            // 3. Poslední záchrana, pokud přijde něco úplně divného
            return Convert.ToDateTime(value);
        }
    }
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
            SqlMapper.AddTypeHandler(new DateTimeHandler());

            var app = builder.Build();

            // Inicializace DB při startu
            app.Services.GetRequiredService<DatabaseInitializer>().Initialize();

            // Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            
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
