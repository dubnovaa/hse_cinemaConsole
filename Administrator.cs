using System;
using System.Collections.Generic;

namespace dz3
{
    public class Administrator
    {
        private string _password;
        public Administrator()
        {
            _password = "123";
        }
        public bool Authorisation()
        {
            bool flag = false;
            Console.WriteLine("Введите пароль (123):");
            string password = Console.ReadLine();
            string choice = "";
            while (true)
            {

                if (_password == password || choice == _password)
                {
                    flag = true;
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Неверный пароль, введите пароль еще раз. Если хотите вернуться в основное меню, нажмите 0");
                    Console.ResetColor();
                    choice = Console.ReadLine();
                    if (choice == "0")
                        break;
                }

            }
            return flag;
        }
        public void ChangeCosts(List<Showing> shows)
        {
            while (true)
            {
                (bool exit, Showing show) = Program.ChooseShowingToWork(shows);
                if (exit)
                    break;
                show.ShowCosts();
                show.ChangeCosts();
            }
        }
    }
}
