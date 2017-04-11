using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boggle
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.HttpStatusCode status;
            UserInfo name = new UserInfo { Nickname = "Joe" };
            BoggleService service = new BoggleService();
            UserID user = service.CreateUser(name, out status);
            Console.WriteLine(user.UserToken);
            Console.WriteLine(status.ToString());

            // This is our way of preventing the main thread from
            // exiting while the server is in use
            Console.ReadLine();
        }
    }
}
