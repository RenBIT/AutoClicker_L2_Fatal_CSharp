using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Fatal
{
    public partial class Form1 : Form
    {
        private int[] TimDec = new int[13];
        private int[] Key = { 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123 };
        private UInt32[] DownPar = { 0x003B0001, 0x003C0001, 0x003D0001, 0x003E0001, 0x003F0001, 0x00400001, 0x00410001, 0x00420001, 0x00430001, 0x00440001, 0x00570001, 0x00580001 };
        private UInt32[] UpPar = { 0xC03B0001, 0xC03C0001, 0xC03D0001, 0xC03E0001, 0xC03F0001, 0xC0400001, 0xC0410001, 0xC0420001, 0xC0430001, 0xC0440001, 0xC0570001, 0xC0580001 };

        private IntPtr Wnd;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(int xPoint, int yPoint);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, Int32 wParam, UInt32 lParam);

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonWnd_MouseDown(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.SizeAll;
            MouseHandleTimer.Enabled = true;
        }

        private void buttonWnd_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor.Current = Cursors.Default;
            MouseHandleTimer.Enabled = false;
            MessageBox.Show("Окно подключено: " + Wnd.ToString());
        }

        //Один обработчик для 12шт. TextBox (Вводим только цифры)
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) && e.KeyChar != 8 && e.KeyChar != 32)
            {
                e.Handled = true;
            }
        }

        //Таймер получаем Окно под курсором  
        private void MouseHandleTimer_Tick(object sender, EventArgs e)
        {
            Wnd = WindowFromPoint(MousePosition.X, MousePosition.Y);
        }

        //Кнопка старт
        private void button2_Click(object sender, EventArgs e)
        {
            //Загрузим настройки программы
            for (int i = 1; i <= 12; i++)
            {
                TextBox textBox = (TextBox)this.Controls["textBox" + i.ToString()];
                if (textBox.Text == "")
                {
                    textBox.Text = "0";
                }
                TimDec[i] = int.Parse(textBox.Text);
            }

            if (TimerDec.Enabled)
            {
                for (int i = 1; i <= 12; i++)
                {
                    TextBox textBox = (TextBox)this.Controls["textBox" + i.ToString()];
                    textBox.Enabled = true;
                }
                TimerDec.Enabled = false;
            }
            else
            {
                for (int i = 1; i <= 12; i++)
                {
                    TextBox textBox = (TextBox)this.Controls["textBox" + i.ToString()];
                    textBox.Enabled = false;
                }
                TimerDec.Enabled = true;
            }
        }

        //Нажимаем клавишу по ёе коду  
        private void sKeyPress(int key, UInt32 downPar, UInt32 upPar)
        {
            PostMessage(Wnd, WM_SYSKEYDOWN, key, downPar);
            System.Threading.Thread.Sleep(2);
            PostMessage(Wnd, WM_SYSKEYUP, key, upPar);
        }

        //При старте программы
        private void Form1_Load(object sender, EventArgs e)
        {
            //Загрузим настройки программы
            for (int i = 1; i <= 12; i++)
            {
                var split = (string)Properties.Settings.Default["Text" + i.ToString()];
                List<string> list = new List<string>();
                list = split.Split(',').ToList();
                this.Controls["textBox" + i.ToString()].Text = list[0];
                CheckBox checkbox = (CheckBox)this.Controls["checkBox" + i.ToString()];
                checkbox.Checked = bool.Parse(list[1]);
            }
        }

        //При закрытие программы
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //При закрытие программы сохраним настройки
            for (int i = 1; i <= 12; i++)
            {
                CheckBox checkbox = (CheckBox)this.Controls["checkBox" + i.ToString()];
                Properties.Settings.Default["Text" + i.ToString()] = this.Controls["textBox" + i.ToString()].Text + "," + checkbox.Checked;
                Properties.Settings.Default.Save();
            }
        }

        //Таймер нажатий клавиш
        private void TimerDec_Tick(object sender, EventArgs e)
        {
            for (int i = 1; i <= 12; i++)
            {
                CheckBox checkbox = (CheckBox)this.Controls["checkBox" + i.ToString()];
                if (checkbox.Checked)
                {
                    TimDec[i]--;
                    if (TimDec[i] == 0)
                    {
                        sKeyPress(Key[i - 1], DownPar[i - 1], UpPar[i - 1]);
                        TimDec[i] = int.Parse(this.Controls["textBox" + i.ToString()].Text);

                    }
                    if (TimDec[i] > 0)
                        this.Controls["label" + i.ToString()].Text = TimDec[i].ToString();
                }
            }
        }
    }
}
