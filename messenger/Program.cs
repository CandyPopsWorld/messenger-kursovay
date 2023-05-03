using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace messenger
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (ConfigurationManager.AppSettings["IsRegistered"] == "true")
            {
                Application.Run(new Form2());
            }
            else
            {
                Application.Run(new Form1());
            }
            //Application.Run(new Form1());
        }
    }
}
