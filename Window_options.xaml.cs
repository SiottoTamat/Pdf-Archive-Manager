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
    /// Interaction logic for Window_options.xaml
    /// </summary>
    public partial class Window_options : Window
    {
        public Window_options()
        {
            InitializeComponent();
        }

        private void Integer_Preview_Input(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static readonly System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void Textbox_options_changed(object sender, TextChangedEventArgs e)
        {
            try 
            { 
                Options.n_char_showed_search = int.Parse(Textbox_n_words_option.Text);
            }
            catch { }
            
        }
    }
}
