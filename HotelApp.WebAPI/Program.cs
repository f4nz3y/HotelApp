using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using HotelApp.Core.Factories;
using HotelApp.Core.Interfaces;
using HotelApp.Core.Services;
using HotelApp.DAL;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Repositories;
using HotelApp.WebAPI.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel API",
        Version = "v1",
        Description = "API для управління готелем"
    });
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
    builder.RegisterType<RoomService>().As<IRoomService>().InstancePerLifetimeScope();

    builder.RegisterType<StandardRoomFactory>()
        .Named<IRoomFactory>("standard")
        .InstancePerDependency();

    builder.RegisterType<DeluxeRoomFactory>()
        .Named<IRoomFactory>("deluxe")
        .InstancePerDependency();

    builder.Register<Func<string, IRoomFactory>>(c =>
    {
        var context = c.Resolve<IComponentContext>();
        return factoryType =>
        {
            return factoryType switch
            {
                "deluxe" => context.ResolveNamed<IRoomFactory>("deluxe"),
                _ => context.ResolveNamed<IRoomFactory>("standard")
            };
        };
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    var roomService = scope.ServiceProvider.GetRequiredService<IRoomService>();

    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }

    roomService.SeedData();
}

app.Run();