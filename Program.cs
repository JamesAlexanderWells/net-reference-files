using ProjectManagerApi.Data;
using ProjectManagerApi.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

var options = new JsonSerializerOptions()
{
    ReferenceHandler = ReferenceHandler.IgnoreCycles
};

// Add services to the container.

builder.Services.AddCors(options =>
      {
          options.AddDefaultPolicy(
             builder =>
             {
                 builder.WithOrigins("http://localhost:4200")
                  .WithHeaders("Authorization");
             });
      });


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));

builder.Services.AddScoped<IIssueRepository, IssueRepository>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();



app.UseCors(builder => builder
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
