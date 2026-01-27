using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;

namespace YTsearch_Winforms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await InitializeWebView();
        }

        private async Task InitializeWebView()
        {
            await webView21.EnsureCoreWebView2Async();
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string indexPath = Path.Combine(executablePath, "wwwroot", "index.html");

            webView21.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView21.CoreWebView2.Settings.IsStatusBarEnabled = false;

            if (Directory.Exists(indexPath))
            {
                MessageBox.Show($"Could not find web folder at: {indexPath}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            webView21.Source = new Uri(indexPath);
        }
    }
}
