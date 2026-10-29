var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=customer_activity_analyzer}/{action=GetCustomerActivity}/{id?}"
);

app.MapGet("/weatherforecast", () =>
{
    // var forecast =  Enumerable.Range(1, 5).Select(index =>
    //     new WeatherForecast
    //     (
    //         DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
    //         Random.Shared.Next(-20, 55),
    //         summaries[Random.Shared.Next(summaries.Length)]
    //     ))
    //     .ToArray();
    return "Hello World!";
})
.WithName("GetWeatherForecast");

app.Run();

