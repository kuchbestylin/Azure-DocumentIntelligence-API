using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.AspNetCore.Cors.Infrastructure;
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
builder.Services.AddAzureClients(options =>
{
    options.AddDocumentAnalysisClient(new Uri("https://ocrautomationapi.cognitiveservices.azure.com/"),
        new AzureKeyCredential("0cd4b5afdd38486b8778e96799ec0b7f"));
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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/", () => JsonSerializer.Serialize("Hello World"));

app.MapPost("/", async (IFormFile file, DocumentAnalysisClient client) =>
{
    try
    {
        // Create a temporary file path
        string tempFilePath = Path.GetTempFileName();
        List<Result> results = new List<Result>();
        String idNumber = "";
        // Copy the content of the uploaded file to the temporary file
        using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var keyValue = new List<KeyValuePair<string, string>>();
        // Open the temporary file as a FileStream
        using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open))
        {
            // Analyze the document
            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", fileStream);
            AnalyzeResult result = operation.Value;


            if (result.Pages.Count() > 1)
            {
                idNumber = ExtractIDFromParagraphs(result.Paragraphs);
                results = ExtractTable(result.Tables);
            }


            if (result.Pages.Count() == 1)
            {
                var resultKeywords = new List<string>() { "Ministry of Education", "Namibia Senior Secondary Certificate" };
                var identityKeywords = new List<string>() { "REPUBLIC OF NAMIBIA NATIONAL IDENTITY CARD" };
                if (result.Paragraphs.Any(paragraph => resultKeywords.Any(kw => kw.ToLower().Contains(paragraph.Content.ToLower()))))
                {
                    results = ExtractTable(result.Tables);
                }
                if (result.Paragraphs.Any(paragraph => identityKeywords.Any(kw => kw.ToLower().Contains(paragraph.Content.ToLower()))))
                {
                    idNumber = ExtractIDFromParagraphs(result.Paragraphs);
                }
            }
        }

        // Delete the temporary file after use
        File.Delete(tempFilePath);
        List<Object> data = new List<Object>()
        {
            new {id = idNumber, results = results}
        };
        return Results.Created("", JsonSerializer.Serialize(new IDAndResult(idNumber, results)));
    }
    catch (Exception ex)
    {
        // Handle exceptions
        Console.WriteLine($"An error occurred: {ex.Message}");
        return Results.BadRequest(JsonSerializer.Serialize(ex.Message));
    }
})
.DisableAntiforgery()
.WithName("PostExample")
.WithOpenApi();


app.Run();



List<Result> ExtractTable(IReadOnlyList<DocumentTable> tables)
{
    var gradeResults = new List<Result>();
    var tableHeaderKeywords = new List<string>() { "Subject", "Level", "Grade" };
    foreach (var table in tables)
    {
        if (table.Cells.Any(c => c.Kind == "columnHeader" && tableHeaderKeywords.Any(thk => thk.ToLower() == c.Content.ToLower())))
        {
            string[,] matrix = new string[table.RowCount, table.ColumnCount];

            foreach (DocumentTableCell cell in table.Cells)
            {
                if (cell.RowIndex < table.RowCount && cell.ColumnIndex < table.ColumnCount)
                {
                    matrix[cell.RowIndex, cell.ColumnIndex] = cell.Content;
                }
            }
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                var row = new string[1, 3];
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    row[0, y] = matrix[x, y];
                }
                gradeResults.Add(new Result(row[0, 0], row[0, 1], row[0, 2]));
            }
        }
    }
    return gradeResults;
}
String ExtractIDFromParagraphs(IReadOnlyList<DocumentParagraph> paragraph)
{
    String nationalID = "";
    foreach (DocumentParagraph para in paragraph)
    {
        if (para.Content != null)
        {
            var value = ExtractNationalId(para.Content);
            if (IsValidNamibianID(value))
                nationalID = value;
        }
    }
    return nationalID;
}
String ExtractID(IReadOnlyList<DocumentKeyValuePair> keyValuePairs)
{
    String nationalID = "";
    foreach (DocumentKeyValuePair kvp in keyValuePairs)
    {
        if (kvp.Value != null)
        {
            var value = ExtractNationalId(kvp.Value.Content);
            if (IsValidNamibianID(value))
                nationalID = value;
        }
    }
    return nationalID;
}

static string ExtractNationalId(string input)
{
    // Define the regular expression pattern
    string pattern = @"(\d{6} \d{4} \d)";

    // Search for the pattern in the input string
    Match match = Regex.Match(input, pattern);

    if (match.Success)
    {
        // Extract the matched national ID
        return match.Groups[1].Value;
    }
    else
    {
        return "";
    }
}

static bool IsValidNamibianID(string id)
{
    id = ExtractNationalId(id);
    // Define a regular expression pattern for validating Namibian IDs
    string pattern = @".*\d{6} \d{4} \d.*$";

    // Check if the input string matches the pattern
    if (Regex.IsMatch(id, pattern))
    {
        // Further validation of the digits
        string[] parts = id.Split(' ');

        // Check if the first two parts contain only digits
        if (parts.Length >= 3 &&
            IsDigitsOnly(parts[0]) &&
            IsDigitsOnly(parts[1]) &&
            IsDigitsOnly(parts[2]))
        {
            return true;
        }
    }

    return false;
}

// Helper function to check if a string contains only digits
static bool IsDigitsOnly(string str)
{
    foreach (char c in str)
    {
        if (!char.IsDigit(c))
        {
            return false;
        }
    }
    return true;
}
public class Result
{
    public Result(string subject, string level, string grade)
    {
        Subject = subject;
        Level = level;
        Grade = grade;
    }

    public string Subject { get; set; }
    public string Level { get; set; }
    public string Grade { get; set; }
}

record IDAndResult(string id, List<Result> results);