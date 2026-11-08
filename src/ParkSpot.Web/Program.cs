using ParkSpot.Web.Entities.Data;
using ParkSpot.Web.Repositories.Interfaces;
using ParkSpot.Web.Services.Interfaces;
using ParkSpot.Web.Entities.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ParkSpot.Web.Services;
using ParkSpot.Web.Middleware;
using ParkSpot.Web.Repositories;
using AuthenticationService = ParkSpot.Web.Services.AuthenticationService;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true).AddEnvironmentVariables();

builder.Services.AddScoped<MySqlConnection>(provider =>
{
    var connectionString = provider.GetRequiredService<IConfiguration>().GetConnectionString("ParkSpot")
            ?? throw new InvalidOperationException("Configure ConnectionStrings:ParkSpot.");

    return new MySqlConnection(connectionString);
});

builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IDbRepository, DbRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationReadRepository, ReservationReadRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IParkingRepository, ParkingRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IManagementService, ManagementService>();
builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddScoped<IReservationRules, ReservationRules>();
builder.Services.AddScoped<IInputValidator, InputValidator>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICurrentActor, CurrentActor>();

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values.SelectMany(value => value.Errors)
                        .Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message));

        return new BadRequestObjectResult(new ErrorResponse(string.Join(" ", errors)));
    };
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IPasswordHasher<Manager>, PasswordHasher<Manager>>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Driver", p => p.RequireRole("Driver"))
    .AddPolicy("Manager", p => p.RequireRole("Manager"));

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
