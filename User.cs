using System;
using System.Linq;
using System.Collections.Generic;

namespace dz3
{
    public class User
    {
        private int _balance;
        public string name { get; private set; }
        //public List<(Showing, int, int)> tickets { get; set; }
        public List<UsersTicket> tickets { get; private set; }
        public int Balance { get => _balance; }


        public User(string name, int Balance, List<UsersTicket> tickets)
        {
            this.name = name;
            this.tickets = tickets;
            _balance = Balance;
        }
        public void ShowBalance()
        {
            Console.WriteLine($"Ваш баланс = {_balance}");
        }


        //public void CreateTicketList()
        //{
        //    tickets = new List<UsersTicket>();
        //}

        public void RequestBalance(bool quiet = false, int addedSum = 0)
        {
            if (!quiet)
            {
                var correctInput = new CorrectInput();
                Console.WriteLine("Введите сумму, которую добавить на ваш счет:");
                addedSum = correctInput.EnterNumber(-1, bounds: false, zeros: true); //значит внутри метода не будет ограничителя ввода, так как внести можно любую неотрицательную сумму

            }
            
            SetBalance(addedSum);
        }

        private void SetBalance(int balance)
        {
            _balance += balance;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"Баланс изменен. Ваш баланс = {_balance}");
            Console.ResetColor();
        }

        public void AddTicket(UsersTicket ticket)
        {
            tickets.Add(ticket);
        }

        //public void CheckValidTickets(List<Showing> shows)
        //{
        //    for (int i = tickets.Count - 1; i >= 0; i--)
        //    {
        //        if (!shows.Contains(tickets[i].show))
        //            tickets.RemoveAt(i);
        //    }
        //}

        public void ShowTickets()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Ваши билеты:");
            Console.ResetColor();
            if (tickets.Count > 0)
            {
                foreach (var ticket in tickets)
                {

                    ticket.GetDescription();
                }
            }
            else
                Console.WriteLine("У вас нет билетов.");
            
        }

        public int CountTickets()
        {
            return tickets.Count;
        }

        public int CountExpenditures()
        {
            var expences = 0;
            foreach (var ticket in tickets)
            {
                expences += ticket.price;
            }
            return expences;
        }


        public int CountDifferentShows()
        {
            var unique_shows = 0;
            var ticketsSortedByTime = tickets.GroupBy(x => x.show.datetime);
            foreach (var time in ticketsSortedByTime)
            {
                var halls_id = tickets.Where(x => x.show.datetime == time.Key).Select(x => x.show.hall_id);
                var unique_id = new HashSet<int>(halls_id);
                unique_shows += unique_id.Count();
            }
            return unique_shows;
        }
    }
    
}