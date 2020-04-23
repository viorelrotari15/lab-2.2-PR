using System;
using System.Globalization;
using System.IO;
using EAGetMail; //Am aduagat EAGetMail namespace

namespace receiveemail
{
    class Program
    {

        // se genereaza  genereaza un nume unic de fișier de e-mail pe baza datei
        static string _generateFileName(int sequence)
        {
            DateTime currentDateTime = DateTime.Now;
            return string.Format("{0}-{1:000}-{2:000}.eml",
                currentDateTime.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")),
                currentDateTime.Millisecond,
                sequence);
        }

        static void Main(string[] args)
        {
            try
            {
                // Se creeaza folderul imbox in directoriul unde se afla aplicatia
                // pentru a salva Emailurile  de pe server
                
                string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
                // Daca folderul nu exista  el automat se creeaza
                if (!Directory.Exists(localInbox))
                {
                    Directory.CreateDirectory(localInbox);
                }

                // Gmail IMAP4 server este "imap.gmail.com"
                //cu tot cu credentiale 
                MailServer oServer = new MailServer("imap.gmail.com",
                                "your emal",
                                "Pass",
                                ServerProtocol.Imap4);

                // Permite SSL conectiunea.
                oServer.SSLConnection = true;

                // Setam 993 SSL port
                oServer.Port = 993;

                MailClient oClient = new MailClient("TryIt");
                oClient.Connect(oServer);

                // recuperam mesajele ne citite / noi emailuri
                oClient.GetMailInfosParam.Reset();
                oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;

                MailInfo[] infos = oClient.GetMailInfos();
                Console.WriteLine("Total {0} unread email(s)\r\n", infos.Length);
                for (int i = 0; i < infos.Length; i++)
                {
                    MailInfo info = infos[i];
                    Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                        info.Index, info.Size, info.UIDL);

                    // Recuperam email de pe  IMAP4 server
                    Mail oMail = oClient.GetMail(info);

                    Console.WriteLine("From: {0}", oMail.From.ToString());
                    Console.WriteLine("Subject: {0}\r\n", oMail.Subject);

                    // Aici se genereaza un fisier unic de email ce depinde de data si ora cand au venit.
                    string fileName = _generateFileName(i + 1);
                    string fullPath = string.Format("{0}\\{1}", localInbox, fileName);

                    // Salvam email pe calculator 
                    oMail.SaveAs(fullPath, true);

                    // Aici marcam emailul daca a fost citit ca data viitoare el sa nu se descarce 
                    
                    if (!info.Read)
                    {
                        oClient.MarkAsRead(info, true);
                        // oClient.Delete(info);  // daca dorim ca sa dispara copia emailului de pe server folosim acaeasta metoda

                    }

                   
                    // daca nu marcam ca citit
                }

                
                oClient.Quit();
                Console.WriteLine("Completed!");
            }
            catch (Exception ep)
            {
                Console.WriteLine(ep.Message);
            }
        }
    }
}