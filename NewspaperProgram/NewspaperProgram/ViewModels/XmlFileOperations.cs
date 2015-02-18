using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;

namespace NewspaperProgram.ViewModels
{
    public class XmlFileOperations
    {
        const string txtExtension = ".txt";
        const string userInfoFolderPath = "../../DataBase/Info/";
        const string userInfoDateInfoPath = "../../DataBase/Info/DateInfo.txt";
        const string xmlExtension = ".xml";
        const string userDataFolderPath = "../../DataBase/UserData/";
        const string dateTimeFormat = "dd/MM/yyyy";
        private int currentPreffix = 0;
        private int currentMaxPreffix = 0;
        private string currentDate = DateTime.Now.ToString(dateTimeFormat);
        private List<string> allInvoicesDates = new List<string>();

        public XmlFileOperations()
        {
            string currentPrefixFilePath = userInfoFolderPath + this.CurrentDate + txtExtension;
            if (File.Exists(currentPrefixFilePath))
            {
                using (StreamReader strReader = new StreamReader(currentPrefixFilePath))
                {
                    int preffix = int.Parse(strReader.ReadToEnd().Trim().Replace(Environment.NewLine, ""));
                    this.CurrentPreffix = preffix + 1;
                    this.CurrentMaxPreffix = preffix;
                }
            }

            this.AllInvoicesDates = GetAllInvoicesDates();
        }

        public int CurrentPreffix
        {
            get
            {
                return this.currentPreffix;
            }
            private set
            {
                this.currentPreffix = value;
            }
        }

        public int CurrentMaxPreffix
        {
            get
            {
                return this.currentMaxPreffix;
            }
            private set
            {
                this.currentMaxPreffix = value;
            }
        }

        public string CurrentDate
        {
            get
            {
                return this.currentDate;
            }
            private set
            {
                this.currentDate = value;
            }
        }

        public List<string> AllInvoicesDates
        {
            get
            {
                List<string> buffer = new List<string>();
                for (int i = 0; i < this.allInvoicesDates.Count; i++)
                {
                    buffer.Add(this.allInvoicesDates[i]);
                }

                return buffer;
            }
            private set
            {
                this.allInvoicesDates = value;
            }
        }

