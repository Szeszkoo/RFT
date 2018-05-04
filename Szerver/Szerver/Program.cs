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
    class Users
    {
        private string jelszo;

        public string Jelszo
        {
            get { return jelszo; }
            set { jelszo = value; }
        }
        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        public Users()
        {

        }
        public Users(string username, int osszeg)
        {
            this.username = username;
            this.osszeg = osszeg;
        }
    }
    class Szamla
    {
        private int osszeg;

        public int Osszeg
        {
            get { return osszeg; }
            set { osszeg = value; }
        }
        private string fioknev;

        public string Fioknev
        {
            get { return fioknev; }
            set { fioknev = value; }
        }
        public Szamla()
        {

        }
        public Szamla(string fiokneve, int osszeg)
        {
            this.fioknev = fiokneve;
            this.osszeg = osszeg;
        }
    }
    class Protokoll
    {
        public StreamReader r;
        public StreamWriter w;
        public StreamWriter w2;
        TcpClient client = null;
        public string user = null;
        public bool admin = false;
        List<string> online = new List<string>();
        List<Users> Felhasznaloklista = new List<Users>();

        public Protokoll(TcpClient c)
        {
            this.client = c;
            r = new StreamReader(c.GetStream(), Encoding.UTF8);
            w = new StreamWriter(c.GetStream(), Encoding.UTF8);
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
                        case "LOGIN":
                            {
                                Login(param[1], param[2]);
                                break;
                            }
                        case "LOGOUT": Logout(); break;
                        case "REGISTER":
                            {
                                if (Register(param[1], param[2]) == true)
                                {
                                    w2 = new StreamWriter("../../users.txt", true);
                                    w2.WriteLine("{0}|{1}", param[1], param[2]);
                                    w2.Flush();
                                    w2.Close();
                                }
                                break;
                            }
                        case "USERDEL":
                            {
                                if (admin == true)
                                    UserDelete(param[1], param[2]);
                                else
                                {
                                    w.WriteLine("Te nem adhatsz hozzá nem vagy admin");
                                }

                            }
                            break;
                        case "USERLIST":
                            {
                                if (admin == true)
                                    userLista();
                                else
                                {
                                    w.WriteLine("Te nem kérheted le ezt nem vagy admin");
                                }
                            }
                            break;
                        case "UTAL":
                            {
                                utal(param[1], int.Parse(param[2]));
                            }
                            break;
                        case "SZAMLA":
                            {
                                Szamla();
                            }
                            break;
                        //case "ONLINE":
                        //    {
                        //        onlineUserek();
                        //    }
                        //    break;

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

        public void utal(string felhasznalonak, int osszeg)
        {
            string FilePath = "../../szamla.txt";
            var text = new StringBuilder();
            Users szemely = new Users();
            //szemely.Username = "gealo";
            //szemely.Osszeg = 1300;
            //foreach (var item in Felhasznaloklista)
            //{
            //    w.WriteLine(item);
            //}
            w.WriteLine(Felhasznaloklista);
            foreach (string s in File.ReadAllLines(FilePath))
            {
                //text.AppendLine(s.Replace(felhasznalonak + "|", felhasznalonak + "|" + osszeg)); //Convert.ToString(osszeg)));

                //text.AppendLine(s.Remove(6, 11));
            }
                szemely.Username = felhasznalonak;
                szemely.Osszeg = osszeg;
                Felhasznaloklista.Add(szemely);
            w.WriteLine("OK");

            using (var file = new StreamWriter(File.Create(FilePath)))
            {
                file.Write(text.ToString());
            }
        }

        public bool Register(string nev, string jelszo)
        {
            string[] paramS;
            string line;
            int i = 1;
            int lineCount = File.ReadLines("../../users.txt").Count();
            while (i <= lineCount)
            {
                line = File.ReadLines("../../users.txt").Skip(i - 1).Take(1).First();
                paramS = line.Split('|');
                if (nev == paramS[0])
                {
                    w.WriteLine("Ilyen felhasználónév már létezik, próbáld újra!");
                    return false;
                }
                i++;
            }
            w.WriteLine("OK");
            return true;
        }
        #region earlierUserdel
        //public bool UserDelete(string nev)
        //{
        //if (this.user == null)
        //{
        //    w.WriteLine("Előbb jelentkezz be!");
        //    return false;
        //}
        //else if (admin == false)
        //{
        //    w.WriteLine("Nincs jogosultságod ehhez!");
        //    return false;
        //}
        //else
        //{
        //    string[] paramS;
        //    string line = null;
        //    int i = 1;
        //    int lineCount = File.ReadLines("../../users.txt").Count();
        //    while (i <= lineCount)
        //    {

        //        line = File.ReadLines("../../users.txt").Skip(i - 1).Take(1).First();
        //        paramS = line.Split('|');
        //        if (nev == paramS[0])
        //        {
        //            w2 = new StreamWriter("../../users.txt", true);
        //            w2.WriteLine(Environment.NewLine); // itt egyenlőre mutyi van xd
        //            w2.Flush();
        //            w2.Close();
        //            w.WriteLine("OK");
        //            return true;
        //        }
        //        i++;
        //    }
        //    w.WriteLine("Ilyen felhasználó nem létezik!");
        //    return false;
        //}
        //}
        #endregion
        private void UserDelete(string nev, string jelszo)
        {
            string FilePath = "../../users.txt";
            var text = new StringBuilder();

            foreach (string s in File.ReadAllLines(FilePath))
            {
                text.AppendLine(s.Replace(nev + "|" + jelszo, ""));
            }
            w.WriteLine("OK");

            using (var file = new StreamWriter(File.Create(FilePath)))
            {
                file.Write(text.ToString());
            }

        }
        void userLista()
        {
            string[] listam = File.ReadAllLines("../../users.txt");
            w.WriteLine("OK*");
            foreach (var item in listam)
            {
                listam = listam[0].Split('|');
                w.WriteLine(item);
            }
            w.WriteLine("OK!");

        }
        void Szamla()
        {
            string[] listam = File.ReadAllLines("../../szamla.txt");
            w.WriteLine("OK*");
            foreach (var item in listam)
            {
                listam = listam[0].Split('|');
                w.WriteLine(item);
            }
            w.WriteLine("OK!");
        }
        //void onlineUserek()
        //{
        //    w.WriteLine("OK*");
        //    foreach (var item in online)
        //    {
        //        w.WriteLine("online: " + item);
        //    }
        //    w.WriteLine("OK!");
        //}

        public void Login(string nev, string jelszo)
        {
            if (this.user != null)
            {
                w.WriteLine("Előbb jelentkezzen ki!");
            }
            else if (nev == "admin")
            {
                admin = true;
                this.user = nev;
                w.WriteLine("Üdvözlünk a kormánynál,admin");
            }
            else
            {
                //online.Add(nev);
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
            if (user == "admin")
            {
                admin = false;
                this.user = null;
                w.WriteLine("Sikeres kijelentkezés,adminom!");
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
            w.WriteLine("REGISTER:                   Rgisztráció felhasználónév|jelszó formátummal!");
            w.WriteLine("USERDEL:                    Felhasználók törlése felhasználónév|jelszó formátummal!(ADMIN ONLY");
            w.WriteLine("USERLIST:                   Ki listázza a felhasználókat!(ADMIN ONLY");
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
        const int portSzam = 1234;
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
                    Console.WriteLine("Érkezett egy kliens!");
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
