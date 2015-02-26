using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NewspaperProgram.ViewModels
{
    public class SendEmail
    {
        private string to = "";
        private string subject = "";
        private string cc = "";
        private string body = "";
        private List<string> attachments = new List<string>();
        private string pathToEmailClient = @"C:\Program Files\Mozilla Thunderbird\thunderbird.exe";

        public SendEmail()
        {
        }

        public string To
        {
            get
            {
                return this.to;
            }
            set
            {
                this.to = value;
            }
        }

        public string Subject
        {
            get
            {
                return this.subject;
            }
            set
            {
                this.subject = value;
            }
        }

        public string Cc
        {
            get
            {
                return this.cc;
            }
            set
            {
                this.cc = value;
            }
        }

        public string Body
        {
            get
            {
                return this.body;
            }
            set
            {
                this.body = value;
            }
        }

        public string Attach
        {
            set
            {
                this.attachments.Add(value);
            }
        }

        public string PathToEmailClient
        {
            get
            {
                return this.pathToEmailClient;
            }
            set
            {
                this.pathToEmailClient = value;
            }
        }

        public bool Send()
        {
            bool isDialogOpen = false;
            string strCommand;
            strCommand = " -compose to=" + (char)34 + this.To + (char)34 + ",";
            strCommand += "cc=" + (char)34 + this.Cc + (char)34 + ",";
            strCommand += "body=" + (char)34 + this.Body + (char)34 + ",";
            strCommand += "subject=" + (char)34 + this.Subject + (char)34 + ",";
            strCommand += "preselectid=" + (char)34 + "id2" + (char)34 + ",";
            strCommand += "attachment=" + "'";
            for (int i = 0; i < this.attachments.Count; i++)
            {
                strCommand += this.attachments[i];
                if (i < this.attachments.Count - 1)
                {
                    strCommand += ",";
                }
            }

            strCommand += "'";

            if (File.Exists(this.PathToEmailClient))
            {
                Process.Start(PathToEmailClient, strCommand);
                isDialogOpen = true;
            }

            return isDialogOpen;
        }
    }
}
