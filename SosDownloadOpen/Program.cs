using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibSosHub;

namespace SosDownloadOpen
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0)
            {
                try
                {
                    var items = SosHubManip.LoadInstallItems(args[0]);
                    if (items.Count == 0)
                    {
                        MessageBox.Show("No download found.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        var item = items[0];
                        System.Diagnostics.Process.Start(item.DownloadUrl);
                        if (!string.IsNullOrEmpty(item.CommandLineArgs))
                        {
                            if (MessageBox.Show($"You'll also need this command-line argument: {item.CommandLineArgs}{Environment.NewLine}Click OK to copy it to clipboard.", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                            {
                                Clipboard.SetText(item.CommandLineArgs, TextDataFormat.UnicodeText);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"There was an error opening the Hub file. Are you sure it's a valid SOS Hub?{Environment.NewLine}Error: {ex.Message}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Drag a SOS Hub EXE to this program to open the download URL in your browser.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