        public void SaveUserData(DependencyObject printArea, DateTime dateOfInvoice, DependencyObject advertisementComboBox, bool shoudOverSave = false)
        {
            List<List<string>> userData = new List<List<string>>();
            userData = UserInfo.GetUserData(printArea as DependencyObject, userData);
            string dateToSave = dateOfInvoice.ToString(dateTimeFormat);
            string xmlFilePath = "";
            if (shoudOverSave == true)
            {
                xmlFilePath = GetXmlOverSavePath(dateToSave);
            }
            else
            {

                xmlFilePath = GetXmlSavePath(dateToSave);
            }

            using (XmlTextWriter xmlWriter = new XmlTextWriter(xmlFilePath, Encoding.UTF8))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteComment("UserData");
                xmlWriter.WriteStartElement("UserData");
                foreach (var data in userData)
                {
                    xmlWriter.WriteStartElement(data[0]);
                    xmlWriter.WriteString(data[1]);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteStartElement((advertisementComboBox as ComboBox).Name);
                xmlWriter.WriteString((advertisementComboBox as ComboBox).SelectedIndex.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }

            this.AllInvoicesDates = GetAllInvoicesDates();
        }

        public void OverwriteDefaultData(DependencyObject printArea, string defaultDataPath, DependencyObject advertisementComboBox)
        {
            List<List<string>> userDefaultData = new List<List<string>>();
            userDefaultData = UserInfo.GetUserData(printArea as DependencyObject, userDefaultData);

            using (XmlTextWriter xmlWriter = new XmlTextWriter(defaultDataPath, Encoding.UTF8))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteComment("UserData");
                xmlWriter.WriteStartElement("UserData");
                foreach (var data in userDefaultData)
                {
                    xmlWriter.WriteStartElement(data[0]);
                    xmlWriter.WriteString(data[1]);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteStartElement((advertisementComboBox as ComboBox).Name);
                xmlWriter.WriteString((advertisementComboBox as ComboBox).SelectedIndex.ToString());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }
        }

        public void LoadUserPreviousData(DateTime serchedDate)
        {
            string invoiceLoadPreviosPath = "";
            if (GetXmlLoadPreviousPath(serchedDate, out invoiceLoadPreviosPath))
            {
                LoadDataFromPath(invoiceLoadPreviosPath);
            }
        }

        public void LoadUserNextData(DateTime serchedDate)
        {
            string invoiceLoadNextPath = "";
            if (GetXmlLoadNextPath(serchedDate, out invoiceLoadNextPath))
            {
                LoadDataFromPath(invoiceLoadNextPath);
            }
        }

        public void GoToSpecificDate(DateTime specificDate)
        {
            string txtFilePath = userInfoFolderPath + specificDate.ToString(dateTimeFormat) + txtExtension;
            if (File.Exists(txtFilePath))
            {
                using (StreamReader strReader = new StreamReader(txtFilePath))
                {
                    int preffix = int.Parse(strReader.ReadToEnd().Trim().Replace(Environment.NewLine, ""));
                    this.CurrentPreffix = preffix;
                    this.CurrentMaxPreffix = preffix;
                }

                string xmlFilePath = userDataFolderPath + this.CurrentPreffix + "." + specificDate.ToString(dateTimeFormat) + xmlExtension;
                LoadDataFromPath(xmlFilePath);
            }
            else
            {
                this.CurrentPreffix = 0;
                this.CurrentMaxPreffix = 0;
            }

            this.CurrentDate = specificDate.ToString(dateTimeFormat);
        }

        private string GetXmlSavePath(string dateToSave)
        {
            string txtFilePath = userInfoFolderPath + dateToSave + txtExtension;
            int suffix = 0;
            if (File.Exists(txtFilePath))
            {
                using (StreamReader strReader = new StreamReader(txtFilePath))
                {
                    int.TryParse(strReader.ReadLine(), out suffix);
                }

                using (StreamWriter strWriter = new StreamWriter(txtFilePath))
                {
                    strWriter.Write(suffix + 1);
                }
            }
            else
            {
                using (StreamWriter strWriter = new StreamWriter(txtFilePath))
                {
                    strWriter.Write(suffix + 1);
                }

                SortAndWriteDate(DateTime.Parse(dateToSave));
            }

            this.CurrentPreffix = suffix + 1;
            this.CurrentMaxPreffix = suffix + 1;

            string xmlSavePath = userDataFolderPath;
            xmlSavePath += (suffix + 1) + ".";
            xmlSavePath += dateToSave;
            xmlSavePath += xmlExtension;

            return xmlSavePath;
        }

        public string GetXmlOverSavePath(string dateToSave)
        {
            string txtFilePath = userInfoFolderPath + dateToSave + txtExtension;
            int suffix = 0;
            string xmlSavePath = userDataFolderPath;
            if (File.Exists(txtFilePath))
            {
                suffix = this.CurrentPreffix;
                xmlSavePath += (suffix) + ".";
            }
            else
            {
                using (StreamWriter strWriter = new StreamWriter(txtFilePath))
                {
                    strWriter.Write(suffix + 1);
                }

                SortAndWriteDate(DateTime.Parse(dateToSave));
                xmlSavePath += (suffix + 1) + ".";
                this.CurrentPreffix = suffix + 1;
                this.CurrentMaxPreffix = suffix + 1;
            }

            xmlSavePath += dateToSave;
            xmlSavePath += xmlExtension;

            return xmlSavePath;
        }

        private void SortAndWriteDate(DateTime dateToWrite)
        {
            List<string> sortedDates = new List<string>();
            int counter = 0;
            bool placeOfDateFound = false;

            if (AllInvoicesDates.Count == 0)
            {
                sortedDates.Add(dateToWrite.ToString(dateTimeFormat));
                placeOfDateFound = true;
            }

            for (int i = 0; i < AllInvoicesDates.Count; i++)
            {
                if (dateToWrite < DateTime.Parse(AllInvoicesDates[i]))
                {
                    sortedDates.Add(dateToWrite.ToString(dateTimeFormat));
                    sortedDates.Add(AllInvoicesDates[i]);
                    placeOfDateFound = true;
                    counter = i + 1;
                    break;
                }

                sortedDates.Add(AllInvoicesDates[i]);
            }

            if (!placeOfDateFound)
            {
                sortedDates.Add(dateToWrite.ToString(dateTimeFormat));
            }
            else
            {
                for (int i = counter; i < AllInvoicesDates.Count; i++)
                {
                    sortedDates.Add(AllInvoicesDates[i]);
                }
            }

            File.Delete(userInfoDateInfoPath);
            using (StreamWriter strWriter = new StreamWriter(userInfoDateInfoPath, true))
            {
                for (int i = 0; i < sortedDates.Count; i++)
                {
                    strWriter.WriteLine(sortedDates[i] + ",");
                }
            }
        }

        private bool GetXmlLoadPreviousPath(DateTime surchedDate, out string xmlFilePath)
        {
            if (surchedDate.ToString(dateTimeFormat) != this.CurrentDate)
            {
                this.CurrentPreffix = 0;
                this.CurrentMaxPreffix = 0;
            }

            if (this.CurrentPreffix > 1)
            {
                this.CurrentPreffix -= 1;
                xmlFilePath = userDataFolderPath + CurrentPreffix + "." + surchedDate.ToString(dateTimeFormat) + xmlExtension;
                return true;
            }
            else
            {
                DateTime dateTocompare = new DateTime();
                if (CheckIsPreviousAvailableDate(surchedDate, out dateTocompare))
                {
                    string preffix = "";
                    using (StreamReader strReader = new StreamReader(userInfoFolderPath + dateTocompare.ToString(dateTimeFormat) + txtExtension))
                    {
                        preffix = strReader.ReadToEnd();
                    }

                    this.CurrentPreffix = int.Parse(preffix);
                    this.CurrentDate = dateTocompare.ToString(dateTimeFormat);
                    if (File.Exists(userInfoFolderPath + dateTocompare.ToString(dateTimeFormat) + txtExtension))
                    {
                        using (StreamReader strReader = new StreamReader(userInfoFolderPath + dateTocompare.ToString(dateTimeFormat) + txtExtension))
                        {
                            this.CurrentMaxPreffix = int.Parse(strReader.ReadToEnd());
                        }
                    }

                    xmlFilePath = userDataFolderPath + preffix + "." + dateTocompare.ToString(dateTimeFormat) + xmlExtension;
                    return true;
                }
            }

            xmlFilePath = null;
            return false;
        }

        private bool GetXmlLoadNextPath(DateTime surchedDate, out string xmlFilePath)
        {
            if (surchedDate.ToString(dateTimeFormat) != this.CurrentDate)
            {
                this.CurrentPreffix = 0;
                this.CurrentMaxPreffix = 0;
            }

            if (this.CurrentPreffix < this.CurrentMaxPreffix)
            {
                this.CurrentPreffix += 1;
                xmlFilePath = userDataFolderPath + this.CurrentPreffix + "." + surchedDate.ToString(dateTimeFormat) + xmlExtension;
                return true;
            }
            else
            {
                DateTime dateToCompare = new DateTime();
                if (CheckIsNextAvailableDate(surchedDate, out dateToCompare))
                {
                    this.CurrentPreffix = 1;
                    if (File.Exists(userInfoFolderPath + dateToCompare.ToString(dateTimeFormat) + txtExtension))
                    {
                        using (StreamReader strReader = new StreamReader(userInfoFolderPath + dateToCompare.ToString(dateTimeFormat) + txtExtension))
                        {
                            this.CurrentMaxPreffix = int.Parse(strReader.ReadToEnd());
                        }
                    }

                    this.CurrentDate = dateToCompare.ToString(dateTimeFormat);
                    xmlFilePath = userDataFolderPath + this.CurrentPreffix + "." + dateToCompare.ToString(dateTimeFormat) + xmlExtension;
                    return true;
                }
            }

            xmlFilePath = null;
            return false;
        }

        public void LoadDataFromPath(string XmlFilePath, bool isDefaultData = false)
        {
            List<string> userData = new List<string>();
            using (XmlReader strReader = XmlReader.Create(XmlFilePath))
            {
                while (strReader.Read())
                {
                    if (strReader.NodeType == XmlNodeType.Element)
                    {
                        if (strReader.Name != "UserData")
                        {
                            userData.Add(strReader.Name);
                        }
                    }
                    else if (strReader.NodeType == XmlNodeType.Text)
                    {
                        if (strReader.Value == "null")
                        {
                            userData.Add("");
                        }
                        else if (strReader.Value == "dInvoiceNumber" && isDefaultData == true)
                        {
                            int invoiceNumber = 0;
                            int.TryParse(AdvertisementFileOperations.LoadInvoiceNumber(), out invoiceNumber);
                            userData.Add((invoiceNumber + 1).ToString());
                        }
                        else
                        {
                            userData.Add(strReader.Value);
                        }
                    }
                }
            }

            LoadDataFromList(userData);
        }

        public void LoadDataFromList(List<string> userData)
        {
            MainWindow mainWindow = ((MainWindow)Application.Current.MainWindow);
            for (int i = 0; i < userData.Count - 1; i++)
            {
                if (i == 0)
                {
                    TextBox textBox = mainWindow.FindName(userData[i]) as TextBox;
                    textBox.Text = userData[i + 1];
                }
                else if (userData[i] == "AdvertisementComboBox")
                {
                    mainWindow.AdvertisementComboBox.SelectedIndex = int.Parse(userData[i + 1]);
                }
                else if (i % 2 == 0)
                {
                    TextBox textBox = mainWindow.FindName(userData[i]) as TextBox;
                    textBox.Text = userData[i + 1];
                }
            }
        }

        private List<string> GetAllInvoicesDates()
        {
            List<string> allDates = new List<string>();
            if (File.Exists(userInfoDateInfoPath))
            {
                using (StreamReader strReader = new StreamReader(userInfoDateInfoPath))
                {
                    string[] str = strReader.ReadToEnd().Replace(Environment.NewLine, "").Trim().Split(new Char[] { ',' });
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (str[i] != "")
                        {
                            allDates.Add(str[i]);
                        }
                    }
                }
            }

            return allDates;
        }

