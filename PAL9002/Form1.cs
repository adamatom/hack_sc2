using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;


namespace PAL9002
{
    public partial class Form1 : Form
    {
        //1.0.3 current unit list: 1c97040
        //convert id number to offset:
        //id = id * 3;
        //id = id << 0x7; (SHL)
        public const UInt32 UnitListLocation = 0x1CBD840;
        // population = 1c97004
        public const UInt32 TotalPopulationLocation = 0x1CBD804;
        // A lot happens in this struct. Might be worthwhile to reverse it further
        //known info:
        public const UInt32 MegaStructureLocation = 0x0250CAE0;
        // 0x0 = 01 when a player, 0x0 when not a player
        //+0x458	= current max population
        //+0x450	= current population
        //+0x98	= hard population maximum
        //+0x460	= minerals
        //+0x468	= gas
        //+0x3c     = player name (null terminated)

        public const UInt32 GameDatabasePtr = 0x16F28D0;

        // Contains a hash table with units worth paying attention to
        public LookupTable units;// = new LookupTable(GameDatabasePtr, );
        ProcessMemoryReader pReader;


        Player player1;
        Player player2;
        Player player3;
        Player player4;
        Player player5;
        Player player6;
        Player player7;
        Player player8;


        PlayerDialog[] pd = new PlayerDialog[8];

        Font myFont = new Font("Verdana", 8);

        List<UInt32> player1pointers = new List<uint>();
        List<UInt32> player2pointers = new List<uint>();
        List<UInt32> player3pointers = new List<uint>();
        List<UInt32> player4pointers = new List<uint>();
        List<UInt32> player5pointers = new List<uint>();
        List<UInt32> player6pointers = new List<uint>();
        List<UInt32> player7pointers = new List<uint>();
        List<UInt32> player8pointers = new List<uint>();

        Dictionary<UInt32, Unit> RegMinerals = new Dictionary<UInt32, Unit>();
        Dictionary<UInt32, Unit> RichMinerals = new Dictionary<UInt32, Unit>();

        Thread workerThread;
        bool startup = true;

      

        public Form1()
        {
            InitializeComponent();

            pReader = new ProcessMemoryReader();
            System.Diagnostics.Process[] myProcesses
                   = System.Diagnostics.Process.GetProcessesByName("SC2");
            if (myProcesses.Length == 0)
            {
                MessageBox.Show("No starcraft process found");

            }
            else
            {
                pReader.ReadProcess = myProcesses[0];

                // open process in read memory mode
                pReader.OpenProcess();
            }

            units = new LookupTable(GameDatabasePtr, pReader);

            for (int i = 0; i < 8; i++)
            {
                pd[i] = new PlayerDialog(i);
            }


            comboBox1.SelectedItem = comboBox1.Items[0];
            player1 = new Player(1, units, pReader, GetDialogForPlayer(1).dataGridView1, GetDialogForPlayer(1).bindingSource1, comboBox1.SelectedItem.ToString());
            player2 = new Player(2, units, pReader, GetDialogForPlayer(2).dataGridView1, GetDialogForPlayer(2).bindingSource1, comboBox1.SelectedItem.ToString());
            player3 = new Player(3, units, pReader, GetDialogForPlayer(3).dataGridView1, GetDialogForPlayer(3).bindingSource1, comboBox1.SelectedItem.ToString());
            player4 = new Player(4, units, pReader, GetDialogForPlayer(4).dataGridView1, GetDialogForPlayer(4).bindingSource1, comboBox1.SelectedItem.ToString());
            player5 = new Player(5, units, pReader, GetDialogForPlayer(5).dataGridView1, GetDialogForPlayer(5).bindingSource1, comboBox1.SelectedItem.ToString());
            player6 = new Player(6, units, pReader, GetDialogForPlayer(6).dataGridView1, GetDialogForPlayer(6).bindingSource1, comboBox1.SelectedItem.ToString());
            player7 = new Player(7, units, pReader, GetDialogForPlayer(7).dataGridView1, GetDialogForPlayer(7).bindingSource1, comboBox1.SelectedItem.ToString());
            player8 = new Player(8, units, pReader, GetDialogForPlayer(8).dataGridView1, GetDialogForPlayer(8).bindingSource1, comboBox1.SelectedItem.ToString());

            GetDialogForPlayer(1).bindingSource1.DataSource = player1.UnitCounts;
            GetDialogForPlayer(1).dataGridView1.DataSource = GetDialogForPlayer(1).bindingSource1;

            GetDialogForPlayer(2).bindingSource1.DataSource = player2.UnitCounts;
            GetDialogForPlayer(2).dataGridView1.DataSource = GetDialogForPlayer(2).bindingSource1;

            GetDialogForPlayer(3).bindingSource1.DataSource = player3.UnitCounts;
            GetDialogForPlayer(3).dataGridView1.DataSource = GetDialogForPlayer(3).bindingSource1;

            GetDialogForPlayer(4).bindingSource1.DataSource = player4.UnitCounts;
            GetDialogForPlayer(4).dataGridView1.DataSource = GetDialogForPlayer(4).bindingSource1;

            GetDialogForPlayer(5).bindingSource1.DataSource = player5.UnitCounts;
            GetDialogForPlayer(5).dataGridView1.DataSource = GetDialogForPlayer(5).bindingSource1;

            GetDialogForPlayer(6).bindingSource1.DataSource = player6.UnitCounts;
            GetDialogForPlayer(6).dataGridView1.DataSource = GetDialogForPlayer(6).bindingSource1;

            GetDialogForPlayer(7).bindingSource1.DataSource = player7.UnitCounts;
            GetDialogForPlayer(7).dataGridView1.DataSource = GetDialogForPlayer(7).bindingSource1;

            GetDialogForPlayer(8).bindingSource1.DataSource = player8.UnitCounts;
            GetDialogForPlayer(8).dataGridView1.DataSource = GetDialogForPlayer(8).bindingSource1;


            startup = false;
            workerThread = new Thread(this.DoWork);
            _shouldStop = false;
            workerThread.Start();


        }
        // Volatile is used as hint to the compiler that this dasta
        // member will be accessed by multiple threads.
        private volatile bool _shouldStop;
        private void DoWork()
        {
            while (_shouldStop == false)
            {
                glCanvas2D1.Invalidate();
                Thread.Sleep(33);
            }

        }

