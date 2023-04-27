using System;
using System.Collections.Generic;
namespace dz3
{
    public class CinemaHall
    {
        private int id;
        public int ID { get => id; }
        public int rows { get; }
        public int seats { get; }
        private List<Showing> showings = new List<Showing>();
        public string name { get; private set; }

        public CinemaHall(int ID, int rows, int seats, string name)
        {
            id = ID;
            this.rows = rows;
            this.seats = seats;
            this.name = name;
        }

        public void GetDescription()
        {
            Console.WriteLine($"\"{name}\", Размер: {rows} х {seats} мест");
        }

        public void AddShowing(Showing show)
        {
            showings.Add(show);
        }

        public void ChangeName(string newName)
        {
            name = newName;
        }
        

        public (int,  int) CountSeats()
        {
            int freeSeats = 0;
            int boughtSeats = 0;
            foreach (var show in showings)
            {
                (int available, int unavailable) = show.CountSeats();
                freeSeats += available;
                boughtSeats += unavailable;
            }

            return (freeSeats, boughtSeats);
        }

        public bool CheckOneShowAtATime(DateTime dt, Movie mov)
        {
            var newTime = dt.AddMinutes(mov.duration);
            bool OneShowAtATime = true;
            foreach (var sh in showings)
            {
                if (dt <= sh.datetime.AddMinutes(sh.movie.duration) && sh.datetime <= dt)
                    OneShowAtATime = false;
                else if (dt <= sh.datetime && sh.datetime <= newTime)
                    OneShowAtATime = false;
            }
            return OneShowAtATime;
        }

        public bool CheckOneShowAtATime(DateTime dt, Movie mov, Showing show)
        {
            var newTime = dt.AddMinutes(mov.duration);
            bool OneShowAtATime = true;
            var showingsToCompare = new List<Showing>(showings);
            showingsToCompare.Remove(show);
            foreach (var sh in showingsToCompare)
            {
                if (dt <= sh.datetime.AddMinutes(sh.movie.duration) && sh.datetime <= dt)
                    OneShowAtATime = false;
                else if (dt <= sh.datetime && sh.datetime <= newTime)
                    OneShowAtATime = false;
            }
            return OneShowAtATime;
        }
        public bool IfDeleteShowing(Showing show)
        {
            foreach (var sh in showings)
            {
                if (Program.CompareShows(show, sh))
                {
                    return true;
                }
            }
            return false;
        }

        public int[,] MakeCosts()
        {
            var correctInput = new CorrectInput();
            var new_costs = new int[rows, seats];
            Program.ColoredOutput("Введите новые цены на сеанс:", ConsoleColor.DarkCyan);
            for (int i = 0; i < rows; i++)
            {
                Console.WriteLine($"Введите цены ряда {i + 1}:");
                for (int j = 0; j < seats; j++)
                {
                    Console.Write($"Место {j + 1}:");
                    var cost = correctInput.EnterNumber(-1, bounds: false, zeros: false);
                    new_costs[i, j] = cost;
                }
            }
            return new_costs;
        }
    }
}
