using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Text;
using System.Windows.Forms;

namespace YTsearch_Winforms
{
    public partial class Webview2ErrorMessage : Form
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();
        Font myFont;

        public Webview2ErrorMessage()
        {
            InitializeComponent();

            byte[] fontData = Properties.Resources.OpenSansRegular;
            IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Properties.Resources.OpenSansRegular.Length);
            AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.OpenSansRegular.Length, IntPtr.Zero, ref dummy);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

            titleLabel.Font = new Font(fonts.Families[0], 20.25F);
            descriptionLabel.Font = new Font(fonts.Families[0], 12F);
            linkLabel1.Font = new Font(fonts.Families[0], 12F);
            label1.Font = new Font(fonts.Families[0], 10F);
        }

        private void Webview2ErrorMessage_Load(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://go.microsoft.com/fwlink/p/?LinkId=2124703");
        }
    }
}
