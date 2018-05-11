﻿using System;
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

    class Bank
    {
        string username, password;
        int money;

        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                username = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value;
            }
        }

        public int Money
        {
            get
            {
                return money;
            }

            set
            {
                money = value;
            }
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
        public List<Bank> banklist = new List<Bank>();
        bool utalas;
        string receiver;
        bool borrow;

        public Protokoll(TcpClient c)
        {
            this.client = c;
            r = new StreamReader(c.GetStream(), Encoding.UTF8);
            w = new StreamWriter(c.GetStream(), Encoding.UTF8);
        }
        public void Read_users()
        {
            StreamReader reader = new StreamReader("../../users.txt");
            while (reader.Peek() >= 0)
            {
                Bank m = new Bank();
                string[] tmp = reader.ReadLine().Split('|');
                m.Username = tmp[0];
                m.Password = tmp[1];
                m.Money = int.Parse(tmp[2]);
                banklist.Add(m);
            }
            reader.Close();
        }
        public void Overwrite_users() //felülírja az adott sort csak a pénz változtatva
        {
            // Read_users();
            int i = 0;
            int lineCount = File.ReadLines("../../users.txt").Count();
            string line;
            string[] paramS;
            string[] lines = File.ReadAllLines("../../users.txt");
            // w.WriteLine("OK*");
            while (i < lineCount)
            {
                line = File.ReadLines("../../users.txt").Skip(i).Take(1).First();
                paramS = line.Split('|');

                //Utalás esetén feltölti a felhasználónak
                if (utalas == true)
                    if (this.receiver == paramS[0] && banklist[i].Username == paramS[0])
                    {
                        lines[i] = banklist[i].Username + "|" + banklist[i].Password + "|" + banklist[i].Money;
                        File.WriteAllLines("../../users.txt", lines);
                        utalas = false;
                    }
                //Ha kölcsönről van szó a banktól vonja ke
                if (borrow == true && banklist[i].Username == "bank")
                {
                    lines[0] = banklist[i].Username + "|" + banklist[i].Password + "|" + banklist[i].Money;
                    File.WriteAllLines("../../users.txt", lines);
                    borrow = false;
                }

                if (this.user == paramS[0] && banklist[i].Username == paramS[0])
                {
                    lines[i] = banklist[i].Username + "|" + banklist[i].Password + "|" + banklist[i].Money;

                    File.WriteAllLines("../../users.txt", lines);
                }
                i++;
            }
            //  w.WriteLine("OK!");

        }
        public void Create_file(string nev)
        {
            string path = @"../../" + nev + ".txt";
            if (!File.Exists(path))
            {
                File.CreateText(path);
            }
        }
        public void Unique_user_write(string nev, string data, string person_name, string task)
        {
            string path = @"../../" + nev + ".txt";
            StreamWriter writer = new StreamWriter(path, true);
            string content = "";

            foreach (var z in banklist)
            {
                if (z.Username == nev)
                {
                    switch (task)
                    {
                        case "DEPOSIT":
                            {
                                content = DateTime.Now + "     Deposited: +" + data + "Ft." + "               New Balance: " + z.Money + "Ft.";
                                writer.WriteLine(content);
                                writer.Flush();
                                writer.Close();
                                content = "";
                            }
                            break;
                        case "WITHDRAW":
                            {
                                content = DateTime.Now + "     Withdraw: -" + data + "Ft." + "               New Balance: " + z.Money + "Ft.";
                                writer.WriteLine(content);
                                writer.Flush();
                                writer.Close();
                                content = "";
                            }
                            break;
                        case "TRANSFER":
                            {
                                content = DateTime.Now + "     Transfered: " + data + "Ft." + "    To: " + person_name + "              New Balance: " + z.Money + "Ft.";
                                writer.WriteLine(content);
                                writer.Flush();
                                writer.Close();
                                content = "";
                            }
                            break;
                        case "BORROW":
                            {
                                content = DateTime.Now + "     Borrowed: " + data + "Ft." + "      From: BANK" + "             New Balance: " + z.Money + "Ft.";
                                writer.WriteLine(content);
                                writer.Flush();
                                writer.Close();
                                content = "";
                            }
                            break;
                    }
                }
            }
        }

        public void StartKomm()
        {
            bool ok = true;
            Read_users();
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
                        //érkezett azóta pár hehe
                        case "LOGIN":
                            {
                                Login(param[1], param[2]);
                            }
                            break;
                        case "LOGOUT": Logout(); break;
                        case "REGISTER":
                            {
                                if (Register(param[1], param[2]) == true)
                                {
                                    w2 = new StreamWriter("../../users.txt", true);
                                    w2.WriteLine("{0}|{1}|0", param[1], param[2]);
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
                                    UserList();
                                else
                                {
                                    w.WriteLine("Te nem kérheted le ezt nem vagy admin");
                                }
                            }
                            break;
                        case "DEPOSIT":
                            {
                                Deposit(Math.Abs(int.Parse(param[1])));
                                Overwrite_users();
                                Unique_user_write(this.user, param[1], null, "DEPOSIT");
                            }
                            break;
                        case "WITHDRAW":
                            {
                                Withdraw(Math.Abs(int.Parse(param[1])));
                                Overwrite_users();
                                Unique_user_write(this.user, param[1], null, "WITHDRAW");
                            }
                            break;

                        case "TRANSFER":
                            {
                                Transfer(param[1], Math.Abs(int.Parse(param[2])));
                                Overwrite_users();
                                Unique_user_write(this.user, param[2], param[1], "TRANSFER");
                            }
                            break;
                        case "BORROW":
                            {
                                Borrow(Math.Abs(int.Parse(param[1])));
                                Overwrite_users();
                                Unique_user_write(this.user, param[1], null, "BORROW");
                            }
                            break;

                        case "BALANCE": Balance(); break;
                        case "HISTORY": History(); break;
                        case "HELP": Help(); break;
                        case "BYE": w.WriteLine("BYE"); ok = false; break;
                        default: w.WriteLine("ERR|Ismeretlen parancs"); break;
                    }
                }
                catch (Exception e)
                {
                    w.WriteLine(e.Message);
                }
                w.Flush();
            }
            Console.WriteLine("A kliens elköszönt");
        }

        public void History()
        {
            if (this.user == null)
            {
                w.WriteLine("Előbb jelentkezzen be!");
            }
            else
            {
                string path = @"../../" + this.user + ".txt";
                string[] content = File.ReadAllLines(path);
                w.WriteLine("OK*");
                foreach (var z in content)
                {
                    w.WriteLine(z);
                }
                w.WriteLine("OK!");
            }
        }
        public void Deposit(int amount)
        {
            if (this.user == null)
            {
                w.WriteLine("Előbb jelentkezzen be!");
            }
            else
            {
                // Read_users();
                foreach (var z in banklist)
                {
                    if (z.Username == this.user)
                    {
                        z.Money += amount;
                    }
                }
                w.WriteLine("Feltöltötted az egyenleged:{0} Forintal", amount);
                // w.WriteLine("We have put up:{0} Ft ", amount);

            }
        }
        public void Withdraw(int amount)
        {
            if (this.user == null)
            {
                w.WriteLine("Előbb jelentkezzen be!");
            }
            else
            {
                // Read_users();
                foreach (var z in banklist)
                {
                    if (z.Username == this.user)
                    {
                        z.Money -= amount;
                    }
                }
                w.WriteLine("Kivettél a számládról:{0} Forintot ", amount);
            }
        }

        public void Balance()
        {
            if (this.user == null)
            {
                w.WriteLine("Előbb jelentkezzen be!");
            }
            else
            {
                //   Read_users();
                //   w.WriteLine("OK*");
                foreach (var z in banklist)
                {
                    if (this.user == z.Username)
                    {
                        w.WriteLine("'" + z.Username.ToUpper() + "'" + " felhasználóhoz tartozó egyenleg: " + z.Money + "Ft.");
                    }
                }
                //   w.WriteLine("OK!");
            }
        }
        public void Transfer(string receiverr, int amount)
        {
            this.receiver = receiverr;
            if (this.user == null)
            {
                w.WriteLine("Előbb jelentkezzen be!");
            }
            else
            {
                utalas = true;
                // Read_users();
                foreach (var z in banklist)
                {
                    if (z.Username == this.user)
                    {
                        z.Money -= amount;
                    }
                }
                foreach (var z in banklist)
                {
                    if (z.Username == this.receiver)
                    {
                        z.Money += amount;
                    }
                }
                w.WriteLine("{0} összegű utalás megtörtént {1} nevű felhasználónak.", amount, this.receiver);
                // w.WriteLine("We have put up:{0} Ft ", amount);

            }

        }
        public void Borrow(int amount)
        {
            bool siker = false;
            if (this.user == null)
            {
                w.WriteLine("Előbb jelentkezzen be!");
            }
            else
            {
                foreach (var z in banklist)
                {
                    if (z.Username == "bank" && z.Money > 0 && z.Money - amount > 0)
                    {
                        borrow = true;

                        z.Money -= amount;
                        foreach (var k in banklist)
                        {

                            if (k.Username == this.user)
                            {
                                k.Money += amount;
                                siker = true;
                            }
                        }
                    }

                }

            }
            if (siker == true)
                w.WriteLine("A kölcsön értéke:{0}Ft", amount);
            else
                w.WriteLine("A bank nem adhat most kölcsönt");

        }

        public bool Register(string nev, string jelszo)
        {
            Read_users();
            int i = 1;
            int lineCount = File.ReadLines("../../users.txt").Count();
            while (i <= lineCount)
            {
                if (banklist[i].Username == nev)
                {
                    w.WriteLine("Ilyen felhasználónév már létezik, próbáld újra!");
                    return false;
                }
                i++;
            }
            w.WriteLine("OK");
            return true;
            #region old_register
            /* string[] paramS;
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
             return true;*/
            #endregion
        }
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
        void UserList()
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
            w.WriteLine("LOGIN|user|passwd:          Bejelentkezés !");
            w.WriteLine("LOGOUT:                     Jelenleg bejelentkezett felhasználó kijelentkeztetése!");
            w.WriteLine("REGISTER|user|passwd:       Regisztráció!");
            w.WriteLine("BALANCE:                    Egyeneleg lekérése!");
            w.WriteLine("DEPOSIT|amount:             Pénz feltöltése folyószámlára!");
            w.WriteLine("WITHDRAW|amount:            Pénz levétele folyószámláról!");
            w.WriteLine("TRANSFER|personName|amount  Pénz utalása folyószámláról más számára!");
            w.WriteLine("BORROW|amount               Kölcsön kérése a banktól ha lehetséges!");
            w.WriteLine("HISTORY                     SZámlatörténet megtekintése!");
            w.WriteLine("USERDEL|user|passwd:        Felhasználók törlése!(ADMIN ONLY");
            w.WriteLine("USERLIST:                   Ki listázza a felhasználókat!(ADMIN ONLY");
            w.WriteLine("HELP:                       Ki listázza a megadható parancsokat!");
            w.WriteLine("EXIT:                       Kilépés!");
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