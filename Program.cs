using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSSC.PRT.PNT7.Domain.Data;
using WSSC.PRT.PNT7.Domain.Services;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (var ctx = Db.DataContext)
            // {
             new LyncService(null).CreateMeeting();
                
            //}
        }
    }
}
