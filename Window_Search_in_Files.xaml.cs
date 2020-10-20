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
using System.Windows.Shapes;

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

        private void Button_TestCreateItem_Click(object sender, RoutedEventArgs e)
        {

            SearchItem item = new SearchItem();
            //GroupBox_Items.Content = item;
            StackPanel_Result_Search.Children.Add(item);
        }
    }
}
