using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NewspaperProgram.ViewModels
{
    public static class PrintInvoice
    {
        public static void Print(FrameworkElement PrintArea, PrintDialog printDialog)
        {
            PrintArea.HorizontalAlignment = HorizontalAlignment.Left;

            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(PrintArea, "Print Invoice");
            }
            PrintArea.HorizontalAlignment = HorizontalAlignment.Center;
        }
    }
}
