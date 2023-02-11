using SQLite;
using Xamarin.Essentials;

namespace Runly.Models
{
    public class PositionList
    {
        [PrimaryKey, AutoIncrement]

        public int Id { get; set; }
        public Location Location { get; set; } 
        //public double Latitude { get; set; }
        //public double Longitude { get; set; }

        public int TimeLasted { get; set; } 
    }
}
