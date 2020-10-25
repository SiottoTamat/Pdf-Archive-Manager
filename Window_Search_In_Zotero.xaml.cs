using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using static WPF_PDF_Organizer.MainWindow;

namespace WPF_PDF_Organizer
{
    /// <summary>
    /// Interaction logic for Window_Search_In_Zotero.xaml
    /// </summary>
    public partial class Window_Search_In_Zotero : Window
    {
        public Window_Search_In_Zotero()
        {
            InitializeComponent();
            Label_Zotero_Database_Path.Content = MainWindow.Zotero_Database_Path;
        }

        

        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            string[] queryArr = new string[] {TextBox_SearchBar_Author.Text, TextBox_SearchBar_Title.Text, TextBox_SearchBar_Type.Text, TextBox_SearchBar_Date.Text };
            //string query = TextBox_SearchBar.Text;
            List<MainWindow.Zotero_Query_Result> List_result = List_Zotero_Search(queryArr);
            DataGrid_Zotero_Results.ItemsSource = List_result;

            // the code below set up the right style for the datagrid
            DataGrid_Zotero_Results.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            DataGrid_Zotero_Results.Columns[1].Width = new DataGridLength(5, DataGridLengthUnitType.Star);
            DataGrid_Zotero_Results.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            DataGrid_Zotero_Results.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
            DataGridTextColumn column = DataGrid_Zotero_Results.Columns[1] as DataGridTextColumn;
            column.ElementStyle = style;
            DataGridTextColumn columnDate = DataGrid_Zotero_Results.Columns[3] as DataGridTextColumn;
            
            columnDate.Binding.StringFormat = "dd/MM/yyyy";
        }
        public  List<MainWindow.Zotero_Query_Result> List_Zotero_Search(string[] queryArr)
        {
            List<MainWindow.ZoteroField[]> result =  MainWindow.QueryZotero(queryArr);

            List<MainWindow.Zotero_Query_Result> List_result = new List<MainWindow.Zotero_Query_Result>();
            if (result != null)
            {
                
                foreach (MainWindow.ZoteroField[] line in result)
                {
                    MainWindow.Zotero_Query_Result ZQresult = new Zotero_Query_Result();
                    foreach (MainWindow.ZoteroField field in line)
                    {
                        
                        if (field != null)
                        {



                            switch (field.ColumnTitle)
                            {
                                case "TITLE":
                                    if (field.Value != null) { ZQresult.Title = field.Value; }
                                    else { ZQresult.Title = ""; }
                                    break;
                                case "AUTHOR_1_LAST":
                                    if (field.Value != null) { ZQresult.Author = field.Value; }
                                    else { ZQresult.Author = ""; }
                                    break;
                                case "TYPE":
                                    if (field.Value != null) { ZQresult.Type = field.Value; }
                                    else { ZQresult.Type = ""; }
                                    break;
                                case "DATE":
                                    if (field.Value != null) { ZQresult.Year = cleanDate(field.Value); }
                                    else { ZQresult.Year = ""; }
                                    break;
                            }

                        }
                    }
                    List_result.Add(ZQresult);
                }
                
            }


            return List_result;
        }

        private string cleanDate(string dateraw)
        {
            string dateclean = dateraw;
            if (dateraw != "")
            {
                dateclean = dateraw.Substring(0, 4);
            }
            return dateclean;
        }

        private void Menuitem_ZoteroFindDb(object sender, RoutedEventArgs e)
        {
            OpenFileDialog window = new OpenFileDialog();
            window.Filter = "SQLite Database File (*.sqlite) |*.sqlite";
            if (window.ShowDialog() == true)
            {
                Zotero_Database_Path = window.FileName;
                Label_Zotero_Database_Path.Content = Zotero_Database_Path;
                
            }
        }

        private void Button_Use_Selected_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Zotero_Query_Result selecteditemDG = (MainWindow.Zotero_Query_Result)DataGrid_Zotero_Results.SelectedItem;
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            window.Show_Zotero_Data(selecteditemDG.Title, selecteditemDG.Author);
        }
    }
}
