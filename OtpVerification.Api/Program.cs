using Microsoft.EntityFrameworkCore;
using OtpVerification.Api.Configuration;
using OtpVerification.Api.Data;
using OtpVerification.Api.Services;
using OtpVerification.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(nameof(SmtpSettings)));
builder.Services.Configure<OtpSettings>(builder.Configuration.GetSection(nameof(OtpSettings)));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedCorsPolicy", policy =>
    {
        policy.WithOrigins("https://scy-8080.entrostat.dev")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddDbContext<OtpDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmailingService, EmailingService>();
builder.Services.AddScoped<IOneTimePinService, OneTimePinService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("RestrictedCorsPolicy");
app.MapControllers();

app.Run();