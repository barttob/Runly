using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Runly.Models
{
    public class TrainingData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string DateDay { get; set; }
        public string DateTime { get; set; }
        public string Time { get; set; }
        public string Distance { get; set; }
        public double Calories { get; set; }
        public double AvrSpeed { get; set; }
        public string TrainingDatabase { get; set; }
    }
}
