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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_PDF_Organizer
{
    /// <summary>
    /// Interaction logic for SearchItem.xaml
    /// </summary>
    public partial class SearchItem : UserControl
    {
        public FileInfo file;
        public SearchItem()
        {
            InitializeComponent();
        }

        private void Button_Namefile_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(file.FullName))
            {
                System.Diagnostics.Process.Start(file.FullName);
            }
        }

        private void Button_Namefile_Pdf_Click(object sender, RoutedEventArgs e)
        {

            string pdfFileName = (file.FullName).Replace(".txt", ".pdf");
            if (File.Exists(pdfFileName))
            {
                System.Diagnostics.Process.Start(pdfFileName);
            }
        }
    }
}
