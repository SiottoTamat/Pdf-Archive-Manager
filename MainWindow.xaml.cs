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
using System.Data.SQLite;
using Org.BouncyCastle.Crypto.Signers;
using Microsoft.Win32;

//using iText.Kernel.Pdf.;

namespace WPF_PDF_Organizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string Zotero_Database_Path = @"C:\Users\Andrea\Desktop\TExt_for_Pdf\zotero.sqlite"; //@"Data Source=C:\\Users\\Andrea\\Desktop\\TExt_for_Pdf\\zotero.sqlite";
        //ImageList imageList = new ImageList();
        public MainWindow()
        {
            
           // imageList.Images.Add()
            InitializeComponent();
            TextBox_Dir.Text = @"D:\Google Drive\@_Work\@_Research";
            if (TextBox_Dir.Text != "")
            {
                ListDirectory(Tree_View, TextBox_Dir.Text);
            }
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
            var modcolumn = gridView.Columns[lastcolumn];
            modcolumn.Width = 40; //OCR column
            modcolumn = gridView.Columns[lastcolumn - 1];
            modcolumn.Width = 40; // Filetype column
            modcolumn = gridView.Columns[0];
            modcolumn.Width = listview.ActualWidth - 86;

            /*for (int i = 0; i < lastcolumn-1; i++)
            {
                gridView.Columns[i].Width = (listview.ActualWidth - SystemParameters.VerticalScrollBarWidth - modcolumn.Width - 10) / lastcolumn;
            }*/


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

        public static List<ZoteroField[]> QueryZotero(string[] query)
        {
            List<ZoteroField[]> endlist = new List<ZoteroField[]>();
            string path = Zotero_Database_Path;
            string cs = $"Data Source ={Zotero_Database_Path}";
            using var con = new SQLiteConnection(cs,true) ;
            
            con.Open();

            using var cmd = new SQLiteCommand(con);
            string command = SQL_Queries.Get_All_Data_for_Item 
                + SQL_Queries.Filter_Author(query[0]) 
                + SQL_Queries.Filter_Title(query[1]) 
                + SQL_Queries.Filter_Type(query[2]) 
                + SQL_Queries.Filter_Date(query[3]);
                


            cmd.CommandText = command;
            using SQLiteDataReader rdr = cmd.ExecuteReader();
            string allcolumns = ""; 
            int row = 0;




            //    rdr.GetOrdinal("TITLE")))
            //        string a = rdr.GetString(rdr.GetOrdinal("TITLE"));

            
            
            //if (rdr.Read())
            while(rdr.Read())
            {
                ZoteroField[] zfields = new ZoteroField[rdr.FieldCount];
                for (int i = 0; i < rdr.FieldCount; i++)
                {

                    {

                        if (!rdr.IsDBNull(i))
                        {
                            string title = rdr.GetName(i);
                            string value = rdr.GetValue(i).ToString();
                            ZoteroField z = new ZoteroField();
                            z.ColumnTitle = title;
                            z.Value = value;
                            zfields[i] = z;
                            //allcolumns += $"{title}: {value}{Environment.NewLine}";

                        }
                    }
                }
                endlist.Add(zfields);
                //return zfields;
            }
                



            return endlist;
        }
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
                string keytext = infoDictionary.GetAsString(key).ToUnicodeString();// key.GetValue();//infoDictionary.GetAsString(key);
                if (key == PdfName.CreationDate|key==PdfName.ModDate)
                {
                    DateTime date = PdfDate.Decode(keytext);//Convert.ToDateTime(keytext);
                    keytext = date.ToString("dd/MM/yyyy HH:mm:ss");
                }
                
                string cleankey = ($"{ key }").Replace("/", "");
                //metadatastring += $"{key}: {infoDictionary.GetAsString(key)}" + Environment.NewLine;

                metadatastring += $"{cleankey}: {keytext}{Environment.NewLine}{Environment.NewLine}" ;
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
        public FlowDocument Get_Highlighted_Text(string filenamePath)
        {
            
            
            //paragraph.Inlines.Add("");
            FlowDocument doc = new FlowDocument();
            //doc.Blocks.Add(paragraph);
            int pageTo;
            bool hasAnnot=false;
            try
            {
                using (PdfReader reader = new PdfReader(filenamePath))
                {
                    PdfDocument pdfDoc = new PdfDocument(reader);
                    pageTo = pdfDoc.GetNumberOfPages();
                    //string annot_string = "";
                    for (int i = 1; i <= pageTo; i++)
                    {
                        

                        PdfPage page = pdfDoc.GetPage(i);
                        IList<iText.Kernel.Pdf.Annot.PdfAnnotation> annots = page.GetAnnotations();

                        if (annots.Count > 0)
                        {
                            hasAnnot = true;
                            Paragraph paragraph = new Paragraph();
                            //annot_string += $"------- Page: {i.ToString()}-------{Environment.NewLine}";
                            //string pageN = "Page " + i.ToString() + ".";
                            paragraph.Inlines.Clear();
                            paragraph.Inlines.Add("------- Page:");
                            paragraph.Inlines.Add($"{i.ToString()}");
                            paragraph.Inlines.Add($"-------{Environment.NewLine}");
                            doc.Blocks.Add(paragraph);

                            foreach (iText.Kernel.Pdf.Annot.PdfAnnotation annot in annots)

                            {
                                Paragraph paragraph_annot = new Paragraph();
                                //Get Annotation from PDF File
                                PdfString annot_Text = annot.GetContents();
                                PdfName subType = annot.GetSubtype();
                                if (annot_Text != null)
                                {
                                    paragraph_annot.Inlines.Add(new Run($"{subType.GetValue()}"){ FontWeight = FontWeights.Bold});
                                    paragraph_annot.Inlines.Add($":{Environment.NewLine}");
                                    paragraph_annot.Inlines.Add($"{annot_Text.ToUnicodeString()}{Environment.NewLine}");
                                    //annot_string += $"{subType.GetValue()}: {annot_Text.ToUnicodeString()}{Environment.NewLine}";
                                    doc.Blocks.Add(paragraph_annot);
                                }


                            }
                            
                            


                        }



                    }
                    if (hasAnnot)
                    {
                        FileInfo file = new FileInfo(filenamePath);
                        Paragraph paragraph_title = new Paragraph();
                        paragraph_title.Inlines.Add(new Run(file.Name) { FontWeight = FontWeights.Bold });
                        doc.Blocks.InsertBefore(doc.Blocks.FirstBlock, paragraph_title);
                    }
                    else
                    {
                        FileInfo file = new FileInfo(filenamePath);
                        Paragraph paragraph_title = new Paragraph();
                        paragraph_title.Inlines.Add($"There are no highlights or comments in: {Environment.NewLine} {file.Name}");
                        doc.Blocks.Add(paragraph_title);
                    }
                    //MessageBox.Show(annot_string);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return doc;
        }
        private void Make_search()
        {
            Window_Search_in_Files window = new Window_Search_in_Files();
            window.TextBox_Directory.Text = TextBox_Dir.Text;
            window.Show();
            window.TextBox_Search.Text = TextBox_SearchBar.Text;
            window.Search_In_Text(window.TextBox_Directory.Text, window.TextBox_Search.Text);
        }

        private void Make_Zotero_search()
        {
            Window_Search_In_Zotero window = new Window_Search_In_Zotero();
            window.Show();
            
        }

        public void Show_Zotero_Data(string title, string author)
        {
            string[] queryArr = new string[] { author, title, "", "" };
            List<ZoteroField[]> answer = QueryZotero(queryArr);
            if (answer != null) 
            { 
            ZoteroField[] result = answer[0];
            
            
                string beautify_result = "";
                
                    foreach (ZoteroField i in result)
                    {
                        if (i != null)
                        {
                            beautify_result += $"{i.ColumnTitle}: {i.Value}{Environment.NewLine}";
                        }
                    }


            Zot_Info_Textblock.Text = beautify_result;
            }
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
        private void MenuItem_Extract_All_Text_Click(object sender, RoutedEventArgs e)
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
                //Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    Label_Info_Bottom_Right.Content = $"Working on: {((ListView_Item)List_View.Items[i]).Name}  Processed:{i}"; 
                //    //$"{templist.Count.ToString()} Files without txt version. Total number of Files: ";
                //    List_View.Items.Clear();

                //}));
                //Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    List_View.Items.Clear();
                //}));
                int index = 0;
                foreach (FileInfo file in templist)//(int i=0; i< templist.Count; i++) 
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //List_View.Items.Add(templist[i]);
                        Label_Info_Bottom_Right.Content = $"Working on: {file.Name}  Processed:{index}";
                        //Label_Info_Bottom_Right.Width = System.Windows.Forms.TextRenderer.MeasureText(Label_Info_Bottom_Right.Content, Label_Info_Bottom_Right.Font).Width;
                        //Label_Info_Bottom_Right.aut
                    }));
                    Extract_Text(file.FullName);
                    index++;
                    //Dispatcher.BeginInvoke(new Action(() =>
                    //{


                    //}));

                }

                //for (int i = 0; i < List_View.Items.Count; i++)
                //{
                //    Dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        Label_Info_Bottom_Right.Content = $"Working on: {((ListView_Item)List_View.Items[i]).Name}  Processed:{i}";

                //    }));
                //    Extract_Text(((ListView_Item)List_View.Items[i]).Path);
                //    //string fileProcessed = System.IO.Path.GetFileName(address);
                //}




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

        private void MenuItem_Extract_Comments_and_Highlights_Click(object sender, RoutedEventArgs e)
        {
            ListView_Item item = (ListView_Item)List_View.SelectedItem;
            FlowDocument doc = Get_Highlighted_Text(item.Path);
            Window_Extract_text window = new Window_Extract_text();
            window.RichTextBox_Extract.Document = doc;
            window.Show();
        }
        private void Button_Search_In_Files_Click(object sender, RoutedEventArgs e)
        {
            Make_search();
        }

        private void Menuitem_ZoteroQuery(object sender, RoutedEventArgs e)
        {
            Make_Zotero_search();          
            
        }
        private void Button_Search_In_Zotero_Click(object sender, RoutedEventArgs e)
        {
            Make_Zotero_search();
        }
        private void Menuitem_ZoteroFindDb(object sender, RoutedEventArgs e)
        {
            OpenFileDialog window = new OpenFileDialog();
            window.Filter = "SQLite Database File (*.sqlite) |*.sqlite";
            if (window.ShowDialog() == true)
            {
                Zotero_Database_Path = window.FileName;
                Label_Zotero_Database.Content = Zotero_Database_Path;
            }
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

        public class ZoteroField
        {
            public string ColumnTitle { get; set;}
            public string Value { get; set; }
        }

        public class Zotero_Query_Result
        {
            public string Author { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
            public string Year { get; set; }

        }








        #endregion

        
    }
}
