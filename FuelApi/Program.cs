using FuelApi.DataStores;
using FuelApi.Models;
using FuelApi.Services;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FuelService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapGet("/api/fuel", ([FromServices] FuelService service, DateTime timestamp) =>
{
    var liters = service.GetFuelFromDate(timestamp);
    return liters;
})
.WithName("Get")
.WithDescription("دریافت میزان سوخت در لحظه مشخص");
app.MapControllers();

DataStore.LoadDataFromExcel();


app.Run();
