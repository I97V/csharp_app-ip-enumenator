using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace app_ip_enumenator
{
    public partial class MainWindow : Window
    {
        LogFile log_file = new LogFile();

        List<Client> clients = new List<Client>();

        List<string> bans = new List<string>();
        List<string> goods = new List<string>();

        string pat_ip = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
        string pat_date = @"\d\d\/\D+\/\d\d\d\d";
        string pat_time = @"\b\d{2}\:\d{2}\:\d{2}\b";
        string pat_error = @"\s\d{3}\s";

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
                tb_mask.Text = "x.x.x.x";
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
            log_file.IsInverse = (cb_inverse.IsChecked == true) ? true : false;

            //log_file.Bounder = int.Parse(tb_bounder.Text);

            if (cb_mask.IsChecked == true)
            {
                log_file.IsMask = true;
                log_file.Mask = tb_mask.Text;
            }
            else
            {
                log_file.IsMask = false;
            }

            FillTable();
        }

        private void FillTable()
        {
            FillFilter(false);
            FillFilter(true);
            FillClient();
            UpdateListView();
        }

        private void FillFilter(bool _sw)
        {
            string p;

            if (_sw)
                p = Path.GetFullPath(@"core/bans.txt");
            else
                p = Path.GetFullPath(@"core/goods.txt");

            if (File.Exists(p))
            {
                using (StreamReader sr = new StreamReader(p))
                {
                    string sLine = "";

                    while (sLine != null)
                    {
                        sLine = sr.ReadLine();

                        if (_sw)
                            bans.Add(sLine);
                        else
                            goods.Add(sLine);
                    }
                }
            }
        }

        private void FillClient()
        {
            if (File.Exists(log_file.Filepath))
            {
                Match mtch_ip;

                using (StreamReader sr = new StreamReader(log_file.Filepath))
                {
                    string sLine = "";
               
                    while (sLine != null)
                    {
                        sLine = sr.ReadLine();
             
                        if (sLine != null)
                        {
                            mtch_ip = Regex.Match(sLine, pat_ip);

                            if (mtch_ip.Success)
                                if (IsRepeated(mtch_ip.Value, sLine))
                                    continue;

                            if (mtch_ip.Success)
                            {
                                Match mtch_date = Regex.Match(sLine, pat_date);
                                Match mtch_time = Regex.Match(sLine, pat_time);
                                Match mtch_error = Regex.Match(sLine, pat_error);

                                Occurrence oc = new Occurrence(mtch_date.Value, mtch_time.Value);

                                clients.Add(new Client(GetStatusIp(mtch_ip.Value), mtch_ip.Value, 1, int.Parse(mtch_error.Value), oc));
                            }
                        }
                    }
                }
            }

            tb_test.Text = clients.Count.ToString();
        }

        private string GetStatusIp(string _ip)
        {
            for (int i = 0; i < bans.Count; i++)
                if (bans[i] == _ip)
                    return "BAN";

            for (int i = 0; i < goods.Count; i++)
                if (bans[i] == _ip)
                    return "GOOD";

            return "";
        }

        private bool IsRepeated(string _ip, string _line)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if(clients[i].Ip == _ip)
                {
                    Match mtch_date = Regex.Match(_line, pat_date);
                    Match mtch_time = Regex.Match(_line, pat_time);
                    Match mtch_error = Regex.Match(_line, pat_error);
                    clients[i].Occurrences.Add(new Occurrence(mtch_date.Value, mtch_time.Value));
                    clients[i].Occurrence++;
                    clients[i].AddError(int.Parse(mtch_error.Value));
                    return true;
                }
            }

            return false;
        }

        private void UpdateListView()
        {
            for (int i=0; i < clients.Count; i++)
            {
                dg_table.Items.Add(clients[i]);
            }
        }
    }
}
