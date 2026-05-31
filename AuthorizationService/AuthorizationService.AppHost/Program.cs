var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapHealthChecks("/api/auth/health/live");
app.MapHealthChecks("/api/auth/health/ready");
app.MapControllers();

app.Run();
