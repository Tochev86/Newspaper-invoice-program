using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NewspaperProgram.ViewModels
{
    public static class RestrictNextPreviousButton
    {
        public static void RestrictButton(Button button, bool isAvailable)
        {
            if (isAvailable)
            {
                button.IsEnabled = true;
            }
            else
            {
                button.IsEnabled = false;
            }
        }
    }
}
