using Newtonsoft.Json;


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace CinemaTicketBookingSystem
{
    // Domain Models
    public class Movie
    {
        public int Id { get; }
        public string Title { get; }
        public string Genre { get; }
        public int DurationMinutes { get; }
        public IReadOnlyList<string> Schedules { get; }

        public Movie(int id, string title, string genre, int durationMinutes, List<string> schedules)
        {
            Id = id;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Genre = genre ?? throw new ArgumentNullException(nameof(genre));
            DurationMinutes = durationMinutes > 0 ? durationMinutes : throw new ArgumentException("Duration must be positive");
            Schedules = schedules?.AsReadOnly() ?? throw new ArgumentNullException(nameof(schedules));
        }
    }

    public class SeatReservation
    {
        public Dictionary<int, Dictionary<string, List<string>>> MovieScheduleSeats { get; } = new();

        public void ReserveSeats(int movieId, string schedule, List<string> seats)
        {
            if (!MovieScheduleSeats.ContainsKey(movieId))
            {
                MovieScheduleSeats[movieId] = new Dictionary<string, List<string>>();
            }

            if (!MovieScheduleSeats[movieId].ContainsKey(schedule))
            {
                MovieScheduleSeats[movieId][schedule] = new List<string>();
            }

            MovieScheduleSeats[movieId][schedule].AddRange(seats);
        }
    }

    public class BookingTransaction
    {
        public int Id { get; }
        public int MovieId { get; }
        public string MovieTitle { get; }
        public string Schedule { get; }
        public IReadOnlyList<string> Seats { get; }
        public string BuyerName { get; }
        public int TotalPrice { get; }
        public DateTime TransactionTime { get; }

        public BookingTransaction(int id, int movieId, string movieTitle, string schedule,
                                List<string> seats, string buyerName, int totalPrice, DateTime transactionTime)
        {
            Id = id;
            MovieId = movieId;
            MovieTitle = movieTitle ?? throw new ArgumentNullException(nameof(movieTitle));
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            Seats = seats?.AsReadOnly() ?? throw new ArgumentNullException(nameof(seats));
            BuyerName = buyerName ?? throw new ArgumentNullException(nameof(buyerName));
            TotalPrice = totalPrice >= 0 ? totalPrice : throw new ArgumentException("Price cannot be negative");
            TransactionTime = transactionTime;
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // Interfaces
    public interface IMovieRepository
    {
        IReadOnlyList<Movie> GetAllMovies();
    }

    public interface ISeatRepository
    {
        SeatReservation GetSeatReservations();
        void SaveSeatReservations(SeatReservation reservations);
    }

    public interface IBookingRepository
    {
        IReadOnlyList<BookingTransaction> GetAllTransactions();
        void AddTransaction(BookingTransaction transaction);
    }

    public interface IUserRepository
    {
        User GetUserByUsername(string username);
        IReadOnlyList<User> GetAllUsers();
    }

    // Concrete Repositories
    public class JsonMovieRepository : IMovieRepository
    {
        private const string MovieFile = "movies.json";

        public IReadOnlyList<Movie> GetAllMovies()
        {
            if (!File.Exists(MovieFile))
                return new List<Movie>().AsReadOnly();

            try
            {
                var movies = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText(MovieFile)) ?? new List<Movie>();
                return movies.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to load movies", ex);
            }
        }
    }

    public class JsonSeatRepository : ISeatRepository
    {
        private const string SeatFile = "seats.json";

        public SeatReservation GetSeatReservations()
        {
            if (!File.Exists(SeatFile))
                return new SeatReservation();

            try
            {
                return JsonConvert.DeserializeObject<SeatReservation>(File.ReadAllText(SeatFile)) ?? new SeatReservation();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to load seat reservations", ex);
            }
        }

        public void SaveSeatReservations(SeatReservation reservations)
        {
            try
            {
                File.WriteAllText(SeatFile, JsonConvert.SerializeObject(reservations, Formatting.Indented));
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to save seat reservations", ex);
            }
        }
    }

    public class JsonBookingRepository : IBookingRepository
    {
        private const string BookingFile = "bookings.json";

        public IReadOnlyList<BookingTransaction> GetAllTransactions()
        {
            if (!File.Exists(BookingFile))
                return new List<BookingTransaction>().AsReadOnly();

            try
            {
                return JsonConvert.DeserializeObject<List<BookingTransaction>>(File.ReadAllText(BookingFile))?.AsReadOnly()
                    ?? new List<BookingTransaction>().AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to load booking transactions", ex);
            }
        }

        public void AddTransaction(BookingTransaction transaction)
        {
            var transactions = new List<BookingTransaction>(GetAllTransactions());
            transactions.Add(transaction);

            try
            {
                File.WriteAllText(BookingFile, JsonConvert.SerializeObject(transactions, Formatting.Indented));
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to save booking transactions", ex);
            }
        }
    }

    public class JsonUserRepository : IUserRepository
    {
        private const string UserFile = "users.json";

        public User GetUserByUsername(string username)
        {
            return GetAllUsers().FirstOrDefault(u => u.Username == username);
        }

        public IReadOnlyList<User> GetAllUsers()
        {
            if (!File.Exists(UserFile))
                return new List<User>().AsReadOnly();
            try
            {
                var users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(UserFile)) ?? new List<User>();
                return users.AsReadOnly();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to load users", ex);
            }
        }
    }

    // Exceptions
    public class RepositoryException : Exception
    {
        public RepositoryException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class BookingException : Exception
    {
        public BookingException(string message) : base(message) { }
    }

    // Services
    public class MovieService
    {
        private readonly IMovieRepository _movieRepository;

        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
        }

        public IReadOnlyList<Movie> GetAvailableMovies()
        {
            return _movieRepository.GetAllMovies();
        }

        public Movie GetMovieById(int movieId)
        {
            var movies = _movieRepository.GetAllMovies();
            return movies.FirstOrDefault(m => m.Id == movieId);
        }
    }

    public class SeatService
    {
        private const int Rows = 4; // A-D
        private const int SeatsPerRow = 5;
        private readonly ISeatRepository _seatRepository;

        public SeatService(ISeatRepository seatRepository)
        {
            _seatRepository = seatRepository ?? throw new ArgumentNullException(nameof(seatRepository));
        }

        public IReadOnlyList<string> GetAvailableSeats(int movieId, string schedule)
        {
            if (string.IsNullOrWhiteSpace(schedule))
                throw new ArgumentException("Schedule cannot be empty", nameof(schedule));

            var allSeats = GenerateAllSeats();
            var reservedSeats = GetReservedSeats(movieId, schedule);

            return allSeats.Except(reservedSeats).ToList().AsReadOnly();
        }

        public void ReserveSeats(int movieId, string schedule, List<string> seats)
        {
            if (seats == null || !seats.Any())
                throw new ArgumentException("At least one seat must be reserved", nameof(seats));

            var reservations = _seatRepository.GetSeatReservations();
            reservations.ReserveSeats(movieId, schedule, seats);
            _seatRepository.SaveSeatReservations(reservations);
        }

        private IReadOnlyList<string> GetReservedSeats(int movieId, string schedule)
        {
            var reservations = _seatRepository.GetSeatReservations();

            if (reservations.MovieScheduleSeats.TryGetValue(movieId, out var scheduleSeats) &&
                scheduleSeats.TryGetValue(schedule, out var reservedSeats))
            {
                return reservedSeats.AsReadOnly();
            }

            return new List<string>().AsReadOnly();
        }

        private List<string> GenerateAllSeats()
        {
            var seats = new List<string>();
            for (char row = 'A'; row < 'A' + Rows; row++)
            {
                for (int number = 1; number <= SeatsPerRow; number++)
                {
                    seats.Add($"{row}{number}");
                }
            }
            return seats;
        }
    }

    public class BookingService
    {
        private const int TicketPrice = 50000;
        private readonly IBookingRepository _bookingRepository;
        private readonly MovieService _movieService;
        private readonly SeatService _seatService;

        public BookingService(IBookingRepository bookingRepository, MovieService movieService, SeatService seatService)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
            _seatService = seatService ?? throw new ArgumentNullException(nameof(seatService));
        }

        public BookingTransaction CreateBooking(int movieId, string schedule, List<string> seats, string buyerName)
        {
            ValidateBookingParameters(movieId, schedule, seats, buyerName);

            var movie = _movieService.GetMovieById(movieId);
            if (movie == null)
                throw new BookingException("Movie not found");

            if (!movie.Schedules.Contains(schedule))
                throw new BookingException("Schedule not available for this movie");

            var availableSeats = _seatService.GetAvailableSeats(movieId, schedule);
            var invalidSeats = seats.Except(availableSeats).ToList();

            if (invalidSeats.Any())
                throw new BookingException($"Seats not available: {string.Join(", ", invalidSeats)}");

            var totalPrice = CalculateTotalPrice(seats.Count);
            var transaction = new BookingTransaction(
                id: _bookingRepository.GetAllTransactions().Count + 1,
                movieId: movieId,
                movieTitle: movie.Title,
                schedule: schedule,
                seats: seats,
                buyerName: buyerName,
                totalPrice: totalPrice,
                transactionTime: DateTime.Now
            );

            _seatService.ReserveSeats(movieId, schedule, seats);
            _bookingRepository.AddTransaction(transaction);

            return transaction;
        }

        public IReadOnlyList<BookingTransaction> GetTransactionHistory()
        {
            return _bookingRepository.GetAllTransactions();
        }

        private int CalculateTotalPrice(int ticketCount)
        {
            return ticketCount * TicketPrice;
        }

        private void ValidateBookingParameters(int movieId, string schedule, List<string> seats, string buyerName)
        {
            if (movieId <= 0) throw new ArgumentException("Invalid movie ID", nameof(movieId));
            if (string.IsNullOrWhiteSpace(schedule)) throw new ArgumentException("Schedule cannot be empty", nameof(schedule));
            if (seats == null || !seats.Any()) throw new ArgumentException("At least one seat must be selected", nameof(seats));
            if (string.IsNullOrWhiteSpace(buyerName)) throw new ArgumentException("Buyer name cannot be empty", nameof(buyerName));
        }
    }

    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public bool Login(string username, string password)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user == null) return false;
            return user.Password == password;
        }
    }

    // UI Layer
    public interface IUserInterface
    {
        void DisplayMessage(string message);
        void DisplayMovies(IEnumerable<Movie> movies);
        void DisplaySchedules(IEnumerable<string> schedules);
        void DisplaySeats(IEnumerable<string> seats);
        void DisplayBookingConfirmation(BookingTransaction transaction);
        void DisplayTransactionHistory(IEnumerable<BookingTransaction> transactions);
        string GetUserInput(string prompt);
        int GetUserChoice(string prompt, int min, int max);
        void ClearScreen();
        void WaitForUser();
    }

    public class ConsoleUserInterface : IUserInterface
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
            return Console.ReadLine()?.Trim();
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
            try
            {
                Console.Clear();
            }
            catch { /* Ignore if clearing fails */ }
        }

        public void WaitForUser()
        {
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }

    public class BookingController
    {
        private readonly MovieService _movieService;
        private readonly BookingService _bookingService;
        private readonly SeatService _seatService;
        private readonly IUserInterface _ui;

        public BookingController(MovieService movieService,
                              BookingService bookingService,
                              SeatService seatService,
                              IUserInterface ui)
        {
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _seatService = seatService ?? throw new ArgumentNullException(nameof(seatService));
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        }

        public void ProcessTicketOrder()
        {
            try
            {
                var movie = SelectMovie();
                if (movie == null) return;

                var schedule = SelectSchedule(movie);
                if (string.IsNullOrEmpty(schedule)) return;

                var seats = SelectSeats(movie.Id, schedule);
                if (seats == null || !seats.Any()) return;

                var buyerName = GetBuyerName();
                var transaction = _bookingService.CreateBooking(movie.Id, schedule, seats, buyerName);

                _ui.DisplayBookingConfirmation(transaction);
            }
            catch (Exception ex)
            {
                _ui.DisplayMessage($"Error: {ex.Message}");
            }
        }

        private Movie SelectMovie()
        {
            var movies = _movieService.GetAvailableMovies();
            if (!movies.Any())
            {
                _ui.DisplayMessage("No movies available.");
                return null;
            }

            _ui.DisplayMovies(movies);
            int choice = _ui.GetUserChoice("\nEnter Movie ID (0 to cancel): ", 0, movies.Max(m => m.Id));

            return choice == 0 ? null : _movieService.GetMovieById(choice);
        }

        private string SelectSchedule(Movie movie)
        {
            if (!movie.Schedules.Any())
            {
                _ui.DisplayMessage("No schedules available for this movie.");
                return null;
            }

            _ui.DisplaySchedules(movie.Schedules);
            int choice = _ui.GetUserChoice("\nSelect schedule (0 to cancel): ", 0, movie.Schedules.Count);

            return choice == 0 ? null : movie.Schedules[choice - 1];
        }

        private List<string> SelectSeats(int movieId, string schedule)
        {
            var availableSeats = _seatService.GetAvailableSeats(movieId, schedule).ToList();
            if (!availableSeats.Any())
            {
                _ui.DisplayMessage("No seats available for this schedule.");
                return null;
            }

            _ui.DisplaySeats(availableSeats);
            int ticketCount = _ui.GetUserChoice($"\nHow many tickets? (1-{availableSeats.Count}, 0 to cancel): ", 0, availableSeats.Count);
            if (ticketCount == 0) return null;

            var selectedSeats = new List<string>();
            for (int i = 0; i < ticketCount; i++)
            {
                string seat;
                do
                {
                    seat = _ui.GetUserInput($"Select seat for ticket {i + 1}: ").ToUpper();

                    if (!availableSeats.Contains(seat))
                    {
                        _ui.DisplayMessage("Seat not available.");
                        seat = null;
                    }
                    else if (selectedSeats.Contains(seat))
                    {
                        _ui.DisplayMessage("Seat already selected.");
                        seat = null;
                    }
                } while (seat == null);

                selectedSeats.Add(seat);
            }

            return selectedSeats;
        }

        private string GetBuyerName()
        {
            string name;
            do
            {
                name = _ui.GetUserInput("\nYour name: ");
                if (string.IsNullOrWhiteSpace(name))
                {
                    _ui.DisplayMessage("Name cannot be empty.");
                }
            } while (string.IsNullOrWhiteSpace(name));

            return name;
        }
    }

    public class CinemaApp
    {
        private readonly MovieService _movieService;
        private readonly BookingService _bookingService;
        private readonly SeatService _seatService;
        private readonly IUserInterface _ui;
        private readonly BookingController _bookingController;
        private readonly AuthService _authService;
        private User _currentUser;

        public CinemaApp(MovieService movieService,
                        BookingService bookingService,
                        SeatService seatService,
                        IUserInterface ui,
                        AuthService authService)
        {
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _seatService = seatService ?? throw new ArgumentNullException(nameof(seatService));
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
            _bookingController = new BookingController(movieService, bookingService, seatService, ui);
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public void Run()
        {
            // Login first
            bool loggedIn = false;
            do
            {
                _ui.ClearScreen();
                _ui.DisplayMessage("=== LOGIN ===");
                var username = _ui.GetUserInput("Username: ");
                var password = _ui.GetUserInput("Password: ");
                if (_authService.Login(username, password))
                {
                    loggedIn = true;
                    _ui.DisplayMessage("Login successful!\n");
                }
                else
                {
                    _ui.DisplayMessage("Login failed. Try again.\n");
                    _ui.WaitForUser();
                }
            } while (!loggedIn);

            while (true)
            {
                _ui.ClearScreen();
                DisplayMainMenu();

                var choice = _ui.GetUserInput("Choose option: ");

                switch (choice)
                {
                    case "1":
                        ShowMovies();
                        _ui.WaitForUser();
                        break;
                    case "2":
                        BookTickets();
                        break;
                    case "3":
                        ShowTransactionHistory();
                        _ui.WaitForUser();
                        break;
                    case "4":
                        return;
                    default:
                        _ui.DisplayMessage("Invalid choice!");
                        _ui.WaitForUser();
                        break;
                }
            }
        }

        private void DisplayMainMenu()
        {
            _ui.DisplayMessage("Cinema Ticket Booking System");
            _ui.DisplayMessage("1. View Movies");
            _ui.DisplayMessage("2. Book Tickets");
            _ui.DisplayMessage("3. View Transaction History");
            _ui.DisplayMessage("4. Exit");
        }

        private void ShowMovies()
        {
            var movies = _movieService.GetAvailableMovies();
            if (!movies.Any())
            {
                _ui.DisplayMessage("\nNo movies available.");
                return;
            }

            _ui.DisplayMovies(movies);
        }

        private void BookTickets()
        {
            bool bookAgain;
            do
            {
                _bookingController.ProcessTicketOrder();
                bookAgain = _ui.GetUserInput("\nBook another ticket? (y/n): ").ToLower() == "y";
            } while (bookAgain);
        }

        private void ShowTransactionHistory()
        {
            var transactions = _bookingService.GetTransactionHistory();
            if (!transactions.Any())
            {
                _ui.DisplayMessage("\nNo transaction history found.");
                return;
            }

            _ui.DisplayTransactionHistory(transactions);
        }
    }

    public class Program
    {
        public static void Main()
        {
            // Initialize dependencies
            var movieRepo = new JsonMovieRepository();
            var seatRepo = new JsonSeatRepository();
            var bookingRepo = new JsonBookingRepository();
            var userRepo = new JsonUserRepository();
            var authService = new AuthService(userRepo);

            var movieService = new MovieService(movieRepo);
            var seatService = new SeatService(seatRepo);
            var bookingService = new BookingService(bookingRepo, movieService, seatService);

            var ui = new ConsoleUserInterface();
            var app = new CinemaApp(movieService, bookingService, seatService, ui, authService);

            // Run the application
            app.Run();
        }
    }
}