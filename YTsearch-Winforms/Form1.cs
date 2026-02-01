using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets; 
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YTsearch_Winforms
{
    public partial class Form1 : Form
    {
        private SimpleWebServer _webServer;
        private string _localUrl; 

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // This checks if the runtime is installed. 
                // If not, it throws a WebView2RuntimeNotFoundException.
                string version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (WebView2RuntimeNotFoundException)
            {
                string message = "Microsoft Edge WebView2 Runtime is missing.\n\n" +
                                 "This application requires WebView2 to function.\n" +
                                 "Would you like to download it now?";

                var result = MessageBox.Show(message, "Missing Component", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    // Open the official download page
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://go.microsoft.com/fwlink/p/?LinkId=2124703", // Direct link to Evergreen Bootstrapper
                        UseShellExecute = true
                    });
                }

                // Close the app gracefully as we cannot proceed without the runtime
                Close();
                return;
            }
            catch (Exception ex)
            {
                // Catch other weird initialization errors (like DLL missing)
                MessageBox.Show($"Error checking WebView2 status: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            string webRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

            if (!Directory.Exists(webRootPath))
            {
                MessageBox.Show("wwwroot folder not found at:\n" + webRootPath, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            try
            {
                int port = GetFreeTcpPort();
                _localUrl = $"http://localhost:{port}/";

                _webServer = new SimpleWebServer(webRootPath, _localUrl);
                _webServer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start internal server: " + ex.Message);
                return;
            }

            await webView21.EnsureCoreWebView2Async();

            webView21.Source = new Uri(_localUrl + "index.html");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _webServer?.Stop();
        }

        private static int GetFreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }



    public class SimpleWebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly string _rootPath;
        private bool _isRunning = false;

        public SimpleWebServer(string rootPath, string prefix)
        {
            _rootPath = rootPath;
            if (!prefix.EndsWith("/")) prefix += "/";
            _listener.Prefixes.Add(prefix);
        }

        public void Start()
        {
            if (_isRunning) return;
            try
            {
                _listener.Start();
                _isRunning = true;
                Task.Run(() => ListenLoop());
            }
            catch (HttpListenerException ex)
            {
                throw new Exception($"Could not bind to port. Details: {ex.Message}");
            }
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                if (_listener.IsListening)
                {
                    _listener.Stop();
                }
                _listener.Close();
            }
            catch (ObjectDisposedException) {  }
            catch (Exception) { }
        }

        private async void ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (!_listener.IsListening) break;

                    var context = await _listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (HttpListenerException) { break; } 
                catch (ObjectDisposedException) { break; } 
                catch (InvalidOperationException) { break; }
            }
        }

        private YouTubeSearchService _searchService = new YouTubeSearchService();

        private async void ProcessRequest(HttpListenerContext context)
        {
            string urlPath = context.Request.Url.AbsolutePath;

            // --- API ROUTE: SEARCH ---
            if (urlPath == "/api/search")
            {
                // USE THE NEW HELPER to avoid System.Web errors
                var queryParams = UrlHelpers.ParseQueryString(context.Request.Url.Query);

                string q = queryParams.ContainsKey("q") ? queryParams["q"] : "";
                string key = queryParams.ContainsKey("key") ? queryParams["key"] : "";
                string pageToken = queryParams.ContainsKey("pageToken") ? queryParams["pageToken"] : "";
                string publishedAfter = queryParams.ContainsKey("publishedAfter") ? queryParams["publishedAfter"] : "";
                string publishedBefore = queryParams.ContainsKey("publishedBefore") ? queryParams["publishedBefore"] : "";

                // If the JS sent a key, save it securely for next time
                if (!string.IsNullOrEmpty(key) && key != "undefined")
                {
                    SecureSettings.SaveApiKey(key);
                }

                string jsonResponse = await _searchService.SearchAsync(q, key, pageToken, publishedAfter, publishedBefore);

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
                return;
            }

            // --- STATIC FILES (Standard File Serving) ---
            try
            {
                string filename = urlPath.Substring(1);
                if (string.IsNullOrEmpty(filename)) filename = "index.html";

                // Security Check: prevent directory traversal
                if (filename.Contains(".."))
                {
                    context.Response.StatusCode = 403;
                    context.Response.Close();
                    return;
                }

                string filePath = Path.Combine(_rootPath, filename.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(filePath))
                {
                    byte[] input = File.ReadAllBytes(filePath);
                    context.Response.ContentType = GetMimeType(Path.GetExtension(filePath));
                    context.Response.ContentLength64 = input.Length;
                    context.Response.OutputStream.Write(input, 0, input.Length);
                    context.Response.OutputStream.Close();
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.OutputStream.Close();
                }
            }
            catch
            {
                try { context.Response.Close(); } catch { }
            }
        }

        private string GetMimeType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".html": return "text/html";
                case ".css": return "text/css";
                case ".js": return "application/javascript";
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".ico": return "image/x-icon";
                case ".svg": return "image/svg+xml";
                case ".woff": return "font/woff";
                case ".woff2": return "font/woff2";
                case ".ttf": return "font/ttf";
                default: return "application/octet-stream";
            }
        }
    }
}