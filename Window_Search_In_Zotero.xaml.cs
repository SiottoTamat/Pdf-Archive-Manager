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
        }

        

        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            string query = TextBox_SearchBar.Text;
            List<MainWindow.Zotero_Query_Result> List_result = List_Zotero_Search();
            DataGrid_Zotero_Results.ItemsSource = List_result;
            
        }
        public List<MainWindow.Zotero_Query_Result> List_Zotero_Search()
        {
            string query = TextBox_SearchBar.Text;
            List<MainWindow.ZoteroField[]> result =  MainWindow.QueryZotero(query);

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
                                    if (field.Value != null) { ZQresult.Year = field.Value; }
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

        
    }
}
