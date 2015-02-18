using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewspaperProgram.ViewModels
{
    public class RestrictInputData
    {
        private string defaultValue = "1";

        public RestrictInputData()
        {
        }

        public string DefaultValue
        {
            get
            {
                return this.defaultValue;
            }

            set
            {
                this.defaultValue = value;
            }
        }

        public string CheckSimpleNumbers(string inputNumber, int maxNumber)
        {
            string resultNumber = "";
            int value = 0;
            bool isValidInt = int.TryParse(inputNumber, out value);

            if (isValidInt == true && value > 0 && value <= maxNumber)
            {
                resultNumber = value.ToString();
                this.DefaultValue = value.ToString();
            }
            else if (isValidInt == true && value >= maxNumber)
            {
                this.DefaultValue = maxNumber.ToString();
                return this.DefaultValue;
            }
            else if (inputNumber == "")
            {
                resultNumber = inputNumber;
                this.DefaultValue = inputNumber;
            }
            else
            {
                resultNumber = this.DefaultValue;
            }

            return resultNumber;
        }

        public string CheckBulstatNumber(string inputBulstatNumber)
        {
            string resultBulstatNumber = "";
            if (inputBulstatNumber.Length > 11)
            {
                return this.DefaultValue;
            }
            else
            {
                for (int i = 0; i < inputBulstatNumber.Length; i++)
                {
                    if (this.DefaultValue.IndexOf(inputBulstatNumber[i]) >= 0)
                    {
                        resultBulstatNumber += inputBulstatNumber[i];
                    }
                    else
                    {
                        int tempNumber = 0;
                        if (int.TryParse(inputBulstatNumber[i].ToString(), out tempNumber))
                        {
                            resultBulstatNumber += inputBulstatNumber[i];
                        }
                        else if (i <= 1)
                        {
                            if (inputBulstatNumber.ToUpper()[i] >= 'A' && inputBulstatNumber.ToUpper()[i] <= 'Z')
                            {
                                resultBulstatNumber += inputBulstatNumber[i];
                            }
                        }
                    }
                }

                this.DefaultValue = resultBulstatNumber;
            }
            return resultBulstatNumber;
        }

        public string CheckComplexNumber(string inputComplexNumber, int maxLength)
        {
            string resultComplexNumber = "";

            if (inputComplexNumber.Length > maxLength)
            {
                return this.DefaultValue;
            }
            else
            {
                for (int i = 0; i < inputComplexNumber.Length; i++)
                {
                    if (this.DefaultValue.IndexOf(inputComplexNumber[i]) >= 0)
                    {
                        resultComplexNumber += inputComplexNumber[i];
                    }
                    else
                    {
                        int tempNumber = 0;
                        if (int.TryParse(inputComplexNumber[i].ToString(), out tempNumber) ||
                            inputComplexNumber[i] == '.' || inputComplexNumber[i] == ',')
                        {
                            resultComplexNumber += inputComplexNumber[i];
                        }
                    }
                }

                this.DefaultValue = resultComplexNumber;
            }

            return resultComplexNumber;
        }

        public bool CheckFloatNumber(string inputString, out float parsedNumber)
        {
            float number = 0;
            string inputStringWithChangedDot = "";

            if (float.TryParse(inputString, out number))
            {
                parsedNumber = number;
                return true;
            }
            else if (inputString.IndexOf('.') >= 0)
            {
                inputStringWithChangedDot = inputString.Replace('.', ',');
                if (float.TryParse(inputStringWithChangedDot, out number))
                {
                    parsedNumber = number;
                    return true;
                }
            }
            else if (inputString.IndexOf(',') >= 0)
            {
                inputStringWithChangedDot = inputString.Replace(',', '.');
                if (float.TryParse(inputStringWithChangedDot, out number))
                {
                    parsedNumber = number;
                    return true;
                }
            }

            parsedNumber = 0;
            return false;
        }

        public string CheckInputTextLength(string inputText)
        {
            if (inputText.Length > 40000)
            {
                return this.DefaultValue;
            }
            else if (Calculations.CountWords(inputText) > 1500)
            {
                return this.DefaultValue;
            }

            this.DefaultValue = inputText;
            return this.DefaultValue;
        }
    }
}
