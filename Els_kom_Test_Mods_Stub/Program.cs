﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Els_kom_Test_Mods_Stub
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Els_kom_Core.Forms.Test_Mods_Stub.MainForm());
        }
    }
}