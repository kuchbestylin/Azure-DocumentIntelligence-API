using System;
using System.Collections.Generic;
using System.Text.Json;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OCR API", Version = "v1" });
});
builder.Services.AddAzureClients(options =>
{
    options.AddDocumentAnalysisClient(new Uri("https://ocrautomationapi.cognitiveservices.azure.com/"), 
        new AzureKeyCredential("0cd4b5afdd38486b8778e96799ec0b7f"));
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(o =>
    {
        o.AllowAnyMethod();
        o.AllowAnyHeader();
        o.AllowAnyOrigin();
    });
});
// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseCors();
app.MapGet("/", () => JsonSerializer.Serialize("Hello World"));
app.UseSwagger();
app.UseSwaggerUI(action =>
{
    action.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Intelligence V1");
});
// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
    // Create the folder if it does not exist
    Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "UploadedFiles")).FullName),
    RequestPath = "/fileupload"
});
app.MapPost("/", (IFormFile file) =>
{
    // Read the base64-encoded string from the request body
    //try
    //{
    //    byte[] bytes = Convert.FromBase64String(base64String);
    //    var keyValue = new List<KeyValuePair<string, string>>();
    //    using (MemoryStream stream = new MemoryStream(bytes))
    //    {
    //        using (FileStream fileStream = new FileStream("outputFile.txt", FileMode.Create))
    //        {
    //            stream.CopyTo(fileStream);
    //            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", fileStream);
    //            AnalyzeResult result = operation.Value;
    //            // Or save the stream to a file:
    //            foreach (var kv in result.KeyValuePairs)
    //            {
    //                keyValue.Add(new KeyValuePair<string, string>(kv.Key.Content, kv.Value.Content));
    //                Console.WriteLine($"{kv.Key.Content} : {kv.Value.Content}");
    //            }
    //        }
    //    }
    //}
    //catch (Exception ex)
    //{
    //    Console.WriteLine(ex);
    //}
    Console.WriteLine("File name is: " + file.Name);
    Results.Ok("Done Successfully");
});
app.MapPost("/iddocument", async (IFormFile file, DocumentAnalysisClient client) =>
{
    // Validate file and save it to disk
    if (file == null || file.Length <= 0)
    {
        return Results.BadRequest("No file found in the request.");
    }
    if (string.IsNullOrEmpty(Path.GetExtension(file.FileName)))
    {
        return Results.BadRequest("File is missing extention");
    }
    if (new List<string> { ".jpg", ".pdf", ".png", ".jpeg" }.Any(ext => ext == Path.GetExtension(file.FileName)))
    {
        return Results.BadRequest("File not Supported");
    }
    var keyValue = new List<KeyValuePair<string, string>>();
    using (var stream = file.OpenReadStream())
    {
        AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", stream);
        AnalyzeResult result = operation.Value;
        foreach (var kv in result.KeyValuePairs)
        {
            keyValue.Add(new KeyValuePair<string, string>(kv.Key.Content, kv.Value.Content));
        }
    }
    return Results.Created("", keyValue);
});

app.MapDefaultEndpoints();

app.Run();