        private PlayerDialog GetDialogForPlayer(int num)
        {
            if (num < 1 || num > 8)
                throw new Exception("Player num must be between 1 and 8");
            return pd[num - 1];
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Read into the super struct (tm) and see what players are around.
            // Read their resources as well
            for (Int32 i = 1; i <= 8; i++)
            {
                //player check - Null name = no player
                if ((UInt32)pReader.ReadChar(MegaStructureLocation + (UInt32)(0x8a0 * i) + (UInt32)Offsets.PlayerName) != 0)
                {
                    // Get their name
                    string name = pReader.ReadString(MegaStructureLocation + (UInt32)(0x8a0 * i) + (UInt32)Offsets.PlayerName, 28);
                    GetDialogForPlayer(i).PlayerName = name;

                    //get mineral count
                    UInt32 min = (UInt32)pReader.ReadInteger(MegaStructureLocation + (UInt32)(0x8a0 * i) + (UInt32)Offsets.Minerals) & 0x0000FFFF;
                    
                    //get gas count
                    UInt32 gas = (UInt32)pReader.ReadInteger(MegaStructureLocation + (UInt32)(0x8a0 * i) + (UInt32)Offsets.Gas) & 0x0000FFFF;

                    //get population
                    byte[] popArr = new byte[4];
                    popArr[2] = 0;
                    popArr[3] = 0;
                    popArr[0] = pReader.ReadByte(MegaStructureLocation + (UInt32)(0x8a0 * i) + (UInt32)Offsets.CurrentPopulation + 1);
                    popArr[1] = pReader.ReadByte(MegaStructureLocation + (UInt32)(0x8a0 * i) + (UInt32)Offsets.CurrentPopulation + 2);
                    int pop = BitConverter.ToInt32(popArr, 0) >> 4;

                    //get current max pop
                    popArr[2] = 0;
                    popArr[3] = 0;
                    popArr[0] = pReader.ReadByte(MegaStructureLocation + (UInt32)(0x8a0 * i) + (UInt32)Offsets.CurrentMaxPopulation + 1);
                    popArr[1] = pReader.ReadByte(MegaStructureLocation + (UInt32)(0x8a0 * i) + (UInt32)Offsets.CurrentMaxPopulation + 2);
                    int maxpop = BitConverter.ToInt32(popArr, 0) >> 4;

                    // print in dialog
                    GetDialogForPlayer(i).toolStripMinerals.Text = min.ToString();
                    GetDialogForPlayer(i).toolStripGas.Text = gas.ToString();
                    GetDialogForPlayer(i).toolStripPopulation.Text = pop.ToString() + "/" + maxpop.ToString();

                    

                }
                else
                {
                    GetDialogForPlayer(i).PlayerName = "Not Detected";
                    if(i == 1) // player one isnt detected, clear the mineral field
                    {
                        RegMinerals.Clear();
                        RichMinerals.Clear();
                    }
                }
            }
            //update form colors
            UpdatePlayerLabels();

            checkBox1.Text = GetDialogForPlayer(1).PlayerName;
            checkBox2.Text = GetDialogForPlayer(2).PlayerName;
            checkBox3.Text = GetDialogForPlayer(3).PlayerName;
            checkBox4.Text = GetDialogForPlayer(4).PlayerName;
            checkBox5.Text = GetDialogForPlayer(5).PlayerName;
            checkBox6.Text = GetDialogForPlayer(6).PlayerName;
            checkBox7.Text = GetDialogForPlayer(7).PlayerName;
            checkBox8.Text = GetDialogForPlayer(8).PlayerName;

            UpdatePlayerLists();
        }

