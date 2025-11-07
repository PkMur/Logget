using System.Text;
using LogGet.Models;
using LogGet.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LogGet;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    builder.Services.AddSingleton<IUserService, InMemoryUserService>();
    // In-memory services for listing users and drivers used by Consultas
    // Persist cadastros to MongoDB-backed services
    builder.Services.AddSingleton<LogGet.Services.IUsuarioService, LogGet.Services.MongoUsuarioService>();
    builder.Services.AddSingleton<LogGet.Services.IMotoristaService, LogGet.Services.MongoMotoristaService>();
        // Configure Mongo settings and register services.
        builder.Services.Configure<LogGet.Services.MongoEntregaService.MongoSettings>(
            builder.Configuration.GetSection("Mongo"));

        // If a Mongo connection string is provided, use Mongo-backed services; otherwise fall back to in-memory implementations.
        var mongoSettings = builder.Configuration.GetSection("Mongo").Get<LogGet.Services.MongoEntregaService.MongoSettings>();
        if (!string.IsNullOrWhiteSpace(mongoSettings?.ConnectionString))
        {
            builder.Services.AddSingleton<LogGet.Services.IEntregaService, LogGet.Services.MongoEntregaService>();
            builder.Services.AddSingleton<LogGet.Services.IUsuarioService, LogGet.Services.MongoUsuarioService>();
            builder.Services.AddSingleton<LogGet.Services.IMotoristaService, LogGet.Services.MongoMotoristaService>();
        }
        else
        {
            // Development fallback: persist in-memory
            builder.Services.AddSingleton<LogGet.Services.IEntregaService, LogGet.Services.InMemoryEntregaService>();
            builder.Services.AddSingleton<LogGet.Services.IUsuarioService, LogGet.Services.InMemoryUsuarioService>();
            builder.Services.AddSingleton<LogGet.Services.IMotoristaService, LogGet.Services.InMemoryMotoristaService>();
        }

        builder.Services.AddControllersWithViews();

        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                          ?? throw new InvalidOperationException("Jwt settings are not configured.");

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("access_token", out var token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        if (!context.Response.HasStarted)
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/Account/Login");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
