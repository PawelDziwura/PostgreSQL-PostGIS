using Postgres.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Postgres
{
    class Program
    {  
        static void Main(string[] args)
        {
            using (NpgsqlConnection connection = 
                new NpgsqlConnection { ConnectionString = "host=localhost;port=5432;database=postgres;user id=postgres;password=postgres" }) 
            {
                GeneratePoints(connection,10);
                PointInVoivodeship(connection);
                DistanceTest(connection);
                Console.Read();
            }
        }

        static void GeneratePoints(NpgsqlConnection connection, int pointCount)
        {
            Random random = new Random();
            connection.Open();
            string cmd = $"SELECT ST_AsText(ST_GeneratePoints( geom , {pointCount} )) from polska;";
            NpgsqlCommand command = new NpgsqlCommand(cmd, connection);
            string multipoint = command.ExecuteScalar() as string;
            connection.Close();

            List<Point> points = multipoint.Replace("MULTIPOINT", "").Replace("(", "").Replace(")", "").Split(',')
                .Select(p => p.Split(' '))
                .Select(p => new Point { X = p[0], Y = p[1], Z = (random.NextDouble()*100).ToString().Replace(",", ".") }).ToList();

            cmd = string.Empty;
            foreach(Point point in points )
            {
                cmd += $"INSERT INTO points (x,y,z) Values ({point.X}, {point.Y}, {point.Z});";
            }
            connection.Open();
            command = new NpgsqlCommand(cmd, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        static void PointInVoivodeship(NpgsqlConnection connection)
        {
            connection.Open();
            string cmd =
                "SELECT wojewodztwa.jpt_nazwa_, count(points.point) " +
                "FROM wojewodztwa " +
                "JOIN " +
                    "(SELECT ST_SetSRID(ST_Point(x, y), 2180) point " +
                    "FROM points) points " +
                "ON (ST_Contains(ST_SetSRID(wojewodztwa.geom, 2180), points.point)) " +
                "GROUP BY wojewodztwa.jpt_nazwa_;";
            NpgsqlCommand command = new NpgsqlCommand(cmd, connection);
            NpgsqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader.GetString(0)}: {reader.GetInt32(1)}");
            }
            connection.Close();
        }

        static void DistanceTest (NpgsqlConnection connection)
        {
            connection.Open();
            string cmd = "SELECT id1, min(d.distance/1000) " +
                "FROM (select ST_Distance(" +
                    "ST_SetSRID(ST_Point(p1.y, p1.x),4326)::geography, ST_SetSRID(ST_Point(p2.y, p2.x),4326)::geography) " +
                "distance, p1.id id1, p2.id id2 " +
                "FROM points p1 join points p2 on (p1.id != p2.id)) d " +
                "GROUP BY id1 " +
                "HAVING min(d.distance/1000) < 30;";
            NpgsqlCommand command = new NpgsqlCommand(cmd, connection);
            NpgsqlDataReader reader = command.ExecuteReader();
            List<double> test = new List<double>();
            while (reader.Read())
            {
                test.Add(reader.GetDouble(1));
            }
            if (test.Any())
                Console.WriteLine("Test failed");
            connection.Close();
        }
    }
}