        public void UpdatePlayerLabels()
        {
            checkBox1.ForeColor = GetDialogForPlayer(1).DrawColor;
            checkBox2.ForeColor = GetDialogForPlayer(2).DrawColor;
            checkBox3.ForeColor = GetDialogForPlayer(3).DrawColor;
            checkBox4.ForeColor = GetDialogForPlayer(4).DrawColor;
            checkBox5.ForeColor = GetDialogForPlayer(5).DrawColor;
            checkBox6.ForeColor = GetDialogForPlayer(6).DrawColor;
            checkBox7.ForeColor = GetDialogForPlayer(7).DrawColor;
            checkBox8.ForeColor = GetDialogForPlayer(8).DrawColor;
        }

        private void UpdatePlayerLists()
        {
            // first determine how many units are in play
            UInt32 popcount = (UInt32)pReader.ReadChar(TotalPopulationLocation);

            player1pointers.Clear();
            player2pointers.Clear();
            player3pointers.Clear();
            player4pointers.Clear();
            player5pointers.Clear();
            player6pointers.Clear();
            player7pointers.Clear();
            player8pointers.Clear();

            UInt32 id = 1;
            for (; id <= popcount; ++id)
            {
                UInt32 myoffset = id;
                myoffset = myoffset * 3;
                myoffset = myoffset << 0x7;

                //read player one's unit list
                int playerid = (int)pReader.ReadByte(UnitListLocation + myoffset + (UInt32)Offsets.PlayerID);

                UInt32 modelptr = (UInt32)pReader.ReadInteger(UnitListLocation + myoffset + (UInt32)Offsets.ModelPtr);
                modelptr = modelptr << 5;
                UInt32 unitid = (UInt32)pReader.ReadInteger(modelptr + (UInt32)Offsets.UnitIDModelPtr);
                // Get the unit type
                //                     if (UnitListLocation + myoffset == 0x1ca6940)
                //                     {
                // 
                //                         int i = 3;
                //                     }
                if (unitid == units.Lookup["MineralField"] && RegMinerals.ContainsKey(UnitListLocation + myoffset)==false)
                {
                    Unit min = new Unit(UnitListLocation + myoffset, units, pReader);
                    min.Update();
                    RegMinerals.Add(UnitListLocation + myoffset, min);
                }
                if (unitid == units.Lookup["RichMineralField"] && RichMinerals.ContainsKey(UnitListLocation + myoffset) == false)
                {
                    Unit min = new Unit(UnitListLocation + myoffset, units, pReader);
                    min.Update();
                    RichMinerals.Add(UnitListLocation + myoffset, min);
                }

                if (playerid == 1)
                {
                    if (units.LookupById.ContainsKey(unitid))
                    {
                        // we found a unit we'd like to track
                        player1pointers.Add(UnitListLocation + myoffset);
                    }

                }
                if (playerid == 2)
                {
                    if (units.LookupById.ContainsKey(unitid))
                    {
                        // we found a unit we'd like to track. 
                        player2pointers.Add(UnitListLocation + myoffset);
                    }

                }
                if (playerid == 3)
                {
                    if (units.LookupById.ContainsKey(unitid))
                    {
                        // we found a unit we'd like to track. 
                        player3pointers.Add(UnitListLocation + myoffset);
                    }

                }
                if (playerid == 4)
                {
                    if (units.LookupById.ContainsKey(unitid))
                    {
                        // we found a unit we'd like to track. 
                        player4pointers.Add(UnitListLocation + myoffset);
                    }
                }
                if (playerid == 5)
                {
                    if (units.LookupById.ContainsKey(unitid))
                    {
                        // we found a unit we'd like to track. 
                        player5pointers.Add(UnitListLocation + myoffset);
                    }
                }
                if (playerid == 6)
                {
                    if (units.LookupById.ContainsKey(unitid))
                    {
                        // we found a unit we'd like to track. 
                        player6pointers.Add(UnitListLocation + myoffset);
                    }
                }
                if (playerid == 7)
                {
                    if (units.LookupById.ContainsKey(unitid))
                    {
                        // we found a unit we'd like to track. 
                        player7pointers.Add(UnitListLocation + myoffset);
                    }
                }
                if (playerid == 8)
                {
                    if (units.LookupById.ContainsKey(unitid))
                    {
                        // we found a unit we'd like to track. 
                        player8pointers.Add(UnitListLocation + myoffset);
                    }
                }
            }

            player1.UpdateUnitList(player1pointers);
            player2.UpdateUnitList(player2pointers);
            player3.UpdateUnitList(player3pointers);
            player4.UpdateUnitList(player4pointers);
            player5.UpdateUnitList(player5pointers);
            player6.UpdateUnitList(player6pointers);
            player7.UpdateUnitList(player7pointers);
            player8.UpdateUnitList(player8pointers);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                GetDialogForPlayer(1).Show();
            }
            else
            {
                GetDialogForPlayer(1).Hide();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                GetDialogForPlayer(2).Show();
            }
            else
            {
                GetDialogForPlayer(2).Hide();
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                GetDialogForPlayer(3).Show();
            }
            else
            {
                GetDialogForPlayer(3).Hide();
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                GetDialogForPlayer(4).Show();
            }
            else
            {
                GetDialogForPlayer(4).Hide();
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                GetDialogForPlayer(5).Show();
            }
            else
            {
                GetDialogForPlayer(5).Hide();
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                GetDialogForPlayer(6).Show();
            }
            else
            {
                GetDialogForPlayer(6).Hide();
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                GetDialogForPlayer(7).Show();
            }
            else
            {
                GetDialogForPlayer(7).Hide();
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                GetDialogForPlayer(8).Show();
            }
            else
            {
                GetDialogForPlayer(8).Hide();
            }
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            //glCanvas2D1.Invalidate();
        }

