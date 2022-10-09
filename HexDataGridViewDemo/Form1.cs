﻿using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace WinFormsTest
{
    public partial class Form1 : Form
    {
        private readonly byte[] data = new byte[] { 0x25, 0xFF, 0x00, 0x20, 0x34 };

        public Form1()
        {
            InitializeComponent();

            bytesEditControl1.SetupGridView(data);
        }
    }
}
