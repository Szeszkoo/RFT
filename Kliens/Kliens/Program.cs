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
            TcpClient csatl = null;
            StreamReader r = null;
            StreamWriter w = null;
            string udv = "";
            try
            {
                string ipcim = "127.0.0.1"; int portszam = 54325;
                IPAddress ip = IPAddress.Parse(ipcim);
                csatl = new TcpClient(ipcim, portszam);
                r = new StreamReader(csatl.GetStream());
                w = new StreamWriter(csatl.GetStream());
                Console.WriteLine("Csatl. ok");

            }
            catch
            {
                csatl = null;
            }

            //kezdődik!:-)))
            udv = r.ReadLine();
            bool bye = false;
            while (!bye)
            {
                Console.WriteLine("Kérem a parancsot!");
                string parancs = Console.ReadLine();
                w.WriteLine(parancs); w.Flush();
                string valasz = r.ReadLine();
                if (valasz == "BYE")
                    bye = true;
                if (valasz == "OK" || valasz.Substring(0, 2) == "ERR")
                {
                    Console.WriteLine(valasz);
                }
                else
                {
                    if (valasz == "OK*")
                    {
                        while (valasz != "OK!")
                        {
                            valasz = r.ReadLine();
                            Console.WriteLine(valasz);
                        }
                    }
                }
            }
        }
    }
}
