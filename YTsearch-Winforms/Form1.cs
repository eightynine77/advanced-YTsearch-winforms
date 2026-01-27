using System;
using System.IO;
using System.Net;
using System.Net.Sockets; // Added for finding free port
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace YTsearch_Winforms
{
    public partial class Form1 : Form
    {
        private SimpleWebServer _webServer;
        private string _localUrl; // Not const anymore, determined at runtime

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // 1. Determine where your wwwroot folder is
            string webRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

            if (Directory.Exists(webRootPath))
            {
                MessageBox.Show("wwwroot folder not found at: " + webRootPath);
                Close();
                return;
            }

            // 2. Start the internal localhost server on a DYNAMIC free port
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

            // 3. Initialize WebView2
            await webView21.EnsureCoreWebView2Async();

            // 4. Point WebView2 to your local server
            webView21.Source = new Uri(_localUrl + "index.html");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cleanup: Stop the server when app closes
            _webServer?.Stop();
        }

        // Helper to find a random open port so we don't crash nicely
        private static int GetFreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }

    // --- INTERNAL WEB SERVER CLASS ---
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
                // If this fails, it bubbles up to Form1_Load to show the error
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
            catch (ObjectDisposedException) { /* Ignored: Server already dead */ }
            catch (Exception) { /* Ignored: Cleanup errors shouldn't crash app */ }
        }

        private async void ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // Check if we are still listening before getting context
                    if (!_listener.IsListening) break;

                    var context = await _listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (HttpListenerException) { break; } // Listener stopped
                catch (ObjectDisposedException) { break; } // Object gone
                catch (InvalidOperationException) { break; }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string filename = context.Request.Url.AbsolutePath.Substring(1); // Remove leading slash
                if (string.IsNullOrEmpty(filename)) filename = "index.html";

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
                // If writing to the stream fails (e.g. browser closed connection), just ignore
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