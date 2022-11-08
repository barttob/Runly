using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace Runly.Models
{
    public class CurrentData
    {
        [PrimaryKey, AutoIncrement]

        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int TimeLasted { get; set; }
    }
}
