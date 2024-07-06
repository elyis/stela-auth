using System.Text;
using dotenv.net;
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

var services = builder.Services;
DotEnv.Load(new DotEnvOptions(false, new List<string> { ".env" }, Encoding.UTF8, true, true, true, 4));

string authDbConnectionString = Environment.GetEnvironmentVariable("AUTH_DB_CONNECTION_STRING") ?? throw new Exception("AUTH_DB_CONNECTION_STRING is not set");
string passwordHashKey = Environment.GetEnvironmentVariable("PASSWORD_HASH_KEY") ?? throw new Exception("PASSWORD_HASH_KEY is not set");

string emailSenderName = Environment.GetEnvironmentVariable("EMAIL_SENDER_NAME") ?? throw new Exception("EMAIL_SENDER_NAME is not set");
string emailSenderEmail = Environment.GetEnvironmentVariable("EMAIL_SENDER_EMAIL") ?? throw new Exception("EMAIL_SENDER_EMAIL is not set");
string emailSmtpServer = Environment.GetEnvironmentVariable("EMAIL_SMTP_SERVER") ?? throw new Exception("EMAIL_SMTP_SERVER is not set");
int emailSmtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT") ?? throw new Exception("EMAIL_SMTP_PORT is not set"));
string emailSenderPassword = Environment.GetEnvironmentVariable("EMAIL_SENDER_PASSWORD") ?? throw new Exception("EMAIL_SENDER_PASSWORD is not set");

string jwtSecret = Environment.GetEnvironmentVariable("JWT_AUTH_SECRET") ?? throw new Exception("JWT_AUTH_SECRET is not set");
string jwtIssuer = Environment.GetEnvironmentVariable("JWT_AUTH_ISSUER") ?? throw new Exception("JWT_AUTH_ISSUER is not set");
string jwtAudience = Environment.GetEnvironmentVariable("JWT_AUTH_AUDIENCE") ?? throw new Exception("JWT_AUTH_AUDIENCE is not set");

services.AddControllers(e => {
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
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    });

services.AddAuthorization();
services.AddSwaggerGen(options => {
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

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}

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

app.Run();
