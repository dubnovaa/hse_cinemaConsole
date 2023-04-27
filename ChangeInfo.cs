using System;
using System.Collections.Generic;
using System.Linq;

namespace dz3
{
    public abstract class ChangeInfo
    {
        protected CorrectInput correctInput;
        public List<Showing> AllShows { get; protected set; }
        public Dictionary<int, Movie> AllMovies { get; protected set; }
        public Dictionary<int, CinemaHall> AllHalls { get; protected set; }
        public abstract string Name { get; }
        public ChangeInfo(List<Showing> showings, Dictionary<int, Movie> movies, Dictionary<int, CinemaHall> halls)
        {
            AllShows = showings;
            AllMovies = movies;
            AllHalls = halls;
            correctInput = new CorrectInput();
        }
        public ChangeInfo(List<Showing> showings, Dictionary<int, CinemaHall> halls)
        {
            AllShows = showings;
            AllHalls = halls;
            correctInput = new CorrectInput();
        }
        public ChangeInfo(List<Showing> showings, Dictionary<int, Movie> movies)
        {
            AllShows = showings;
            AllMovies = movies;
            correctInput = new CorrectInput();
        }
        public abstract void Add();
        public abstract void Delete();
        public abstract void Edit();
        private int ChooseEditing(string obj)
        {
            Program.ColoredOutput($"Введите номер опции:\n\n0. Вернуться к основному выбору администратора\n\n1. Добавить {obj}\n2. Удалить {obj}\n3. Отредактировать информацию о {obj}e\n", ConsoleColor.DarkMagenta);
            int choice = correctInput.EnterNumber(3);
            return choice;
        }
        public void WorkWithObject(bool returnChoice)
        {
            while (true)
            {
                var action = ChooseEditing(Name);
                switch (action)
                {
                    case 0:
                        returnChoice = true;
                        break;
                    case 1: 
                        Add();
                        break;
                    case 2: 
                        Delete();
                        break;
                    case 3: 
                        Edit();
                        break;
                }
                if (returnChoice)
                    break;
            }
        }

    }

    public class ChangeShowing : ChangeInfo
    {
        public ChangeShowing(List<Showing> showings, Dictionary<int, Movie> movies, Dictionary<int, CinemaHall> halls): base(showings, movies, halls)
        {
            
        }

        public override string Name => "сеанс";

        public override void Add()
        {

            (int id_hallToAdd, CinemaHall hallToAdd) = ChangeTheHall();
            (int id_movieToAdd, Movie movieToAdd) = ChangeTheMovie();
            var costs = hallToAdd.MakeCosts();
            Program.ColoredOutput("Введите дату и время сеанса:", ConsoleColor.DarkCyan);
            var date = correctInput.CheckIfDatetime(CanBePast: false);
            while (true)
            {
                var OneShowAtATime = hallToAdd.CheckOneShowAtATime(date, movieToAdd);
                if (!OneShowAtATime)
                {
                    Program.ColoredOutput("В это время будет пересечение с другим сеансом, введите другое время", ConsoleColor.DarkRed);
                    date = correctInput.CheckIfDatetime(CanBePast: false);
                }
                else
                    break;
            }
            var dateStr = date.ToString();
            var seats = new int[hallToAdd.rows, hallToAdd.seats];
            var showAdd = new Showing(dateStr, id_hallToAdd, costs, seats, id_movieToAdd, Revenue: 0);
            showAdd.defineHall(hallToAdd);
            showAdd.defineMovie(movieToAdd);
            showAdd.MakeEmptyHall();
            AllShows.Add(showAdd);
            Program.ColoredOutput("Сеанс добавлен", ConsoleColor.DarkGreen);
        }

        public override void Delete()
        {
            Console.WriteLine("Введите номер сеанса, который вы хотите удалить");
            Program.listShowings(AllShows);
            var choice = correctInput.EnterNumber(AllShows.Count, zeros: false);
            var showDelete = AllShows[choice - 1];
            if (showDelete.CountTickets() > 0)
                Program.ColoredOutput("Невозможно удалить этот сеанс, на него уже куплены билеты", ConsoleColor.DarkRed);
            else
            {
                AllShows.RemoveAt(choice - 1);
                Program.ColoredOutput("Сеанс удален", ConsoleColor.DarkGreen);
            }
        }

