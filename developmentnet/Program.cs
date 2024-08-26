using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
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

app.MapGet("/country", (StringValues channel) => $"tag1 {channel[0]} &tag2 {channel[1]}");

app.MapPost("/todoaction/{id}", async (string id) =>
{
    return Results.Created($"/todoaction/{id}", id);
}).WithOpenApi(genoptions =>
{
    var parameter = genoptions.Parameters[0];
    parameter.Description = "Enter Input";
    return genoptions;
});

app.MapPost("/upload", async (IFormFile file) =>
{
    string filename = "upload/" + file.FileName;
    using var stream = File.OpenWrite(filename);
    await file.CopyToAsync(stream);
});

app.MapPost("/uploadcollection", async (IFormFileCollection collection) =>
{
    foreach (var file in collection)
    {
        string filename = "upload/" + file.FileName;
        using var stream = File.OpenWrite(filename);
        await file.CopyToAsync(stream);
    }
});

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
