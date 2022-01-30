using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Diga.Core.Api.Win32.Tools;
using Diga.Core.Threading;

namespace TestForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            UIDispatcher.UIThread.Post(() =>
            {
                this.label1.Text = "Hallo Welt";
            });
            await Task.Delay(1000);
            await SetLable("hallo Weltxxx");

            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(500);
                await SetLable("Label MSG:" + i.ToString());
            }
        }

        private async Task SetLable(string value)
        {
            await UIDispatcher.UIThread.InvokeAsync(() =>
            {
                this.label1.Text = value;
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
            var openWindows = WindowInfo.GetTopLevelWindows();
            foreach (var wnd in openWindows)
            {
                this.listBox1.Items.Add($"hwnd:{wnd.Key.ToInt64()}, Class:{wnd.Value}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string result =  UIDispatcher.UIThread.Invoke<string>(  async () =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(100);
                        await SetLable("Value:" + i);
                    }

                    //throw new Exception("Dies ist eine gewollte Exception!");
                    return "hallo";
                });


                MessageBox.Show("Bearbeitung fertig:" + result);

            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception:" + exception.Message);
            }

        }
    }
}
