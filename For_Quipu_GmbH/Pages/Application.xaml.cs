using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace For_Quipu_GmbH.Pages
{
    /// <summary>
    /// Interaction logic for Application.xaml
    /// </summary>
    public partial class Application : Page
    {
        public Application()
        {
            InitializeComponent();
            btn_Cancell.IsEnabled = false;
            btn_Run.IsEnabled = false;
        }

        private BackgroundWorker worker = new BackgroundWorker();
        private int countall_a = 0;
        private int max_a = 0;
        private string max_url = "";
        private bool countMessageOpenFile = false;
        private bool canceled = false;
        private bool countResult = false;
        List<string> urls = new List<string>();
        List<int> aForSites = new List<int>();

        private void Read_File(string load_file)
        {
            string s = "";

            using (StreamReader file = new StreamReader(load_file))
            {
                while (s != null)
                {
                    s = file.ReadLine();

                    if (s != null && s.Length != 0 && s.StartsWith("https://")) urls.Add(s);
                }
            }
        }

        private void btn_Open_File_Click(object sender, RoutedEventArgs e)
        {
            urls.Clear();
            aForSites.Clear();
            countall_a = 0;
            progressBar.Value = 0;
            countResult = false;
            result.Text = "";
            lbMaxA.Text = "";
            if (!countMessageOpenFile)
            {
                MessageBox.Show("Open a text file in this format:\nURL\nURL\n...");
                countMessageOpenFile = true;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Documents (*.txt)|*.txt";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                Read_File(openFileDialog.FileName);
                fileName.Content = "File " + Path.GetFileName(openFileDialog.FileName) + " was loaded";
                progressBar.Maximum = urls.Count();
                btn_Run.IsEnabled = true;
            }
            else MessageBox.Show("Something went wrong");
        }

        private void btn_Cancell_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Value = 100;
            canceled = true;
        }

        private void btn_Run_Click(object sender, RoutedEventArgs e)
        {
            canceled = false;
            btn_Run.IsEnabled = false;
            countall_a = 0;
            progressBar.Value = 0;
            result.Text = "";
            btn_Cancell.IsEnabled = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            countall_a = 0;
            max_a = 0;
            max_url = "";
            int count_a = 0;

            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            foreach (var url in urls)
            {
                try
                {
                    HtmlAgilityPack.HtmlDocument doc = web.Load(url);

                    foreach (var price in doc.DocumentNode.SelectNodes("//a"))
                    {
                        count_a++;
                        if (canceled)
                        {
                            break;
                        }
                    }
                    if (count_a > max_a)
                    {
                        max_a = count_a;
                        max_url = url;
                    }
                    aForSites.Add(count_a);
                    if (canceled)
                    {
                        break;
                    }
                    int progress = 1;
                    ((BackgroundWorker)sender).ReportProgress(progress);
                    countall_a += count_a;
                }
                catch (InvalidDataException ex)
                {
                    MessageBox.Show("Error");
                }
            }
            e.Result = true;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value += e.ProgressPercentage;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!canceled)
            {
                lbMaxA.Text = "Max count a" + "\n" +
                    "Url: " + max_url + "\n" +
                    "Attributes <a>: " + max_a;
                for (int i = 0; i < urls.Count(); i++)
                {
                    if (urls[i].Length != 0)
                        result.Text += "URL: " + urls[i] + "\n" +
                            "Number of attributes <a>: " + aForSites[i] + "\n\n";
                }
                if (!countResult)
                {
                    result.Text += "Total attributes <a> = " + countall_a;
                    countResult = true;
                }
                urls.Clear();
                aForSites.Clear();
                btn_Run.IsEnabled = false;
                btn_Cancell.IsEnabled = false;
            }
            else
            {
                if (!countResult)
                {
                    MessageBox.Show("Operation was cancelled \nPlease open file again");
                    countResult = true;
                }
                btn_Run.IsEnabled = false;
                btn_Cancell.IsEnabled = false;
                urls.Clear();
                aForSites.Clear();
            }
        }
    }
}