        public override void Edit()
        {
            Console.WriteLine("Введите номер сеанса, который вы хотите изменить");
            Program.listShowings(AllShows);
            var choice = correctInput.EnterNumber(AllShows.Count, zeros: false);
            var showEdit = AllShows[choice - 1];
            if (showEdit.CountTickets() > 0)
            {
                Console.WriteLine("На этот сеанс куплены билеты, вы не можете ничего изменить");
            }
            else
            {
                Console.WriteLine("Введите номер того, что вы хотите изменить.\n1. Дату и время сеанса\n2. Зал сеанса\n3. Фильм");
                var choice3 = correctInput.EnterNumber(3, zeros: false);
                switch (choice3)
                {
                    case 1:
                        showEdit.ChangeShowTime();
                        Program.ColoredOutput("Время изменено", ConsoleColor.DarkGreen);
                        break;
                    case 2:
                        (int id, CinemaHall hallChange) = ChangeTheHall();
                        showEdit.SetHallID(id);
                        showEdit.defineHall(hallChange);
                        var costs_new = showEdit.hall.MakeCosts();
                        showEdit.SetCosts(costs_new);
                        showEdit.MakeNewSeats();
                        showEdit.MakeEmptyHall();
                        Program.ColoredOutput("Зал изменен", ConsoleColor.DarkGreen);
                        break;
                    case 3:
                        (int mid, Movie movieToChange) = ChangeTheMovie();
                        showEdit.SetMovieID(mid);
                        showEdit.defineMovie(movieToChange);
                        Program.ColoredOutput("Фильм изменен", ConsoleColor.DarkGreen);
                        break;
                }

            }
        }

        public (int, CinemaHall) ChangeTheHall()
        {
            var correctInput = new CorrectInput();
            Program.ColoredOutput("Введите номер зала, в котором будет сеанс:", ConsoleColor.DarkCyan);
            Program.listHalls(AllHalls);
            var hall_id = correctInput.EnterDictKey(AllHalls.Keys.ToList());
            var hallReturn = AllHalls[hall_id];

            return (hall_id, hallReturn);
        }
        
        public (int, Movie) ChangeTheMovie()
        {
            var correctInput = new CorrectInput();
            Program.ColoredOutput("Введите номер фильма, который будет показываться на сеансе:", ConsoleColor.DarkCyan);
            Program.listMovies(AllMovies);
            var id_movieToAdd = correctInput.EnterDictKey(AllMovies.Keys.ToList());
            var movieToAdd = AllMovies[id_movieToAdd];
            return (id_movieToAdd, movieToAdd);
        }
    }

    public class ChangeMovie : ChangeInfo
    {
        public ChangeMovie(List<Showing> showings, Dictionary<int, Movie> movies):base(showings, movies)
        {
        }

        public override string Name => "фильм";

        public override void Add()
        {
            Program.ColoredOutput("Введите название фильма", ConsoleColor.DarkCyan);
            var name = Console.ReadLine();
            Program.ColoredOutput("Введите продолжительность фильма в минутах", ConsoleColor.DarkCyan);
            var duration = correctInput.EnterNumber(-1, bounds: false, zeros: false);
            Program.ColoredOutput("Введите возрастной рейтинг фильма («0+», «6+», «12+», «16+», «18+»)", ConsoleColor.DarkCyan);
            var rated = correctInput.CheckRated();
            AllMovies.Add(AllMovies.Keys.Max() + 1, new Movie(AllMovies.Keys.Max() + 1, name, duration, rated));
            Program.ColoredOutput("Фильм добавлен", ConsoleColor.DarkGreen);
        }

        public override void Delete()
        {
            Program.ColoredOutput("Выберите номер фильма, который вы хотите удалить", ConsoleColor.DarkCyan);
            Program.listMovies(AllMovies);
            bool flag1 = true;
            var delete = correctInput.EnterDictKey(AllMovies.Keys.ToList());
            for (int i = AllShows.Count - 1; i >= 0; i--)
            {
                if (AllShows[i].movie.ID == AllMovies[delete].ID && AllShows[i].CountTickets() > 0)
                {
                    flag1 = false;
                    Program.ColoredOutput("Невозможно удалить этот фильм, на его сеансы уже куплены билеты", ConsoleColor.DarkRed);
                    break;
                    //shows[i].ReturnTickets();
                    //shows.RemoveAt(i);
                }

            }
            if (flag1)
            {
                for (int i = AllShows.Count - 1; i >= 0; i--)
                {
                    if (AllShows[i].movie.name == AllMovies[delete].name)
                        AllShows.RemoveAt(i);
                }
                AllMovies.Remove(delete);
                Program.ColoredOutput("Фильм удален", ConsoleColor.DarkGreen);
            }
        }

