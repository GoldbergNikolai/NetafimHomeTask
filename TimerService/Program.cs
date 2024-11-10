using Microsoft.EntityFrameworkCore;
using TimerService;
using TimerService.Data;
using TimerService.Services.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.Configurations();
services.AddDbContext<TimerDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
services.AddHostedService<TimerExpirationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

//Create Db in not exists
using (var serviceScope = app.Services.CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<TimerDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();


