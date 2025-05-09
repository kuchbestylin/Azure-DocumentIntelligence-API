using Green.DocumentIntelligence.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.DocumentIntelligence.Services
{
    public interface IGreenIntelligenceService
    {
        string ScanNamibianNationalId(IFormFile file);
        Task<string> ScanNamibianNationalIdAsync(IFormFile file);

        List<Grade12Result> ExtractGrade12Results(IFormFile file);
        Task<List<Grade12Result>> ExtractGrade12ResultsAsync(IFormFile file);
    }
}
