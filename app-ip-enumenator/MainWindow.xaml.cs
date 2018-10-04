using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        List<string> bans_ips = new List<string>();
        List<string> bans_cidrs = new List<string>();
        List<string> goods = new List<string>();

        string pat_ip = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
        string pat_date = @"\d\d\/\D+\/\d\d\d\d";
        string pat_time = @"\b\d{2}\:\d{2}\:\d{2}\b";
        string pat_error = @"\s\d{3}\s";

        public MainWindow()
        {
            InitializeComponent();
            tb_mask_1.IsReadOnly = true;
            tb_mask_1.Background = new SolidColorBrush(Colors.LightGray);
            tb_mask_2.IsReadOnly = true;
            tb_mask_2.Background = new SolidColorBrush(Colors.LightGray);
            tb_mask_3.IsReadOnly = true;
            tb_mask_3.Background = new SolidColorBrush(Colors.LightGray);
            tb_mask_4.IsReadOnly = true;
            tb_mask_4.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();

            log_file.Filepath = ofd.FileName;
            log_file.Filename = Path.GetFileName(ofd.FileName);

            tb_file_name.Text = DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString();
        }

        private void Mask_Checked(object sender, RoutedEventArgs e)
        {
            if (cb_mask.IsChecked == true)
            {
                tb_mask_1.IsReadOnly = false;
                tb_mask_1.Background = new SolidColorBrush(Colors.White);
                tb_mask_2.IsReadOnly = false;
                tb_mask_2.Background = new SolidColorBrush(Colors.White);
                tb_mask_3.IsReadOnly = false;
                tb_mask_3.Background = new SolidColorBrush(Colors.White);
                tb_mask_4.IsReadOnly = false;
                tb_mask_4.Background = new SolidColorBrush(Colors.White);
            }           
            else
            {
                tb_mask_1.IsReadOnly = true;
                tb_mask_1.Background = new SolidColorBrush(Colors.LightGray);
                tb_mask_2.IsReadOnly = true;
                tb_mask_2.Background = new SolidColorBrush(Colors.LightGray);
                tb_mask_3.IsReadOnly = true;
                tb_mask_3.Background = new SolidColorBrush(Colors.LightGray);
                tb_mask_4.IsReadOnly = true;
                tb_mask_4.Background = new SolidColorBrush(Colors.LightGray);
            }
               
        }

        private void Btn_Begin_Click(object sender, RoutedEventArgs e)
        {

            dg_table.Items.Clear();
            clients.Clear();

            if (tb_file_name.Text == "")
                return;

            if (!File.Exists(log_file.Filepath))
                return;

            log_file.Bounder = int.Parse(tb_border.Text);

            log_file.IsMask = cb_mask.IsChecked == true ? true : false;

            FillTable();

            File.Move(log_file.Filepath, @"logs/" + tb_file_name.Text + ".log");
        }

        private void FillTable()
        {
            FillFilter(false);
            FillFilter(true);
            FillCidr();
            FillClient();
            UpdateListView();
        }

        private void FillFilter(bool _sw)
        {
            string p;

            if (_sw)
                p = Path.GetFullPath(@"core/bans_ips.txt");
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
                            bans_ips.Add(sLine);
                        else
                            goods.Add(sLine);
                    }
                }
            }
        }

        private void FillCidr()
        {
            string p = Path.GetFullPath(@"core/bans_cidrs.txt");

            if (File.Exists(p))
            {
                using (StreamReader sr = new StreamReader(p))
                {
                    string sLine = "";

                    while (sLine != null)
                    {
                        sLine = sr.ReadLine();

                        bans_cidrs.Add(sLine);
                    }
                }
            }
        }

        private void FillClient()
        {
            if (File.Exists(log_file.Filepath))
            {
                using (StreamReader sr = new StreamReader(log_file.Filepath))
                {
                    string sLine = "";
               
                    while (sLine != null)
                    {
                        sLine = sr.ReadLine();
             
                        if (sLine != null)
                        {
                            Match mtch_ip = Regex.Match(sLine, pat_ip);

                            if (mtch_ip.Success)
                                if (IsRepeated(mtch_ip.Value, sLine))
                                    continue;

                            if (mtch_ip.Success)
                            {
                                Match mtch_date = Regex.Match(sLine, pat_date);
                                Match mtch_time = Regex.Match(sLine, pat_time);
                                Match mtch_error = Regex.Match(sLine, pat_error);

                                Occurrence oc = new Occurrence(mtch_date.Value, mtch_time.Value, mtch_error.Value);

                                clients.Add(new Client(GetStatusIp(mtch_ip.Value), mtch_ip.Value, 1, mtch_error.Value, oc));
                            }
                        }
                    }
                }
            }
        }

        private string GetStatusIp(string _ip)
        {
            for (int i = 0; i < bans_ips.Count; i++)
                if (bans_ips[i] == _ip)
                    return "BAN";

            for (int i = 0; i < bans_cidrs.Count; i++)
                if (GetStatusForCidr(_ip))
                    return "BAN";

            for (int i = 0; i < goods.Count; i++)
                if (goods[i] == _ip)
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
                    clients[i].Occurrences.Add(new Occurrence(mtch_date.Value, mtch_time.Value, mtch_error.Value));
                    clients[i].Occurrence++;
                    clients[i].AddError(mtch_error.Value);
                    return true;
                }
            }

            return false;
        }

        private void UpdateListView()
        {
            for (int i=0; i < clients.Count; i++)
            {
                if (log_file.IsMask)
                {
                    Match m_ip = Regex.Match(clients[i].Ip, GetPattern());

                    if (m_ip.Success)
                        dg_table.Items.Add(clients[i]);

                    continue;
                }

                if (clients[i].Occurrence >= log_file.Bounder)
                    dg_table.Items.Add(clients[i]);
            }
        }

        private bool GetStatusForCidr(string _ip)
        {
            IPAddress incomingIp = IPAddress.Parse(_ip);

            foreach (string ip_dip in bans_cidrs)
            {
                if(ip_dip != null && ip_dip != "")
                {
                    IPNetwork network = IPNetwork.Parse(ip_dip);

                    if (IPNetwork.Contains(network, incomingIp))
                        return true;
                }
            }

            return false;
        }

        private string GetPattern()
        {
            string s = "";

            if (tb_mask_1.Text != "")
                s = s + tb_mask_1.Text + @"\.";
            else
                s = s + tb_mask_1.Text + @"\d{1,3}\.";

            if (tb_mask_2.Text != "")
                s = s + tb_mask_2.Text + @"\.";
            else
                s = s + tb_mask_2.Text + @"\d{1,3}\.";

            if (tb_mask_3.Text != "")
                s = s + tb_mask_3.Text + @"\.";
            else
                s = s + tb_mask_3.Text + @"\d{1,3}\.";

            if (tb_mask_4.Text != "")
                s = s + tb_mask_4.Text;
            else
                s = s + tb_mask_4.Text + @"\d{1,3}";

            s = s + "";

            return s;
        }

        private void dg_table_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dg_dater.Items.Clear();

            Client c = dg_table.SelectedItem as Client;

            lb_ip.Content = "IP: " + c.Ip;

            for (int i = 0; i < c.Occurrences.Count; i++)
            {
                dg_dater.Items.Add(c.Occurrences[i]);
            }

			tb_ip.Text = c.Ip;
        }

        private void tb_border_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tb_border.Text == "")
                tb_border.Text = "1";
        }

        private void WriteToFile(string _path, string _text)
        {
            using (StreamWriter sr = new StreamWriter(_path, true))
            {
                sr.WriteLine(_text);
            }
        }

        private void AddBan_Click(object sender, RoutedEventArgs e)
        {
            Client c = dg_table.SelectedItem as Client;

            WriteToFile(@"core/bans_ips.txt", c.Ip);

            MessageBox.Show(c.Ip + " added!", "TO BAN");

			tb_ip.Text = "Deny from " + c.Ip;
        }

        private void AddGood_Click(object sender, RoutedEventArgs e)
        {
            Client c = dg_table.SelectedItem as Client;

            WriteToFile(@"core/goods.txt", c.Ip);

            MessageBox.Show(c.Ip + " added!", "TO GOOD");
        }

        private void AddCidr_Click(object sender, RoutedEventArgs e)
        {
            WriteToFile(@"core/bans_cidrs.txt", tb_cidr.Text);

            MessageBox.Show(tb_cidr.Text + " added!", "TO CIDR-BAN");
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            string f = @"outputs/Output" + Path.GetFileNameWithoutExtension(log_file.Filename) + "m" + log_file.Mask + "b" + log_file.Bounder + ".log";

            using (StreamWriter sr = new StreamWriter(f))
            {
                for (int i = 0; i < dg_table.Items.Count; i++)
                {
                    Client c = dg_table.Items[i] as Client;
                    sr.WriteLine(c.Ip + " " + c.Status + " " + c.Occurrence + " " + c.Error + " ");

                    for (int j = 0; j < c.Occurrences.Count; j++)
                    {
                        sr.WriteLine("\t\t" + c.Occurrences[j].Time + " " + c.Occurrences[j].Date);
                    }
                }
            }

            MessageBox.Show("Well!", "Record to file");
        }
    }
}