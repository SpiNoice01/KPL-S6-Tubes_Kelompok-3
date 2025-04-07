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
    public List<string> Seats { get; set; }
    public string Buyer { get; set; }
    public int Price { get; set; }
    public DateTime Timestamp { get; set; }
}

class Program
{
    static string movieFile = "movies.json";
    static string seatFile = "seats.json";
    static string transactionFile = "transactions.json";

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
                    OrderTicket();
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

    static void OrderTicket()
    {
        var movies = GetMovies();
        Console.WriteLine();
        Console.WriteLine("  <> Pilih Film:");
        foreach (var movie in movies)
        {
            Console.WriteLine($"      [{movie.Id}] {movie.Title}");
        }
        Console.Write("  Masukkan ID Film: ");
        int movieId;
        if (!int.TryParse(Console.ReadLine(), out movieId) || !movies.Exists(m => m.Id == movieId))
        {
            Console.WriteLine("  <> Film tidak ditemukan!");
            Console.ReadLine();
            return;
        }

        var selectedMovie = movies.Find(m => m.Id == movieId);
        Console.WriteLine();
        Console.WriteLine("  <> Pilih Jadwal:");
        for (int i = 0; i < selectedMovie.Schedule.Count; i++)
        {
            Console.WriteLine($"      [{i + 1}] {selectedMovie.Schedule[i]}");
        }
        Console.Write("  Masukkan nomor jadwal: ");
        int scheduleIndex;
        if (!int.TryParse(Console.ReadLine(), out scheduleIndex) || scheduleIndex < 1 || scheduleIndex > selectedMovie.Schedule.Count)
        {
            Console.WriteLine("  <> Jadwal tidak valid!");
            Console.ReadLine();
            return;
        }

        string selectedSchedule = selectedMovie.Schedule[scheduleIndex - 1];
        Console.Write("  Berapa tiket yang ingin dibeli? ");
        int ticketCount;
        if (!int.TryParse(Console.ReadLine(), out ticketCount) || ticketCount < 1)
        {
            Console.WriteLine("  <> Jumlah tiket tidak valid!");
            Console.ReadLine();
            return;
        }

        List<string> seats = new List<string>();
        for (int i = 0; i < ticketCount; i++)
        {
            Console.WriteLine();
            Console.Write($"  Pilih Kursi untuk tiket {i + 1} (A1 - D5): ");
            string seat = Console.ReadLine().ToUpper();
            seats.Add(seat);
        }

        Console.WriteLine();
        Console.Write("  Masukkan nama Anda: ");
        string buyer = Console.ReadLine();

        // Hitung harga tiket
        int hargaPerTiket = 50000; // bisa disesuaikan
        int totalHarga = hargaPerTiket * ticketCount;

        // Buat dan simpan transaksi
        var transaction = new Transaction
        {
            MovieId = selectedMovie.Id,
            MovieTitle = selectedMovie.Title,
            Schedule = selectedSchedule,
            Seats = seats,
            Buyer = buyer,
            Price = totalHarga,
            Timestamp = DateTime.Now
        };
        SaveTransaction(transaction);

        Console.WriteLine();
        Console.WriteLine("  <> Pemesanan Berhasil!");
        Console.WriteLine($"      Film   : {selectedMovie.Title}");
        Console.WriteLine($"      Jadwal : {selectedSchedule}");
        Console.WriteLine($"      Kursi  : {string.Join(", ", seats)}");
        Console.WriteLine($"      Nama   : {buyer}");
        Console.WriteLine($"      Total  : Rp{totalHarga:N0}");
        Console.WriteLine();
        Console.WriteLine("  Tekan Enter untuk kembali ke menu...");
        Console.ReadLine();
    }

    static void SaveTransaction(Transaction transaction)
    {
        List<Transaction> transactions = new List<Transaction>();
        if (File.Exists(transactionFile))
        {
            string json = File.ReadAllText(transactionFile);
            transactions = JsonConvert.DeserializeObject<List<Transaction>>(json) ?? new List<Transaction>();
        }

        transaction.Id = transactions.Count + 1;
        transactions.Add(transaction);

        string updatedJson = JsonConvert.SerializeObject(transactions, Formatting.Indented);
        File.WriteAllText(transactionFile, updatedJson);
    }
}