        public override void Edit()
        {
            var movieEdit = Program.ChooseMovie(AllMovies);
            Program.ColoredOutput("Выберитее опцию:\n1. Изменить название фильма\n2. Изменить продолжительность фильма\n3.Изменить рейтинг фильма", ConsoleColor.DarkMagenta);
            var choice2 = correctInput.EnterNumber(3, zeros: false);
            switch (choice2)
            {
                case 1:
                    Program.ColoredOutput("Введите название фильма", ConsoleColor.DarkCyan);
                    var newName = Console.ReadLine();
                    movieEdit.ChangeName(newName);
                    break;
                case 2:
                    Program.ColoredOutput("Введите продолжительность фильма в минутах", ConsoleColor.DarkCyan);
                    var newDuration = correctInput.EnterNumber(-1, bounds: false, zeros: false);
                    movieEdit.ChangeDuration(newDuration);
                    break;
                case 3:
                    Program.ColoredOutput("Введите возрастной рейтинг фильма («0+», «6+», «12+», «16+», «18+»)", ConsoleColor.DarkCyan);
                    var newRated = correctInput.CheckRated();
                    movieEdit.ChangeRated(newRated);
                    break;
            }
            Program.ColoredOutput("Фильм изменен", ConsoleColor.DarkGreen);
        }
    }

    public class ChangeHall : ChangeInfo
    {
        public ChangeHall(List<Showing> showings, Dictionary<int, CinemaHall> halls):base(showings, halls)
        {

        }
        public override string Name => "зал";

        public override void Add()
        {
            var correctInput = new CorrectInput();
            Program.ColoredOutput("Введите название зала", ConsoleColor.DarkCyan);
            var name = Console.ReadLine();
            Program.ColoredOutput("Введите количество рядов", ConsoleColor.DarkCyan);
            var rows = correctInput.EnterNumber(-1, bounds: false, zeros: false);
            Program.ColoredOutput("Введите количество мест в ряду", ConsoleColor.DarkCyan);
            var seats = correctInput.EnterNumber(-1, bounds: false, zeros: false);
            AllHalls.Add(AllHalls.Keys.Max() + 1, new CinemaHall(AllHalls.Keys.Max() + 1, rows, seats, name));
            Program.ColoredOutput("Зал добавлен", ConsoleColor.DarkGreen);
        }

        public override void Delete()
        {
            var correctInput = new CorrectInput();
            Program.ColoredOutput("Выберите номер зала, который вы хотите удалить", ConsoleColor.DarkCyan);
            Program.listHalls(AllHalls);
            bool flag1 = true;
            var delete = correctInput.EnterDictKey(AllHalls.Keys.ToList());
            for (int i = AllShows.Count - 1; i >= 0; i--)
            {
                if (AllShows[i].hall_id == AllHalls[delete].ID && AllShows[i].CountTickets() > 0)
                {

                    flag1 = false;
                    Program.ColoredOutput("Невозможно удалить этот зал, на его сеансы уже куплены билеты", ConsoleColor.DarkRed);
                    break;
                    //shows[i].ReturnTickets();
                    //shows.RemoveAt(i);
                }

            }
            if (flag1)
            {
                for (int i = AllShows.Count - 1; i >= 0; i--)
                {
                    if (AllHalls[delete].IfDeleteShowing(AllShows[i]))
                        AllShows.RemoveAt(i);
                }
                AllHalls.Remove(delete);
                Program.ColoredOutput("Зал удален", ConsoleColor.DarkGreen);
            }
        }

        public override void Edit()
        {
            var hallEdit = Program.ChooseHall(AllHalls);
            Program.ColoredOutput("Вы можете изменить только название зала. Введите новое название:", ConsoleColor.DarkCyan);
            var newName = Console.ReadLine();
            hallEdit.ChangeName(newName);
            Program.ColoredOutput("Зал изменен", ConsoleColor.DarkGreen);
        }
    }

}
