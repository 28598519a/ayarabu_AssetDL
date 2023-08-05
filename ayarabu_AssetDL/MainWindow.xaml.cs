using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace ayarabu_AssetDL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 同時下載的線程池上限
        /// </summary>
        int pool = 50;

        private async void btn_download_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.InitialDirectory = App.Root;
            openFileDialog.Filter = "manifest.csv|*.csv";
            if (!openFileDialog.ShowDialog() == true)
                return;

            StreamReader reader = new StreamReader(File.OpenRead(openFileDialog.FileName));
            List<Tuple<string,string>> AssetList = new List<Tuple<string, string>>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line != null)
                {
                    // 取得csv中的第一個columns的資料
                    string[] values = line.Split(',');
                    string name = values[0];
                    string url = values[1];
                    
                    AssetList.Add(new Tuple<string, string>(url, name));
                }
            }

            App.TotalCount = AssetList.Count;

            if (App.TotalCount > 0)
            {
                App.Respath = Path.Combine(App.Root, "Asset");
                if (!Directory.Exists(App.Respath))
                    Directory.CreateDirectory(App.Respath);

                int count = 0;
                List<Task> tasks = new List<Task>();
                foreach (Tuple<string, string> asset in AssetList)
                {
                    string url = asset.Item1;
                    string name = asset.Item2;

                    string path = Path.Combine(App.Respath, name);

                    tasks.Add(DownLoadFile(url, path, cb_isCover.IsChecked == true ? true : false));
                    count++;

                    // 阻塞線程，等待現有工作完成再給新工作
                    if ((count % pool).Equals(0) || App.TotalCount == count)
                    {
                        // await is better than Task.Wait()
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                    }

                    // 用await將線程讓給UI更新
                    lb_counter.Content = $"進度 : {count} / {App.TotalCount}";
                    await Task.Delay(1);
                }

                if (cb_Debug.IsChecked == true)
                {
                    using (StreamWriter outputFile = new StreamWriter("404.log", false))
                    {
                        foreach (string s in App.log)
                            outputFile.WriteLine(s);
                    }
                }

                string failmsg = String.Empty;
                if (App.TotalCount - App.glocount > 0)
                    failmsg = $"，{App.TotalCount - App.glocount}個檔案失敗";

                System.Windows.MessageBox.Show($"下載完成，共{App.glocount}個檔案{failmsg}", "Finish");
                lb_counter.Content = String.Empty;
            }
        }

        /// <summary>
        /// 從指定的網址下載檔案
        /// </summary>
        public async Task<Task> DownLoadFile(string downPath, string savePath, bool overWrite)
        {
            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            if (File.Exists(savePath) && overWrite == false)
                return Task.FromResult(0);

            App.glocount++;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    // Don't use DownloadFileTaskAsync, if 404 it will create a empty file, use DownloadDataTaskAsync instead.
                    byte[] data = await wc.DownloadDataTaskAsync(downPath);

                    // Data from DmmR18Web need to do gzip decompress
                    if (downPath.Contains("DmmR18Web"))
                        data = Decompress(data);

                    File.WriteAllBytes(savePath, data);
                }
                catch (Exception ex)
                {
                    App.glocount--;

                    if (cb_Debug.IsChecked == true)
                        App.log.Add(downPath + Environment.NewLine + savePath + Environment.NewLine);

                    // 沒有的資源直接跳過，避免報錯。
                    //System.Windows.MessageBox.Show(ex.Message.ToString() + Environment.NewLine + downPath + Environment.NewLine + savePath);
                }
            }
            return Task.FromResult(0);
        }

        public byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}
