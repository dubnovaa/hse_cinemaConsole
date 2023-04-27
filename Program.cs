using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

/* 
0. при чтении и записи данных я использую файлы, лежащие в основной папке Solution, а не в bin
(везде указывала условный путь к файлам, а не чисто их имя). не убери их, пожалуйста, случайно из этой папки)))

1. ОБЫЧНОЕ НАСЛЕДОВАНИЕ: файл BaseFilter.cs; фильтры для аналитики по продажам
2. АБСТРАКТНОЕ НАСЛЕДОВАНИЕ: файл ChangeInfo.cs; это модель наследования для структуризации
действий админа по изменению существующих данных о залах, фильмах и сеансах. семинарист одобрил
такую модель

3. среди существующих пользователей есть и с купленными билетами, и без (при авторизации тебе
будут выведены в консоль их логины для удобства). но можно создавать и новые аккаунты, конечно
 
 */
namespace dz3
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                (var shows, var cinemahalls, var movies, var users) = ReadAllInfo();
                CorrectInput correctInput = new();
                bool endProgram = false;
                while (true)
                {
                    ColoredOutput("Если вы пользователь, нажмите 1\nЕсли вы администратор, нажмите 2\nЕсли вы хотите завершить программу, нажмите 0", ConsoleColor.DarkMagenta);
                    int choice1 = correctInput.EnterNumber(2); //значит внутри метода будут допущены значения [0;2]
                    switch (choice1)
                    {
                        case 0: // завершить программу
                            endProgram = true;
                            break;
                        case 1: //режим пользователя
                            var user = InitializeUser(users);
                            user.RequestBalance();
                            UserInterface(user, shows);
                            break;
                        case 2: // режим администратора
                            Administrator admin = new();
                            bool flag = admin.Authorisation();
                            if (!flag) //если после ввода неправильного пароля пользователь захочет перейти в другой режим
                                continue;
                            AdminInterface(admin, shows, movies, cinemahalls, users);
                            break;
                    }
                    if (endProgram)
                        break;
                }
                WriteAllInfo(shows, cinemahalls, movies, users);
                Console.WriteLine("Программа завершена.");
            }
            catch
            {
                ColoredOutput("Скорее всего, невалидные данные в файле.", ConsoleColor.DarkRed);
            }
        }


        internal static void ColoredOutput(string str, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ResetColor();
        }

        // USER'S INTERFACE
        static void UserInterface(User user, List<Showing> shows)
        {
            while (true)
            {
                var correctInput = new CorrectInput();
                Console.WriteLine("\nВыберите номер опции:\n");
                ColoredOutput("0 - Если вы хотите выйти из режима пользователя\n" +
                    "1. Если вы хотите пополнить баланс\n" +
                    "2. вы хотите посмотреть свободные  места или купить билет(-ы)\n" +
                    "3. Если вы хотите посмотреть купленные билеты", ConsoleColor.DarkMagenta);
                bool exitMode = false;
                int choice2 = correctInput.EnterNumber(3);
                switch (choice2)
                {
                    case 0:
                        exitMode = true;
                        break;
                    case 1:
                        user.RequestBalance();
                        break;
                    case 3:
                        user.ShowTickets();
                        break;
                    case 2:
                        while (true)
                        {
                            (bool noShows, List<Showing> relevantShows) = RelevantShows(shows);
                            if (noShows)
                                break;
                            (bool exitShows, Showing showing) = ChooseShowingToWork(relevantShows);
                            if (exitShows)
                                break;
                            WorkingWithShow(showing, user);
                        }
                        break;
                }
                if (exitMode)
                    break;
            }
        }

        static void WorkingWithShow(Showing show, User user)
        {
            var correctInput = new CorrectInput();
            while (true)
            {
                ColoredOutput("Если вы хотите посмотреть свободные места, нажмите 1\nЕсли вы хотите купить билеты, нажмите 2\n" +
                "Если вы хотите выбрать другой сеанс или вернуться в основное меню пользователя, нажмите 0", ConsoleColor.DarkMagenta);
                var choice = correctInput.EnterNumber(2);
                bool flagDifferentShow = false;
                switch (choice)
                {
                    case 0:
                        flagDifferentShow = true;
                        break;
                    case 1:
                        show.ShowFreeSeats();
                        break;
                    case 2:
                        show.ShowCosts();
                        show.ShowFreeSeats();
                        show.BuyTickets(user);
                        break;
                }
                if (flagDifferentShow)
                    break;
            }
        }

        static User InitializeUser(List<User> users)
        {
            Console.WriteLine("Введите ваш логин, регистр неважен:\n(логины, у которых уже есть купленные билеты: n, Zhenya, sima, s)\n(логины, у которых нет купленных билетов: Misha, roy)");
            string name = Console.ReadLine();
            bool existed = false;
            User client;
            int index = -1;
            foreach (var user1 in users)
                if (name.ToLower() == user1.name.ToLower())
                {
                    ColoredOutput("Ваш аккаунт найден.", ConsoleColor.DarkGreen);
                    index = users.IndexOf(user1);
                    existed = true;
                    break;
                }
            if (!existed)
            {
                ColoredOutput("Новый пользователь", ConsoleColor.DarkGreen);
                client = new User(name, 0, tickets: new List<UsersTicket>());
                users.Add(client);
            }
            else
                client = users[index];

            return client;
        }

        //ADMIN'S WORK
        static void AdminInterface(Administrator admin, List<Showing> shows, Dictionary<int, Movie> movies, Dictionary<int, CinemaHall> cinemahalls, List<User> users)
        {
            bool changeMode = false;
            while (true)
            {
                var choice = ChooseActionAdminist();
                bool returnToChoice = false;
                switch (choice)
                {
                    case 0:
                        changeMode = true;
                        break;
                    case 1: //1. Отредактировать информацию о фильмах
                        WorkWithMovies(ref movies, shows, returnToChoice);
                        break;
                    case 2: //2. Отредактировать информацию о залах
                        WorkWithHalls(ref cinemahalls, shows, returnToChoice);
                        break;
                    case 3: //3. Отредактировать информацию о сеансах
                        WorkWithShowings(ref shows, movies, cinemahalls, returnToChoice);
                        break;
                    case 4: //4. Изменить  цены на билеты
                        admin.ChangeCosts(shows);
                        break;
                    case 5: //5. Выбрать фильтры аналитики и вывести по ним загруженность залов и выручку!!!!
                        var showingsAnalytics = shows;
                        var choices = ChooseFilters();
                        foreach (string filter in choices)
                        {
                            showingsAnalytics = FilterShowings(filter, showingsAnalytics, movies, cinemahalls);
                        }
                        if (showingsAnalytics.Count == 0)
                        {
                            ColoredOutput("Нет сеансов, удовлетворяющих выбранным фильтрам.", ConsoleColor.DarkMagenta);
                            break;
                        }
                        CountAnalytics(showingsAnalytics);
                        break;
                    case 6: //6. купивших наибольшее количество билетов
                        MostTicketsUsers(users);

                        break;
                    case 7: //7. купивших билеты на наибольшее количество разных сеансов
                        MostDifferentShows(users);
                        break;
                    case 8: //8. потративших наибольшую сумму денег
                        MostMoney(users);
                        break;

                }
                if (changeMode)
                    break;
            }
        }

        static void WorkWithShowings(ref List<Showing> shows, Dictionary<int, Movie> movies, Dictionary<int, CinemaHall> cinemahalls, bool returnChoice)
        {
            ChangeInfo ChangeShowings = new ChangeShowing(shows, movies, cinemahalls);
            ChangeShowings.WorkWithObject(returnChoice);
            shows = ChangeShowings.AllShows;
        }

        static void WorkWithHalls(ref Dictionary<int, CinemaHall> cinemahalls, List<Showing> shows, bool returnChoice)
        {
            ChangeInfo ChangeHalls = new ChangeHall(shows, cinemahalls);
            ChangeHalls.WorkWithObject(returnChoice);
            cinemahalls = ChangeHalls.AllHalls;
        }

        static void WorkWithMovies(ref Dictionary<int, Movie> movies, List<Showing> shows, bool returnChoice)
        {
            ChangeInfo ChangeMovies = new ChangeMovie(shows, movies);
            ChangeMovies.WorkWithObject(returnChoice);
            movies = ChangeMovies.AllMovies;
        }

        internal static Movie ChooseMovie(Dictionary<int, Movie> movies)
        {
            var correctInput = new CorrectInput();
            ColoredOutput("Выберите номер фильма, с которым хотите работать", ConsoleColor.DarkCyan);
            listMovies(movies);
            var numberEdit = correctInput.EnterDictKey(movies.Keys.ToList());
            var movieEdit = movies[numberEdit];
            return movieEdit;
        }

        internal static CinemaHall ChooseHall(Dictionary<int, CinemaHall> cinemahalls)
        {
            var correctInput = new CorrectInput();
            ColoredOutput("Выберите номер зала, с которым вы хотите работать", ConsoleColor.DarkCyan);
            listHalls(cinemahalls);
            var numberEdit = correctInput.EnterDictKey(cinemahalls.Keys.ToList());
            var hallEdit = cinemahalls[numberEdit];
            return hallEdit;
        }

        internal static bool CompareShows(Showing show1, Showing show2)
        {
            bool equal = false;
            if (show1.movie.ID == show2.movie.ID && show1.hall_id == show2.hall_id)
            {
                if (show1.datetime == show2.datetime)
                {
                    equal = true;
                }

            }
            return equal;

        }

        static int ChooseActionAdminist()
        {
            var correctInput = new CorrectInput();
            ColoredOutput("\nВыберите номер опции:\n", ConsoleColor.DarkCyan);
            Console.WriteLine("0 - Если вы хотите выйти из режима администратора\n");
            ColoredOutput("\t\tРедактирование информации о кинотеатре:", ConsoleColor.DarkBlue);
            Console.WriteLine("1. Отредактировать информацию о фильмах\n" +
                    "2. Отредактировать информацию о залах\n" +
                    "3. Отредактировать информацию о сеансах\n" +
                    "4. Изменить  цены на билеты\n");
            ColoredOutput("\t\tАналитика по продажам:", ConsoleColor.DarkBlue);
            Console.WriteLine("5. Посмотреть общую выручку по проданным билетам (с фильтрами)\n");
            ColoredOutput("\t\tКлиентская аналитика:\n\tПоказать клиентов,", ConsoleColor.DarkBlue);
            Console.WriteLine("6. купивших наибольшее количество билетов\n" +
                "7. купивших билеты на наибольшее количество разных сеансов\n" +
                "8. потративших наибольшую сумму денег\n");

            int choice = correctInput.EnterNumber(8);
            return choice;
        }
        
        //FILTER USERS

        static int TopUsers(List<User> users)
        {
            if (users.Count >= 5)
                return 5;
            else if (users.Count >= 3 && users.Count < 5)
                return 3;
            else
                return 1;
        }

        static void MostMoney(List<User> users)
        {
            List<(User user, int expences)> users_expenditures = new();
            foreach (var client in users)
            {
                users_expenditures.Add((client, client.CountExpenditures()));
            }
            var users_expencesSorted = users_expenditures.OrderByDescending(x => x.expences).ToList();
            var top2 = TopUsers(users);
            ColoredOutput($"Топ-{top2} клиентов, которые потратили наибольшее количество денег:", ConsoleColor.DarkYellow);
            for (int index = 0; index < top2; index++)
            {
                Console.WriteLine($"№{index + 1}: {users_expencesSorted[index].user.name} - потрачено {users_expencesSorted[index].expences} руб.");
            }
        }

        static void MostDifferentShows(List<User> users)
        {
            List<(User user, int ticketsAmount)> users_uniqueShows = new List<(User user, int ticketsAmount)>();
            foreach (var client in users)
            {
                users_uniqueShows.Add((client, client.CountDifferentShows()));
            }
            var users_uniqueShowsSorted = users_uniqueShows.OrderByDescending(x => x.ticketsAmount).ToList();
            var top1 = TopUsers(users);
            ColoredOutput($"Топ-{top1} клиентов, которые купили билеты на наибольшее количество разных сеансов:", ConsoleColor.DarkYellow);
            for (int index = 0; index < top1; index++)
            {
                Console.WriteLine($"№{index + 1}: {users_uniqueShowsSorted[index].user.name} - различных сеансов: {users_uniqueShowsSorted[index].ticketsAmount}");
            }
        }

        static void MostTicketsUsers(List<User> users)
        {
            List<(User user, int ticketsAmount)> users_tickets = new List<(User user, int ticketsAmount)>();
            foreach (var client in users)
            {
                users_tickets.Add((client, client.CountTickets()));
            }
            var users_ticketsSorted = users_tickets.OrderByDescending(x => x.ticketsAmount).ToList();
            var top = TopUsers(users);
            ColoredOutput($"Топ-{top} клиентов, которые купили наибольшее количество билетов:", ConsoleColor.DarkYellow);
            for (int index = 0; index < top; index++)
            {
                Console.WriteLine($"№{index + 1}: {users_ticketsSorted[index].user.name} - билетов: {users_ticketsSorted[index].ticketsAmount}");
            }

        }

        //FILTER SHOWINGS, ANALYTICS

        static string[] ChooseFilters()
        {

            string[] choices;
            ColoredOutput("Введите через пробел номер(-а) фильтров, по которым  вы хотите посмотреть аналитику:", ConsoleColor.DarkCyan);
            Console.WriteLine("1. На один конкретный фильм\n2. В одном конкретном зале\n3. На одном конкретном сеансе\n" +
                "4. За конкретный промежуток времени (в течение дня/за несколько дней)\n" +
                "5. За весь период по всем фильмам\n6. На фильмы с одним конкретным возрастным рейтингом");
            while (true)
            {
                bool correct_input = true;
                choices = Console.ReadLine().Split();
                foreach (var ch in choices)
                {
                    if (!int.TryParse(ch, out int x) || x < 1 || x > 7)
                    {
                        ColoredOutput("Некорректный ввод, попробуйте еще раз ввести фильтры через пробел", ConsoleColor.DarkRed);
                        correct_input = false;
                        break;
                    }

                }
                if (correct_input)
                {
                    if (choices.Contains("5") && choices.Count() == 2 && choices.Contains("2"))
                        break;
                    else if (!choices.Contains("5") || choices.Count() == 1)
                        break;
                    else
                        Console.WriteLine("Вместе с фильтром 5 (за весь период по всем фильмам) можно выбрать только фильтр 2 (конкретный зал). Попробуйте еще раз");
                }

            }
            return choices;
        }

        static List<Showing> FilterShowings(string filter, List<Showing> showings, Dictionary<int, Movie> movies, Dictionary<int, CinemaHall> halls)
        {
            var result = new List<Showing>();
            BaseFilter Filter;
            switch (filter)
            {
                case "1": //На один конкретный фильм
                    Filter = new FilterByMovie(showings, movies);
                    result = Filter.Filter();
                    break;
                case "2": //В одном конкретном зале
                    Filter = new FilterByHall(showings, halls);
                    result = Filter.Filter();
                    break;
                case "3"://На одном конкретном сеансе
                    Filter = new FilterByShowing(showings);
                    result = Filter.Filter();
                    break;
                case "4":// За конкретный промежуток времени
                    Filter = new FilterByTime(showings);
                    result = Filter.Filter();
                    break;
                case "5"://За весь период по всем фильмам
                    Filter = new BaseFilter(showings);
                    result = Filter.Filter();
                    break;
                case "6":// На фильмы с одним конкретным возрастным рейтингом
                    Filter = new FilterByRated(showings);
                    result = Filter.Filter();
                    break;
            }

            return result;


        }

        static void CountAnalytics(List<Showing> showingsAnalytics)
        {
            var revenue = 0;

            foreach (var showing in showingsAnalytics)
            {
                revenue += showing.Revenue;

            }
            ColoredOutput("Статистика:", ConsoleColor.DarkMagenta);
            Console.WriteLine($"Выручка: {revenue}");
            int freeSeats;
            int boughtSeats;
            var sortedByHall = showingsAnalytics.GroupBy(show => show.hall);
            foreach (var result in sortedByHall)
            {
                freeSeats = 0;
                boughtSeats = 0;

                foreach (var show in result)
                {
                    (int AddFreeSeats, int AddBoughtSeats) = show.CountSeats();
                    freeSeats += AddFreeSeats;
                    boughtSeats += AddBoughtSeats;
                }
                Console.WriteLine($"Статистика зала \"{result.Key.name}\" на сеансах, удовлетворяющих фильтрам: \nпродано билетов: {boughtSeats},\n свободных мест: {freeSeats}");
            }
            Console.WriteLine();
        }

        static (bool, List<Showing>) RelevantShows(List<Showing> showings)
        {
            bool noShows = true;
            var now = DateTime.Now;
            var relevantShows = new List<Showing>();
            foreach (var show in showings)
            {
                if (show.datetime > now)
                {
                    noShows = false;
                    relevantShows.Add(show);
                }
            }
            if (noShows)
            {
                Console.WriteLine("Нет доступных сеансов (все сеансы уже прошли)");
            }
            
            return (noShows, relevantShows);
        }

        internal static (bool, Showing) ChooseShowingToWork(List<Showing> showings, bool zeros = true)//, User user)
        {
            bool exit = false;
            var correctInput = new CorrectInput();
            if (zeros)
                Console.WriteLine("\nВыберите номер сеанса, с которым вы хотите работать.\nВведите 0, если хотите вернуться в " +
                                            "основное меню");
            else
                Console.WriteLine("\nВыберите номер сеанса, с которым вы хотите работать.");
            listShowings(showings);
            int choice;
            if (zeros)
                choice = correctInput.EnterNumber(showings.Count);
            else
                choice = correctInput.EnterNumber(showings.Count, zeros: false);
            Showing cinema;
            if (choice == 0)
            {
                cinema = showings[0]; //не будет использоваться, нужна любая ссылка, чтобы вернуть хоть какое-то значение
                exit = true;
            }
            else
            {
                cinema = showings[choice - 1];
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine($"\tВы работаете с сеансом \t\"{cinema.movie.name}\", {cinema.datetime}, зал: {cinema.hall.name}");//,\nВаш баланс = {user.GetBalance()}");
                Console.ResetColor();
            }
            return (exit, cinema);

        }

        // LISTING OBJECTS: SHOWS, HALLS, MOVIES

        internal static void listShowings(List<Showing> showings)
        {
            Console.WriteLine();
            var sortedByMovie = showings.GroupBy(show => show.movie.name);
            foreach (var movie in sortedByMovie)
            {
                ColoredOutput($"Фильм \"{movie.Key}\":", ConsoleColor.DarkBlue);
                foreach (var showing in movie)
                {
                    //ColoredOutput($"Сеанс №{showings.IndexOf(showing) + 1}: ", ConsoleColor.Blue);
                    Console.Write($"Сеанс №{showings.IndexOf(showing) + 1}: ");
                    showing.GetDescription();
                }
            }
            Console.WriteLine();
        }

        internal static void listHalls(Dictionary<int, CinemaHall> halls)
        {
            Console.WriteLine();

            foreach (var hall in halls)
            {

                Console.Write($"Зал №{hall.Key + 1}: "); //\"{hall.Value.name}\"");
                hall.Value.GetDescription();
            }
            Console.WriteLine();
        }

        internal static void listMovies(Dictionary<int, Movie> movies)
        {
            Console.WriteLine();
            
            foreach (var movie in movies)
            {
               
                Console.Write($"Фильм №{movie.Key + 1}: ");
                movie.Value.GetDescription();
               
            }
            Console.WriteLine();
        }


        // READING FROM FILES

        static (List<Showing> , Dictionary<int, CinemaHall>,  Dictionary<int, Movie>, List<User>) ReadAllInfo()
        {
            var users = ReadUsersJson();
            var cinemahalls = ReadHallsJson();
            var movies = ReadMoviesJson();
            var shows = ReadShowingsJson(cinemahalls, movies, users);
            return (shows, cinemahalls, movies, users);
        }

        static List<Showing> ReadShowingsJson(Dictionary<int, CinemaHall> halls, Dictionary<int, Movie> movies, List<User> users)
        {
            List<Showing> shows;
            using (var file = File.Open("../../../showings.json", FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(file))
                {
                    string jsonString = streamReader.ReadToEnd();
                    shows = JsonConvert.DeserializeObject<List<Showing>>(jsonString);
                }

                foreach (var sh in shows)
                {
                    if (halls.ContainsKey(sh.hall_id))
                    {
                        sh.defineHall(halls[sh.hall_id]);
                        //Console.WriteLine("hall defined");
                        sh.hall.AddShowing(sh);
                    }

                    else
                        shows.Remove(sh);
                    if (movies.ContainsKey(sh.movie_id))
                    {
                        sh.defineMovie(movies[sh.movie_id]);
                        //Console.WriteLine("movie defined");
                    }
                        
                    else
                        shows.Remove(sh);

                    foreach (User user in users)
                    {
                        
                        foreach (var ticket in user.tickets)
                        {
                            if (halls.ContainsKey(ticket.show.hall_id))
                            {
                                ticket.show.defineHall(halls[ticket.show.hall_id]);
                                //Console.WriteLine("ticket hall defined");
                            }
                            else
                                user.tickets.Remove(ticket);

                            if (movies.ContainsKey(ticket.show.movie_id))
                            {
                                ticket.show.defineMovie(movies[ticket.show.movie_id]);
                                //Console.WriteLine("ticket movie defined");
                            }
                            else
                                user.tickets.Remove(ticket);
                            if (CompareShows(sh, ticket.show))
                                sh.AddTicket(ticket);
                        }
                    }
               
                }
            }
            //var sortedShows = shows.OrderBy(x => x.datetime).ThenBy(x=>x.movie.name).ToList();
            var sortedShows = shows.OrderBy(x => x.movie.name).ThenBy(x => x.datetime).ToList();

            return sortedShows;
        }

        static Dictionary<int, CinemaHall> ReadHallsJson()
        {
            List<CinemaHall> halls;
            Dictionary<int, CinemaHall> dictionary = new Dictionary<int, CinemaHall>();
            using (var file = File.Open("../../../halls.json", FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(file))
                {
                    string jsonString = streamReader.ReadToEnd();
                    halls = JsonConvert.DeserializeObject<List<CinemaHall>>(jsonString);
                }

            }
            foreach (var hall in halls)
            {
                
                dictionary.Add(hall.ID, hall);
            }

            return dictionary;
        }

        static Dictionary<int, Movie> ReadMoviesJson()
        {
            List<Movie> movies;
            Dictionary<int, Movie> dictionary = new Dictionary<int, Movie>();
            using (var file = File.Open("../../../movies.json", FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(file))
                {
                    string jsonString = streamReader.ReadToEnd();
                    movies = JsonConvert.DeserializeObject<List<Movie>>(jsonString);
                }

            }
            foreach (var movie in movies)
            {
                dictionary.Add(movie.ID, movie);
            }

            return dictionary;
        }

        static List<User> ReadUsersJson()
        {
            List<User> users;
            using (var file = File.Open("../../../users.json", FileMode.Open, FileAccess.Read))
            {
                using (var streamReader = new StreamReader(file))
                {
                    string jsonString = streamReader.ReadToEnd();
                    users = JsonConvert.DeserializeObject<List<User>>(jsonString);
                }

            }
                return users;
        }

        // WRITING TO FILES

        static void WriteAllInfo(List<Showing> shows, Dictionary<int, CinemaHall> cinemahalls, Dictionary<int, Movie> movies, List<User> users)
        {
            WriteUsersJson(users);
            WriteShowsJson(shows);
            WriteHallsJson(cinemahalls.Values.ToList());
            WriteMoviesJson(movies.Values.ToList());
        }

        static void WriteShowsJson(List<Showing> shows)
        {
            using (var file = File.Open("../../../showings.json", FileMode.Truncate, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(file))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(streamWriter, shows);
                }
            }
        }

        static void WriteHallsJson(List<CinemaHall> halls)
        {
            using (var file = File.Open("../../../halls.json", FileMode.Truncate, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(file))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(streamWriter, halls);
                }
            }
        }

        static void WriteMoviesJson(List<Movie> movies)
        {
            using (var file = File.Open("../../../movies.json", FileMode.Truncate, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(file))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(streamWriter, movies);
                }
            }
        }

        static void WriteUsersJson(List<User> users)
        {
            using (var file = File.Open("../../../users.json", FileMode.Truncate, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(file))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(streamWriter, users);
                }
            }
        }

        

    }


}
