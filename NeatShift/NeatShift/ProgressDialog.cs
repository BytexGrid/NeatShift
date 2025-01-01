// FreeMove -- Move directories without breaking shortcuts or installations 
//    Copyright(C) 2020  Luca De Martini

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using NeatShift.Services;

namespace NeatShift
{
    public partial class ProgressDialog : Form
    {
        private readonly IOOperation _operation;

        public ProgressDialog(IOOperation operation)
        {
            InitializeComponent();
            _operation = operation;

            _operation.ProgressChanged += Operation_ProgressChanged;
            _operation.Completed += Operation_Completed;
            _operation.Cancelled += Operation_Cancelled;

            button_Cancel.Click += (s, e) => _operation.Cancel();
        }

        private void Operation_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Operation_ProgressChanged(sender, e)));
                return;
            }

            progressBar1.Value = e.ProgressPercentage;
            label_Progress.Text = e.Message;
        }

        private void Operation_Completed(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Operation_Completed(sender, e)));
                return;
            }

            Close();
        }

        private void Operation_Cancelled(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Operation_Cancelled(sender, e)));
                return;
            }

            Close();
        }
    }
}
