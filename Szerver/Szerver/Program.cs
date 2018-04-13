using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Szerver
{
   
    class Protokoll
    {
        public StreamReader r;
        public StreamWriter w;
        public string user = null;

        public Protokoll(TcpClient c)
        {
            this.r = new StreamReader(c.GetStream(), Encoding.UTF8);
            this.w = new StreamWriter(c.GetStream(), Encoding.UTF8);
        }

        public void StartKomm()
        {
            w.WriteLine("Aukció 1.0 béta");
            w.Flush();
            bool ok = true;
            while (ok)
            {
                string parancs = null;
                try
                {
                    string uzenet = r.ReadLine();
                    string[] param = uzenet.Split('|');
                    parancs = param[0].ToUpper();
                    switch (parancs)
                    {
                        //függvények érkeznek majd ide
                        case "LOGIN": Login(param[1],param[2]);break;
                        case "LOGOUT":Logout();break;
                        case "HELP":
                            {
                                Help();
                            }
                            break;
                        case "LIST":
                            {
                                Lista();
                            }
                            break;
                        case "BYE": w.WriteLine("BYE"); ok = false; break;
                        default: w.WriteLine("ERR|Ismeretlen parancs"); break;
                    }
                }
                catch (Exception e)
                {
                    w.WriteLine("ERR|{0}", e.Message);
                }
                w.Flush();
            }
            Console.WriteLine("A kliens elköszönt");
        }
        public void Login(string nev, string jelszo)
        {
            if (this.user != null)
            {
                w.WriteLine("Előbb jelentkezzen ki!");
            }
            else
            {
                this.user = nev;
                w.WriteLine("OK");

            }
        }
        public void Logout()
        {
            if (this.user == null)
            {
                w.WriteLine("Nincs bejelentkezve senki!");
            }
            else
            {
                this.user = null;
                w.WriteLine("Sikeres kijelentkezés!");
            }
        }
        private void Help()
        {
            w.WriteLine("OK*");
            w.WriteLine("LOGIN:                      Bejelentkezés felhasználónév|jelszó formátummal!");
            w.WriteLine("LOGOUT:                     Jelenleg bejelentkezett felhasználó kijelentkeztetése!");
            w.WriteLine("HELP:                       Ki listázza a megadható parancsokat!");
            w.WriteLine("LIST :                      Ki listázza a motorokat!");
            w.WriteLine("EXIT:                       Kilépés!");
            w.WriteLine("OK!");
        }
        void Lista()
        {
            string[] listam = File.ReadAllLines("lista.txt");
            w.WriteLine("OK*");
            foreach (var item in listam)
            {
                listam = listam[0].Split('|');
                w.WriteLine(item);
            }
            w.WriteLine("OK!");
        }
    }

    class Program
    {
        const int portSzam = 54325;
        const string ipcim = "127.0.0.1";

        static void Main(string[] args)
        {

            Console.WriteLine("Szerver elindult");
            try
            {
                IPAddress ip = IPAddress.Parse(ipcim);
                TcpListener k = new TcpListener(ip, portSzam);
                k.Start();
                while (true)
                {
                    Console.WriteLine("A szerver bejövő kapcsolatra vár");
                    TcpClient client = k.AcceptTcpClient();
                    Console.WriteLine("Valaki jött, akar vmit!");
                    Protokoll p = new Protokoll(client);
                    Thread t = new Thread(p.StartKomm);
                    t.Start();
                }
            }
            catch { Console.WriteLine("Nem indult el"); }
            Console.ReadLine();
        }
    }
}
