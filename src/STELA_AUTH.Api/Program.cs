using System.Text;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using STELA_AUTH.App.Service;
using STELA_AUTH.Core.Enums;
using STELA_AUTH.Core.IRepository;
using STELA_AUTH.Core.IService;
using STELA_AUTH.Infrastructure.Data;
using STELA_AUTH.Infrastructure.Repository;
using STELA_AUTH.Infrastructure.Service;
using STELA_AUTH.Shared.Provider;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);
ApplyMigrations(app);
InitDatabase(app.Services);

app.MapGet("/", () => $"Auth server work");

app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    DotEnv.Load();
    var authDbConnectionString = GetEnvVar("AUTH_DB_CONNECTION_STRING");
    var passwordHashKey = GetEnvVar("PASSWORD_HASH_KEY");

    var emailSenderName = GetEnvVar("EMAIL_SENDER_NAME");
    var emailSenderEmail = GetEnvVar("EMAIL_SENDER_EMAIL");
    var emailSmtpServer = GetEnvVar("EMAIL_SMTP_SERVER");
    var emailSmtpPort = int.Parse(GetEnvVar("EMAIL_SMTP_PORT"));
    var emailSenderPassword = GetEnvVar("EMAIL_SENDER_PASSWORD");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqQProfileImageQueueName = GetEnvVar("RABBITMQ_PROFILE_IMAGE_QUEUE_NAME");
    var rabbitMqUserName = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");
    var redisConnectionString = GetEnvVar("REDIS_CONNECTION_STRING");

    services.AddControllers(e =>
    {
        e.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
    });

    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience
        });

    services.AddAuthorization();

    ConfigureSwagger(services);

    services.AddDbContext<AuthDbContext>(options =>
    {
        options.UseNpgsql(authDbConnectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
    });

    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
    });

    services.AddSingleton<Hmac512Provider>(provider => new Hmac512Provider(passwordHashKey));
    services.AddSingleton<IEmailService, EmailService>(provider => new EmailService(
        emailSenderName,
        emailSenderEmail,
        emailSmtpServer,
        emailSmtpPort,
        emailSenderPassword
    ));
    services.AddSingleton<IJwtService, JwtService>(provider => new JwtService(
        jwtSecret,
        jwtIssuer,
        jwtAudience
    ));
    services.AddSingleton<RabbitMqUpdateProfileBackgroundService>(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            return new RabbitMqUpdateProfileBackgroundService(
                scopeFactory,
                rabbitMqHostname,
                rabbitMqQProfileImageQueueName,
                rabbitMqUserName,
                rabbitMqPassword
            );
        });

    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IAccountService, AccountService>();

    services.AddScoped<IAccountRepository, AccountRepository>();
    services.AddScoped<IUnconfirmedAccountRepository, UnconfirmedAccountRepository>();

    services.AddHostedService(provider => provider.GetRequiredService<RabbitMqUpdateProfileBackgroundService>());
}

async void InitDatabase(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
    var hmac512Provider = scope.ServiceProvider.GetRequiredService<Hmac512Provider>();
    var adminEmail = GetEnvVar("ADMIN_EMAIL");
    var adminPassword = GetEnvVar("ADMIN_PASSWORD");

    var admin = await context.GetByEmail(adminEmail);
    if (admin == null)
        await context.AddAsync("root", "root", adminEmail, hmac512Provider.Compute(adminPassword), AccountRole.Admin.ToString());
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}

void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AuthDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "stela_auth_api",
            Description = "Api",
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Bearer auth scheme",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();

        options.EnableAnnotations();
    });
}