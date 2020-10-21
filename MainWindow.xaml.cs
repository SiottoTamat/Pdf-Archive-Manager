using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;

//using iText.Kernel.Pdf.;

namespace WPF_PDF_Organizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //ImageList imageList = new ImageList();
        public MainWindow()
        {
            
           // imageList.Images.Add()
            InitializeComponent();
            TextBox_Dir.Text = "D:\\Google Drive\\@_Work\\@_Research\\@_Sources";
            ListDirectory(Tree_View, TextBox_Dir.Text);
        }
        #region Treeview
        private static TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name };
            directoryNode.Tag = directoryInfo.FullName;
            foreach (var directory in directoryInfo.GetDirectories())
                try
                {
                    directoryNode.Items.Add(CreateDirectoryNode(directory));
                }
                catch
                {
                    MessageBox.Show("error with a directory handling in the treeview");
                }


            /* foreach (var file in directoryInfo.GetFiles())
                 try
                 {
                     directoryNode.Items.Add(new TreeViewItem { Header = file.Name });
                 }
                 catch
                 {
                     MessageBox.Show("error with a file handling in the treeview");
                 }
                 */

            return directoryNode;

        }// it creates the nodes of the left treeview
        private void Tree_View_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem tree_Item = (TreeViewItem)e.NewValue;// as TreeViewItem;
            if (tree_Item != null)
            {
                //MessageBox.Show(tree_Item.Tag.ToString());
                string folderPath = tree_Item.Tag.ToString();
                List_View.Items.Clear();
                DirectoryInfo nodeDirInfo = new DirectoryInfo(folderPath);

                foreach (FileInfo file in nodeDirInfo.GetFiles())
                {
                    Add_File_To_ListView(file);


                }
                //populate OCR column
                
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            for (int i = 0; i < List_View.Items.Count; i++)
                            {
                                FileInfo file = new FileInfo(((ListView_Item)List_View.Items[i]).Path);
                                if (file.Extension == ".pdf")
                                {
                                    if (Find_fonts(file.FullName) != null && Find_fonts(file.FullName).Count > 0)
                                    {
                                        ((ListView_Item)List_View.Items[i]).OCR = true;
                                        Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            List_View.Items.Refresh();
                                        }));
                                    }

                                }
                            }
                        }
                        catch { }


                    });
                
                

            }

        }
        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Items.Clear();
            List_View.Items.Clear();
            TreeViewItem rootItem;
            DirectoryInfo rootDirectoryInfo = new DirectoryInfo(path);
            if (rootDirectoryInfo.Exists)
            {
                rootItem = new TreeViewItem();
                //rootItem.(rootDirectoryInfo.Name);

                treeView.Items.Add(CreateDirectoryNode(rootDirectoryInfo));

            }
        }//it populates the left treeview

        #endregion

        #region ListView
        private void Add_File_To_ListView(FileInfo file)
        {

            bool ocr = false;
            
            
            ListView_Item item = new ListView_Item {
                Name = System.IO.Path.GetFileNameWithoutExtension(file.Name),
                Type = file.Extension,
                Path = file.FullName,
                OCR = ocr
            };
            List_View.Items.Add(item);
        }

        private void List_View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Info_Textblock.Text = "";
            int count = e.AddedItems.Count;
            if (count > 0)
            {
                ListView_Item item = (ListView_Item)e.AddedItems[0];
                if (item.Type == ".pdf")
                {
                    Info_Textblock.Text = Pdf_get_metadata(item.Path);
                }

            }

        }

        private void List_View_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (null != listView)
            {
                var gridView = listView.View as GridView;
                if (null != gridView)
                {
                    // ... and update its column widths
                    UpdateColumnWidths(listView, gridView);
                }
            }

        }
        public static void UpdateColumnWidths(ListView listview, GridView gridView)
        

        {
            int lastcolumn = gridView.Columns.Count - 1;
            var rightcolumn = gridView.Columns[lastcolumn];
            rightcolumn.Width = 40;
            for (int i = 0; i < lastcolumn; i++)
            {
                gridView.Columns[i].Width = (listview.ActualWidth - SystemParameters.VerticalScrollBarWidth - rightcolumn.Width - 10) / lastcolumn;
            }


        }
        private void ListView_groupbox_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        
        private void List_View_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView_Item item = ((ListView)sender).SelectedItem as ListView_Item;
            if (item == null)
            {
                return;
            }
            else
            {
                System.Diagnostics.Process.Start(item.Path);
            }

        }

        #endregion

        #region Tools Functions
        public  string Pdf_get_metadata(string path)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(path));
            Document doc = new Document(pdfDoc);
            PdfFont pdfFont;
            pdfFont = null;

            PdfDictionary infoDictionary = pdfDoc.GetTrailer().GetAsDictionary(PdfName.Info);
            string metadatastring = $"Metadata: {Environment.NewLine}" ;
            foreach (PdfName key in infoDictionary.KeySet())
            {
                string cleankey = ($"{ key }").Replace("/", "");
                //metadatastring += $"{key}: {infoDictionary.GetAsString(key)}" + Environment.NewLine;

                metadatastring += $"{cleankey}: {infoDictionary.GetAsString(key)}{Environment.NewLine}{Environment.NewLine}" ;
                // Console.WriteLine($"{key}: {infoDictionary.GetAsString(key)}");
                
            }
            metadatastring += $"-----------------{Environment.NewLine}";
            /*
                        int numberOfPdfObjects = pdfDoc.GetNumberOfPdfObjects();
                        // Search for the font dictionary
                        HashSet<string> fontList = new HashSet<string>();
                        string fonts = "";
                        string type = "";
                        for (int i = 0; i < numberOfPdfObjects; i++)
                        {
                            PdfObject pdfObject = pdfDoc.GetPdfObject(i);
                            if (pdfObject != null && pdfObject.IsDictionary() && PdfName.Font.Equals(((PdfDictionary)pdfObject).GetAsName(PdfName.Type)))
                            {
                                fontList.Add(((PdfDictionary)pdfObject).GetAsName(PdfName.BaseFont).GetValue().ToString());
                                //fonts += ((PdfDictionary)pdfObject).GetAsName(PdfName.BaseFont).GetValue().ToString() + "; ";
                            }

                        }
            */
            HashSet<string> fontinfo = Find_fonts(path);
            if (fontinfo !=null && fontinfo.Count > 0)
            {
                metadatastring += $"OCR present because pdf has {fontinfo.Count} fonts.{Environment.NewLine}-----------------{Environment.NewLine}";
            }
            else
            {
                metadatastring += $"OCR NOT present because pdf has no fonts.{Environment.NewLine}-----------------{Environment.NewLine}";
            }
                
            
            metadatastring += $"Pages: {pdfDoc.GetNumberOfPages().ToString()}" + Environment.NewLine;
            return metadatastring;
        }
        private HashSet<string> Find_fonts(string path)
        {
            HashSet<string> result = new HashSet<string>();
            try { 
            
            //result.Count = 0;
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(path));
            Document doc = new Document(pdfDoc);
            PdfFont pdfFont;
            pdfFont = null;
            int numberOfPdfObjects = pdfDoc.GetNumberOfPdfObjects();
            // Search for the font dictionary
            HashSet<string> fontList = new HashSet<string>();
            string fonts = "";
            string type = "";
            for (int i = 0; i < numberOfPdfObjects; i++)
            {
                PdfObject pdfObject = pdfDoc.GetPdfObject(i);
                if (pdfObject != null && pdfObject.IsDictionary() && PdfName.Font.Equals(((PdfDictionary)pdfObject).GetAsName(PdfName.Type)))
                {
                    result.Add(((PdfDictionary)pdfObject).GetAsName(PdfName.BaseFont).GetValue().ToString());
                    //fonts += ((PdfDictionary)pdfObject).GetAsName(PdfName.BaseFont).GetValue().ToString() + "; ";
                }

            }

            return result;
        }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
        private void Extract_Text(string address)             // the function that extracts the text from the pages of the pdf and saves all the content in a file
        {
            if (System.IO.Path.GetExtension(address) == ".pdf")
            {


                //grab the name of the file 
                string File_Name = System.IO.Path.GetFileNameWithoutExtension(address);
                string Folder_Name = System.IO.Path.GetDirectoryName(address);
                // open the pdf file
                // for every page take all text
                // add the text to the txt file
                // save the txt file in the same folder with the same name
                string txt_Name = File_Name + ".txt";
                string txt_Full_Address = System.IO.Path.Combine(Folder_Name, txt_Name);
                try
                {
                    if (File.Exists(txt_Full_Address))// if file is already there say that you did not overwrite it
                    {
                        Console.WriteLine(txt_Name + ": The file has not been overwritten");
                    }
                    else
                    {
                        using (StreamWriter fs = File.CreateText(txt_Full_Address))
                        {
                            fs.WriteLine("----> File: " + txt_Name + "<----");
                            try
                            {
                                PdfReader reader = new PdfReader(address);
                                PdfDocument pdfDoc = new PdfDocument(reader);
                                StringWriter output = new StringWriter();

                                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                                {
                                    fs.WriteLine(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));// (reader, i, new SimpleTextExtractionStrategy()));
                                    fs.WriteLine("--------- end of page " + i.ToString() + "---------");
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }

                        }
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }
        public void Get_Highlighted_Text(string filenamePath)
        {


            int pageTo;

            try
            {
                using (PdfReader reader = new PdfReader(filenamePath))
                {
                    PdfDocument pdfDoc = new PdfDocument(reader);
                    pageTo = pdfDoc.GetNumberOfPages();
                    string annot_string = "";
                    for (int i = 1; i <= pageTo; i++)
                    {


                        PdfPage page = pdfDoc.GetPage(i);
                        IList<iText.Kernel.Pdf.Annot.PdfAnnotation> annots = page.GetAnnotations();

                        if (annots.Count > 0)
                        {
                            annot_string += $"------- Page: {i.ToString()}-------{Environment.NewLine}";
                            string pageN = "Page " + i.ToString() + ".";


                            foreach (iText.Kernel.Pdf.Annot.PdfAnnotation annot in annots)

                            {

                                //Get Annotation from PDF File
                                PdfString annot_Text = annot.GetContents();
                                PdfName subType = annot.GetSubtype();
                                if (annot_Text != null)
                                {
                                    annot_string += $"{subType.GetValue()}: {annot_Text.ToUnicodeString()}{Environment.NewLine}";

                                }


                            }






                        }



                    }
                    MessageBox.Show(annot_string);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private void Make_search()
        {
            Window_Search_in_Files window = new Window_Search_in_Files();
            window.TextBox_Directory.Text = TextBox_Dir.Text;
            window.Show();
            window.TextBox_Search.Text = TextBox_SearchBar.Text;
            window.Search_In_Text(window.TextBox_Directory.Text, window.TextBox_Search.Text);
        }
        #endregion

        #region Buttons and Clicks
        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            
            
                string searchString = TextBox_SearchBar.Text;
                List<string> files = new List<string>();
                try
                {
                    foreach (string d in Directory.GetDirectories(TextBox_Dir.Text))
                    {
                        foreach (string f in Directory.GetFiles(d, "*" + searchString + "*"))
                        {
                            files.Add(f);
                        }

                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }

                List_View.Items.Clear();
                foreach (string path in files)
                {
                    FileInfo file = new FileInfo(path);
                    Add_File_To_ListView(file);
                }
            


        }
        private void Button_Extract_All_Text_Click(object sender, RoutedEventArgs e)
        {

            Task.Factory.StartNew(() =>
            {
                List<FileInfo> templist = new List<FileInfo>();
            
            for (int i = 0; i < List_View.Items.Count; i++)
            {
                ListView_Item item = (ListView_Item)List_View.Items[i];
                string address = item.Path;
                try
                {
                    if (System.IO.Path.GetExtension(address) == ".pdf")
                    {
                        string txtfile = address.Replace(".pdf", ".txt");
                        if (!System.IO.File.Exists(txtfile))
                        {
                                FileInfo file = new FileInfo(address);
                                templist.Add(file);

                            
                            

                            /*Process_Form.WorkingOn = item.;
                            Process_Form.FileProcessed = index_Foreach.ToString();
                            Process_Form.Refresh();
                            index_Foreach += 1;
                            */
                            
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Error with this file: " + address);
                }


            }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Label_Info_Total_N_Files.Content = $"{templist.Count.ToString()} Files without txt version. ";
                    List_View.Items.Clear();

                }));
                
                    
                for (int i=0; i< templist.Count; i++) 
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        List_View.Items.Add(templist[i]);
                    }));
                }

                for(int i=0; i<List_View.Items.Count; i++)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Label_InfoCommands.Content = $"Working on: {((ListView_Item)List_View.Items[i]).Name}  Processed:{i}";

                    }));
                    Extract_Text(((ListView_Item)List_View.Items[i]).Path);
                    //string fileProcessed = System.IO.Path.GetFileName(address);
                }




                MessageBox.Show($"{templist.Count.ToString()} Files successfully created.");
            });
        }
        
        private void TextBox_Dir_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    TextBox_Dir.Text = fbd.SelectedPath;
                }
                ListDirectory(Tree_View, TextBox_Dir.Text);
            }
        }
        private void Button_Extract_Comments_and_Highlights_Click(object sender, RoutedEventArgs e)
        {
            ListView_Item item = (ListView_Item)List_View.SelectedItem;
            Get_Highlighted_Text(item.Path);
        }

        private void Menu_Exit_Click(object sender, RoutedEventArgs e)
        {

        }


        private void MenuItem_Check_if_pdf_has_txt_Click(object sender, RoutedEventArgs e)
        {
            string dirpath = TextBox_Dir.Text;
            DirectoryInfo nodeDirInfo = new DirectoryInfo(dirpath);
            List_View.Items.Clear();
            foreach (FileInfo file in nodeDirInfo.GetFiles("*.pdf", SearchOption.AllDirectories))
            {
                string nametxt = file.FullName.Replace(".pdf", ".txt");
                if (!File.Exists(nametxt))
                {
                    Add_File_To_ListView(file);
                }
            }
        }

        private void MenuItem_Search_in_text_files_Click(object sender, RoutedEventArgs e)
        {
            Make_search();
        }
        private void Button_Search_In_Files_Click(object sender, RoutedEventArgs e)
        {
            Make_search();
        }
        #endregion


        #region Classes Definitions
        
        public class ListView_Item
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public string Path { get; set; }

            public bool OCR { get; set; }

            public string MessageOCR
            {
                get { if (OCR) { return "Yes"; } else { return ""; } }
                set { }
            }
            public System.Windows.Media.Color BackgroundColor
            {
                get {
                    System.Windows.Media.Color background = new System.Windows.Media.Color();
                    if (Type == ".pdf") {
                        
                        background = System.Windows.Media.Color.FromArgb(127, 233, 26, 7);
                        
                    }
                    else {
                        
                        background = System.Windows.Media.Color.FromArgb(255, 245, 245, 245);

                         
                    }
                    return background;
                }
                set { }
            }
        }

        public class FoundItem
        {
            class singlefind
            {
                public int page { get; set; }
                public FlowDocument rtfDoc { get; set; }
            }
            public FileInfo file { get; set; }

            public List<FoundItem> Items { get; set; }
        }




        #endregion

       
    }
}
