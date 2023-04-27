using System;
namespace dz3
{
    
    public class UsersTicket
    {
        public Showing show { get; }
        public int row { get; }
        public int price { get; }
        public int seat { get; }

        public UsersTicket(Showing show, int row, int seat, int price)
        {
            this.price = price;
            this.show = show;
            this.row = row;
            this.seat = seat;
        }

        public Showing GetShowing()
        {
            return show;
        }

        public void GetDescription()
        {
            Console.WriteLine($"Фильм: {show.movie.name}\nКогда: {show.GetDateTime()}\n" +
                        $"Зал: {show.hall.name}, Ряд: {row}, Место: {seat}");
            Console.WriteLine();
        }
    }
}
