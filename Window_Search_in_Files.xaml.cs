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
            StackPanel_Result_Search.Children.Clear();
    foreach (string filename in Directory.GetFiles(folder))
            {
                FileInfo file = new FileInfo(filename);
                if (file.Extension == ".txt")
                {


                    string text = System.IO.File.ReadAllText(filename);
                    string[] pages = Split_Pages(text);
                    Regex rgx = new Regex(searchstring);
                    for (int i=0; i<pages.Length; i++)
                    {
                        //MessageBox.Show(pages[i]);
                        
                        foreach (Match m in rgx.Matches(pages[i]))
                        {
                            int max, min;
                            if(m.Index - 120 < 0) { min = 0; } else { min = m.Index - 120; }
                            if(m.Index + 120> pages[i].Length) { max = pages[i].Length-m.Index; } else { max = 240; }
                            string area = pages[i].Substring(min, max);
                            MessageBox.Show($"In {file.Name} at page {i}: found '{m.Value}' at position {m.Index}.{Environment.NewLine}...{area}...");
                            SearchItem item = new SearchItem();
                            item.Label_NameFile.Content = file.Name;
                            item.Label_Page.Content = i.ToString();
                            item.RichTextBox_search.Document.Blocks.Add( new Paragraph(new Run($"...{area}...")));
                            StackPanel_Result_Search.Children.Add(item);
                        }
                    }
                    
                    
                }
                /*Match m = Regex.Match(text, searchstring, RegexOptions.IgnoreCase);
                //if (m.Success)
                {

                }
                    MessageBox.Show($"Found '{m.Value}' at position {m.Index}.");
                }*/
            }
            return null;
        }
        string[] Split_Pages(string text)
        {
            string[] result = Regex.Split(text, "---------\\d---------");
            return result;
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
