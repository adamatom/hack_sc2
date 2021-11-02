using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PAL9002
{
    public partial class PlayerDialog : Form
    {
        const int MF_BYPOSITION = 0x400;

        [DllImport("User32")]
        private static extern int RemoveMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("User32")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("User32")]
        private static extern int GetMenuItemCount(IntPtr hWnd);

        private int playerNum = 1;

        private Color color;

        private string playername = "";
        public string PlayerName
        {
            get { return playername; }
            set 
            {
                playername = value;
                labelPlayer.Text = playername;
                this.Text = playername;
            }
        }

        public Color DrawColor
        {
            set
            {
                color = value;
            }
            get
            {
                return color;
            }
        }

        public PlayerDialog(int playernum)
        {
            InitializeComponent();
            comboBox1.Items.Add(Color.Red.Name);
            comboBox1.Items.Add(Color.Blue.Name);
            comboBox1.Items.Add(Color.Teal.Name);
            comboBox1.Items.Add(Color.Purple.Name);
            comboBox1.Items.Add(Color.Yellow.Name);
            comboBox1.Items.Add(Color.Orange.Name);
            comboBox1.Items.Add(Color.Green.Name);
            comboBox1.Items.Add(Color.Pink.Name);
            playerNum = playernum+1;
            if(playerNum < 9)
            {
                labelPlayer.Text = "Player " + playerNum.ToString();
                this.Text = "Player " + playerNum.ToString();

                comboBox1.SelectedIndex = playerNum - 1;
                labelPlayer.ForeColor = Color.FromName(comboBox1.Items[comboBox1.SelectedIndex].ToString());
                color = labelPlayer.ForeColor;
            }
            //color = Color.AliceBlue;
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            labelPlayer.ForeColor = Color.FromName(comboBox1.Items[comboBox1.SelectedIndex].ToString());
            color =labelPlayer.ForeColor;
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = new Font("Verdana", 7, FontStyle.Regular);
                Color c = Color.FromName(n);
                Brush b = new SolidBrush(c);
                g.DrawString(n, f, Brushes.Black, rect.X, rect.Top);
                g.FillRectangle(b, rect.X + 40, rect.Y , rect.Width - 10, rect.Height);
            }
        }

        private void PlayerDialog_Load(object sender, EventArgs e)
        {
            IntPtr hMenu = GetSystemMenu(this.Handle, false);

            int menuItemCount = GetMenuItemCount(hMenu);

            RemoveMenu(hMenu, menuItemCount - 1, MF_BYPOSITION);

            if (playerNum < 9)
            {
                comboBox1.SelectedIndex = playerNum - 1;
                labelPlayer.ForeColor = Color.FromName(comboBox1.Items[comboBox1.SelectedIndex].ToString());
                color = labelPlayer.ForeColor;
            }
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
