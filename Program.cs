using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Npgsql;

class Program
{
    public static void Main(string[] args)  => WebHost.CreateDefaultBuilder(args)
        .Configure(app =>
        {
            app.UseRouting();

            string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            app.UseEndpoints(e =>
            {
                e.MapGet("/", context =>
                {
                    NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                    var titles = new List<string>();

                    try
                    {
                        Console.WriteLine("Connecting to PostgreSql...");
                        connection.Open();

                        string sql = "SELECT userCod FROM public.\"devOps\"";
                        using var cmd = new NpgsqlCommand(sql, connection);
                        using var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            titles.Add(reader.GetString(0));
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        context.Response.StatusCode = 500;
                        return Task.CompletedTask;
                    }
                    connection.Close();

                    context.Response.StatusCode = 200;

                    foreach (var i in titles)
                        context.Response.WriteAsync(i + "\n");

                    return Task.CompletedTask;
                });
            });
            Prepare(connectionString);

        }).Build().Run();

        private static void Prepare(string connectionString)
        {
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);

            connection.Open();
            using var transation = connection.BeginTransaction();

            using NpgsqlCommand cmd1 = new NpgsqlCommand("DROP TABLE IF EXISTS public.\"devOps\"", connection, transation);
            cmd1.ExecuteNonQuery();

            using NpgsqlCommand cmd2 = new NpgsqlCommand("DROP SEQUENCE IF EXISTS public.\"devOps_id_seq\"", connection, transation);
            cmd2.ExecuteNonQuery();

            using NpgsqlCommand cmd3 = new NpgsqlCommand("CREATE SEQUENCE public.\"devOps_id_seq\" INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1;", connection, transation);
            cmd3.ExecuteNonQuery();

            using NpgsqlCommand cmd4 = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS public.\"devOps\" (id integer NOT NULL DEFAULT nextval('public.\"devOps_id_seq\"'::regclass), userCod varchar(255))", connection, transation);
            cmd4.ExecuteNonQuery();
            
            for (int i = 0; i < 5; i++)
            {
                using NpgsqlCommand insertCommand = new NpgsqlCommand( $"INSERT INTO public.\"devOps\" (userCod) VALUES ('Users #{i}');", connection, transation);
                insertCommand.ExecuteNonQuery();
	        }
            transation.Commit();
            connection.Close();
        }
}