        private void DrawPlayer(GLView.GLGraphics g, Player p, PlayerDialog pd)//, float factorx, float factory)
        {
            p.UpdateUnits();
            foreach (Unit unit in p.UnitList.Values)
            {
                if (unit.IsDead) continue;
                // draw the fucker
                g.DrawRasterText(unit.LocationX, unit.LocationY, unit.TypeIDString.Substring(0, 1), pd.DrawColor);

                if (unit.TargetLocationX != 0.0f && unit.TargetLocationY != 0.0f)
                {
                    g.DrawLine(unit.LocationX, unit.LocationY, unit.TargetLocationX, unit.TargetLocationY, Color.Aqua);
                }

            }

        }

        private void Form1_Activated(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                GetDialogForPlayer(1).Activate();
            if (checkBox2.Checked)
                GetDialogForPlayer(2).Activate();
            if (checkBox3.Checked)
                GetDialogForPlayer(3).Activate();
            if (checkBox4.Checked)
                GetDialogForPlayer(4).Activate();
            if (checkBox5.Checked)
                GetDialogForPlayer(5).Activate();
            if (checkBox6.Checked)
                GetDialogForPlayer(6).Activate();
            if (checkBox7.Checked)
                GetDialogForPlayer(7).Activate();
            if (checkBox8.Checked)
                GetDialogForPlayer(8).Activate();
        }
        private const float mineralsize = 0.25f;
        private void glCanvas2D1_Render(object sender, GLView.GLGraphics g)
        {

            foreach (Unit unit in RegMinerals.Values)
            {
                // draw the fucker
                g.FillRectangle(unit.LocationX - mineralsize, unit.LocationY - mineralsize,
                    unit.LocationX + mineralsize, unit.LocationY + mineralsize, Color.Aquamarine);
            }

            foreach (Unit unit in RichMinerals.Values)
            {
                // draw the fucker
                g.FillRectangle(unit.LocationX - mineralsize, unit.LocationY - mineralsize,
                    unit.LocationX + mineralsize, unit.LocationY + mineralsize, Color.Gold);
            }


            DrawPlayer(g, player1, GetDialogForPlayer(1));//, positionXfactor, positionYfactor);
            DrawPlayer(g, player2, GetDialogForPlayer(2));//, positionXfactor, positionYfactor);
            DrawPlayer(g, player3, GetDialogForPlayer(3));//, positionXfactor, positionYfactor);
            DrawPlayer(g, player4, GetDialogForPlayer(4));//, positionXfactor, positionYfactor);
            DrawPlayer(g, player5, GetDialogForPlayer(5));//, positionXfactor, positionYfactor);
            DrawPlayer(g, player6, GetDialogForPlayer(6));//, positionXfactor, positionYfactor);
            DrawPlayer(g, player7, GetDialogForPlayer(7));//, positionXfactor, positionYfactor);
            DrawPlayer(g, player8, GetDialogForPlayer(8));//, positionXfactor, positionYfactor);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _shouldStop = true;
            workerThread.Join();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startup == false)
            {
                player1.Race = comboBox1.SelectedItem.ToString();
                player2.Race = comboBox1.SelectedItem.ToString();
                player3.Race = comboBox1.SelectedItem.ToString();
                player4.Race = comboBox1.SelectedItem.ToString();
                player5.Race = comboBox1.SelectedItem.ToString();
                player6.Race = comboBox1.SelectedItem.ToString();
                player7.Race = comboBox1.SelectedItem.ToString();
                player8.Race = comboBox1.SelectedItem.ToString();
            }


        }

        private void openGLCtrl1_Load(object sender, EventArgs e)
        {

        }

