
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Kliens
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient conn = null;
            StreamReader r = null;
            StreamWriter w = null;
            try
            {
                string ip_add = "127.0.0.1"; int port_num = 1234;
                IPAddress ip = IPAddress.Parse(ip_add);
                conn = new TcpClient(ip_add, port_num);
                r = new StreamReader(conn.GetStream());
                w = new StreamWriter(conn.GetStream());
                Console.WriteLine("Connection established.");
                Console.WriteLine("Welcome in FRASZKÓ BANK Zrt. Netbank.");
                Console.WriteLine("If you are first time on our site, you can use the HELP command to get started.");
                Console.WriteLine();
            }
            catch
            {
                conn = null;
            }

            bool bye = false;
            while (!bye)
            {
                Console.WriteLine("Waiting for next command...!");
                string command = Console.ReadLine();
                w.WriteLine(command); w.Flush();
                string response = r.ReadLine();
                if (response == "BYE")
                    bye = true;
                if (response == "OK" || response.Substring(0, 2) == "ERR")
                {
                    Console.WriteLine(response);
                }
                else if (response == "OK*")
                {
                    while (response != "OK!")
                    {
                        response = r.ReadLine();
                        Console.WriteLine(response);
                    }
                }

                else
                    Console.WriteLine(response);
            }
        }
    }
}
