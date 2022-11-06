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
        public int Time { get; set; }
        public double Distance { get; set; }
    }
}
