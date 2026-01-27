using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Threading.Tasks;

namespace YTsearch_Winforms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            await InitializeWebView();
        }

        private async Task InitializeWebView()
        {
            // 1. Ensure the CoreWebView2 environment is ready
            // This is required before accessing any 'CoreWebView2' properties.
            await webView21.EnsureCoreWebView2Async();

            // 2. Set up the Virtual Host Mapping
            // This maps "http://lumigest.local" to your local "wwwroot" folder.

            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string webFolderPath = Path.Combine(executablePath, "wwwroot");

            // Verify folder exists to avoid silent failures
            if (!Directory.Exists(webFolderPath))
            {
                MessageBox.Show($"Could not find web folder at: {webFolderPath}");
                return;
            }

            // The mapping requires the CoreWebView2 object (not the control wrapper)
            webView21.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "lumigest.local",
                webFolderPath,
                CoreWebView2HostResourceAccessKind.Allow
            );

            // 3. Optional: UI Cleanup settings
            webView21.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView21.CoreWebView2.Settings.IsStatusBarEnabled = false;

            // 4. Navigate to your local file via the fake domain
            webView21.Source = new Uri("http://lumigest.local/index.html");
        }
    }
}
