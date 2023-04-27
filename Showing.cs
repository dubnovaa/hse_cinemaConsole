using System;
using System.Collections.Generic;
//using System.Text.Json.Serialization;

namespace dz3
{
    public class Showing
    {
        private int revenue;
        public int Revenue { get => revenue; }
        private int[,] costs;
        public int[,] Costs { get => costs; }
        private int[,] freeSeats;
        public int[,] FreeSeats { get => freeSeats; }
        public CinemaHall hall { get; private set; }
        public Movie movie { get; private set; }
        public DateTime datetime { get; private set; }
        public int hall_id { get; private set; }
        public int movie_id { get; private set; }
        private List<UsersTicket> tickets = new List<UsersTicket>();
        private CorrectInput correctInput = new CorrectInput();

       
        public Showing(string datetime, int hall_id, int[,] Costs, int[,] FreeSeats, int movie_id, int Revenue)
        {
            this.datetime = DateTime.Parse(datetime);
            this.hall_id = hall_id;
            costs = Costs;
            this.movie_id = movie_id;
            freeSeats = FreeSeats;
            revenue = Revenue;
        }

        public void DefineNewShowing(CinemaHall hall, Movie mov)
        {
            this.hall = hall;
            movie = mov;
            MakeNewSeats();
            MakeEmptyHall();
            revenue = 0;
        }
        public void SetMovieID(int id)
        {
            movie_id = id;
        }
        public void SetHallID(int id)
        {
            hall_id = id;
        }
        public int CountTickets()
        {
            return tickets.Count;
        }
        public void SetDatetime(DateTime time)
        {
            datetime = time;
        }

        public DateTime GetDateTime()
        {
            return datetime;
        }
        public void defineHall(CinemaHall hall)
        {
            this.hall = hall;
        }
        public void MakeNewSeats()
        {
            freeSeats = new int[hall.rows, hall.seats];
        }
        public void defineMovie(Movie mov)
        {
            movie = mov;
        }
        //public void SetStartRevenue()
        //{
        //    Revenue = 0;
        //}

        public void AddTicket(UsersTicket ticket)
        {
            tickets.Add(ticket);
        }

        public void GetDescription()
        {
            Console.WriteLine($"\"{movie.name}\", {movie.rated}, {datetime}, зал: {hall.name}");
        }

        public void MakeEmptyHall()
        {
            for (int i = 0; i < hall.rows; i++)
            {
                for (int j = 0; j < hall.seats; j++)
                {
                    freeSeats[i, j] = 0;
                }
                
            }
        }

        public int[,] MakeEmptyHall(CinemaHall cinema_hall)
        {
            int[,] seats = new int[cinema_hall.rows, cinema_hall.seats];
            for (int i = 0; i < cinema_hall.rows; i++)
            {
                for (int j = 0; j < cinema_hall.seats; j++)
                {
                    seats[i, j] = 0;
                }

            }
            return seats;
        }

        public void SetCosts(int[,] new_costs)
        {
            costs = new_costs;
        }

        public void ShowCosts()
        {
            Console.WriteLine("Текущие цены на билеты (в рублях):");
            Console.Write("Место \t");
            for (int j = 0; j < hall.seats; j++)
                Console.Write($"№{j + 1}\t");
            Console.WriteLine();
            for (int i = 0; i < hall.rows; i++)
            {
                Console.Write($"Ряд {i + 1}:\t");
                for (int j = 0; j < hall.seats; j++)
                {
                    Console.Write($"{costs[i, j]} \t");
                }
                Console.WriteLine();
            }
        }

        //public void ReturnTickets()
        //{
        //    if (tickets.Count > 0)
        //    {
        //        foreach (var ticket in tickets)
        //        {
        //            ticket.buyer.ChangeBalance(ticket.price);
        //        }
        //        Console.WriteLine("Билеты на сеанс возвращены покупателям");
        //    }
        //    else
        //    {
        //        Console.WriteLine("На этот сеанс не было куплено билетов");
        //    }
            
        //}

        public void BuyTickets(User user)
        {
            while (true)
            {
                Program.ColoredOutput("\tЕсли хотите вернуться назад, введите ряд 0", ConsoleColor.DarkCyan);
                Console.WriteLine("Напишите номер ряда, в котором вы хотите купить место:");
                int row = correctInput.AccurateInput(hall.rows, zeros: true);
                if (row == 0)
                    break;
                Console.WriteLine($"Напишите номер места в ряду {row}, которое вы хотите купить:");
                int seat = correctInput.AccurateInput(hall.seats);

                if (freeSeats[row - 1, seat - 1] == 0)
                {
                    if (costs[row - 1, seat - 1] <= user.Balance)
                    {
                        freeSeats[row - 1, seat - 1] = 1;
                        user.RequestBalance(quiet: true, addedSum: -costs[row - 1, seat - 1]);
                        revenue += costs[row - 1, seat - 1];
                        var ticket = new UsersTicket(this, row, seat, costs[row - 1, seat - 1]);
                        user.AddTicket(ticket);
                        tickets.Add(ticket);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("На вашем счету недостаточно средств.");
                        Console.ResetColor();
                    }
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Это место занято, попробуйте другое");
                    Console.ResetColor();
                }
            }

        }

        public void ShowFreeSeats()
        {
            Console.WriteLine("Доступные места: (0 - место доступно,  1 - место выкуплено)");
            Console.Write("Место \t");
            for (int j = 0; j < hall.seats; j++)
                Console.Write($"№{j + 1}\t");
            Console.WriteLine();
            for (int i = 0; i < hall.rows; i++)
            {
                Console.Write($"Ряд {i + 1}:\t");
                for (int j = 0; j < hall.seats; j++)
                {
                    Console.Write($"{freeSeats[i, j]} \t");
                }
                Console.WriteLine();
            }
        }

        public (int, int) CountSeats()
        {
            int availableSeats = 0;
            int unavailableSeats = 0;
            for (int i = 0; i < hall.rows; i++)
            {
                for (int j = 0; j < hall.seats; j++)
                {
                    if (freeSeats[i, j] == 0)
                        availableSeats += 1;
                    else
                        unavailableSeats += 1;
                }

            }
            return (availableSeats, unavailableSeats);
        }

        

        public void ChangeCosts()
        {
            Console.WriteLine("Напишите номер ряда, в котором вы хотите изменить цену:");
            int row = correctInput.AccurateInput(hall.rows);
            Console.WriteLine($"Напишите номер места в ряду {row}, чью цену вы хотите изменить");
            int seat = correctInput.AccurateInput(hall.seats);
            Console.WriteLine("Напишите новую цену:");
            int newCost = correctInput.EnterNumber(-1, bounds: false, zeros: false);
            costs[row - 1, seat - 1] = newCost;
        }

        public void ChangeShowTime()
        {
            Program.ColoredOutput("Введите дату и время сеанса:", ConsoleColor.DarkCyan);
            var date = correctInput.CheckIfDatetime(CanBePast: false);
            while (true)
            {
                var OneShowAtATime = hall.CheckOneShowAtATime(date, movie, this);
                if (!OneShowAtATime)
                {
                    Program.ColoredOutput("В это время будет пересечение с другим сеансом, введите другое время", ConsoleColor.DarkRed);
                    date = correctInput.CheckIfDatetime(CanBePast: false);
                }
                else
                {
                    SetDatetime(date);
                    break;
                }
            }
        }

    }
}
