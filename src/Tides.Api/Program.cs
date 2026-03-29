using Microsoft.EntityFrameworkCore;
using Tides.Api.Exceptions;
using Tides.Api.Hubs;
using Tides.Api.Services;
using Tides.Core.Services;
using Tides.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<TidesDbContext>(options =>
    options.UseNpgsql(dataSource));

builder.Services.AddScoped<IDrawGeneratorService, DrawGeneratorService>();
builder.Services.AddScoped<IPointsCalculatorService, PointsCalculatorService>();
builder.Services.AddScoped<ICarnivalService, CarnivalService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TidesDbContext>();
    db.Database.Migrate();
    SeedData.Seed(db);
}

app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var (statusCode, title) = exception switch
        {
            NotFoundException => (404, "Not Found"),
            InvalidOperationException => (400, "Bad Request"),
            ArgumentException => (400, "Validation Error"),
            _ => (500, "Internal Server Error")
        };
        context.Response.StatusCode = statusCode;
        var detail = exception?.InnerException?.Message ?? exception?.Message;
        await context.Response.WriteAsJsonAsync(new { status = statusCode, title, detail });
    });
});

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.MapHub<ResultsHub>("/hubs/results");

app.Run();
