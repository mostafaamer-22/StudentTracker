using Asp.Versioning;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using StudentTracker.Api.Configurations;
using StudentTracker.Api.Middleware;
using StudentTracker.Api.OpenApi;
using StudentTracker.Application;
using StudentTracker.Infrastructure;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddApplicationStrapping();
await builder.Services.AddInfrastructureStrapping(builder.Configuration);
builder.Services.AddAppServicesDIConfig();

builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();

builder.Services
    .AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen(c =>
{
    c.UseOneOfForPolymorphism();
    c.DescribeAllParametersInCamelCase();
});

builder.Services.AddHealthChecks();

builder.Services.AddCorsConfig(builder.Configuration);

builder.Services.AddRateLimitConfig();

var app = builder.Build();

app.UseExceptionHandler();

app.UseCors("CorsPolicy");

app.UseStaticFiles();

app.UseSwagger();

app.UseSwaggerUI();

app.UseRequestLocalization();

app.UseHsts();

app.UseHttpsRedirection();

app.UseMiddleware<RequestContextLoggingMiddleware>();

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<RateLimitingMiddleware>();

app.Run();