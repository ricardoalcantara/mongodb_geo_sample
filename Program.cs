using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace mongodb_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }

        static async Task Run()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("sampledb");
            var collection = database.GetCollection<Company>("companies");
            await collection.Indexes.CreateOneAsync(Builders<Company>.IndexKeys.Geo2DSphere(p => p.Point));

            await collection.DeleteManyAsync(Builders<Company>.Filter.Ne(f => f.Name, "Ricardo"));
            // var dtpPosition = GeoJson.Position(-22.9519265,-43.1851923);
            var dtpPosition = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(-22.95346821, -43.18583823));
            var dtpCompany = new Company
            {
                Name = "Dtp",
                Cnpj = "1234567890",
                Point = dtpPosition,
            };
            await collection.InsertOneAsync(dtpCompany);

            var hortifrutiPosition = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(-22.95248789, -43.18417835));
            var hortifrutiCompany = new Company
            {
                Name = "Hortifruti",
                Cnpj = "0987654321",
                Point = hortifrutiPosition
            };
            await collection.InsertOneAsync(hortifrutiCompany);

            var metroPosition = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(-22.95153513, -43.1840917));

            // Print result in meters
            Console.WriteLine(HaversineInKM(-22.95346821, -43.18583823, -22.95153513, -43.1840917) * 1000); // 279.925490653774
            Console.WriteLine(HaversineInKM(-22.95248789, -43.18417835, -22.95153513, -43.1840917) * 1000); // 106.432033267438


            // 78 returns Hortifruti
            // 250 retirm Dtprca
            var filter = Builders<Company>.Filter.NearSphere(f => f.Point, metroPosition, 250);

            var list = await collection.Find(filter)
                .ToListAsync();

            foreach (var companh in list)
            {
                Console.WriteLine(companh.Name);
            }
        }

        static double _eQuatorialEarthRadius = 6378.1370D;
        static double _d2r = (Math.PI / 180D);

        static private int HaversineInM(double lat1, double long1, double lat2, double long2)
        {
            return (int)(1000D * HaversineInKM(lat1, long1, lat2, long2));
        }

        static private double HaversineInKM(double lat1, double long1, double lat2, double long2)
        {
            double dlong = (long2 - long1) * _d2r;
            double dlat = (lat2 - lat1) * _d2r;
            double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(lat1 * _d2r) * Math.Cos(lat2 * _d2r) * Math.Pow(Math.Sin(dlong / 2D), 2D);
            double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
            double d = _eQuatorialEarthRadius * c;

            return d;
        }
    }

    [BsonIgnoreExtraElements]
    public class Company
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Cnpj { get; set; }
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Point { get; internal set; }
    }
}
