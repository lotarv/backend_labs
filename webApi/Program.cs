// создается билдер веб приложения

using Backend.BLL.Services;
using Backend.DAL.Interfaces;
using Backend.DAL.Repositories;
using Backend.Validators;
using Dapper;
using FluentValidation;
using System.Text.Json;
using WebApi.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

DefaultTypeMap.MatchNamesWithUnderscores = true;
builder.Services.AddScoped<UnitOfWork>();

// зависимость, которая автоматически подхватывает все контроллеры в проекте

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));
builder.Services.Configure<WebApi.Config.RabbitMqSettings>(
    builder.Configuration.GetSection(nameof(WebApi.Config.RabbitMqSettings)));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IAuditLogOrderRepository, AuditLogOrderRepository>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AuditLogOrderService>();
builder.Services.AddScoped<RabbitMqService>();
builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));
builder.Services.AddScoped<ValidatorFactory>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});
// добавляем swagger
builder.Services.AddSwaggerGen();

// собираем билдер в приложение
var app = builder.Build();

// добавляем 2 миддлвари для обработки запросов в сваггер
app.UseSwagger();
app.UseSwaggerUI();

// добавляем миддлварю для роутинга в нужный контроллер
app.MapControllers();

// вместо *** должен быть путь к проекту Migrations
// по сути в этот момент будет происходить накатка миграций на базу

Migrations.Program.Main([]); 

// запускам приложение
app.Run();
