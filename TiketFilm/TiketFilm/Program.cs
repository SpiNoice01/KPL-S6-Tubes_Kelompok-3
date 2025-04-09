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
    public Dictionary<int, Dictionary<string, List<string>>> MovieScheduleSeats { get; set; } = new();
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
    static string transactionFile = "transactions.json"; // Added missing variable

    static void Main()
    {
        var menuActions = new Dictionary<string, Action>
        {
            { "1", () => {
                ShowMovies();
                Console.WriteLine("\n  Tekan Enter untuk kembali ke menu...");
                Console.ReadLine();
            }},
            { "2", () => {
                bool ulang;
                do
                {
                    OrderTicket();
                    Console.Write("\n  Ingin memesan lagi? (y/n): ");
                    string lagi = Console.ReadLine().ToLower();
                    ulang = lagi == "y";
                } while (ulang);
            }},
            { "3", () => Environment.Exit(0) }
        };

        while (true)
        {
            Console.Clear();
            Console.WriteLine("  <> Sistem Pemesanan Tiket Bioskop CLI");
            Console.WriteLine("  1. Lihat Film");
            Console.WriteLine("  2. Pesan Tiket");
            Console.WriteLine("  3. Keluar");
            Console.Write("  Pilih menu: ");
            string choice = Console.ReadLine();

            if (menuActions.ContainsKey(choice))
            {
                menuActions[choice].Invoke();
            }
            else
            {
                Console.WriteLine("  <> Pilihan tidak valid!");
                Console.ReadLine();
            }
        }
    }

    static List<Movie> GetMovies()
    {
        if (!File.Exists(movieFile)) return new List<Movie>();
        string json = File.ReadAllText(movieFile);
        return JsonConvert.DeserializeObject<List<Movie>>(json) ?? new List<Movie>();
    }

    static Seat GetSeatData()
    {
        if (!File.Exists(seatFile)) return new Seat();
        string json = File.ReadAllText(seatFile);
        return JsonConvert.DeserializeObject<Seat>(json) ?? new Seat();
    }

    static void SaveSeatData(Seat data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(seatFile, json);
    }

    static void ShowMovies()
    {
        var movies = GetMovies();
        Console.WriteLine("\n  <> Daftar Film yang Tersedia:");
        foreach (var movie in movies)
        {
            Console.WriteLine($"      [{movie.Id}] {movie.Title} ({movie.Genre}) - {movie.Duration} menit");
            Console.WriteLine("          Jadwal: " + string.Join(", ", movie.Schedule));
        }
    }

    static void OrderTicket()
    {
        var movies = GetMovies();
        var seatData = GetSeatData();

        Console.WriteLine("\n  <> Pilih Film:");
        foreach (var movie in movies)
        {
            Console.WriteLine($"      [{movie.Id}] {movie.Title}");
        }

        Console.Write("  Masukkan ID Film: ");
        if (!int.TryParse(Console.ReadLine(), out int movieId) || !movies.Exists(m => m.Id == movieId))
        {
            Console.WriteLine("  <> Film tidak ditemukan!");
            Console.ReadLine();
            return;
        }

        var selectedMovie = movies.Find(m => m.Id == movieId);
        Console.WriteLine("\n  <> Pilih Jadwal:");
        for (int i = 0; i < selectedMovie.Schedule.Count; i++)
        {
            Console.WriteLine($"      [{i + 1}] {selectedMovie.Schedule[i]}");
        }

        Console.Write("  Masukkan nomor jadwal: ");
        if (!int.TryParse(Console.ReadLine(), out int scheduleIndex) || scheduleIndex < 1 || scheduleIndex > selectedMovie.Schedule.Count)
        {
            Console.WriteLine("  <> Jadwal tidak valid!");
            Console.ReadLine();
            return;
        }

        string selectedSchedule = selectedMovie.Schedule[scheduleIndex - 1];

        // Dapatkan kursi yang sudah dipesan
        if (!seatData.MovieScheduleSeats.ContainsKey(movieId))
            seatData.MovieScheduleSeats[movieId] = new Dictionary<string, List<string>>();

        if (!seatData.MovieScheduleSeats[movieId].ContainsKey(selectedSchedule))
            seatData.MovieScheduleSeats[movieId][selectedSchedule] = new List<string>();

        var bookedSeats = seatData.MovieScheduleSeats[movieId][selectedSchedule];

        Console.WriteLine("\n  <> Kursi yang sudah dipesan:");
        Console.WriteLine("      " + (bookedSeats.Count > 0 ? string.Join(", ", bookedSeats) : "Belum ada kursi yang dipesan."));

        Console.WriteLine("\n  <> Kursi yang tersedia:");
        List<string> allSeats = GenerateAllSeats();
        List<string> availableSeats = allSeats.FindAll(s => !bookedSeats.Contains(s));
        Console.WriteLine("      " + string.Join(", ", availableSeats));

        Console.Write("\n  Berapa tiket yang ingin dibeli? ");
        if (!int.TryParse(Console.ReadLine(), out int ticketCount) || ticketCount < 1)
        {
            Console.WriteLine("  <> Jumlah tiket tidak valid!");
            Console.ReadLine();
            return;
        }

        List<string> seats = new();
        for (int i = 0; i < ticketCount; i++)
        {
            while (true)
            {
                Console.Write($"  Pilih Kursi untuk tiket {i + 1} (contoh A1 - D5): ");
                string seat = Console.ReadLine().ToUpper();

                if (!availableSeats.Contains(seat))
                {
                    Console.WriteLine("  <> Kursi tidak tersedia!");
                    continue;
                }

                seats.Add(seat);
                break;
            }
        }

        Console.Write("\n  Masukkan nama Anda: ");
        string buyer = Console.ReadLine();

        // Simpan kursi ke struktur baru
        seatData.MovieScheduleSeats[movieId][selectedSchedule].AddRange(seats);
        SaveSeatData(seatData);

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

        Console.WriteLine("\n  <> Pemesanan Berhasil!");
        Console.WriteLine($"      Film   : {selectedMovie.Title}");
        Console.WriteLine($"      Jadwal : {selectedSchedule}");
        Console.WriteLine($"      Kursi  : {string.Join(", ", seats)}");
        Console.WriteLine($"      Nama   : {buyer}");
        Console.WriteLine($"      Total  : Rp{totalHarga:N0}");
    }

    static List<string> GenerateAllSeats()
    {
        var result = new List<string>();
        char[] rows = { 'A', 'B', 'C', 'D' };
        for (int i = 1; i <= 5; i++)
        {
            foreach (var row in rows)
            {
                result.Add($"{row}{i}");
            }
        }
        return result;
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
