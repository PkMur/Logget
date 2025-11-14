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
    // Serviços em memória para listagem de usuários e motoristas usados nas consultas
    // Persistir cadastros usando serviços com backend MongoDB
        // Configurar opções do Mongo e registrar serviços.
        builder.Services.Configure<LogGet.Services.MongoEntregaService.MongoSettings>(
            builder.Configuration.GetSection("Mongo"));

    // Se uma connection string do Mongo for fornecida, tentar inicializar serviços com backend MongoDB.
    // Se a inicialização falhar (rede/TLS/autenticação), usar serviços em memória para manter a aplicação utilizável em desenvolvimento.
        var mongoSettings = builder.Configuration.GetSection("Mongo").Get<LogGet.Services.MongoEntregaService.MongoSettings>();
        var useMongo = false;
        if (!string.IsNullOrWhiteSpace(mongoSettings?.ConnectionString))
        {
            try
            {
                // Fazer uma conexão leve para verificar disponibilidade e credenciais
                var testClient = new MongoDB.Driver.MongoClient(mongoSettings.ConnectionString);
                // Enviar ping ao servidor (timeout curto)
                var database = testClient.GetDatabase(mongoSettings.DatabaseName);
                var command = new MongoDB.Bson.BsonDocument("ping", 1);
                database.RunCommand<MongoDB.Bson.BsonDocument>(command);

                useMongo = true;
            }
            catch (Exception ex)
            {
                // Não propagar exceção — usar fallback em memória e registrar o problema para que o desenvolvedor saiba o motivo
                // Aviso: falha ao conectar ao MongoDB. Usando serviços em memória.
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Aviso: falha ao inicializar conexão MongoDB. Revertendo para armazenamento em memória. Erro: {ex.Message}");
                useMongo = false;
            }
        }

        if (useMongo)
        {
            builder.Services.AddSingleton<LogGet.Services.IEntregaService, LogGet.Services.MongoEntregaService>();
            builder.Services.AddSingleton<LogGet.Services.IUsuarioService, LogGet.Services.MongoUsuarioService>();
            builder.Services.AddSingleton<LogGet.Services.IMotoristaService, LogGet.Services.MongoMotoristaService>();
        }
        else
        {
            // Fallback para desenvolvimento: persistir em memória
            builder.Services.AddSingleton<LogGet.Services.IEntregaService, LogGet.Services.InMemoryEntregaService>();
            builder.Services.AddSingleton<LogGet.Services.IUsuarioService, LogGet.Services.InMemoryUsuarioService>();
            builder.Services.AddSingleton<LogGet.Services.IMotoristaService, LogGet.Services.InMemoryMotoristaService>();
        }

    // O serviço de autenticação deve validar credenciais usando o IUsuarioService configurado
        builder.Services.AddSingleton<IUserService, LogGet.Services.UserAuthenticationService>();

        builder.Services.AddControllersWithViews();

    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                      ?? throw new InvalidOperationException("Configurações JWT não foram definidas.");        builder.Services.AddAuthentication(options =>
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

        // Seed inicial: garantir usuário administrador quando usando Mongo
        if (useMongo)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var usuarios = scope.ServiceProvider.GetRequiredService<IUsuarioService>();
                var anyUser = usuarios.ListAll().Any();
                if (!anyUser)
                {
                    usuarios.Add(new UsuarioViewModel
                    {
                        Nome = "Administrador",
                        CPF = "00000000000",
                        RG = string.Empty,
                        Email = "admin@local",
                        Login = "admin",
                        Senha = "123456",
                        Telefone = string.Empty,
                        IsActive = true
                    });
                    Console.WriteLine("Seed: usuário admin (login: admin / senha: 123456) criado no Mongo.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seed: falha ao garantir usuário admin no Mongo: {ex.Message}");
            }

            // Migração: normalizar NumeroPedido para apenas 4 dígitos (ex.: 0001)
            try
            {
                var mongoSettings2 = builder.Configuration.GetSection("Mongo").Get<LogGet.Services.MongoEntregaService.MongoSettings>();
                if (!string.IsNullOrWhiteSpace(mongoSettings2?.ConnectionString))
                {
                    var client = new MongoDB.Driver.MongoClient(mongoSettings2.ConnectionString);
                    var db = client.GetDatabase(mongoSettings2.DatabaseName);
                    var coll = db.GetCollection<MongoDB.Bson.BsonDocument>("entregas");

                    var cursor = coll.FindSync<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument(), options: null);
                    var docs = new System.Collections.Generic.List<MongoDB.Bson.BsonDocument>();
                    while (cursor.MoveNext())
                    {
                        docs.AddRange(cursor.Current);
                    }

                    var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var d in docs)
                    {
                        var cur = d.Contains("NumeroPedido") ? d["NumeroPedido"].ToString() : string.Empty;
                        if (!string.IsNullOrEmpty(cur)) used.Add(cur);
                    }

                    int nextCounter = 1;
                    string NextFree()
                    {
                        while (true)
                        {
                            var candidate = nextCounter.ToString("0000");
                            nextCounter++;
                            if (!used.Contains(candidate)) return candidate;
                        }
                    }

                    int updated = 0;
                    foreach (var d in docs)
                    {
                        var id = d["_id"];
                        var cur = d.Contains("NumeroPedido") ? d["NumeroPedido"].ToString() : string.Empty;
                        if (string.IsNullOrWhiteSpace(cur)) continue;

                        // Já está no formato 4 dígitos?
                        if (System.Text.RegularExpressions.Regex.IsMatch(cur, "^\\d{4}$"))
                        {
                            continue;
                        }

                        var digits = new string(cur.Where(char.IsDigit).ToArray());
                        if (string.IsNullOrWhiteSpace(digits)) continue;

                        if (!int.TryParse(digits, out var num))
                        {
                            continue;
                        }

                        var target = num.ToString("0000");
                        if (used.Contains(target))
                        {
                            target = NextFree();
                        }

                        if (!string.Equals(cur, target, StringComparison.Ordinal))
                        {
                            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("_id", id);
                            var update = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Update.Set("NumeroPedido", target);
                            coll.UpdateOne(filter, update);

                            used.Add(target);
                            updated++;
                        }
                    }

                    if (updated > 0)
                    {
                        Console.WriteLine($"Migração: normalizados {updated} NumeroPedido(s) para o formato 4 dígitos.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migração: falha ao normalizar NumeroPedido: {ex.Message}");
            }
        }

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
