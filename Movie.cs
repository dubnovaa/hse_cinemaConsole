using System;
namespace dz3
{
    public class Movie
    {
        public string name { get; private set; }
        public int duration { get; private set; }
        public string rated { get; private set; }
        private int mid; //movie id
        public int ID { get => mid; }

        public Movie(int ID, string name, int duration, string rated)
        {
            mid = ID;
            this.name = name;
            this.duration = duration;
            this.rated = rated;
        }

        public void ChangeName(string name)
        {
            this.name = name;
        }
        public void ChangeDuration(int time)
        {
            duration = time;
        }
        public void ChangeRated(string rating)
        {
            rated = rating;
        }

        public void GetDescription()
        {
            Console.WriteLine($"\"{name}\", {rated}, {duration} мин");
        }
       
    }
}
