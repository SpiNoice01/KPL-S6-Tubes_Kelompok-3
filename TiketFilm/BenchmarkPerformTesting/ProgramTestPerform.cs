using BenchmarkDotNet.Attributes;
using CinemaTicketBookingSystem;
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
    private readonly MovieService _movieService;
    private readonly BookingService _bookingService;
    private readonly SeatService _seatService;
    private readonly Movie testMovie;
    private readonly SeatReservation testSeats;

    public BenchmarkTests()
    {
        // Initialize services
        var movieRepo = new JsonMovieRepository();
        var seatRepo = new JsonSeatRepository();
        var bookingRepo = new JsonBookingRepository();
        var userRepo = new JsonUserRepository();

        _movieService = new MovieService(movieRepo);
        _seatService = new SeatService(seatRepo);
        _bookingService = new BookingService(bookingRepo, _movieService, _seatService);

        // Create test movie
        testMovie = new Movie(
            id: 1,
            title: "Film A",
            genre: "Action",
            durationMinutes: 120,
            schedules: new List<string> { "10:00", "13:00" }
        );

        // Create test seats
        testSeats = new SeatReservation();
        testSeats.ReserveSeats(1, "10:00", new List<string>());

        // Save test data
        File.WriteAllText("movies.json", JsonConvert.SerializeObject(new List<Movie> { testMovie }, Formatting.Indented));
        File.WriteAllText("seats.json", JsonConvert.SerializeObject(testSeats, Formatting.Indented));
        File.WriteAllText("transactions.json", "[]");
    }

    [Benchmark]
    public void BenchmarkGetMovies()
    {
        _ = _movieService.GetAvailableMovies();
    }

    [Benchmark]
    public void BenchmarkOrderTicket()
    {
        // Create a booking transaction
        var transaction = _bookingService.CreateBooking(
            movieId: 1,
            schedule: "10:00",
            seats: new List<string> { "A1" },
            buyerName: "BenchmarkUser"
        );
    }

    [Benchmark]
    public void BenchmarkShowTransactionHistory()
    {
        // Create a test transaction if none exists
        if (!File.Exists("transactions.json") || File.ReadAllText("transactions.json").Length < 5)
        {
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "10:00",
                seats: new List<string> { "A1" },
                buyerName: "Test"
            );
        }

        // Get transaction history
        _ = _bookingService.GetTransactionHistory();
    }
}
