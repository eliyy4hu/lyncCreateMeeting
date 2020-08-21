using System;
using WSSC.PRT.PNT7.Domain.Services;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
             new LyncService().CreateMeeting();
        }
    }
}
