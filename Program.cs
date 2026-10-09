var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// 1. Izinkan CORS untuk semua origin/Netlify/Mobile
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agrikultor Matenek API v1");
        c.RoutePrefix = string.Empty;
    });
}

// 2. Wajib: UseCors dipanggil SEBELUM MapControllers/UseAuthorization
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();