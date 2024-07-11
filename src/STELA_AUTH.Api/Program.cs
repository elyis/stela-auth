using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using STELA_AUTH.App.Service;
using STELA_AUTH.Core.IRepository;
using STELA_AUTH.Core.IService;
using STELA_AUTH.Infrastructure.Data;
using STELA_AUTH.Infrastructure.Repository;
using STELA_AUTH.Shared.Provider;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var authDbConnectionString = Environment.GetEnvironmentVariable("AUTH_DB_CONNECTION_STRING");
    var passwordHashKey = Environment.GetEnvironmentVariable("PASSWORD_HASH_KEY");
    var emailSenderName = Environment.GetEnvironmentVariable("EMAIL_SENDER_NAME");
    var emailSenderEmail = Environment.GetEnvironmentVariable("EMAIL_SENDER_EMAIL");
    var emailSmtpServer = Environment.GetEnvironmentVariable("EMAIL_SMTP_SERVER");
    var emailSmtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"));
    var emailSenderPassword = Environment.GetEnvironmentVariable("EMAIL_SENDER_PASSWORD");
    var jwtSecret = Environment.GetEnvironmentVariable("JWT_AUTH_SECRET");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_AUTH_ISSUER");
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUTH_AUDIENCE");

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
        options.UseNpgsql(authDbConnectionString);
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

    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IAccountService, AccountService>();

    services.AddScoped<IAccountRepository, AccountRepository>();
    services.AddScoped<IUnconfirmedAccountRepository, UnconfirmedAccountRepository>();
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

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "stela_api",
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