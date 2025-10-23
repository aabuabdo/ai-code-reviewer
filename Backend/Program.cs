using Microsoft.OpenApi.Models;
using Backend.Services; // ✅ Import your AI service namespace
using System.Net;
using DotNetEnv;

Env.Load();
// 🔒 إجبار .NET يستخدم TLS 1.2 و 1.3
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

ServicePointManager.ServerCertificateValidationCallback +=
    (sender, cert, chain, sslPolicyErrors) => true;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ✅ Register your AI review service (with HttpClient)
builder.Services.AddSingleton<AiReviewService>();


// ✅ Enable CORS (allow all for development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ✅ Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Code Reviewer API",
        Version = "v1",
        Description = "A simple API that reviews code snippets using AI."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Code Reviewer API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// ✅ Enable the CORS policy
app.UseCors("AllowAll");

// ✅ Map controllers (like ReviewController)
app.MapControllers();

app.Run();
