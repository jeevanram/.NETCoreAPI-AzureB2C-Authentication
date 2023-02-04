using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configBuilder = new ConfigurationBuilder()
           .SetBasePath(builder.Environment.ContentRootPath)
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
           .AddEnvironmentVariables();

var configuration = configBuilder.Build();

// Adds Microsoft Identity platform (Azure AD B2C) support to protect this Api
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.Audience = configuration["AzureAdB2C:ClientId"];
            o.Authority = $"https://login.microsoftonline.com/{configuration["AzureAdB2C:TenantId"]}/v2.0";
        });
// End of the Microsoft Identity platform block    

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            Implicit = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{configuration["AzureAdB2C:TenantId"]}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{configuration["AzureAdB2C:TenantId"]}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string> {
                    {
                        $"https://{configuration["AzureAdB2C:Domain"]}/{configuration["AzureAdB2C:ClientId"]}/access_as_user","Access POCService"
                    }
                }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { $"https://{configuration["AzureAdB2C:Domain"]}/{configuration["AzureAdB2C:ClientId"]}/access_as_user" }
                    }
    });
    
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(configuration["AzureAdB2C:ClientId"]);
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


