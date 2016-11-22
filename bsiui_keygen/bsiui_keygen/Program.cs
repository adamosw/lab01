using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bsiui_keygen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("USER: ");
            string login = Console.ReadLine();
            int number;

            bool isSucceeded = int.TryParse(login, out number);

            if (isSucceeded)
            {
                int i = 0;
                int j;
                int k = 27;
                int modulo;
                int tmp;
                List<char> result = new List<char>();
                List<char> alphabet = "abcdefghijklmnopqrstuvwxyz0".ToCharArray().ToList();
                
                while (i < 64 && i <= 25)
                {
                    j = i + (i * 8);
                    j = j + number;
                    tmp = j;
                    j = j / k;
                    modulo = tmp % k;
                    result.Add(alphabet[modulo]);

                    i = i + 1;
                }

                i = 26;
                k = 27;

                while (i < 64 && i <= 51)
                {
                    j = i * 7;
                    j = j + number;
                    tmp = j;
                    j = j / k;
                    modulo = tmp % k;
                    result.Add(alphabet[modulo]);

                    i = i + 1;
                }

                Console.WriteLine("KEY: {0}", new string(result.ToArray()));
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Invalid number.");
                Console.ReadLine();
            }
        }
    }
}
