using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PrescribingSystem.Data;
using PrescribingSystem.Interface;
using PrescribingSystem.Models;
using PrescribingSystem.Service;
using System.Globalization;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // =====================================================
        // 🌍 CULTURE SETTINGS
        // =====================================================
        var culture = new CultureInfo("en-ZA");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // =====================================================
        // 🔹 ACCESS HTTP CONTEXT
        // =====================================================
        builder.Services.AddHttpContextAccessor();

        // =====================================================
        // 🔹 EMAIL SERVICE (SMTP)
        // =====================================================
        var smtpSettings = new SmtpSettings();
        builder.Configuration.GetSection("SmtpSettings").Bind(smtpSettings);

        builder.Services.AddSingleton(smtpSettings);
        builder.Services.AddSingleton<IEmailSender, EmailServices>();

        // =====================================================
        // 🔹 DATABASE (SQL SERVER)
        // =====================================================
        var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(sqlConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // =====================================================
        // 🔹 HANGFIRE SETUP
        // =====================================================
        builder.Services.AddHangfire(config =>
            config.UseSqlServerStorage(sqlConnectionString));

        builder.Services.AddHangfireServer();

        // =====================================================
        // 🔹 ACCESS DB SERVICE (EXTERNAL DB)
        // =====================================================
        builder.Services.AddSingleton<AccessDbService>(provider =>
            new AccessDbService(builder.Configuration));

        // =====================================================
        // 🔹 APPLICATION SERVICES
        // =====================================================
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // =====================================================
        // 🔹 IDENTITY (AUTH + ROLES)
        // =====================================================
        builder.Services
            .AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // =====================================================
        // 🔹 MVC + RAZOR
        // =====================================================
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // =====================================================
        // BUILD APP
        // =====================================================
        var app = builder.Build();

        // =====================================================
        // 🔹 HANGFIRE DASHBOARD
        // =====================================================
        app.UseHangfireDashboard();

        // =====================================================
        // ⏱ AUTO SYNC JOB (CORRECT PLACE)
        // =====================================================
        RecurringJob.AddOrUpdate<SyncService>(
            "sync-copied-data",
            service => service.SyncData(),
            Cron.Minutely
        );

        // =====================================================
        // 🔹 LOCALIZATION
        // =====================================================
        var localizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-ZA"),
            SupportedCultures = new[] { culture },
            SupportedUICultures = new[] { culture }
        };

        app.UseRequestLocalization(localizationOptions);

        // =====================================================
        // 🔹 ROLE SEEDING
        // =====================================================
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[]
            {
                "CargoCoordinator",
                "Hatchman",
                "Manager",
                "Supervisor",
                "Operations"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // =====================================================
        // 🔹 MIDDLEWARE PIPELINE
        // =====================================================
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        // =====================================================
        // 🔹 ROUTING
        // =====================================================
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        app.Run();
    }
}