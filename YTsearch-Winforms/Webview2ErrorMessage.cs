using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            titleText.Font = new Font(fonts.Families[0], 16.0F);
        }

        private void Webview2ErrorMessage_Load(object sender, EventArgs e)
        {

        }
    }
}
