var builder = WebApplication.CreateBuilder(args);

// 1. Register Controllers & CORS
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 2. Configure HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

// Active-kan CORS
app.UseCors("AllowBlazor");

app.UseAuthorization();

// 3. Map Controllers (Routing ke RecommendationController)
app.MapControllers();

app.Run();