        public bool CheckIsNextAvailableDate(DateTime surchedDate, out DateTime nextDate)
        {
            bool isAvailable = false;

            for (int i = 0; i < this.AllInvoicesDates.Count; i++)
            {
                DateTime dateTocompare = new DateTime();
                if (DateTime.TryParse(this.AllInvoicesDates[i], out dateTocompare))
                {
                    if (dateTocompare > surchedDate || this.CurrentPreffix < this.CurrentMaxPreffix)
                    {
                        nextDate = dateTocompare;
                        isAvailable = true;
                        return isAvailable;
                    }
                }
            }

            nextDate = new DateTime();
            return isAvailable;
        }

        public bool CheckIsPreviousAvailableDate(DateTime surchedDate, out DateTime previoustDate)
        {
            bool isAvailable = false;

            for (int i = this.AllInvoicesDates.Count - 1; i >= 0; i--)
            {
                DateTime dateTocompare = new DateTime();
                if (DateTime.TryParse(this.AllInvoicesDates[i], out dateTocompare))
                {
                    if (dateTocompare < surchedDate || this.CurrentPreffix > 1)
                    {
                        previoustDate = dateTocompare;
                        isAvailable = true;
                        return isAvailable;
                    }
                }
            }

            previoustDate = new DateTime();
            return isAvailable;
        }

