using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Green.DocumentIntelligence;
using Green.DocumentIntelligence.Models;
using Green.DocumentIntelligence.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
        policy.AllowAnyOrigin();
    });
});

builder.Services.AddGreenIntelligence(o =>
{
    o.Endpoint = "https://green-ocr-automations.cognitiveservices.azure.com/";
    o.Token = "2f7c881b55674e62bc7c9c12167120b4";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/", () => JsonSerializer.Serialize("Hello World"));

app.MapPost("/identitynumber", async (HttpContext httpContext, [FromServices] IGreenIntelligenceService greenIntelligence) =>
{
    try
    {
        var form = await httpContext.Request.ReadFormAsync();
        var file = form.Files["file"];

        if (file == null)
        {
            return Results.BadRequest("No file uploaded.");
        }

        var result = await greenIntelligence.ScanNamibianNationalIdAsync(file);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        // Handle exceptions
        Console.WriteLine($"An error occurred: {ex.Message}");
        return Results.BadRequest(JsonSerializer.Serialize(new { error = ex.Message }));
    }
})
.DisableAntiforgery()
.WithName("ExtractNationalId")
.WithOpenApi();

app.MapPost("/grade12results", async (HttpContext httpContext, [FromServices] IGreenIntelligenceService greenIntelligence) =>
{
    try
    {
        var form = await httpContext.Request.ReadFormAsync();
        var file = form.Files["file"];

        if (file == null)
        {
            return Results.BadRequest("No file uploaded.");
        }

        List<Grade12Result> result = await greenIntelligence.ExtractGrade12ResultsAsync(file);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        // Handle exceptions
        Console.WriteLine($"An error occurred: {ex.Message}");
        return Results.BadRequest(JsonSerializer.Serialize(new { error = ex.Message }));
    }
})
.DisableAntiforgery()
.WithName("ExtractGrade12Results")
.WithOpenApi();


app.Run();
