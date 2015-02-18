using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewspaperProgram
{
    public static class TranslateNumberToWords
    {
        public static string ConvertNumberToWords(int integerNumber, bool isTousend = false)
        {
            if (integerNumber == 0)
            {
                return "нула";
            }

            if (integerNumber < 0)
            {
                return "минус " + ConvertNumberToWords(Math.Abs(integerNumber));
            }

            if (integerNumber == 1)
            {
                return "един";
            }

            if (integerNumber == 2 && !isTousend)
            {
                return "два";
            }

            if (integerNumber >= 1000 && integerNumber < 2000)
            {
                return "хиляда " + ConvertNumberToWords(integerNumber % 1000);
            }
            else if (integerNumber >= 2000 && integerNumber < 10000)
            {
                return ConvertNumberToWords(integerNumber / 1000, true) + " хиляди " + ConvertNumberToWords(integerNumber % 1000);
            }
            else if (integerNumber >= 10000 && integerNumber < 100000)
            {
                return ConvertNumberToWords(integerNumber / 1000, true) + " хиляди " + ConvertNumberToWords(integerNumber % 1000);
            }
            else if (integerNumber >= 100000)
            {
                return "Въведената сума е твърде голяма !";
            }

            string words = "";

            if (integerNumber > 0)
            {

                var unitsMap = new[] { "нула", "едно", "две", "три", "четири", "пет", "шест", "седем", "осем", "девет", "десет", "единадесет", "дванадесет", "тринадесет", "четиринадесет", "петнадесет", "шестнадесет", "седемнадесет", "осемнадесет", "деветнадесет" };
                var tensMap = new[] { "нула", "десет", "двадесет", "тридесет", "четиридесет", "петдесет", "шестдесет", "седемдесет", "осемдесет", "деведесет" };
                var HundrtsMap = new[] { "", "сто", "двеста", "триста", "четиристотин", "петстотин", "шестотин", "седемстотин", "осемстотин", "деветстотин" };

                if (integerNumber < 20)
                    words += unitsMap[integerNumber];
                else if (integerNumber < 100)
                {
                    words += tensMap[integerNumber / 10];
                    if ((integerNumber % 10) > 0)
                        words += " и " + unitsMap[integerNumber % 10];
                }
                else
                {
                    words += HundrtsMap[integerNumber / 100];
                    if (integerNumber % 100 > 20)
                    {
                        words += tensMap[integerNumber / 10 % 10];
                    }
                    else if ((integerNumber / 10) % 10 > 0 && integerNumber.ToString()[integerNumber.ToString().Length - 2] != '1')
                    {
                        words += " и " + tensMap[(integerNumber / 10) % 10];
                    }

                    if ((integerNumber % 10) > 0)
                    {
                        if (integerNumber % 100 < 20)
                        {
                            words += " и " + unitsMap[integerNumber % 100];
                        }
                        else
                        {
                            words += " и " + unitsMap[integerNumber % 10];
                        }
                    }
                    else if (integerNumber % 100 == 10)
                    {
                        words += " и " + unitsMap[integerNumber % 100];
                    }
                }

                if (integerNumber > 10 && integerNumber.ToString()[integerNumber.ToString().Length - 2] != '1')
                {
                    string tempWord = "";
                    if (integerNumber % 10 == 1)
                    {
                        for (int i = 0; i < words.Length - 2; i++)
                        {
                            tempWord += words[i];
                        }

                        tempWord += "ин";
                        words = tempWord;
                    }
                    else if (integerNumber % 10 == 2)
                    {
                        for (int i = 0; i < words.Length - 1; i++)
                        {
                            tempWord += words[i];
                        }

                        tempWord += "а";
                        words = tempWord;
                    }
                }
            }

            return words;
        }
    }
}
