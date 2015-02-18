using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewspaperProgram.ViewModels
{
    public static class AdvertisementFileOperations
    {
        const string importantDataFolderPath = "../../DataBase/ImportantData";
        const string dataInfoFolderPath = "../../DataBase/Info";
        const string userDataFolderPath = "../../DataBase/UserData";
        const string ordinaryAdvertisementPath = "../../DataBase/Advertisement";
        const string citizenAdvertisemenSuffix = "/2.doc";
        private static string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static void SaveInvoiceNumber(string invoiceNumber)
        {
            File.WriteAllText(importantDataFolderPath + "/InvoiceNumber.txt", invoiceNumber, Encoding.UTF8);
        }

        public static void ReplaceAdvertisementForToday(string advertisementSuffix)
        {
            bool shouldReplace = false;
            if (advertisementSuffix == citizenAdvertisemenSuffix)
            {
                shouldReplace = ShouldReplace("/CitizenAdvertisementInfo.txt");
            }
            else
            {
                shouldReplace = ShouldReplace("/CompanyAdvertisementInfo.txt");
            }

            if (shouldReplace == true)
            {
                bool isAdvertisementExists = false;
                isAdvertisementExists = File.Exists(ordinaryAdvertisementPath + advertisementSuffix);
                if (isAdvertisementExists)
                {
                    File.Delete(myDocumentsPath + advertisementSuffix);
                    File.Move(ordinaryAdvertisementPath + advertisementSuffix, myDocumentsPath + advertisementSuffix);
                }

                File.Delete(ordinaryAdvertisementPath + advertisementSuffix);
                using (StreamWriter strWriter = new StreamWriter(ordinaryAdvertisementPath + advertisementSuffix))
                {
                    if (advertisementSuffix == citizenAdvertisemenSuffix)
                    {
                        strWriter.WriteLine("ЕКСПРЕСНИ ОБЯВИ ЗА ГРАЖДАНИ" + Environment.NewLine);
                    }
                    else
                    {
                        strWriter.WriteLine("ЕКСПРЕСНА ФИРМЕНА ИНФОРМАЦИЯ" + Environment.NewLine);
                    }
                }
            }
        }

        private static bool ShouldReplace(string advertisementInfoSuffix)
        {
            DateTime dateOfAdvertisement = new DateTime();
            bool shouldReplace = false;
            using (StreamReader strReader = new StreamReader(ordinaryAdvertisementPath + advertisementInfoSuffix))
            {
                DateTime.TryParse(strReader.ReadLine(), out dateOfAdvertisement);
            }

            if (dateOfAdvertisement <= DateTime.Now)
            {
                shouldReplace = true;
            }

            return shouldReplace;
        }

        public static void CheckForRepeatingAdvertisement(string advertisementSuffix)
        {
            bool shouldReplace = false;
            string AdvertisementInfoPath = "";
            if (advertisementSuffix == citizenAdvertisemenSuffix)
            {
                shouldReplace = ShouldReplace("/CitizenAdvertisementInfo.txt");
                AdvertisementInfoPath = "/CitizenAdvertisementInfo.txt";
            }
            else
            {
                shouldReplace = ShouldReplace("/CompanyAdvertisementInfo.txt");
                AdvertisementInfoPath = "/CompanyAdvertisementInfo.txt";
            }

            if (shouldReplace)
            {
                List<string> advertisementInfo = new List<string>();
                using (StreamReader strReader = new StreamReader(ordinaryAdvertisementPath + AdvertisementInfoPath))
                {
                    string[] strArray = strReader.ReadToEnd().Split(Environment.NewLine.ToCharArray());
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        if (strArray[i] != "")
                        {
                            advertisementInfo.Add(strArray[i]);
                        }
                    }
                }

                using (StreamWriter strWriter = new StreamWriter(ordinaryAdvertisementPath + AdvertisementInfoPath))
                {
                    for (int i = 0; i < advertisementInfo.Count; i++)
                    {
                        if (i == 0)
                        {
                            strWriter.WriteLine(DateTime.Now.AddDays(1).ToString("dd/MM/yyyy"));
                        }
                        else if (i % 2 != 0)
                        {
                            if ((int.Parse(advertisementInfo[i]) - 1) <= 0)
                            {
                                i++;
                                continue;
                            }
                            else
                            {
                                strWriter.WriteLine((int.Parse(advertisementInfo[i]) - 1));
                            }
                        }
                        else
                        {
                            strWriter.WriteLine(advertisementInfo[i]);
                        }
                    }
                }

                using (StreamWriter strWriter = new StreamWriter(ordinaryAdvertisementPath + advertisementSuffix, true, Encoding.UTF8))
                {
                    for (int i = 1; i < advertisementInfo.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            strWriter.WriteLine(advertisementInfo[i].ToUpper() + System.Environment.NewLine);
                        }
                    }
                }
            }
        }

        public static string LoadInvoiceNumber()
        {
            string invoiceNumber = File.ReadAllText(importantDataFolderPath + "/InvoiceNumber.txt", Encoding.UTF8);
            return invoiceNumber;
        }

        public static void AddRecurringAdvertisemen(string advertisementSuffix, int numberOfRepeating, string advertisement)
        {
            if (advertisementSuffix == citizenAdvertisemenSuffix)
            {
                using (StreamWriter strWriter = new StreamWriter(ordinaryAdvertisementPath + "/CitizenAdvertisementInfo.txt", true, Encoding.UTF8))
                {
                    strWriter.WriteLine(numberOfRepeating);
                    strWriter.WriteLine(advertisement.ToUpper());
                }
            }
            else
            {
                using (StreamWriter strWriter = new StreamWriter(ordinaryAdvertisementPath + "/CompanyAdvertisementInfo.txt", true, Encoding.UTF8))
                {
                    strWriter.WriteLine(numberOfRepeating);
                    strWriter.WriteLine(advertisement.ToUpper());
                }
            }
            
        }

        public static void SaveAdvertisemen(string content, string advertisemenSuffix)
        {
            using (StreamWriter sw = new StreamWriter(myDocumentsPath + advertisemenSuffix, true, Encoding.UTF8))
            {
                sw.WriteLine(content + System.Environment.NewLine);
            }
        }
    }
}
