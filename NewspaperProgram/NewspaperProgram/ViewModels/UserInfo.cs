using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NewspaperProgram.ViewModels
{
    public static class UserInfo
    {
        public static List<List<string>> GetUserData(DependencyObject parent, List<List<string>> userData)
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is TextBox)
                {
                    List<string> data = new List<string>();
                    data.Add((child as TextBox).Name);
                    if ((child as TextBox).Text.Trim() == "")
                    {
                        data.Add("null");
                    }
                    else
                    {
                        data.Add((child as TextBox).Text);
                    }
                    
                    userData.Add(data);

                }

                if ((child as DependencyObject) != null)
                {
                    GetUserData(child as DependencyObject, userData);
                }
            }

            return userData;
        }
    }
}
