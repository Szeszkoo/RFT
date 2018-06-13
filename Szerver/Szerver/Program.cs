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
        public void Unique_user_write(string name, string data, string person_name, string task)
        {
            string path = @"../../" + name + ".txt";
            StreamWriter writer = new StreamWriter(path, true);
            string content = "";

            foreach (var z in banklist)
            {
                if (z.Username == name)
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


        public void Start_Communication()
        {
            bool ok = true;
            Read_users();
            while (ok)
            {
                string command = null;
                try
                {
                    string message = r.ReadLine();
                    string[] param = message.Split('|');
                    command = param[0].ToUpper();
                    switch (command)
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
                                    Read_users();
                                }
                                break;
                            }
                        case "USERDEL":
                            {
                                if (admin == true)
                                    UserDelete(param[1], param[2]);
                                else
                                {
                                    w.WriteLine("Admin only function!");
                                }
                            }
                            break;
                        case "USERLIST":
                            {
                                if (admin == true)
                                    UserList();
                                else
                                {
                                    w.WriteLine("Admin only function!");
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
                        case "FS":
                            {
                                Savings(Math.Abs(int.Parse(param[1])), Math.Abs(int.Parse(param[2])));
                                Overwrite_users();
                            }
                            break;

                        case "BALANCE": Balance(); break;
                        case "HISTORY": History(); break;
                        case "HELP": Help(); break;
                        case "ADMINHELP":
                            {
                                if (admin == true)
                                {
                                    Admin_Help();
                                    break;

                                }
                                else
                                {
                                    w.WriteLine("You are not an admin!");
                                    break;
                                }
                            }
                        case "BYE": w.WriteLine("BYE"); ok = false; break;
                        default: w.WriteLine("ERR|Unkown command!"); break;
                    }
                }
                catch (Exception e)
                {
                    w.WriteLine(e.Message);
                }
                w.Flush();
            }
            Console.WriteLine("The client said goodbye!");
        }

        public void History()
        {
            if (this.user == null)
            {
                w.WriteLine("You have to log in first!");
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
                w.WriteLine("You have to log in first!");
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
                w.WriteLine("You have been deposited:{0} Ft.", amount);
                // w.WriteLine("We have put up:{0} Ft ", amount);
            }
        }
        public void Withdraw(int amount)
        {
            if (this.user == null)
            {
                w.WriteLine("You have to log in first!");
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
                w.WriteLine("You have been witdraw:{0} Ft. ", amount);
            }
        }

        public void Balance()
        {
            if (this.user == null)
            {
                w.WriteLine("You have to log in first!");
            }
            else
            {
                //   Read_users();
                foreach (var z in banklist)
                {
                    if (this.user == z.Username)
                    {
                        w.WriteLine("'" + z.Username.ToUpper() + "'" + " user's balance: " + z.Money + "Ft.");
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
                w.WriteLine("You have to log in first!");
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
                w.WriteLine("You have to log in first!");
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
        public void Savings(int amount, int month)
        {
            if (this.user == null)
            {
                w.WriteLine("You have to log in first!");
            }
            else
            {
                amount = amount * month / 10;
                foreach (var z in banklist)
                {
                    if (z.Username == this.user)
                    {
                        z.Money += amount;
                    }
                }
                //w.WriteLine("OK*");
                //for (int i = 0; i < month; i++)
                //{
                //    w.WriteLine(i);
                //    w.WriteLine("OK!");
                //}
                w.WriteLine("Your bank account raised with this amount:{0}Ft ", amount);
                // w.WriteLine("We have put up:{0} Ft ", amount);

            }
        }
        public bool Register(string name, string pw)
        {
            Read_users();
            int i = 1;
            int lineCount = File.ReadLines("../../users.txt").Count();
            while (i <= lineCount)
            {
                if (banklist[i].Username == name)
                {
                    w.WriteLine("This username is already taken! ");
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
        private void UserDelete(string name, string pw)
        {
            string FilePath = "../../users.txt";
            var text = new StringBuilder();

            foreach (string s in File.ReadAllLines(FilePath))
            {
                text.AppendLine(s.Replace(name + "|" + pw, ""));
            }
            w.WriteLine("OK");

            using (var file = new StreamWriter(File.Create(FilePath)))
            {
                file.Write(text.ToString());
            }

        }
        void UserList()
        {
            string[] list = File.ReadAllLines("../../users.txt");
            w.WriteLine("OK*");
            foreach (var item in list)
            {
                list = list[0].Split('|');
                w.WriteLine(item);
            }
            w.WriteLine("OK!");

        }

        public void Login(string name, string pw)
        {
            if (this.user != null)
            {
                w.WriteLine("You have to log out first!");
            }
            else if (this.user == name)
            {
                w.WriteLine("This user is already logged in!");
            }
            else if (name == "admin" && pw == "admin")
            {
                admin = true;
                this.user = name;
                w.WriteLine("Welcome Admin!");
            }
            else
            {
                //online.Add(nev);
                this.user = name;
                w.WriteLine("OK");
            }
        }
        public void Logout()
        {
            if (user == null)
            {
                w.WriteLine("There's none logged in!");
            }
            else if (user == "admin")
            {
                admin = false;
                this.user = null;
                w.WriteLine("Admin logout succesfull!");
            }
            else
            {
                this.user = null;
                w.WriteLine("You have been logged out successfully!");
            }

        }
        private void Help()
        {
            w.WriteLine("OK*");
            w.WriteLine("LOGIN|user|passwd:          Log in!");
            w.WriteLine("LOGOUT:                     Log out!");
            w.WriteLine("REGISTER|user|passwd:       Registration!");
            w.WriteLine("BALANCE:                    Balance status!");
            w.WriteLine("DEPOSIT|amount:             Deposit to the account!!");
            w.WriteLine("WITHDRAW|amount:            Withdraw from the account!");
            w.WriteLine("TRANSFER|personName|amount  Transfer money to other accounts!");
            w.WriteLine("BORROW|amount               Take a loan if possible!");
            w.WriteLine("HISTORY                     Account history!");
            w.WriteLine("HELP:                       List out the commands!");
            w.WriteLine("BYE:                        Quit the program!");
            w.WriteLine("OK!");
        }
        private void Admin_Help()
        {

            w.WriteLine("OK*");
            w.WriteLine("LOGIN|user|passwd:          Log in!");
            w.WriteLine("LOGOUT:                     Log out!");
            w.WriteLine("REGISTER|user|passwd:       Registration!");
            w.WriteLine("BALANCE:                    Balance status!");
            w.WriteLine("DEPOSIT|amount:             Deposit to the account!!");
            w.WriteLine("WITHDRAW|amount:            Withdraw from the account!");
            w.WriteLine("TRANSFER|personName|amount  Transfer money to other accounts!");
            w.WriteLine("BORROW|amount               Take a loan if possible!");
            w.WriteLine("HISTORY                     Account history!");
            w.WriteLine("USERDEL|user|passwd:        Usere delete!(ADMIN ONLY");
            w.WriteLine("USERLIST:                   List out the users!(ADMIN ONLY");
            w.WriteLine("HELP:                       List out the commands!");
            w.WriteLine("BYE:                        Quit the program!");
            w.WriteLine("OK!");
        }
    }

    class Program
    {
        const int port_num = 1234;
        const string ip_add = "127.0.0.1";

        static void Main(string[] args)
        {

            Console.WriteLine("Server started!");
            try
            {
                IPAddress ip = IPAddress.Parse(ip_add);
                TcpListener k = new TcpListener(ip, port_num);
                k.Start();
                while (true)
                {
                    Console.WriteLine("Server is waiting for an incominc connection!");
                    TcpClient client = k.AcceptTcpClient();
                    Console.WriteLine("A client has arrived!");
                    Protokoll p = new Protokoll(client);
                    Thread t = new Thread(p.Start_Communication);
                    t.Start();
                    //t.Join();
                }
            }
            catch { Console.WriteLine("Could not start!"); }
            Console.ReadLine();
        }
    }
}
