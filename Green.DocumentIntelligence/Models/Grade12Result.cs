using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.DocumentIntelligence.Models
{
    public class Grade12Result
    {
        public Grade12Result(string subject, string level, string grade)
        {
            Subject = subject;
            Level = level;
            Grade = grade;
        }

        public string Subject { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
    }
}
