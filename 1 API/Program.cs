using _1_API.Mapper;
using _2_Domain;
using _3_Data;
using _3_Data.Contexts;
using _3_Data.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//dependency inyection

builder.Services.AddScoped<ICustomerData, CustomerData>();
builder.Services.AddScoped<ICustomerDomain, CustomerDomain>();


//automapper
builder.Services.AddAutoMapper(
    typeof(RequestToModels),
    typeof(ModelsToRequest),
    typeof(ModelsToResponse));

//Conexion a MySQL
var connectionString = builder.Configuration.GetConnectionString("makiConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));

builder.Services.AddDbContext<MakiContext>(
    dbContextOptions =>
    {
        dbContextOptions.UseMySql(connectionString,
            ServerVersion.AutoDetect(connectionString),
            options => options.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: System.TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)
        );
    });

var app = builder.Build();

//generar BD
using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetService<MakiContext>())
{
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();