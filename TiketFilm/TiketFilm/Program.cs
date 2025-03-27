using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

// Model Film
public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Genre { get; set; }
    public int Duration { get; set; } // dalam menit
    public List<string> Schedule { get; set; }
}

// Model Kursi
public class Seat
{
    public Dictionary<string, List<string>> ScheduleSeats { get; set; }
}

// Model Transaksi
public class Transaction
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; }
    public string Schedule { get; set; }
    public string Seat { get; set; }
    public string Buyer { get; set; }
    public int Price { get; set; }
    public DateTime Timestamp { get; set; }
}

class Program
{
    static string movieFile = "movies.json";

    static void Main()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("  <> Sistem Pemesanan Tiket Bioskop CLI");
            Console.WriteLine("  1. Lihat Film");
            Console.WriteLine("  2. Pesan Tiket");
            Console.WriteLine("  3. Keluar");
            Console.Write("  Pilih menu: ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    ShowMovies();
                    Console.WriteLine();
                    Console.WriteLine("  \nTekan Enter untuk kembali ke menu...");
                    Console.ReadLine();
                    break;
                case "2":
                    Console.WriteLine();
                    Console.WriteLine("  <> Fitur belum dibuat");
                    Console.WriteLine("  \nTekan Enter untuk kembali ke menu...");
                    Console.ReadLine();
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("  <> Pilihan tidak valid!");
                    Console.ReadLine();
                    break;
            }
        }
    }

    static List<Movie> GetMovies()
    {
        if (!File.Exists(movieFile)) return new List<Movie>();

        string json = File.ReadAllText(movieFile);
        return JsonConvert.DeserializeObject<List<Movie>>(json) ?? new List<Movie>();
    }

    static void ShowMovies()
    {
        var movies = GetMovies();
        Console.WriteLine();
        Console.WriteLine("  <> Daftar Film yang Tersedia:");
        foreach (var movie in movies)
        {
            Console.WriteLine($"      [{movie.Id}] {movie.Title} ({movie.Genre}) - {movie.Duration} menit");
            Console.WriteLine("          Jadwal: " + string.Join(", ", movie.Schedule));
            Console.WriteLine();
        }
    }
}
