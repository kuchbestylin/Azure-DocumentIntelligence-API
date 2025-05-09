using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Green.DocumentIntelligence.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Green.DocumentIntelligence.Services
{
    public class GreenIntelligenceService : IGreenIntelligenceService
    {
        private readonly DocumentAnalysisClient client;

        public GreenIntelligenceService(DocumentAnalysisClient client)
        {
            this.client = client;
        }

        public List<Grade12Result> ExtractGrade12Results(IFormFile file)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Grade12Result>> ExtractGrade12ResultsAsync(IFormFile file)
        {
            string tempFilePath = Path.GetTempFileName();

            // Copy the content of the uploaded file to the temporary file
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
                await file.CopyToAsync(stream);


            // Open the temporary file as a FileStream
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open))
            {
                try
                {
                    // Analyze the document
                    AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", fileStream);
                    AnalyzeResult analyzeResult = operation.Value;

                    // Delete the temporary file after use
                    File.Delete(tempFilePath);

                    string pattern = @"(?<=\n)(?<subject>[A-Za-z ]+)\s*\n\s*(?<level>Higher|Ordinary)\s*\n\s*(?<grade>[A-Ga-g1-9]\(\w+\))";

                    MatchCollection matches = Regex.Matches(analyzeResult.Content, pattern, RegexOptions.Multiline);

                    List<Grade12Result> results = new List<Grade12Result>();

                    foreach (Match match in matches)
                    {
                        string subject = match.Groups["subject"].Value.Trim();
                        string level = match.Groups["level"].Value.Trim();
                        string grade = match.Groups["grade"].Value.Trim();

                        results.Add(new Grade12Result(subject, level, grade));
                    }

                    return results;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        public string ScanNamibianNationalId(IFormFile file)
        {
            string tempFilePath = Path.GetTempFileName();

            // Copy the content of the uploaded file to the temporary file
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
                file.CopyToAsync(stream);


            // Open the temporary file as a FileStream
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open))
            {
                // Analyze the document and get value
                AnalyzeResult analyzeResult = client.AnalyzeDocument(WaitUntil.Completed, "prebuilt-document", fileStream).Value;

                // Delete the temporary file after use
                File.Delete(tempFilePath);

                return DocumentUtils.MatchIdWithSpaces(analyzeResult.Content).Value;
            }
        }

        public async Task<string> ScanNamibianNationalIdAsync(IFormFile file)
        {
            string tempFilePath = Path.GetTempFileName();

            // Copy the content of the uploaded file to the temporary file
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
                await file.CopyToAsync(stream);

            
            // Open the temporary file as a FileStream
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Open))
            {
                try
                {
                    // Analyze the document
                    AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", fileStream);
                    AnalyzeResult analyzeResult = operation.Value;

                    // Delete the temporary file after use
                    File.Delete(tempFilePath);

                    return DocumentUtils.MatchIdWithSpaces(analyzeResult.Content).Value;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ex.Message;
                }
            }
        }
    }
}
