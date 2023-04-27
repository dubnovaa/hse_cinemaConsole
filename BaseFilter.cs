using System;
using System.Collections.Generic;

namespace dz3
{
    public class BaseFilter
    {
        protected CorrectInput correctInput = new CorrectInput();
        protected List<Showing> result = new List<Showing>();
        protected List<Showing> AllShows;
        protected Dictionary<int, Movie> AllMovies;
        protected Dictionary<int, CinemaHall> AllHalls;
        
        public BaseFilter(List<Showing> showings)
        {
            AllShows = showings;
        }
        public BaseFilter(List<Showing> showings, Dictionary<int, Movie> movies)
        {
            AllShows = showings;
            AllMovies = movies;
        }
        public BaseFilter(List<Showing> showings, Dictionary<int, CinemaHall> halls)
        {
            AllShows = showings;
            AllHalls = halls;
        }
        public virtual List<Showing> Filter()
        {
            result = AllShows;
            return result;
        }

    }
    
    public class FilterByShowing : BaseFilter
    {
        public FilterByShowing(List<Showing> showings): base(showings) 
        {

        }

        public override List<Showing> Filter()
        {
            (bool exit, Showing show) = Program.ChooseShowingToWork(AllShows, zeros: false);
            result.Add(show);
            return result;
        }
    }
    public class FilterByTime : BaseFilter
    {
        public FilterByTime(List<Showing> showings) : base(showings)
        {

        }

        public override List<Showing> Filter()
        {
            Program.ColoredOutput("Введите начало периода:", ConsoleColor.DarkCyan);
            var start = correctInput.CheckIfDatetime();
            Program.ColoredOutput("Введите конец периода:", ConsoleColor.DarkCyan);
            var finish = correctInput.CheckIfDatetime();

            foreach (var sh in AllShows)
            {
                if (sh.datetime >= start && sh.datetime <= finish)
                    result.Add(sh);
            }
            return result;
        }
    }
    public class FilterByRated : BaseFilter
    {
        public FilterByRated(List<Showing> showings) : base(showings)
        {

        }

        public override List<Showing> Filter()
        {
            Program.ColoredOutput("Введите возрастной рейтинг фильма («0+», «6+», «12+», «16+», «18+»)", ConsoleColor.DarkCyan);
            var rated = correctInput.CheckRated();
            foreach (var sh in AllShows)
            {
                if (sh.movie.rated == rated)
                    result.Add(sh);
            }
            return result;
        }
        
    }

    public class FilterByMovie : BaseFilter
    {
        public FilterByMovie(List<Showing> showings, Dictionary<int, Movie> movies) : base(showings, movies)
        {

        }

        public override List<Showing> Filter()
        {
            var mov = Program.ChooseMovie(AllMovies);
            foreach (var sh in AllShows)
            {
                if (sh.movie_id == mov.ID)
                    result.Add(sh);
            }
            return result;
        }
    }
    public class FilterByHall : BaseFilter
    {
        public FilterByHall(List<Showing> showings, Dictionary<int, CinemaHall> halls) : base(showings, halls)
        {

        }
        public override List<Showing> Filter()
        {
            var hall = Program.ChooseHall(AllHalls);
            foreach (var sh in AllShows)
            {
                if (sh.hall_id == hall.ID)
                    result.Add(sh);
            }
            return result;
        }
    }
}
