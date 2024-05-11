using Admin_Parqueo.Data;
using Admin_Parqueo.Servicios; // Asegúrate de incluir el namespace del servicio
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Agregar el servicio de parqueo
string connectionString = builder.Configuration.GetConnectionString("BaseParqueoConnection");
builder.Services.AddScoped<ServiceParqueo>(provider => new ServiceParqueo(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
