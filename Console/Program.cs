using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

//set `<your-endpoint>` and `<your-key>` variables with the values from the Azure portal to create your `AzureKeyCredential` and `DocumentIntelligenceClient` instance
string endpoint = "https://ocrautomationapi.cognitiveservices.azure.com/";
string key = "0cd4b5afdd38486b8778e96799ec0b7f";
AzureKeyCredential credential = new AzureKeyCredential("0cd4b5afdd38486b8778e96799ec0b7f");
var client = new DocumentAnalysisClient(new Uri(endpoint), credential);

string filePath = "C:\\Users\\Administrator\\Desktop\\pdf1_grade12_merged.pdf";


AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", new FileStream(filePath, FileMode.Open));
AnalyzeResult result = operation.Value;

Dictionary<string, (string Level, string Grade)> resultTable = new Dictionary<string, (string Level, string Grade)>();
String idNumber = "";

if (result.Pages.Count() > 1)
{
    Console.WriteLine($"Your NationalID: {ExtractID(result.KeyValuePairs)}\n");


    foreach (var row in ExtractTable(result.Tables))
    {
        Console.WriteLine($"Key:\"{row.Key}\" \t Value:{row.Value} \n");
    }
}


if(result.Pages.Count() == 1)
{
    var resultKeywords = new List<string>() { "Ministry of Education", "Namibia Senior Secondary Certificate" };
    var identityKeywords = new List<string>() { "REPUBLIC OF NAMIBIA NATIONAL IDENTITY CARD" };
    if (result.Paragraphs.Any(paragraph => resultKeywords.Any(kw => kw.ToLower().Contains(paragraph.Content.ToLower()))))
    {
        foreach (var row in ExtractTable(result.Tables))
        {
            Console.WriteLine($"Key:\"{row.Key}\" \t Value:{row.Value} \n");
        }
    }
    if (result.Paragraphs.Any(paragraph => identityKeywords.Any(kw => kw.ToLower().Contains(paragraph.Content.ToLower()))))
    {
        Console.WriteLine($"Your NationalID: \"{ExtractIDFromParagraphs(result.Paragraphs)}\"\n");
    }
}


Dictionary<string, (string Level, string Grade)> ExtractTable(IReadOnlyList<DocumentTable> tables)
{
    var gradeResults = new Dictionary<string, (string Level, string Grade)>();
    var tableHeaderKeywords = new List<string>() { "Subject", "Level", "Grade" };
    foreach (var table in tables)
    {
        if(table.Cells.Any(c => c.Kind == "columnHeader" && tableHeaderKeywords.Any(thk => thk.ToLower() == c.Content.ToLower())))
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
                gradeResults.Add(row[0, 0], (row[0, 1], row[0, 2]));
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
    // Typical ID Format 123456 1234 1
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