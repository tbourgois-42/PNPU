using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PNUDispatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Watcher wWatcher = new Watcher();

            // Wait for the user to quit the program.
            Console.WriteLine("Press 'q' to quit the sample.");
            while (Console.Read() != 'q') ;


        }
    }
}
