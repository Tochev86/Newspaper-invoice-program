using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using NewspaperProgram.ViewModels;
using System.Collections;
using System.Windows.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace NewspaperProgram
{
    public partial class MainWindow : Window
    {
        private const string validationTipSuffix = "ValidationTip";
        private const string validationTipButtonSuffix = "Button";
        private const string citizenAdvertisementSuffix = "/2.doc";
        private const string companyAdvertisementSuffix = "/4.doc";
        private const string citizenAdvertisementInfoSuffix = "/CitizenAdvertisementInfo.txt";
        private const string companyAdvertisementInfoSuffix = "/CompanyAdvertisementInfo.txt";
        private const string citizenDefaultDataPath = "../../DataBase/ImportantData/CitizenDefaultData.xml";
        private const string companyDefaultDataPath = "../../DataBase/ImportantData/CompanyDefaultData.xml";
        private RestrictInputData invoiceMultiplyingClass = new RestrictInputData();
        private RestrictInputData customerBulstatClass = new RestrictInputData();
        private RestrictInputData executantBulstatClass = new RestrictInputData();
        private RestrictInputData invoiceDateClass = new RestrictInputData();
        private RestrictInputData pricePerWordClass = new RestrictInputData();
        private RestrictInputData inputTextValidationClass = new RestrictInputData();
        private RestrictInputData restrictDeleteDayClass = new RestrictInputData();
        private RestrictInputData restrictDeleteMonthClass = new RestrictInputData();
        private RestrictInputData restrictDeleteYearClass = new RestrictInputData();
        private XmlFileOperations xmlFileOperationsClass = new XmlFileOperations();
        private XmlFileOperations defaultDataFileOperationClass = new XmlFileOperations();
        private bool isCalenderVisible = false;
        private bool isInvoiceSaved = false;
        private bool isNewBlankInvoiceMade = false;
        private bool isDataAdded = false;
        private string savedAdvertisementText = " ";
        private string dateTimeFormat = "dd/MM/yyyy";
        private string newInvoiceOnDate = DateTime.Now.ToString("dd/MM/yyyy");
        private List<string> copiedInvoice = new List<string>();
        private KeyEventArgs customKeyEventArgs;
        private RoutedEventArgs customRoutedEventArgs;

        public MainWindow()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                this.xmlFileOperationsClass.LoadDataFromPath(citizenDefaultDataPath, true);
                this.xmlFileOperationsClass.SaveUserData(PrintArea as DependencyObject, DateTime.Now, AdvertisementComboBox);
                string currentDate = DateTime.Now.ToString(this.dateTimeFormat);
                InvoiceDate.Text = currentDate;

                CheckNexPreviousButtonAvailability(DateTime.Parse(currentDate));
                this.isNewBlankInvoiceMade = true;
                AdvertisementFileOperations.ReplaceAdvertisementForToday(citizenAdvertisementSuffix);
                AdvertisementFileOperations.ReplaceAdvertisementForToday(companyAdvertisementSuffix);
                AdvertisementFileOperations.CheckForRepeatingAdvertisement(citizenAdvertisementSuffix);
                AdvertisementFileOperations.CheckForRepeatingAdvertisement(companyAdvertisementSuffix);
                InputText_KeyUp(new Object(), this.customKeyEventArgs);
            }));
        }
        private void InputText_KeyUp(object sender, KeyEventArgs e)
        {
            this.customKeyEventArgs = e;
            int numberOfWords = Calculations.CountWords(InputText.Text);
            NumberOfWords.Text = numberOfWords.ToString();

            decimal multiplyNumber = 0;
            decimal.TryParse(InvoiceMultiplying.Text, out multiplyNumber);
            decimal wholePrice = Calculations.CalculateWholePrice(numberOfWords, PricePerWord.Text, multiplyNumber);
            FinalPrice.Text = wholePrice.ToString("0.00") + "лв.";

            decimal DDS = Calculations.CalculateDDSPrice(wholePrice);
            DDSPrice.Text = DDS.ToString("0.00") + "лв.";

            decimal priceWithoutDDS = wholePrice - DDS;
            PriceWitoutDDS.Text = priceWithoutDDS.ToString("0.00");

            string priceInWords = TranslateNumberToWords.ConvertNumberToWords(Int32.Parse(Math.Floor(wholePrice).ToString("0")));
            if (wholePrice < 2 && wholePrice >= 1)
            {
                PriceInWords.Text = priceInWords + " лев и ";
            }
            else
            {

                PriceInWords.Text = priceInWords + " лева и ";
            }

            decimal fractionalPortion = (wholePrice - Math.Truncate(wholePrice)) * 100;
            PriceInWords.Text += TranslateNumberToWords.ConvertNumberToWords(Int32.Parse(Math.Floor(fractionalPortion).ToString("00"))) + " стотинки ";
        }

        private void InputText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double oldValueOfLabel = Canvas.GetTop(PricePerWordValidationTip);
            Canvas.SetTop(PricePerWordValidationTip, 394 + InputText.ActualHeight);
        }

        private void InputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = InputText.Text.Trim();
            inputText = inputText.Replace(System.Environment.NewLine, " ");
            string newInputText = this.inputTextValidationClass.CheckInputTextLength(inputText);

            if (inputText.Length != newInputText.Length)
            {
                InputText.Text = newInputText;
                InputText.SelectionStart = newInputText.Length;
                InputTextValidationTip.Visibility = System.Windows.Visibility.Visible;
            }
            else if (inputText != InputText.Text.Trim())
            {
                InputText.Text = newInputText;
                InputText.SelectionStart = newInputText.Length;
            }
            else
            {
                InputTextValidationTip.Visibility = System.Windows.Visibility.Hidden;
            }

            DateTime currentDate = new DateTime();
            if (inputText != "" && DateTime.TryParse(InvoiceDate.Text, out currentDate) && InvoiceDate.Text.Length >= 10)
            {
                Save.IsEnabled = true;
            }
            else
            {
                Save.IsEnabled = false;
            }

            if (InvoiceDate.Text == this.newInvoiceOnDate &&
                this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
            {
                if (inputText != "")
                {
                    this.isDataAdded = true;
                }
                else
                {
                    this.isDataAdded = false;
                }
            }
        }

        private void InvoiceDateTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.customRoutedEventArgs = e;
            string currentDate = DateTime.Today.ToString(this.dateTimeFormat);
            InvoiceDate.Text = currentDate;
            CheckNexPreviousButtonAvailability(DateTime.Parse(currentDate));
        }

        private void InvoiceDateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string inputDateAsString = InvoiceDate.Text.Trim();
            DateTime inputDate = new DateTime();
            bool isDateValid = DateTime.TryParse(inputDateAsString, out inputDate);
            if (isDateValid == false || inputDateAsString.Length != 10)
            {
                InvoiceDateValidationTip.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                InvoiceDate.Text = inputDate.ToString(this.dateTimeFormat);
            }
        }

        private void InvoiceDateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = InvoiceDate.Text.Trim();
            string newDateNumber = this.invoiceDateClass.CheckComplexNumber(inputText, 16);
            InvoiceDate.Text = newDateNumber;
            if (inputText.Length != newDateNumber.Length)
            {
                InvoiceDate.SelectionStart = newDateNumber.Length;
            }

            DateTime checkedNewDate = new DateTime();
            if (DateTime.TryParse(newDateNumber, out checkedNewDate) && newDateNumber.Trim().Length >= 10 && InputText.Text.Trim() != "")
            {
                if (checkedNewDate.ToString(this.dateTimeFormat) != DateTime.Now.ToString(this.dateTimeFormat))
                {
                    InvoiceCalendar.SelectedDatesChanged -= InvoiceCalendar_SelectedDatesChanged;
                    InvoiceCalendar.SelectedDate = checkedNewDate;
                    InvoiceCalendar.DisplayDate = checkedNewDate;
                    InvoiceCalendar.SelectedDatesChanged += InvoiceCalendar_SelectedDatesChanged;
                    CheckNexPreviousButtonAvailability(DateTime.Parse(InvoiceDate.Text));
                }

                Save.IsEnabled = true;
                InvoiceDateValidationTip.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                Save.IsEnabled = false;
            }
        }

        private void PrintInvoiceButton_Clicked(object sender, RoutedEventArgs e)
        {
            PreviousInvoiceButton.Visibility = System.Windows.Visibility.Hidden;
            NextInvoiceButton.Visibility = System.Windows.Visibility.Hidden;
            InvoiceCalendar.Visibility = System.Windows.Visibility.Hidden;
            AdvertisementComboBox.BorderBrush = Brushes.Transparent;
            AdvertisementComboBox.Background = Brushes.Transparent;
            this.isCalenderVisible = false;
            ShowHideCalendarButton.Width = 0;

            PrintDialog printDialog = new PrintDialog();
            PrintInvoice.Print(PrintArea, printDialog);

            AdvertisementComboBox.ClearValue(ComboBox.BorderBrushProperty);
            AdvertisementComboBox.ClearValue(ComboBox.BackgroundProperty);
            ShowHideCalendarButton.Width = Double.NaN;
            PreviousInvoiceButton.Visibility = System.Windows.Visibility.Visible;
            NextInvoiceButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void PrintCitizenAdvertisementButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/2.doc";
            if (File.Exists(filePath))
            {
                Process.Start(filePath);
            }
            else
            {
                MessageBox.Show("Файлът \"2.doc\" не съществува!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PrintCompanyAdvertisementButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/4.doc";
            if (File.Exists(filePath))
            {
                Process.Start(filePath);
            }
            else
            {
                MessageBox.Show("Файлът \"4.doc\" не съществува!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Save.IsEnabled)
            {
                AdvertisementFileOperations.SaveInvoiceNumber(InvoiceNumber.Text);
                DateTime currentDate = new DateTime();
                DateTime.TryParse(InvoiceDate.Text, out currentDate);

                if (this.savedAdvertisementText != InputText.Text.Trim() && this.isNewBlankInvoiceMade == true &&
                    this.newInvoiceOnDate == currentDate.ToString(this.dateTimeFormat) &&
                    this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
                {
                    int multiplayngAdvertisemen = int.Parse(InvoiceMultiplying.Text);
                    if (AdvertisementComboBox.SelectedIndex == 0)
                    {
                        AdvertisementFileOperations.SaveAdvertisemen(InputText.Text, citizenAdvertisementSuffix);
                        if (multiplayngAdvertisemen > 1)
                        {
                            AdvertisementFileOperations.AddRecurringAdvertisemen(citizenAdvertisementSuffix, multiplayngAdvertisemen - 1, InputText.Text);
                        }
                    }
                    else
                    {
                        AdvertisementFileOperations.SaveAdvertisemen(InputText.Text, companyAdvertisementSuffix);
                        if (multiplayngAdvertisemen > 1)
                        {
                            AdvertisementFileOperations.AddRecurringAdvertisemen(companyAdvertisementSuffix, multiplayngAdvertisemen - 1, InputText.Text);
                        }
                    }
                }

                this.xmlFileOperationsClass.SaveUserData(PrintArea as DependencyObject, currentDate, AdvertisementComboBox, true);
                if (this.isNewBlankInvoiceMade == true && this.newInvoiceOnDate == currentDate.ToString(this.dateTimeFormat) &&
                    this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
                {
                    this.isNewBlankInvoiceMade = false;
                    this.savedAdvertisementText = InputText.Text.Trim();
                }

                CheckNexPreviousButtonAvailability(currentDate);
                this.isInvoiceSaved = true;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollViewerSttings.Height = ProgramMainWindow.ActualHeight - 60;
        }

        private void CashCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BankTransferCheckBox.IsChecked = false;
        }

        private void BankTransferCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CashCheckBox.IsChecked = false;
        }

        private void CashCheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            CashCheckBox.IsChecked = true;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = sender as TextBox;
            string name = obj.Name + validationTipSuffix;
            Label presentLabelName = FindName(name) as Label;
            presentLabelName.Visibility = System.Windows.Visibility.Hidden;
        }

        private void SimpleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox obj = sender as TextBox;
            string inputText = obj.Text.Trim();
            string name = obj.Name + validationTipSuffix;
            Label presentLabelName = FindName(name) as Label;
            if (inputText.Length <= 0)
            {
                presentLabelName.Visibility = System.Windows.Visibility.Visible;
                obj.Text = inputText;
            }
            else
            {
                obj.Text = inputText;
            }
        }

        private void InvoiceMultiplying_TextChanged(object sender, TextChangedEventArgs e)
        {
            int maxNumber = 1000;
            string inputText = InvoiceMultiplying.Text.Trim();
            string newInvoiceMultiplyingNumber = this.invoiceMultiplyingClass.CheckSimpleNumbers(inputText, maxNumber);
            InvoiceMultiplying.Text = newInvoiceMultiplyingNumber;
            if (inputText.Length != newInvoiceMultiplyingNumber.Length)
            {
                InvoiceMultiplying.SelectionStart = newInvoiceMultiplyingNumber.Length;
            }

            if (newInvoiceMultiplyingNumber.Length == maxNumber.ToString().Length)
            {
                InvoiceMultiplyingValidationTip.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                InvoiceMultiplyingValidationTip.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void CustomerBulstatTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = CustomerBulstat.Text.Trim();
            string newBulstatNumber = this.customerBulstatClass.CheckBulstatNumber(inputText);
            CustomerBulstat.Text = newBulstatNumber;
            if (inputText.Length != newBulstatNumber.Length)
            {
                CustomerBulstat.SelectionStart = newBulstatNumber.Length;
            }
        }

        private void ExecutantBulstatTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = ExecutantBulstat.Text.Trim();
            string newBulstatNumber = this.executantBulstatClass.CheckBulstatNumber(inputText);
            ExecutantBulstat.Text = newBulstatNumber;
            if (inputText.Length != newBulstatNumber.Length)
            {
                ExecutantBulstat.SelectionStart = newBulstatNumber.Length;
            }
        }

        private void ExecutantBulstatTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string inputText = ExecutantBulstat.Text.Trim();
            if (inputText.Length < 9)
            {
                ExecutantBulstatValidationTip.Visibility = System.Windows.Visibility.Visible;
                ExecutantBulstat.Text = inputText.ToUpper();
            }
            else
            {
                ExecutantBulstat.Text = inputText.ToUpper();
            }
        }

        private void PricePerWordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = PricePerWord.Text.Trim();
            string newPricePerWord = this.pricePerWordClass.CheckComplexNumber(inputText, 4);
            float maxNumber = 1000;
            float parsedNumber = 0;

            if (newPricePerWord == "" || (this.pricePerWordClass.CheckFloatNumber(newPricePerWord, out parsedNumber) && parsedNumber < maxNumber))
            {
                PricePerWord.Text = newPricePerWord;
                PricePerWordValidationTip.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                PricePerWord.Text = maxNumber.ToString();
                PricePerWordValidationTip.Visibility = System.Windows.Visibility.Visible;
            }

            if (inputText.Length != newPricePerWord.Length)
            {
                PricePerWord.SelectionStart = newPricePerWord.Length;
            }
        }

        private void ValidationTipButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Button obj = sender as Button;
            string name = obj.Name.Replace(validationTipButtonSuffix, "");
            Label presentLabelName = FindName(name) as Label;
            presentLabelName.Visibility = System.Windows.Visibility.Hidden;
        }

        private void InvoiceNumber_Loaded(object sender, RoutedEventArgs e)
        {
            int invoiceNumber = 0;
            int.TryParse(AdvertisementFileOperations.LoadInvoiceNumber(), out invoiceNumber);
            InvoiceNumber.Text = (invoiceNumber + 1).ToString();
        }

        private void ProgramMainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.isInvoiceSaved == false && this.isDataAdded == true)
            {
                MessageBoxResult result = MessageBox.Show("Не сте запазили обявата, наистина ли искате да излезете!",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    if (InvoiceDate.Text != this.newInvoiceOnDate ||
                        (InvoiceDate.Text == this.newInvoiceOnDate && this.xmlFileOperationsClass.CurrentPreffix != this.xmlFileOperationsClass.CurrentMaxPreffix))
                    {
                        this.xmlFileOperationsClass.GoToSpecificDate(DateTime.Parse(this.newInvoiceOnDate));
                    }
                }
                else
                {
                    this.xmlFileOperationsClass.DeleteLastFileForDate(this.newInvoiceOnDate);
                }
            }
            else if (this.isInvoiceSaved == false)
            {
                this.xmlFileOperationsClass.DeleteLastFileForDate(this.newInvoiceOnDate);
            }
        }

        private void PreviousInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime parsedDate = new DateTime();
            if (DateTime.TryParse(InvoiceDate.Text, out parsedDate))
            {
                if (parsedDate.ToString(this.dateTimeFormat) == this.newInvoiceOnDate && this.isNewBlankInvoiceMade == true)
                {
                    this.xmlFileOperationsClass.SaveUserData(PrintArea as DependencyObject, parsedDate, AdvertisementComboBox, true);
                }

                this.xmlFileOperationsClass.LoadUserPreviousData(parsedDate);
                DateTime currentDate = DateTime.Parse(InvoiceDate.Text);
                CheckNexPreviousButtonAvailability(currentDate);

                InvoiceCalendar.SelectedDatesChanged -= InvoiceCalendar_SelectedDatesChanged;
                InvoiceCalendar.SelectedDate = currentDate;
                InvoiceCalendar.DisplayDate = currentDate;
                InvoiceCalendar.SelectedDatesChanged += InvoiceCalendar_SelectedDatesChanged;

                InputText_KeyUp(new Object(), this.customKeyEventArgs);
            }
        }

        private void NextInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime parsedDate = new DateTime();
            if (DateTime.TryParse(InvoiceDate.Text, out parsedDate))
            {
                if (parsedDate.ToString(this.dateTimeFormat) == this.newInvoiceOnDate && this.isNewBlankInvoiceMade == true)
                {
                    this.xmlFileOperationsClass.SaveUserData(PrintArea as DependencyObject, parsedDate, AdvertisementComboBox, true);
                }

                this.xmlFileOperationsClass.LoadUserNextData(parsedDate);
                DateTime currentDate = DateTime.Parse(InvoiceDate.Text);
                CheckNexPreviousButtonAvailability(currentDate);

                InvoiceCalendar.SelectedDatesChanged -= InvoiceCalendar_SelectedDatesChanged;
                InvoiceCalendar.SelectedDate = currentDate;
                InvoiceCalendar.DisplayDate = currentDate;
                InvoiceCalendar.SelectedDatesChanged += InvoiceCalendar_SelectedDatesChanged;

                InputText_KeyUp(new Object(), this.customKeyEventArgs);
            }
        }

        private void InvoiceCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime chousenDate = DateTime.Parse(InvoiceCalendar.SelectedDate.ToString());
            DateTime parsedDate = new DateTime();
            DateTime.TryParse(InvoiceDate.Text, out parsedDate);
            bool isDateExist = this.xmlFileOperationsClass.ChekIfDateExist(chousenDate);
            if (!isDateExist)
            {
                if (parsedDate.ToString(this.dateTimeFormat) == this.newInvoiceOnDate && this.isNewBlankInvoiceMade == true)
                {
                    this.xmlFileOperationsClass.SaveUserData(PrintArea as DependencyObject, parsedDate, AdvertisementComboBox, true);
                }
            }

            InvoiceDate.Text = chousenDate.ToString(this.dateTimeFormat);
            InvoiceCalendar.Visibility = System.Windows.Visibility.Hidden;
            this.isCalenderVisible = false;

            this.xmlFileOperationsClass.GoToSpecificDate(chousenDate);
            if (!isDateExist)
            {
                newInvoice_Click(new Object(), this.customRoutedEventArgs);
            }

            CheckNexPreviousButtonAvailability(DateTime.Parse(InvoiceDate.Text));
            InputText_KeyUp(new Object(), this.customKeyEventArgs);
        }

        private void ShowHideCalendarButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isCalenderVisible == true)
            {
                InvoiceCalendar.Visibility = System.Windows.Visibility.Hidden;
                this.isCalenderVisible = false;
            }
            else
            {
                InvoiceCalendar.Visibility = System.Windows.Visibility.Visible;
                this.isCalenderVisible = true;
            }
        }

        private void newInvoice_Click(object sender, RoutedEventArgs e)
        {
            if (this.isNewBlankInvoiceMade == true && this.isDataAdded == true)
            {
                MessageBoxResult result = MessageBox.Show("Не сте запазили предишната обявата, наистина ли искате да направите нова обява!",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    if (InvoiceDate.Text != this.newInvoiceOnDate ||
                        (InvoiceDate.Text == this.newInvoiceOnDate && this.xmlFileOperationsClass.CurrentPreffix != this.xmlFileOperationsClass.CurrentMaxPreffix))
                    {
                        this.xmlFileOperationsClass.GoToSpecificDate(DateTime.Parse(this.newInvoiceOnDate));
                        CheckNexPreviousButtonAvailability(DateTime.Parse(this.newInvoiceOnDate));
                    }
                }
                else
                {
                    this.xmlFileOperationsClass.DeleteLastFileForDate(this.newInvoiceOnDate);

                    DateTime currentDate = new DateTime();
                    DateTime.TryParse(InvoiceDate.Text, out currentDate);
                    if (AdvertisementComboBox.SelectedIndex == 0)
                    {
                        this.xmlFileOperationsClass.LoadDataFromPath(citizenDefaultDataPath, true);
                    }
                    else
                    {
                        this.xmlFileOperationsClass.LoadDataFromPath(companyDefaultDataPath, true);
                    }

                    InvoiceDate.Text = currentDate.ToString(this.dateTimeFormat);
                    InvoiceNumber_Loaded(new Object(), this.customRoutedEventArgs);
                    this.xmlFileOperationsClass.SaveUserData(PrintArea as DependencyObject, currentDate, AdvertisementComboBox);
                    this.newInvoiceOnDate = currentDate.ToString(this.dateTimeFormat);
                    CheckNexPreviousButtonAvailability(currentDate);

                    Save.IsEnabled = false;
                    this.isInvoiceSaved = false;
                    this.isNewBlankInvoiceMade = true;
                    this.isDataAdded = false;
                }
            }
            else
            {
                if (this.isNewBlankInvoiceMade == true)
                {
                    this.xmlFileOperationsClass.DeleteLastFileForDate(this.newInvoiceOnDate);
                }

                DateTime currentDate = new DateTime();
                DateTime.TryParse(InvoiceDate.Text, out currentDate);
                if (AdvertisementComboBox.SelectedIndex == 0)
                {
                    this.xmlFileOperationsClass.LoadDataFromPath(citizenDefaultDataPath, true);
                }
                else
                {
                    this.xmlFileOperationsClass.LoadDataFromPath(companyDefaultDataPath, true);
                }

                InvoiceDate.Text = currentDate.ToString(this.dateTimeFormat);
                InvoiceNumber_Loaded(new Object(), this.customRoutedEventArgs);
                this.xmlFileOperationsClass.SaveUserData(PrintArea as DependencyObject, currentDate, AdvertisementComboBox);
                this.newInvoiceOnDate = currentDate.ToString(this.dateTimeFormat);
                CheckNexPreviousButtonAvailability(currentDate);

                Save.IsEnabled = false;
                this.isInvoiceSaved = false;
                this.isNewBlankInvoiceMade = true;
                this.isDataAdded = false;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Сигурни ли сте, че желаете да изтриете обявата!",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                DateTime parsedDate = new DateTime();
                DateTime.TryParse(InvoiceDate.Text, out parsedDate);
                if (parsedDate.ToString(this.dateTimeFormat) == this.newInvoiceOnDate && this.isNewBlankInvoiceMade == true &&
                    this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
                {
                    this.isNewBlankInvoiceMade = false;
                    this.isDataAdded = false;
                    this.isInvoiceSaved = true;
                }

                this.xmlFileOperationsClass.DeleteCurrentInvoice();
                MessageBox.Show("Обявата беше успешно изтрита!", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                CheckAndGoToAnotherDate(parsedDate);
            }
        }

        private void DeleteInvoicesForADayButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime dayToDelete = new DateTime();
            if (DateTime.TryParse(DeleteInvoicesForADayTextBox.Text, out dayToDelete))
            {
                MessageBoxResult result = MessageBox.Show("Сигурни ли сте, че желаете да изтриете всички обяви на дата \"" +
                    dayToDelete.ToString(this.dateTimeFormat) + "\"!",
                    "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (dayToDelete.ToString(this.dateTimeFormat) == this.newInvoiceOnDate && this.isNewBlankInvoiceMade == true &&
                        this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
                    {
                        this.isNewBlankInvoiceMade = false;
                        this.isDataAdded = false;
                        this.isInvoiceSaved = true;
                    }

                    this.xmlFileOperationsClass.DeleteAllInvoicesForASpecificDay(dayToDelete.ToString(this.dateTimeFormat));
                    MessageBox.Show("Всички обяви на дата \"" + dayToDelete.ToString(this.dateTimeFormat) + "\" бяха успешно изтрити!",
                        "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    CheckAndGoToAnotherDate(dayToDelete);
                }
            }
        }

        private void DeleteInvoicesForAMonthButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime monthToDelete = new DateTime();
            if (DateTime.TryParse(DeleteInvoicesForAMonthTextBox.Text, out monthToDelete))
            {
                MessageBoxResult result = MessageBox.Show("Сигурни ли сте, че желаете да изтриете всички обяви за месец \"" +
                    monthToDelete.ToString("MM/yyyy") + "\"!",
                    "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (this.newInvoiceOnDate.Contains(monthToDelete.ToString("MM/yyyy")) && this.isNewBlankInvoiceMade == true &&
                        this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
                    {
                        this.isNewBlankInvoiceMade = false;
                        this.isDataAdded = false;
                        this.isInvoiceSaved = true;
                    }

                    this.xmlFileOperationsClass.DeleteAllInvoicesForAMonthOrYear(monthToDelete.ToString("MM/yyyy"));
                    MessageBox.Show("Всички обяви за месец \"" + monthToDelete.ToString("MM/yyyy") + "\" бяха успешно изтрити!",
                        "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    CheckAndGoToAnotherDate(monthToDelete);
                }
            }
        }

        private void DeleteInvoicesForAYearButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime yearToDelete = new DateTime();
            if (DateTime.TryParse("01.01." + DeleteInvoicesForAYearTextBox.Text, out yearToDelete))
            {
                MessageBoxResult result = MessageBox.Show("Сигурни ли сте, че желаете да изтриете всички обяви за \"" +
                    yearToDelete.ToString("yyyy") + "\" година!",
                    "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (this.newInvoiceOnDate.Contains(yearToDelete.ToString("yyyy")) && this.isNewBlankInvoiceMade == true &&
                        this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
                    {
                        this.isNewBlankInvoiceMade = false;
                        this.isDataAdded = false;
                        this.isInvoiceSaved = true;
                    }

                    this.xmlFileOperationsClass.DeleteAllInvoicesForAMonthOrYear(yearToDelete.ToString("yyyy"));
                    MessageBox.Show("Всички обяви за \"" + yearToDelete.ToString("yyyy") + "\" година бяха успешно изтрити!",
                        "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    CheckAndGoToAnotherDate(yearToDelete);
                }
            }
        }

        private void DeleteInvoicesForADayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = DeleteInvoicesForADayTextBox.Text.Trim();
            string newDateNumber = this.restrictDeleteDayClass.CheckComplexNumber(inputText, 10);
            DeleteInvoicesForADayTextBox.Text = newDateNumber;
            if (inputText.Length != newDateNumber.Length)
            {
                DeleteInvoicesForADayTextBox.SelectionStart = newDateNumber.Length;
            }
        }

        private void DeleteInvoicesForAMonthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = DeleteInvoicesForAMonthTextBox.Text.Trim();
            string newDateNumber = this.restrictDeleteMonthClass.CheckComplexNumber(inputText, 7);
            DeleteInvoicesForAMonthTextBox.Text = newDateNumber;
            if (inputText.Length != newDateNumber.Length)
            {
                DeleteInvoicesForAMonthTextBox.SelectionStart = newDateNumber.Length;
            }
        }

        private void DeleteInvoicesForAYearTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = DeleteInvoicesForAYearTextBox.Text.Trim();
            RestrictInputData restrictionClass = new RestrictInputData();
            string newDateNumber = this.restrictDeleteYearClass.CheckComplexNumber(inputText, 4);
            DeleteInvoicesForAYearTextBox.Text = newDateNumber;
            if (inputText.Length != newDateNumber.Length)
            {
                DeleteInvoicesForAYearTextBox.SelectionStart = newDateNumber.Length;
            }
        }

        private void ChangeDefaultDataButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime parsedDate = new DateTime();
            if (DateTime.TryParse(InvoiceDate.Text, out parsedDate) && this.isInvoiceSaved == false &&
                this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix &&
                InputText.Text.Trim() != "")
            {
                this.xmlFileOperationsClass.SaveUserData(PrintArea as DependencyObject, parsedDate, AdvertisementComboBox, true);
                this.isNewBlankInvoiceMade = true;
                this.isDataAdded = true;
                this.isInvoiceSaved = false;
            }

            NextInvoiceButton.Visibility = System.Windows.Visibility.Hidden;
            PreviousInvoiceButton.Visibility = System.Windows.Visibility.Hidden;
            InvoiceCalendar.Visibility = System.Windows.Visibility.Hidden;
            ShowHideCalendarButton.Width = 0;
            InvoiceDate.Visibility = System.Windows.Visibility.Hidden;
            AdvertisementComboBox.Visibility = System.Windows.Visibility.Hidden;
            MenuFile.IsEnabled = false;
            MenuOptions.IsEnabled = false;
            if (AdvertisementComboBox.SelectedIndex == 0)
            {
                this.defaultDataFileOperationClass.LoadDataFromPath(citizenDefaultDataPath, true);
            }
            else
            {
                this.defaultDataFileOperationClass.LoadDataFromPath(companyDefaultDataPath, true);
            }

            DefaultDataConfirmButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void DefaultDataConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (AdvertisementComboBox.SelectedIndex == 0)
            {
                this.defaultDataFileOperationClass.OverwriteDefaultData(PrintArea, citizenDefaultDataPath, AdvertisementComboBox);
            }
            else
            {
                this.defaultDataFileOperationClass.OverwriteDefaultData(PrintArea, companyDefaultDataPath, AdvertisementComboBox);
            }

            AdvertisementFileOperations.SaveInvoiceNumber(InvoiceNumber.Text);
            DefaultDataConfirmButton.Visibility = System.Windows.Visibility.Hidden;
            NextInvoiceButton.Visibility = System.Windows.Visibility.Visible;
            PreviousInvoiceButton.Visibility = System.Windows.Visibility.Visible;
            ShowHideCalendarButton.Width = Double.NaN;
            InvoiceDate.Visibility = System.Windows.Visibility.Visible;
            AdvertisementComboBox.Visibility = System.Windows.Visibility.Visible;
            MenuFile.IsEnabled = true;
            MenuOptions.IsEnabled = true;

            DateTime parsedDate = new DateTime();
            DateTime.TryParse(InvoiceDate.Text, out parsedDate);
            this.xmlFileOperationsClass.GoToSpecificDate(parsedDate);
            InputText_KeyUp(new Object(), this.customKeyEventArgs);
            CheckNexPreviousButtonAvailability(parsedDate);
        }

        private void CopyInvoice_Click(object sender, RoutedEventArgs e)
        {
            List<List<string>> invoiceData = new List<List<string>>();
            invoiceData = UserInfo.GetUserData(PrintArea, invoiceData);
            this.copiedInvoice = new List<string>();
            foreach (var data in invoiceData)
            {
                this.copiedInvoice.Add(data[0]);
                if (data[1] == "null")
                {
                    this.copiedInvoice.Add("");
                }
                else
                {
                    this.copiedInvoice.Add(data[1]);
                }
            }

            PasteInvoice.IsEnabled = true;
        }

        private void PasteInvoice_Click(object sender, RoutedEventArgs e)
        {
            string currentDate = InvoiceDate.Text;
            string currentInvoiceNumber = InvoiceNumber.Text;
            this.xmlFileOperationsClass.LoadDataFromList(this.copiedInvoice);
            InvoiceDate.Text = currentDate;
            InvoiceNumber.Text = currentInvoiceNumber;
            this.isInvoiceSaved = false;
            if (this.isNewBlankInvoiceMade == true && currentDate == this.newInvoiceOnDate &&
                this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
            {
                this.isDataAdded = true;
            }
            InputText_KeyUp(new Object(), this.customKeyEventArgs);
        }

        private void AdvertisementComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.isNewBlankInvoiceMade == true && this.isDataAdded == false)
            {
                if (InvoiceDate.Text == this.newInvoiceOnDate && this.xmlFileOperationsClass.CurrentPreffix == this.xmlFileOperationsClass.CurrentMaxPreffix)
                {
                    newInvoice_Click(new Object(), this.customRoutedEventArgs);
                }
            }
        }

        private void SendCitizenAdvertisementEmail_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
            Microsoft.Office.Interop.Outlook.MailItem mailItem = app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
            mailItem.Subject = "Обяви за граждани";
            mailItem.To = "someone@example.com";
            mailItem.Body = "Обяви за граждани за деня";
            mailItem.Attachments.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/2.doc");
            mailItem.Display(true);
        }

        private void SendCompanyAdvertisementEmail_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
            Microsoft.Office.Interop.Outlook.MailItem mailItem = app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
            mailItem.Subject = "Фирмена информация";
            mailItem.To = "someone@example.com";
            mailItem.Body = "Фирмена информация за деня";
            mailItem.Attachments.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/4.doc");
            mailItem.Display(true);
        }

        private void CheckAndGoToAnotherDate(DateTime DateToCheck)
        {
            if (this.xmlFileOperationsClass.CurrentPreffix == 0 && this.xmlFileOperationsClass.CurrentMaxPreffix == 0)
            {
                DateTime previousDate = new DateTime();
                DateTime nextDate = new DateTime();
                bool isPreviousAvailable = this.xmlFileOperationsClass.CheckIsPreviousAvailableDate(DateToCheck, out previousDate);
                bool isNextAvailable = this.xmlFileOperationsClass.CheckIsNextAvailableDate(DateToCheck, out nextDate);
                if (isNextAvailable)
                {
                    NextInvoiceButton_Click(new Object(), this.customRoutedEventArgs);
                }
                else if (isPreviousAvailable)
                {
                    PreviousInvoiceButton_Click(new Object(), this.customRoutedEventArgs);
                }
                else
                {
                    newInvoice_Click(new Object(), this.customRoutedEventArgs);
                }
            }
            else
            {
                CheckNexPreviousButtonAvailability(DateToCheck);
            }

            InputText_KeyUp(new Object(), this.customKeyEventArgs);
        }

        private void CheckNexPreviousButtonAvailability(DateTime currentDate)
        {
            DateTime date = new DateTime();
            bool isPreviousAvailable = this.xmlFileOperationsClass.CheckIsPreviousAvailableDate(currentDate, out date);
            RestrictNextPreviousButton.RestrictButton(PreviousInvoiceButton, isPreviousAvailable);
            bool isNextAvailable = this.xmlFileOperationsClass.CheckIsNextAvailableDate(currentDate, out date);
            RestrictNextPreviousButton.RestrictButton(NextInvoiceButton, isNextAvailable);
        }
    }
}
