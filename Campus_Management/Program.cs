using Campus_Management.Data;
using Campus_Management.Repositories;
using Campus_Management.Repositories.Interfaces;
using Campus_Management.Services;
using Campus_Management.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Entity Framework Core - SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// College Repository
builder.Services.AddScoped<ICollegeRepository, CollegeRepository>();

// College Service
builder.Services.AddScoped<ICollegeService, CollegeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();