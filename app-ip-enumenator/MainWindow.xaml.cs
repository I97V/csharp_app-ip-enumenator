using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace app_ip_enumenator
{
    public partial class MainWindow : Window
    {
        LogFile log_file = new LogFile();

        public MainWindow()
        {
            InitializeComponent();
            tb_mask.IsReadOnly = true;
            tb_mask.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();

            log_file.Filepath = ofd.FileName;
            log_file.Filename = Path.GetFileName(ofd.FileName);

            tb_file_name.Text = log_file.Filename;
        }

        private void Mask_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_mask.IsChecked == true)
            {
                tb_mask.IsReadOnly = false;
                tb_mask.Background = new SolidColorBrush(Colors.White);
            }           
            else
            {
                tb_mask.IsReadOnly = true;
                tb_mask.Background = new SolidColorBrush(Colors.LightGray);
                tb_mask.Text = "";
            }
               
        }

        private void Btn_Begin_Click(object sender, RoutedEventArgs e)
        {
            if (tb_file_name.Text == "")
                return;

            if (!File.Exists(log_file.Filepath))
                return;

            log_file.IsRecord = (cb_record.IsChecked == true) ? true : false;

            log_file.Bounder = int.Parse(tb_bounder.Text);
        }
    }
}
