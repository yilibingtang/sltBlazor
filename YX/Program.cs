using YX.Components;
using YX.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container (moved to extension for decoupling)
builder.Services.AddYXServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline (moved to extension for decoupling)
app.UseYXDefaults();

app.Run();
