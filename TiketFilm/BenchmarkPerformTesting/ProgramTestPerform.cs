using BenchmarkDotNet.Attributes;
using TiketFilmCore;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


using BenchmarkDotNet.Running;

class BenchmarkRunnerMain
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<BenchmarkTests>();
    }
}


[MemoryDiagnoser]
public class BenchmarkTests
{
    private readonly List<TiketFilmCore.Movie> testMovies;
    private readonly TiketFilmCore.Seat testSeats;

    public BenchmarkTests()
    {
        // Persiapan data dummy
        TiketFilmCore.Program.IsTesting = true;

        testMovies = new()
        {
            new TiketFilmCore.Movie
            {
                Id = 1,
                Title = "Film A",
                Genre = "Action",
                Duration = 120,
                Schedule = new List<string> { "10:00", "13:00" }
            }
        };

        testSeats = new();
        testSeats.MovieScheduleSeats[1] = new Dictionary<string, List<string>>
        {
            ["10:00"] = new List<string>()
        };

        // Simpan dummy data
        File.WriteAllText("movies.json", JsonConvert.SerializeObject(testMovies, Formatting.Indented));
        File.WriteAllText("seats.json", JsonConvert.SerializeObject(testSeats, Formatting.Indented));
        File.WriteAllText("transactions.json", "[]");
    }

    [Benchmark]
    public void BenchmarkGetMovies()
    {
        _ = TiketFilmCore.Program.GetMovies();
    }

    [Benchmark]
    public void BenchmarkOrderTicket()
    {
        // Simulasi pemesanan satu kursi
        var transaction = new TiketFilmCore.Transaction
        {
            MovieId = 1,
            MovieTitle = "Film A",
            Schedule = "10:00",
            Seats = new List<string> { "A1" },
            Buyer = "BenchmarkUser",
            Price = 50000,
            Timestamp = DateTime.Now
        };

        // Tambah kursi secara langsung
        var seats = TiketFilmCore.Program.GetSeatData();
        if (!seats.MovieScheduleSeats.ContainsKey(1))
            seats.MovieScheduleSeats[1] = new Dictionary<string, List<string>>();
        if (!seats.MovieScheduleSeats[1].ContainsKey("10:00"))
            seats.MovieScheduleSeats[1]["10:00"] = new List<string>();
        seats.MovieScheduleSeats[1]["10:00"].Add("A1");

        File.WriteAllText("seats.json", JsonConvert.SerializeObject(seats, Formatting.Indented));
        TiketFilmCore.Program.SaveTransaction(transaction);
    }

    [Benchmark]
    public void BenchmarkShowTransactionHistory()
    {
        // Tulis dummy transaksi jika kosong
        if (!File.Exists("transactions.json") || File.ReadAllText("transactions.json").Length < 5)
        {
            var tx = new TiketFilmCore.Transaction
            {
                Id = 1,
                MovieId = 1,
                MovieTitle = "Film A",
                Schedule = "10:00",
                Seats = new List<string> { "A1" },
                Buyer = "Test",
                Price = 50000,
                Timestamp = DateTime.Now
            };
            TiketFilmCore.Program.SaveTransaction(tx);
        }

        // Jalankan show history
        TiketFilmCore.Program.ShowTransactionHistory();
    }
}
