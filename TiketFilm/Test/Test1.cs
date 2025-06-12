using Microsoft.VisualStudio.TestTools.UnitTesting;
using CinemaTicketBookingSystem;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TiketFilm.Tests
{
    [TestClass]
    public class ProgramTests
    {
        private MovieService _movieService;
        private BookingService _bookingService;
        private SeatService _seatService;
        private AuthService _authService;
        private TestUserInterface _ui;
        private CinemaApp _app;

        [TestInitialize]
        public void Setup()
        {
            // Initialize dependencies
            var movieRepo = new JsonMovieRepository();
            var seatRepo = new JsonSeatRepository();
            var bookingRepo = new JsonBookingRepository();
            var userRepo = new JsonUserRepository();

            _movieService = new MovieService(movieRepo);
            _seatService = new SeatService(seatRepo);
            _bookingService = new BookingService(bookingRepo, _movieService, _seatService);
            _authService = new AuthService(userRepo);
            _ui = new TestUserInterface();
            _app = new CinemaApp(_movieService, _bookingService, _seatService, _ui, _authService);

            // Create test data
            SetupTestData();
        }

        private void SetupTestData()
        {
            // Create test movie
            var movie = new Movie(
                id: 1,
                title: "Test Movie",
                genre: "Action",
                durationMinutes: 120,
                schedules: new List<string> { "10:00", "13:00" }
            );

            // Save test movie
            File.WriteAllText("movies.json", JsonConvert.SerializeObject(new List<Movie> { movie }, Formatting.Indented));

            // Create test user
            var users = new List<User>
            {
                new User { Username = "testuser", Password = "testpass" }
            };
            File.WriteAllText("users.json", JsonConvert.SerializeObject(users, Formatting.Indented));

            // Initialize empty files
            File.WriteAllText("seats.json", JsonConvert.SerializeObject(new SeatReservation(), Formatting.Indented));
            File.WriteAllText("transactions.json", "[]");
        }

        [TestMethod]
        public void TestMainDisplaysMenu()
        {
            using (var sw = new StringWriter())
            using (var sr = new StringReader("4\n")) // Simulasi input: pilih menu keluar
            {
                Console.SetOut(sw);
                Console.SetIn(sr);

                // Run the app
                _app.Run();

                var output = sw.ToString();
                Assert.IsTrue(output.Contains("Cinema Ticket Booking System"));
            }
        }

        [TestMethod]
        public void TestMovieService_GetAvailableMovies()
        {
            var movies = _movieService.GetAvailableMovies();
            Assert.IsNotNull(movies);
            Assert.AreEqual(1, movies.Count);
            Assert.AreEqual("Test Movie", movies[0].Title);
        }

        [TestMethod]
        public void TestMovieService_GetMovieById()
        {
            var movie = _movieService.GetMovieById(1);
            Assert.IsNotNull(movie);
            Assert.AreEqual("Test Movie", movie.Title);

            var nonExistentMovie = _movieService.GetMovieById(999);
            Assert.IsNull(nonExistentMovie);
        }

        [TestMethod]
        public void TestSeatService_GetAvailableSeats()
        {
            var seats = _seatService.GetAvailableSeats(1, "10:00");
            Assert.IsNotNull(seats);
            Assert.AreEqual(20, seats.Count); // 4 rows * 5 seats per row
            Assert.IsTrue(seats.Contains("A1"));
            Assert.IsTrue(seats.Contains("D5"));
        }

        [TestMethod]
        public void TestSeatService_ReserveSeats()
        {
            var seatsToReserve = new List<string> { "A1", "A2" };
            _seatService.ReserveSeats(1, "10:00", seatsToReserve);

            var availableSeats = _seatService.GetAvailableSeats(1, "10:00");
            Assert.IsFalse(availableSeats.Contains("A1"));
            Assert.IsFalse(availableSeats.Contains("A2"));
            Assert.IsTrue(availableSeats.Contains("A3"));
        }

        [TestMethod]
        public void TestBookingService_CreateBooking()
        {
            var transaction = _bookingService.CreateBooking(
                movieId: 1,
                schedule: "10:00",
                seats: new List<string> { "A1" },
                buyerName: "Test Buyer"
            );

            Assert.IsNotNull(transaction);
            Assert.AreEqual(1, transaction.MovieId);
            Assert.AreEqual("Test Movie", transaction.MovieTitle);
            Assert.AreEqual("10:00", transaction.Schedule);
            Assert.AreEqual("Test Buyer", transaction.BuyerName);
            Assert.AreEqual(1, transaction.Seats.Count);
            Assert.AreEqual("A1", transaction.Seats[0]);
            Assert.AreEqual(50000, transaction.TotalPrice); // 1 ticket * 50000
        }

        [TestMethod]
        public void TestBookingService_GetTransactionHistory()
        {
            // Create a test transaction
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "10:00",
                seats: new List<string> { "A1" },
                buyerName: "Test Buyer"
            );

            var transactions = _bookingService.GetTransactionHistory();
            Assert.IsNotNull(transactions);
            Assert.AreEqual(1, transactions.Count);
            Assert.AreEqual("Test Buyer", transactions[0].BuyerName);
        }

        [TestMethod]
        public void TestAuthService_Login()
        {
            Assert.IsTrue(_authService.Login("testuser", "testpass"));
            Assert.IsFalse(_authService.Login("testuser", "wrongpass"));
            Assert.IsFalse(_authService.Login("nonexistent", "testpass"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBookingService_InvalidMovieId()
        {
            _bookingService.CreateBooking(
                movieId: -1,
                schedule: "10:00",
                seats: new List<string> { "A1" },
                buyerName: "Test Buyer"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBookingService_EmptySchedule()
        {
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "",
                seats: new List<string> { "A1" },
                buyerName: "Test Buyer"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBookingService_EmptySeats()
        {
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "10:00",
                seats: new List<string>(),
                buyerName: "Test Buyer"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBookingService_EmptyBuyerName()
        {
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "10:00",
                seats: new List<string> { "A1" },
                buyerName: ""
            );
        }

        [TestMethod]
        [ExpectedException(typeof(BookingException))]
        public void TestBookingService_InvalidSchedule()
        {
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "15:00", // Not in movie schedules
                seats: new List<string> { "A1" },
                buyerName: "Test Buyer"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(BookingException))]
        public void TestBookingService_InvalidSeat()
        {
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "10:00",
                seats: new List<string> { "Z1" }, // Invalid seat
                buyerName: "Test Buyer"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(BookingException))]
        public void TestBookingService_DuplicateSeat()
        {
            // First booking
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "10:00",
                seats: new List<string> { "A1" },
                buyerName: "Test Buyer 1"
            );

            // Try to book the same seat
            _bookingService.CreateBooking(
                movieId: 1,
                schedule: "10:00",
                seats: new List<string> { "A1" },
                buyerName: "Test Buyer 2"
            );
        }
    }

    // Test UI implementation that captures output
    public class TestUserInterface : IUserInterface
    {
        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void DisplayMovies(IEnumerable<Movie> movies)
        {
            Console.WriteLine("\nAvailable Movies:");
            foreach (var movie in movies)
            {
                Console.WriteLine($"[{movie.Id}] {movie.Title} ({movie.Genre}) - {movie.DurationMinutes} mins");
                Console.WriteLine($"  Schedules: {string.Join(", ", movie.Schedules)}");
            }
        }

        public void DisplaySchedules(IEnumerable<string> schedules)
        {
            Console.WriteLine("\nAvailable Schedules:");
            int index = 1;
            foreach (var schedule in schedules)
            {
                Console.WriteLine($"[{index++}] {schedule}");
            }
        }

        public void DisplaySeats(IEnumerable<string> seats)
        {
            Console.WriteLine("\nAvailable Seats:");
            var seatList = seats.ToList();
            for (int i = 0; i < seatList.Count; i++)
            {
                Console.Write($"{seatList[i]} ");
                if ((i + 1) % 5 == 0) Console.WriteLine();
            }
            Console.WriteLine();
        }

        public void DisplayBookingConfirmation(BookingTransaction transaction)
        {
            Console.WriteLine("\nBooking Confirmation:");
            Console.WriteLine($"Movie: {transaction.MovieTitle}");
            Console.WriteLine($"Schedule: {transaction.Schedule}");
            Console.WriteLine($"Seats: {string.Join(", ", transaction.Seats)}");
            Console.WriteLine($"Name: {transaction.BuyerName}");
            Console.WriteLine($"Total: Rp{transaction.TotalPrice:N0}");
        }

        public void DisplayTransactionHistory(IEnumerable<BookingTransaction> transactions)
        {
            Console.WriteLine("\nTransaction History:");
            foreach (var t in transactions)
            {
                Console.WriteLine($"ID: {t.Id}");
                Console.WriteLine($"Movie: {t.MovieTitle}");
                Console.WriteLine($"Schedule: {t.Schedule}");
                Console.WriteLine($"Seats: {string.Join(", ", t.Seats)}");
                Console.WriteLine($"Name: {t.BuyerName}");
                Console.WriteLine($"Total: Rp{t.TotalPrice:N0}");
                Console.WriteLine($"Time: {t.TransactionTime}");
                Console.WriteLine(new string('-', 40));
            }
        }

        public string GetUserInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public int GetUserChoice(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out var choice) && choice >= min && choice <= max)
                {
                    return choice;
                }
                Console.WriteLine($"Invalid input. Please enter a number between {min} and {max}.");
            }
        }

        public void ClearScreen()
        {
            // Do nothing in test mode
        }

        public void WaitForUser()
        {
            // Do nothing in test mode
        }
    }
}
