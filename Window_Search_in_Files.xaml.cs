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
                    Regex rgx = new Regex(searchstring, RegexOptions.IgnoreCase);
                    
                    for (int i=0; i<pages.Length; i++)
                    {
                        //MessageBox.Show(pages[i]);

                        foreach (Match m in rgx.Matches(pages[i]))
                        {
                            
                            string pre_string = pages[i].Substring(0, m.Index);
                            string post_string = pages[i].Substring(m.Index + m.Length);

                            if (pre_string.Length > 200) { pre_string = pre_string.Substring(pre_string.Length - 200); } else { }
                            if (post_string.Length > 200) { post_string = post_string.Substring(0, 200); }

                            SearchItem item = new SearchItem();
                            item.file = file;
                            
                            TextBlock buttonTextBlock =(TextBlock)item.Button_Namefile.FindName("Button_Namefile_TextBlock");
                            TextBlock buttonTextBlockPdf = (TextBlock)item.Button_Namefile.FindName("Button_Namefile_TextBlock_pdf");
                            buttonTextBlock.Text = file.Name;
                            string pdfFullFileName = (file.FullName).Replace(".txt", ".pdf");
                            if (File.Exists(pdfFullFileName))
                            {
                                string pdfFileName = (file.Name).Replace(".txt", ".pdf");
                                buttonTextBlockPdf.Text = pdfFileName;
                            }
                            else
                            {
                                buttonTextBlockPdf.Text = "No pdf version of this file.";
                            }





                            item.Label_Page.Content = "Page "+(i+1).ToString();


                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add("...");
                            paragraph.Inlines.Add(pre_string);
                            paragraph.Inlines.Add(new Run(m.Value) { FontWeight = FontWeights.Bold, Background = Brushes.Yellow }) ;
                            paragraph.Inlines.Add(post_string);
                            paragraph.Inlines.Add("...");
                            item.RichTextBox_search.Document.Blocks.Clear();
                            item.RichTextBox_search.Document.Blocks.Add(paragraph);
                            StackPanel_Result_Search.Children.Add(item);
                        }
                    }
                    Label_Nfinds.Content = StackPanel_Result_Search.Children.Count;
                    
                    
                }
                
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
