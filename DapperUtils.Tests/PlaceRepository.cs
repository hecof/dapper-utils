using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DapperUtils.Tests
{
    public class PlaceRepository
    {
        static PlaceRepository()
        {
            DapperUtils.SqlServerDapperUtils.UseMapping(new SqlMapping<Place>()
                .MapColumn("Location", "Location"));
        }

        private IDbConnection Connection
        {
            get 
            {
                var cnx = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString);
                cnx.Open();
                return cnx;
            }
        }
        
        public void Add(Place place)
        {
            using (var cnx = Connection)
            {
                cnx.Add(place);
            }
        }

        public void Update(Place place, IEnumerable<string> fields = null)
        {
            using (var cnx = Connection)
            {
                cnx.Update<Place>(place, fields);
            }
        }
    }
}
