
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

//DB 

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupabaseDb"))
);
// Add Email Service
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));



//JWT Authentication
var jwt =builder.Configuration.GetSection("Jwt"); 
builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme

).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // ValidIssuer = jwt["Issuer"],
        // ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Cookies["access_token"];

           context.Token = accessToken;
           
            return Task.CompletedTask;
        }
    };
  
});
//Swagger token jwt gen 
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Name = "access_token",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "cookieAuth",
        In = ParameterLocation.Cookie,
        Description = "Nhập Cookie Access: "
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "cookieAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});

//Set policy in middlerware
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("USER_VIEW", policy =>
        policy.RequireClaim("permission", "USER_VIEW"));

    options.AddPolicy("USER_CREATE", policy =>
        policy.RequireClaim("permission", "USER_CREATE"));

    options.AddPolicy("USER_DELETE", policy =>
        policy.RequireClaim("permission", "USER_DELETE"));
        
     options.AddPolicy("USER_UPDATE", policy =>
        policy.RequireClaim("permission", "USER_UPDATE"));
});


builder.Services.AddScoped<RefreshTokenServices>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IUserService, UserServices>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IPermissionServices,PermissionServices>();
builder.Services.AddScoped<IRoleServices,RoleServices>();
builder.Services.AddScoped<IProductServices,ProductService>();
builder.Services.AddScoped<ICartServices,CartService>();
builder.Services.AddScoped<IOrderService,OrderService>();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddAuthentication();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI();
    

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    
}


app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
