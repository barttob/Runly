using SQLite;

namespace Runly.Models
{
    //Zestaw danych dla bazy danych lokalizacji w treningu
    public class CurrentData
    {
        [PrimaryKey, AutoIncrement]

        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int TimeLasted { get; set; }
    }
}