        public bool ChekIfDateExist(DateTime surchedDate)
        {
            bool isDateExist = false;
            string path = userInfoFolderPath + surchedDate.ToString(dateTimeFormat) + txtExtension;
            if (File.Exists(path))
            {
                isDateExist = true;
            }

            return isDateExist;
        }

        public void Reset()
        {
            this.CurrentMaxPreffix = 0;
            this.CurrentPreffix = 0;
            this.CurrentDate = DateTime.Now.ToString(dateTimeFormat);
            this.allInvoicesDates = new List<string>();
        }

        public void DeleteLastFileForDate(string dateToDeleteLastFile)
        {
            string txtFilePath = userInfoFolderPath + dateToDeleteLastFile + txtExtension;
            int invoiceNumber = int.Parse(AdvertisementFileOperations.LoadInvoiceNumber().Trim().Replace(Environment.NewLine, ""));

            if (File.Exists(txtFilePath))
            {
                using (StreamReader strReader = new StreamReader(txtFilePath))
                {
                    int preffix = int.Parse(strReader.ReadToEnd().Trim().Replace(Environment.NewLine, ""));
                    this.CurrentPreffix = preffix;
                    this.CurrentMaxPreffix = preffix;
                }

                string xmlFilePath = userDataFolderPath + this.CurrentPreffix + "." + dateToDeleteLastFile + xmlExtension;

                if (this.CurrentPreffix <= 1)
                {
                    File.Delete(txtFilePath);
                    DeleteDateFromAllInvoicesDates(dateToDeleteLastFile);

                    this.CurrentPreffix = 0;
                    this.CurrentMaxPreffix = 0;
                }
                else
                {
                    using (StreamWriter strWriter = new StreamWriter(txtFilePath))
                    {
                        strWriter.Write(this.CurrentPreffix - 1);
                    }

                    this.CurrentPreffix = this.CurrentPreffix - 1;
                    this.CurrentMaxPreffix = this.CurrentMaxPreffix - 1;
                }

                File.Delete(xmlFilePath);
            }

            AdvertisementFileOperations.SaveInvoiceNumber((invoiceNumber).ToString());
        }

