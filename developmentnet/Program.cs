using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Channels;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(o=>o.AddFixedWindowLimiter(policyName: "ratepolicy", options =>
{
    options.QueueLimit = 1;
    options.PermitLimit = 1;
    options.Window = TimeSpan.FromSeconds(5);
    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
}).RejectionStatusCode=401);

var app = builder.Build();

app.MapGet("/basicget", (string channel) => "Welcome to " + channel).RequireRateLimiting("ratepolicy");

app.MapGet("/country", (string[] channel) => $"tag1 {channel[0]} &tag2 {channel[1]}");

app.UseRateLimiter();

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

//test
