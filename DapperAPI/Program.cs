using AutoMapper;
using DapperAPI.Data;
using DapperAPI.Interface;
using DapperAPI.Mapper;
using DapperAPI.Repository;
using DapperAPI.Setting;
using ServiceStack;
using DapperAPI.EntityModel;
using System.Text.Json;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using DapperAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Threading;


var builder = WebApplication.CreateBuilder(args);


// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options
    => options.SuppressModelStateInvalidFilter = true);
// Add services to the container.
builder.Services.Configure<SqlConnectionSetting>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped(typeof(IMasterRepository<>), typeof(MasterRepository<>));
builder.Services.AddScoped(typeof(IMasterDetailRepository<OM_ITEM,OM_ITEM_UOM>), typeof(MasterDetailRepository<OM_ITEM,OM_ITEM_UOM>));
builder.Services.AddScoped(typeof(IMasterDetailRepository<WT_STK_OUT_HEAD, WT_STK_OUT_ITEM>), typeof(MasterDetailRepository<WT_STK_OUT_HEAD, WT_STK_OUT_ITEM>));
builder.Services.AddScoped(typeof(IMasterDetailRepository<WT_STK_OUT_ITEM, WT_STK_OUT_ITEM>), typeof(MasterDetailRepository<WT_STK_OUT_ITEM, WT_STK_OUT_ITEM>));
builder.Services.AddScoped(typeof(IMasterDetailRepository<OM_ITEM_UOM, OM_ITEM_UOM>), typeof(MasterDetailRepository<OM_ITEM_UOM, OM_ITEM_UOM>));
builder.Services.AddScoped<IDbConnectionProvider, DbConnectionProvider>();
builder.Services.AddScoped<IUserValidationService, UserValidationService>();



// Add services to the container.
builder.Services.Configure<AppSettings>(builder.Configuration);



builder.Services.AddCors(options =>
{
options.AddPolicy("AllowAll", builder =>
    builder.WithOrigins("*")
           .AllowAnyMethod()
           .AllowAnyHeader());

});


builder.Services.AddAuthentication(cfg => {
cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => {
x.RequireHttpsMetadata = false;
x.SaveToken = false;

x.TokenValidationParameters = new TokenValidationParameters
{

ValidateIssuerSigningKey = true,
IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JWT").GetSection("Key").Value)
    ),
ValidateIssuer = false,
ValidateAudience = false,
ValidateLifetime = true,
ClockSkew = TimeSpan.Zero
};
});

builder.Services.AddAuthorization();


builder.Services.AddControllers()
        .AddJsonOptions(options =>
{
options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});


var mapperConfiguration = new MapperConfiguration(cfg =>
{
cfg.AddProfile(typeof(MappingProfile));
});

var mapper = mapperConfiguration.CreateMapper();
builder.Services.AddSingleton(mapper);



var app = builder.Build();




app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<JwtMiddleware>();


app.UseCors("AllowAll");

try
{
    Log.Information("Starting up");
    app.Run();
}

catch(Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

