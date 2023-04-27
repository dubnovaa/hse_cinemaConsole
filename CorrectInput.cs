using System;
using System.Collections.Generic;
using System.Linq;

namespace dz3
{
    public class CorrectInput
    {
       
            public int CheckIfNumber() //ждет, пока не введут натуральное число. возвращает корректный ввод
            {
                var input = Console.ReadLine();
                bool res = int.TryParse(input, out int x);
                while (!res)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Некорректный ввод, попробуйте еще раз:");
                    Console.ResetColor();
                    input = Console.ReadLine();
                    res = int.TryParse(input, out x);
                }

                return x;
            }

        public DateTime CheckIfDatetime(bool CanBePast = true)
        {
            Console.WriteLine("Введите дату и время в формате мм/дд/гггг чч:мм или гггг-мм-дд чч:мм\nЕсли дата не уточнена, будет взят сегодняшний день\nЕсли время не уточнено, то оно будет считаться как 00:00");
            var input = Console.ReadLine();
            bool correct = DateTime.TryParse(input, out DateTime res);
            while (!correct || (!CanBePast && res < DateTime.Now) )
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Некорректный ввод или дата должна быть в будущем, попробуйте еще раз:");
                Console.ResetColor();
                input = Console.ReadLine();
                correct = DateTime.TryParse(input, out res);
            }
            return res;
        }

            public bool NumberIsPositive(int input, bool zeros=true)
            {
                bool res;
                if (input >= 0 && zeros)
                    res = true;
                else if (!zeros && input > 0)
                    res = true;
                else
                    res = false;
                return res;

            }


            public bool NumberWithinBounds(int input, int limiter, bool zeros) //проверяет, подходит ли введенное число в рамки выбора, который был поставлен. 
            { //bool zeros говорит о том, допускаются ли в предоставленном выборе нули
                bool res;
                if (zeros && (0 <= input) && (input <= limiter))
                    res = true;
                else if (!zeros && (0 < input) && (input <= limiter))
                    res = true;
                else
                    res = false;
                return res;


            }
            public int AccurateInput(int limiter, bool zeros=false) //метод специально для проверки корректности введенного места или ряда
            {
                CorrectInput correctInput = new CorrectInput();
                int res;
                while (true)
                {
                    res = correctInput.CheckIfNumber();
                    if (zeros && 0 <= res && res <= limiter)
                        break;
                    else if (1 <= res && res <= limiter)
                        break;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Некорректный ряд/место, попробуйте еще раз");
                    Console.ResetColor();
                }
                return res;

            }

        public string CheckRated()
        {
            string str;
            while (true)
            {
                str = Console.ReadLine().Trim('+');
                
                var bools = new List<bool> { str == "0", str == "6", str == "12", str == "16", str == "18" };
                
                if (bools.Any(x=> x== true))
                    break;
                Console.WriteLine("Некорректный ввод, попробуйте еще раз (в формате \'0\' или \'0+\')");
            }

            return string.Concat(str, "+");
        }

        public int EnterDictKey(List<int> options)
        {
            int choice = CheckIfNumber() - 1;
            bool flag = false;
            while (true)
            {
                foreach (int i in options)
                {
                    if (choice == i)
                    {
                        flag = true;
                        break;
                    } 
                }
                if (flag)
                    break;
                else
                {
                    Program.ColoredOutput("Нет такого варианта, попробуйте еще раз", ConsoleColor.DarkRed);
                    choice = CheckIfNumber() - 1;
                    
                }
                   
                
            }
            return choice;
        }

            public int EnterNumber(int limiter, bool bounds = true, bool zeros = true) //полная проверка вводимого числа
            {// limiter - максимальный возможный вариант ответа, bounds - нужен ли limiter (там, где bounds=false, limiter можно ставить любым, он не будет использоваться)
             //zeros - может ли введенное число быть 0
                var choice = CheckIfNumber();
                bool flag;
                if (bounds) //есть варианты ответа
                {
                    flag = NumberWithinBounds(choice, limiter, zeros);
                    while (!flag)
                    {
                        Program.ColoredOutput("Такого варианта нет, попробуйте еще раз:", ConsoleColor.DarkRed);
                        choice = CheckIfNumber();
                        flag = NumberWithinBounds(choice, limiter, zeros);
                    }

                }
                else
                {
                    flag = NumberIsPositive(choice, zeros);
                    while (!flag)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Число должно быть положительным:");
                        Console.ResetColor();
                        choice = CheckIfNumber();
                        flag = NumberIsPositive(choice, false);
                    }
                }
                return choice;
            }


        
    }
}