//         private void openGLCtrl1_OpenGLDraw(object sender, PaintEventArgs e)
//         {
//             //  Get the OpenGL object, for quick access.
//             SharpGL.OpenGL gl = this.openGLCtrl1.OpenGL;
//             gl.MatrixMode(OpenGL.PROJECTION);
//             gl.LoadIdentity();
//             gl.Enable(OpenGL.LINE_SMOOTH);
// 
//             gl.Ortho(mCameraPosition.X - ((float)openGLCtrl1.Width) * mZoomFactor / 2, mCameraPosition.X + ((float)openGLCtrl1.Width) * mZoomFactor / 2, mCameraPosition.Y - ((float)openGLCtrl1.Height) * mZoomFactor / 2, mCameraPosition.Y + ((float)openGLCtrl1.Height) * mZoomFactor / 2, -1.0f, 1.0f);
// 
//             gl.MatrixMode(OpenGL.MODELVIEW);
//             gl.LoadIdentity();
// 
//           
// 
// 
//             // Clear screen
//             gl.ClearColor(0.0f, 0.0f, 0.0f,1.0f);
//             gl.ClearDepth(1.0f);
//             gl.Clear(OpenGL.COLOR_BUFFER_BIT | OpenGL.DEPTH_BUFFER_BIT);
// 
//             //  Bind the texture.
//             texture.Bind(gl);
// 
//             gl.Begin(OpenGL.QUADS);
// 
//             // Front Face
//             gl.TexCoord(0.0f, 0.0f); gl.Vertex(-100.0f, -100.0f, 0.0f);	// Bottom Left Of The Texture and Quad
//             gl.TexCoord(1.0f, 0.0f); gl.Vertex(100.0f, -100.0f, 0.0f);	// Bottom Right Of The Texture and Quad
//             gl.TexCoord(1.0f, 1.0f); gl.Vertex(100.0f, 100.0f, 0.0f);	// Top Right Of The Texture and Quad
//             gl.TexCoord(0.0f, 1.0f); gl.Vertex(-100.0f, 100.0f, 0.0f);	// Top Left Of The Texture and Quad
// 
//             gl.End();
// 
//         }


    }
}