        public void DeleteCurrentInvoice()
        {
            if (this.CurrentPreffix == this.CurrentMaxPreffix)
            {
                DeleteLastFileForDate(this.CurrentDate);
                GoToSpecificDate(DateTime.Parse(this.CurrentDate));
            }
            else
            {
                int preffix = this.CurrentPreffix;
                this.CurrentMaxPreffix = this.CurrentMaxPreffix - 1;
                string xmlToDeleteFilePath = userDataFolderPath + preffix + "." + this.CurrentDate + xmlExtension;
                File.Delete(xmlToDeleteFilePath);
                string txtFilePath = userInfoFolderPath + this.CurrentDate + txtExtension;
                using (StreamWriter strWriter = new StreamWriter(txtFilePath))
                {
                    strWriter.Write(this.CurrentMaxPreffix);
                }

                for (int i = preffix; i <= this.CurrentMaxPreffix; i++)
                {
                    File.Move(userDataFolderPath + (i + 1) + "." + this.CurrentDate + xmlExtension,
                        userDataFolderPath + i + "." + this.CurrentDate + xmlExtension);
                }

                string xmlPath = userDataFolderPath + this.CurrentPreffix + "." + this.CurrentDate + xmlExtension;
                LoadDataFromPath(xmlPath);
            }
        }

        public void DeleteAllInvoicesForASpecificDay(string dayToDelete)
        {
            string txtFileToDeletePath = userInfoFolderPath + dayToDelete + txtExtension;
            int numberOfXmlFiles = 0;
            if (File.Exists(txtFileToDeletePath))
            {
                using (StreamReader strReader = new StreamReader(txtFileToDeletePath))
                {
                    int.TryParse(strReader.ReadLine(), out numberOfXmlFiles);
                }
            }

            File.Delete(txtFileToDeletePath);
            for (int i = 1; i <= numberOfXmlFiles; i++)
            {
                File.Delete(userDataFolderPath + i + "." + dayToDelete + xmlExtension);
            }

            DeleteDateFromAllInvoicesDates(dayToDelete);
            if (dayToDelete == this.CurrentDate)
            {
                this.CurrentPreffix = 0;
                this.CurrentMaxPreffix = 0;
            }
        }

        public void DeleteAllInvoicesForAMonthOrYear(string monthOrYearToDelete)
        {
            List<string> allDaysToDelete = new List<string>();

            for (int i = 0; i < this.allInvoicesDates.Count; i++)
            {
                string suffix = "";
                for (int k = AllInvoicesDates[i].Length - 1; k >= AllInvoicesDates[i].Length - monthOrYearToDelete.Length; k--)
                {
                    suffix = allInvoicesDates[i][k] + suffix;
                }

                if (suffix == monthOrYearToDelete)
                {
                    allDaysToDelete.Add(this.AllInvoicesDates[i]);
                }
            }

            for (int i = 0; i < allDaysToDelete.Count; i++)
            {
                DeleteAllInvoicesForASpecificDay(allDaysToDelete[i]);
            }

        }

        private void DeleteDateFromAllInvoicesDates(string dateToDelete)
        {
            File.Delete(userInfoDateInfoPath);
            using (StreamWriter strWriter = new StreamWriter(userInfoDateInfoPath, true))
            {
                for (int i = 0; i < this.allInvoicesDates.Count; i++)
                {
                    if (this.allInvoicesDates[i] != dateToDelete && this.allInvoicesDates[i] != "")
                    {
                        strWriter.WriteLine(this.allInvoicesDates[i] + ",");
                    }
                }
            }

            this.AllInvoicesDates = GetAllInvoicesDates();
        }
    }
}
