using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace WPF_PDF_Organizer
{
    /// <summary>
    /// Interaction logic for Window_Search_in_Files.xaml
    /// </summary>
    public partial class Window_Search_in_Files : Window
    {
        public Window_Search_in_Files()
        {
            InitializeComponent();
        }
        #region Tools
    List<MainWindow.FoundItem> Search_In_Text(string folder, string searchstring, bool subdir=false)
        {
    foreach (string filename in Directory.GetFiles(folder))
            {
                FileInfo file = new FileInfo(filename);
                if (file.Extension == ".txt")
                {

                
                string text = System.IO.File.ReadAllText(filename);
                Match m = Regex.Match(text, searchstring, RegexOptions.IgnoreCase);
                if (m.Success)
                {

                }
                    MessageBox.Show($"Found '{m.Value}' at position {m.Index}.");
                }
            }
            return null;
        }

        #endregion


        #region ButtonsAndClicks
        private void Button_TestCreateItem_Click(object sender, RoutedEventArgs e)
        {

            SearchItem item = new SearchItem();
            //GroupBox_Items.Content = item;
            //StackPanel_Result_Search.Children.Add(item);
            List<MainWindow.FoundItem> list = Search_In_Text(TextBox_Directory.Text, TextBox_Search.Text);
        }
        #endregion
    }
}
