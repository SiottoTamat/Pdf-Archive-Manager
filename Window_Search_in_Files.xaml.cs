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
using System.Linq.Expressions;
using System.Windows.Threading;


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
        public void Search_In_Text(string folder, string searchstring)
        {
            StackPanel_Result_Search.Children.Clear();
            SearchOption option = SearchOption.TopDirectoryOnly;
            if (CheckBox_In_Subfolders.IsChecked == true)
            {
                option = SearchOption.AllDirectories;
            }
            try
            {

                int index = 0;
                string[] files = Directory.GetFiles(folder, "*.txt", option);
                int nfiles = files.Length;
                Task.Run(() =>
                {


                    foreach (string filename in files)
                    {
                        Dispatcher.Invoke(new Action(() =>
                          {
                              index++;
                              string counter = $"{index} of {nfiles} files.";
                              Label_Counter.Content = counter;

                          }));
                        FileInfo file = new FileInfo(filename);


                        string text = System.IO.File.ReadAllText(filename);
                        string[] pages = Split_Pages(text);
                        Regex rgx = new Regex(searchstring, RegexOptions.IgnoreCase);
                        int limit_n_words_showed = Options.n_char_showed_search / 2;

                        for (int i = 0; i < pages.Length; i++)
                        {
                            foreach (Match m in rgx.Matches(pages[i]))
                            {

                                string pre_string = pages[i].Substring(0, m.Index);
                                string post_string = pages[i].Substring(m.Index + m.Length);

                                if (pre_string.Length > limit_n_words_showed) { pre_string = pre_string.Substring(pre_string.Length - limit_n_words_showed); } else { }
                                if (post_string.Length > limit_n_words_showed) { post_string = post_string.Substring(0, limit_n_words_showed); }
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    SearchItem item = new SearchItem();
                                    item.file = file;



                                    TextBlock buttonTextBlock = (TextBlock)item.Button_Namefile.FindName("Button_Namefile_TextBlock");
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

                                    item.Label_Page.Content = "Page " + (i + 1).ToString();

                                    Paragraph paragraph = new Paragraph();
                                    paragraph.Inlines.Add("...");
                                    paragraph.Inlines.Add(pre_string);
                                    paragraph.Inlines.Add(new Run(m.Value) { FontWeight = FontWeights.Bold, Background = Brushes.Yellow });
                                    paragraph.Inlines.Add(post_string);
                                    paragraph.Inlines.Add("...");
                                    item.RichTextBox_search.Document.Blocks.Clear();
                                    item.RichTextBox_search.Document.Blocks.Add(paragraph);
                                    StackPanel_Result_Search.Children.Add(item);
                                }));
                            }
                        }
                        Dispatcher.Invoke(new Action(() =>
                        {
                            Label_Nfinds.Content = StackPanel_Result_Search.Children.Count;
                        }));


                    }

                    string[] Split_Pages(string text)
                    {
                        string[] result = Regex.Split(text, "---------\\d---------");
                        return result;
                    }
                });
            }
            catch { }

        }

        public static void CloneDocument(FlowDocument from, FlowDocument to)
        {
            TextRange range = new TextRange(from.ContentStart, from.ContentEnd);
            MemoryStream stream = new MemoryStream();
            //System.Windows.Markup.XamlWriter.Save(range, stream);
            range.Save(stream, DataFormats.Rtf);
            TextRange range2 = new TextRange(to.ContentEnd, to.ContentEnd);
            range2.Load(stream, DataFormats.Rtf);
        }
        #endregion


        #region ButtonsAndClicks
        public void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            Search_In_Text(TextBox_Directory.Text, TextBox_Search.Text);
        }
        private void Menuitem_SaveAs_Rtf(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.DefaultExt = "rtf";
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter =
            "Rich text files (*.rtf)|*.rtf|All files (*.*)|*.*";
            List<SearchItem> copylist = new List<SearchItem>();
             foreach (SearchItem item in StackPanel_Result_Search.Children) 
            {
                copylist.Add(item);
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                
                string oldfilename = "";
                RichTextBox temp = new RichTextBox();
                FlowDocument targetdocument = new FlowDocument();
                foreach (SearchItem item in StackPanel_Result_Search.Children)
                {
                    bool newname = false;
                    Paragraph paragraphTitle = new Paragraph();
                    paragraphTitle.TextAlignment = TextAlignment.Center;

                    Paragraph paragraphSeparator = new Paragraph();
                    paragraphSeparator.TextAlignment = TextAlignment.Center;
                    string fileSeparator = $"{Environment.NewLine}________________________{Environment.NewLine}";
                    paragraphSeparator.Inlines.Add(new Run(fileSeparator));



                    Paragraph paragraphPage = new Paragraph();
                    string separator = $"{item.Label_Page.Content}";
                    paragraphPage.Inlines.Add(new Run(separator) { FontWeight = FontWeights.Bold });


                    string newfilename = item.file.Name;
                    if (newfilename != oldfilename)
                    {
                        
                        string titleline = $"{newfilename}: {Environment.NewLine}";
                        paragraphTitle.Inlines.Add(new Run(titleline) { FontWeight = FontWeights.Bold });

                        //separator =  + separator;
                        oldfilename = newfilename;
                        newname = true;
                    }
                    
                    






                    CloneDocument(item.RichTextBox_search.Document, targetdocument);



                    
                    if (newname)
                    {
                        targetdocument.Blocks.InsertBefore(targetdocument.Blocks.LastBlock, paragraphSeparator);
                        targetdocument.Blocks.InsertBefore(targetdocument.Blocks.LastBlock, paragraphTitle);
                    }
                    
                    targetdocument.Blocks.InsertBefore(targetdocument.Blocks.LastBlock, paragraphPage);


                    
                }

                try
                {
                    TextRange range;
                    FileStream fStream;
                    range = new TextRange(targetdocument.ContentStart, targetdocument.ContentEnd);
                    fStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    range.Save(fStream, DataFormats.Rtf);
                    fStream.Close();
                    
                    
                }
                catch
                {

                }
                StackPanel_Result_Search.Children.Clear();
                foreach (SearchItem item in copylist)
                {
                    StackPanel_Result_Search.Children.Add(item);
                }
            }
        }

        #endregion

        private void TextBox_Directory_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    TextBox_Directory.Text = fbd.SelectedPath;
                }

            }
        }

        private void Menuitem_Options(object sender, RoutedEventArgs e)
        {
            Window_options window = new Window_options();
            window.Show();
        }
    }
}
