using Microsoft.SqlServer.Types;

namespace DapperUtils.Tests
{
    public class Place
    {
        public Place()
        {
            Location = SqlGeography.Null;
        }

        public Place(string name, double latitude, double longitude)
        {
            Name = name;
            Location = SqlGeography.Point(latitude, longitude, 4326);
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public SqlGeography Location { get; set; }
    }
}
