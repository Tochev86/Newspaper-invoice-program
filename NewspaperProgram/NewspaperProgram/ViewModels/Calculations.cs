using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewspaperProgram.ViewModels
{
    public static class Calculations
    {

        private const decimal DDS = 6;

        public static int CountWords(string inputText)
        {
            int counter = 0;
            inputText = inputText.TrimStart();
            for (int i = 0; i < inputText.Length; i++)
            {
                if (i == 0 && inputText[i] != ' ')
                {
                    counter++;
                }
                else
                {
                    if (inputText[i] != ' ' && inputText[i - 1] == ' ')
                    {
                        counter++;
                    }
                }
            }

            return counter;
        }

        public static decimal CalculateWholePrice(int numberOfWords, string priceForAWord, decimal multiplyNumber)
        {
            decimal result = 0;
            decimal pricePerWord = 0;
            string newPriceForAWord = "";
            if (!decimal.TryParse(priceForAWord, out pricePerWord))
            {
                int indexOFDot = priceForAWord.IndexOf('.');
                for (int i = 0; i < priceForAWord.Length; i++)
                {
                    if (i != indexOFDot)
                    {
                        newPriceForAWord += priceForAWord[i];
                    }
                    else
                    {
                        newPriceForAWord += ",";
                    }
                }
                decimal.TryParse(newPriceForAWord, out pricePerWord);
            }

            result = (numberOfWords * pricePerWord) * multiplyNumber;
            return result;
        }

        public static decimal CalculateDDSPrice(decimal wholePrice)
        {
            decimal DDSResult = 0;
            DDSResult = wholePrice / DDS;
            return DDSResult;
        }
    }
}
