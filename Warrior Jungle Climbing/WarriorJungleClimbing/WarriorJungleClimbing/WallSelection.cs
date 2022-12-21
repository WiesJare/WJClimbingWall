using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;



namespace WarriorJungleClimbing
{

    public partial class WallSelection : Form
    {
        public WallSelection()
        {
            InitializeComponent();
            this.Location = new Point(Screen.AllScreens[Screen.AllScreens.GetUpperBound(0) / 2].Bounds.X,
                                      Screen.AllScreens[Screen.AllScreens.GetUpperBound(0) / 2].Bounds.Y);
            //this.Location = new Point(Screen.AllScreens[Screen.AllScreens.GetLowerBound(0) / 2].Bounds.X,
            //                          Screen.AllScreens[Screen.AllScreens.GetLowerBound(0) / 2].Bounds.Y);
            this.StartPosition = FormStartPosition.Manual;
        }


        const int numLeds = 49;
        // The serial port for the ESP32 board
        static SerialPort serialPort;
        /***Snake Game***/
        private List<Circle> Snake = new List<Circle>();
        private Circle food = new Circle();
        List<int> snakeBody = new List<int>();
        int snakeHead;
        int snakeFood;
        bool firstPlay = true;
        /***********************Globals**************************/
        public static ClimbWall LightWall = new ClimbWall();
        public List<Climber> climberList = new List<Climber>();
        public List<Route> routeList = new List<Route>();
        serialSend lightControl = new serialSend();
        List<Pixel> wallState = new List<Pixel>();
        List<int> regularHolds = new List<int>();
        List<int> startHolds = new List<int>();
        List<int> aboveHolds = new List<int>();
        List<int> dualHolds = new List<int>();
        public char[,] randomRoute = new char[15, 30];
        public int[,] holdDifficultiesHands = new int[15, 30];
        public int[,] holdDifficultiesFeet = new int[15, 30];
        public int[,] timesUsedHands = new int[15, 30];
        public int[,] timesUsedFeet = new int[15, 30];
        public bool[,] usedAsStart = new bool[15, 30];
        public bool[,] usedAsFoot = new bool[15, 30];
        public bool abortLightThread = true;
        public bool sortByDifficulty = true;
        public int num = 0;
        public int editedRoute = -1;
        public char[,] newRoute = new char[15, 30];
        public int[,] lightCrosswalk = new int[,]
        //  A       B       C       D       E       F       G       H       I       J       K       L       M       N       O
        {{  -1,     -1,     -1,     32,     33,     34,     35,     36,     37,     38,     39,     40,     41,     42,     43},    //30
        {   28,     29,     30,     31,     95,     96,     97,     98,     99,     100,    101,    102,    103,    104,    44},    //29
        {   27,     91,     92,     93,     94,     158,    159,    160,    161,    162,    163,    164,    165,    105,    45},    //28
        {   26,     90,     154,    155,    156,    157,    221,    222,    223,    224,    225,    226,    166,    106,    46},    //27
        {   25,     89,     153,    217,    218,    219,    220,    284,    285,    286,    287,    227,    167,    107,    47},    //26
        {   24,     88,     152,    216,    280,    281,    282,    283,    347,    348,    288,    228,    168,    108,    48},    //25
        {   23,     87,     151,    215,    279,    343,    344,    345,    346,    349,    289,    229,    169,    109,    49},    //24
        {   22,     86,     150,    214,    278,    342,    406,    407,    408,    350,    290,    230,    170,    110,    50},    //23
        {   21,     85,     149,    213,    277,    341,    405,    410,    409,    351,    291,    231,    171,    111,    51},    //22
        {   20,     84,     148,    212,    276,    340,    404,    411,    412,    352,    292,    232,    172,    112,    52},    //21
        {   19,     83,     147,    211,    275,    339,    403,    414,    413,    353,    293,    233,    173,    113,    53},    //20
        {   18,     82,     146,    210,    274,    338,    402,    415,    416,    354,    294,    234,    174,    114,    54},    //19
        {   17,     81,     145,    209,    273,    337,    401,    418,    417,    355,    295,    235,    175,    115,    55},    //18
        {   16,     80,     144,    208,    272,    336,    400,    419,    420,    356,    296,    236,    176,    116,    56},    //17
        {   15,     79,     143,    207,    271,    335,    399,    422,    421,    357,    297,    237,    177,    117,    57},    //16
        {   14,     78,     142,    206,    270,    334,    398,    423,    424,    358,    298,    238,    178,    118,    58},    //15
        {   13,     77,     141,    205,    269,    333,    397,    426,    425,    359,    299,    239,    179,    119,    59},    //14
        {   12,     76,     140,    204,    268,    332,    396,    427,    428,    360,    300,    240,    180,    120,    60},    //13
        {   11,     75,     139,    203,    267,    331,    395,    430,    429,    361,    301,    241,    181,    121,    61},    //12
        {   10,     74,     138,    202,    266,    330,    394,    431,    432,    362,    302,    242,    182,    122,    62},    //11
        {   9,      73,     137,    201,    265,    329,    393,    434,    433,    363,    303,    243,    183,    123,    63},    //10
        {   8,      72,     136,    200,    264,    328,    392,    435,    383,    364,    304,    244,    184,    124,    125},   //9
        {   7,      71,     135,    199,    263,    327,    391,    436,    382,    365,    305,    245,    185,    186,    126},   //8
        {   6,      70,     134,    198,    262,    326,    390,    437,    381,    366,    306,    246,    247,    187,    127},   //7
        {   5,      69,     133,    197,    261,    325,    389,    438,    380,    367,    307,    308,    248,    188,    189},   //6
        {   4,      68,     132,    196,    260,    324,    388,    439,    379,    368,    369,    309,    249,    250,    190},   //5
        {   3,      67,     131,    195,    259,    323,    387,    440,    378,    377,    370,    310,    311,    251,    191},   //4
        {   2,      66,     130,    194,    258,    322,    386,    441,    442,    376,    371,    319,    312,    252,    253},   //3
        {   1,      65,     129,    193,    257,    321,    385,    446,    443,    375,    372,    318,    313,    314,    254},   //2
        {   0,      64,     128,    192,    256,    320,    384,    445,    444,    374,    373,    317,    316,    315,    255} }; //1


        /***********************First Load/ Import all routes and climbers**************************/
        private void WallSelection_Load(object sender, EventArgs e)
        {
            serialPort = new SerialPort("COM6", 512000);
            serialPort.Open();
            serialPort.WriteTimeout = 10;
            byte light;
            byte red = 255;
            byte green = 255;
            byte blue = 255;
            /*for (int i = 0; i < 64; i++)
            {
                light = (byte)i;
                //serialPort.WriteLine();
                byte[] buf = new byte[4];
                buf[0] = light;
                buf[1] = red;
                buf[2] = green;
                buf[3] = blue;
                serialPort.Write(buf,0,4);
            }
            
            serialPort.WriteLine("65,0,255,0,");
            serialPort.WriteLine("130,0,255,0,");
            serialPort.WriteLine("195,0,255,0,");
            serialPort.WriteLine("260,0,255,0,");
            serialPort.WriteLine("325,0,255,0,");
            serialPort.WriteLine("390,0,255,0,");
            serialPort.WriteLine("455,0,255,0,");
            //serialPort.Close();
            */

            buildClimbers();
            buildRoutes();
            showHome();
            hideClimbers();
            hideRoutes();
            hideParty();
            homePanel.Dock = DockStyle.Fill;
            ClimbersPanel.Dock = DockStyle.Fill;
            addClimberPanel.Dock = DockStyle.Fill;
            routesPanel.Dock = DockStyle.Fill;
            addRoutePanel.Dock = DockStyle.Fill;
            partyPanel.Dock = DockStyle.Fill;
            addClimberPanel.Visible = false;
            addRoutePanel.Visible = false;
            calcHoldDifficulties();


            FormBorderStyle = FormBorderStyle.None;
            //WindowState = FormWindowState.Maximized;

            var lightThread = new Thread(InitializeLights1);
            lightThread.IsBackground = true;
            lightThread.Start();
            //InitializeLights();

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Button button = new Button();
                    int size;
                    size = (this.Height / 31);
                    button.Size = new Size(size, size);
                    int locx, locy;
                    locx = (i + 1) * (this.Height / 31);
                    locy = (j + 1) * (this.Height / 31);
                    button.Location = new Point(locx, locy);
                    button.Padding = new Padding(0);
                    button.Margin = new Padding(0);
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.MouseOverBackColor = Color.Pink;
                    button.Name = "btn," + i + "," + j;
                    button.Enabled = false;
                    routesPanel.Controls.Add(button);
                }
            }
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Button button = new Button();
                    int size;
                    size = (this.Height / 31);
                    button.Size = new Size(size, size);
                    int locx, locy;
                    locx = (i + 1) * (this.Height / 31);
                    locy = (j + 1) * (this.Height / 31);
                    button.Location = new Point(locx, locy);
                    button.Padding = new Padding(0);
                    button.Margin = new Padding(0);
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.MouseOverBackColor = Color.Pink;
                    button.MouseEnter += new System.EventHandler(this.newHoldHover);
                    button.Name = "btn," + i + "," + j;
                    button.Enabled = true;
                    button.MouseUp += new MouseEventHandler(hold_Click);
                    addRoutePanel.Controls.Add(button);
                }
            }
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Button button = new Button();
                    int size;
                    size = (this.Height / 31);
                    button.Size = new Size(size, size);
                    int locx, locy;
                    locx = (i + 1) * (this.Height / 31);
                    locy = (j + 1) * (this.Height / 31);
                    button.Location = new Point(locx, locy);
                    button.Padding = new Padding(0);
                    button.Margin = new Padding(0);
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.MouseOverBackColor = Color.Pink;
                    button.MouseEnter += new System.EventHandler(this.newHoldHover);
                    button.Name = "btnParty," + i + "," + j;
                    button.Enabled = true;
                    button.MouseUp += new MouseEventHandler(hold_Click);
                    partyPanel.Controls.Add(button);
                }
            }
            for (int i = 1; i <= 15; i++)//Labels on top
            {
                char line = (char)(64 + i);
                Label label = new Label();
                int size;
                size = (this.Height / 31);
                label.Size = new Size(size, size);
                int locx, locy;
                locx = i * (this.Height / 31);
                locy = 0;
                label.Location = new Point(locx, locy);
                label.Text = i.ToString();
                label.ForeColor = Color.Black;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.AutoSize = false;
                label.Name = "lbl," + i + ",0";
                routesPanel.Controls.Add(label);
            }
            for (int i = 1; i <= 15; i++)//Labels on top
            {
                char line = (char)(64 + i);
                Label label = new Label();
                int size;
                size = (this.Height / 31);
                label.Size = new Size(size, size);
                int locx, locy;
                locx = i * (this.Height / 31);
                locy = 0;
                label.Location = new Point(locx, locy);
                label.Text = line.ToString();
                label.ForeColor = Color.Black;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.AutoSize = false;
                label.Name = "lbl," + i + ",0";
                addRoutePanel.Controls.Add(label);
            }
            for (int i = 1; i <= 30; i++)//Labels on side
            {
                string line = i.ToString();
                Label label = new Label();
                int size;
                size = (this.Height / 31);
                label.Size = new Size(size, size);
                int locx, locy;
                locx = 0;
                locy = i * (this.Height / 31);
                label.Location = new Point(locx, locy);
                label.Text = line.ToString();
                label.ForeColor = Color.Black;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.AutoSize = false;
                label.Name = "lbl,0," + i;
                routesPanel.Controls.Add(label);
            }
            for (int i = 1; i <= 30; i++)//Labels on side
            {
                string line = i.ToString();
                Label label = new Label();
                int size;
                size = (this.Height / 31);
                label.Size = new Size(size, size);
                int locx, locy;
                locx = 0;
                locy = i * (this.Height / 31);
                label.Location = new Point(locx, locy);
                label.Text = line.ToString();
                label.ForeColor = Color.Black;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.AutoSize = false;
                label.Name = "lbl,0," + i;
                addRoutePanel.Controls.Add(label);
            }

            clearNewRoute();
            //WallSelection_ResizeEnd(this, e);
            //if (lightThread.IsAlive)
            //lightThread.Join();
            //lightThread.Abort();
        }
        //These functions create the base objects for the program*****************************
        public void buildWall()
        {

        }

        public void InitializeLights()
        {
            var thread = new Thread(() =>
            {
                Thread.Sleep(4000);//Wait 4 seconds for OPC to open


                var pixelList = new List<int>();

                for (int i = 1; i < 30; i++)
                {
                    for (int j = 0; j < 15; j++)
                    {
                        pixelList.Add(lightCrosswalk[i, j]);
                    }

                    for (int j = 0; j < OpcConstants.StrandLength; j++)
                    {
                        if (pixelList.Contains(j))
                        {
                            wallState.Add(new Pixel(255, 255, 255));
                        }
                        else
                        {
                            wallState.Add(new Pixel(15, 15, 15));
                        }
                    }
                    //lightControl.WriteFrame(wallState);
                    wallState.Clear();
                    Thread.Sleep(50);
                }
                List<int> WJLogoGreen = new List<int> { 25, 89, 153, 285, 286, 287, 24, 88, 152, 281, 347, 348, 288, 87, 151, 279, 343, 344, 346, 349, 86, 150, 278, 342, 406, 408, 350, 85, 149, 277, 341, 405, 409, 351, 84, 148, 212, 276, 340, 404, 411, 412, 352, 83, 147, 211, 275, 339, 403, 414, 413, 353, 146, 210, 274, 402, 415, 416, 145, 209, 418, 417, 423, 424, 358, 298, 238, 178, 118, 426, 425, 359, 299, 239, 179, 119, 360, 300, 240, 361, 301, 241, 362, 302, 242, 363, 303, 243, 328, 392, 364, 304, 244, 327, 391, 365, 305, 245, 326, 390, 437, 366, 306, 246, 389, 438, 380, 367, 307, 439, 379, 368 };
                List<int> WJLogoWhite = new List<int> { 34, 35, 36, 37, 38, 30, 31, 95, 101, 102, 103, 27, 91, 105, 45, 2, 66, 252, 253, 129, 193, 257, 372, 318, 313, 320, 384, 445, 444, 374 };
                for (int j = 0; j < OpcConstants.StrandLength; j++)
                {
                    if (WJLogoGreen.Contains(j))
                        wallState.Add(new Pixel(0, 255, 0));
                    else if (WJLogoWhite.Contains(j))
                        wallState.Add(new Pixel(255, 255, 255));
                    else
                        wallState.Add(new Pixel(0, 0, 0));
                }
                //lightControl.WriteFrame(wallState);
                OpcClient test = new OpcClient();
                test.WriteFrame(wallState);
            });
            thread.Start();

        }

        public void InitializeLights1()
        {

            Thread.Sleep(4000);//Wait 4 seconds for OPC to open


            var pixelList = new List<int>();

            for (int i = 1; i < 30; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    pixelList.Add(lightCrosswalk[i, j]);
                }

                for (int j = 0; j < OpcConstants.StrandLength; j++)
                {
                    if (pixelList.Contains(j))
                    {
                        wallState.Add(new Pixel(255, 255, 255));
                    }
                    else
                    {
                        wallState.Add(new Pixel(15, 15, 15));
                    }
                }
                //lightControl.WriteFrame(wallState);
                wallState.Clear();
                Thread.Sleep(50);
            }
            List<int> WJLogoGreen = new List<int> { 25, 89, 153, 285, 286, 287, 24, 88, 152, 281, 347, 348, 288, 87, 151, 279, 343, 344, 346, 349, 86, 150, 278, 342, 406, 408, 350, 85, 149, 277, 341, 405, 409, 351, 84, 148, 212, 276, 340, 404, 411, 412, 352, 83, 147, 211, 275, 339, 403, 414, 413, 353, 146, 210, 274, 402, 415, 416, 145, 209, 418, 417, 423, 424, 358, 298, 238, 178, 118, 426, 425, 359, 299, 239, 179, 119, 360, 300, 240, 361, 301, 241, 362, 302, 242, 363, 303, 243, 328, 392, 364, 304, 244, 327, 391, 365, 305, 245, 326, 390, 437, 366, 306, 246, 389, 438, 380, 367, 307, 439, 379, 368 };
            List<int> WJLogoWhite = new List<int> { 34, 35, 36, 37, 38, 30, 31, 95, 101, 102, 103, 27, 91, 105, 45, 2, 66, 252, 253, 129, 193, 257, 372, 318, 313, 320, 384, 445, 444, 374 };
            for (int j = 0; j < OpcConstants.StrandLength; j++)
            {
                if (WJLogoGreen.Contains(j))
                    wallState.Add(new Pixel(0, 255, 0));
                else if (WJLogoWhite.Contains(j))
                    wallState.Add(new Pixel(255, 255, 255));
                else
                    wallState.Add(new Pixel(0, 0, 0));
            }
            //lightControl.WriteFrame(wallState);


        }

        public void hold_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Button button = (Button)sender;
            int x, y, sIndex, eIndex;
            sIndex = button.Name.IndexOf(',');
            eIndex = button.Name.IndexOf(',', sIndex + 1);
            x = Int32.Parse(button.Name.Substring(sIndex + 1, eIndex - (sIndex + 1)));
            y = Int32.Parse(button.Name.Substring(eIndex + 1, button.Name.Length - (eIndex + 1)));
            Button btn = addRoutePanel.Controls.Find("btn," + x + "," + y, true).First() as Button;

            wallState.Clear();
            startHolds.Clear();
            regularHolds.Clear();
            aboveHolds.Clear();
            dualHolds.Clear();

            if (me.Button == System.Windows.Forms.MouseButtons.Right)//Right Click//Starting Hold
            {
                if (newRoute[x, y] == '2')//Already starting hold
                {
                    newRoute[x, y] = '0';
                    btn.BackColor = Color.FromArgb(190, 190, 190);
                }
                else //Change to starting hold
                {
                    newRoute[x, y] = '2';
                    btn.BackColor = Color.Red;
                }
            }
            if (me.Button == System.Windows.Forms.MouseButtons.Left)//Left Click//Normal Hold
            {
                if (newRoute[x, y] == '1')//Already starting hold
                {
                    newRoute[x, y] = '0';
                    btn.BackColor = Color.FromArgb(190, 190, 190);
                }
                else //Change to starting hold
                {
                    newRoute[x, y] = '1';
                    btn.BackColor = Color.Green;
                }
            }
            if (me.Button == System.Windows.Forms.MouseButtons.Middle)//Middle Click//Foot Only
            {
                if (newRoute[x, y] == '3')//Already foot hold
                {
                    newRoute[x, y] = '0';
                    btn.BackColor = Color.FromArgb(190, 190, 190);
                }
                else //Change to foot hold
                {
                    newRoute[x, y] = '3';
                    btn.BackColor = Color.Black;
                }
            }

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    if (newRoute[i, j] == '1')
                    {
                        regularHolds.Add(lightCrosswalk[j, i]);
                    }
                    else if (newRoute[i, j] == '2')
                    {
                        startHolds.Add(lightCrosswalk[j, i]);
                    }
                    else if (newRoute[i, j] == '3')
                    {
                        aboveHolds.Add(lightCrosswalk[j, i]);
                    }
                }
            }
            for (int i = 0; i < OpcConstants.StrandLength; i++)
            {
                if (regularHolds.Contains(i))
                {
                    wallState.Add(new Pixel(0, 255, 0));
                }
                else if (startHolds.Contains(i))
                {
                    wallState.Add(new Pixel(255, 0, 0));
                }
                else
                {
                    wallState.Add(new Pixel(0, 0, 0));
                }
            }
            //lightControl.WriteFrame(wallState);
            wallState.Clear();
            startHolds.Clear();
            regularHolds.Clear();
            aboveHolds.Clear();
            dualHolds.Clear();
        }

        public void clearNewRoute()
        {
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    newRoute[i, j] = '0';
                    Button btn = addRoutePanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                    btn.BackColor = Color.FromArgb(190, 190, 190);
                }
            }
        }

        public void buildClimbers()
        {
            int counter = 0;
            string line;
            try
            {
                string path = Application.StartupPath + @"\climbers.climbers";
                System.IO.StreamReader file = new System.IO.StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    string[] section = new string[7]; //7 sections
                                                      //id~first~last~highest~average~set:set:set~climbed:climbed:climbed:climbed:climbed

                    int index = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        section[i] = "";
                    }
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '~')
                        {
                            index++;
                            continue;
                        }
                        else
                        {
                            section[index] += line[i].ToString();
                        }
                    }
                    Climber newClimber = new Climber();
                    newClimber.id = Int32.Parse(section[0]);
                    newClimber.firstName = section[1];
                    newClimber.lastName = section[2];
                    newClimber.belayCert = Boolean.Parse(section[3]);
                    newClimber.highestDifficulty = Int32.Parse(section[4]);
                    newClimber.averageDifficulty = Int32.Parse(section[5]);
                    climberList.Add(newClimber);
                    updateClimbers();


                    counter++;
                }
                file.Close();
            }
            catch
            {
                return;
            }
        }

        public void buildRoutes()
        {
            int counter = 0;
            string line;
            try
            {
                string path = Application.StartupPath + @"\routes.routes";
                System.IO.StreamReader file = new System.IO.StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    string[] section = new string[9];
                    int index = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        section[i] = "";
                    }
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '~')
                        {
                            index++;
                            continue;
                        }
                        else
                        {
                            section[index] += line[i].ToString();
                        }
                    }
                    Route addRoute = new Route();
                    addRoute.id = Int32.Parse(section[0]);
                    addRoute.nickname = section[1];
                    addRoute.numDifficulty = Int32.Parse(section[2]);
                    if (addRoute.numDifficulty > 9)
                        addRoute.charDifficulty = section[3][0];
                    addRoute.creator = Int32.Parse(section[4]);
                    addRoute.dateCreated = section[5];
                    addRoute.active = bool.Parse(section[6]);
                    for (int j = 0; j < 30; j++)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            addRoute.routeHolds[i, j] = section[7][(j * 15) + i];

                        }
                    }
                    addRoute.note = section[8];
                    routeList.Add(addRoute);
                    //parse routes and call updateroutes to display them all


                    updateRoutes();

                    counter++;
                }
                file.Close();
            }
            catch
            {

            }
        }

        public void calcHoldDifficulties()
        {
            //Set initial holds to 0
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    holdDifficultiesHands[i, j] = 0;
                    holdDifficultiesFeet[i, j] = 0;
                    timesUsedHands[i, j] = 0;
                    timesUsedFeet[i, j] = 0;
                    randomRoute[i, j] = '0';
                    usedAsStart[i, j] = false;
                    usedAsFoot[i, j] = false;
                }
            }

            int activeCount = 0;
            for (int x = 0; x < routeList.Count; x++)
            {
                if (routeList[x].active)
                {
                    activeCount++;
                    //Console.WriteLine(activeCount + ": " + convertDifficulty(routeList[x].numDifficulty, routeList[x].charDifficulty));
                    for (int j = 0; j < 30; j++)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            if (routeList[x].routeHolds[i, j] == '1' || routeList[x].routeHolds[i, j] == '2' || routeList[x].routeHolds[i, j] == '3') //If this hold is on this route
                            {
                                if (routeList[x].routeHolds[i, j] == '2')
                                    usedAsStart[i, j] = true;

                                if (routeList[x].routeHolds[i, j] == '1' || routeList[x].routeHolds[i, j] == '2')
                                {
                                    holdDifficultiesHands[i, j] += convertDifficulty(routeList[x].numDifficulty, routeList[x].charDifficulty);
                                    timesUsedHands[i, j]++;
                                }
                                else if (routeList[x].routeHolds[i, j] == '3')
                                {
                                    holdDifficultiesFeet[i, j] += convertDifficulty(routeList[x].numDifficulty, routeList[x].charDifficulty);
                                    usedAsFoot[i, j] = true;
                                    timesUsedFeet[i, j]++;
                                }
                            }

                            if (usedAsStart[i, j])
                            {
                                Console.Write("X" + " ");
                            }
                            else
                            {
                                Console.Write("." + " ");
                            }
                        }
                        Console.WriteLine(j);
                    }
                }
                Console.WriteLine("0 1 2 3 4 5 6 7 8 9 0 1 2 3 4");
                Console.WriteLine(" ");
            }

            for (int j = 0; j < 30; j++)
            {
                for (int i = 0; i < 15; i++)
                {
                    if (holdDifficultiesHands[i, j] > 0)
                        holdDifficultiesHands[i, j] = holdDifficultiesHands[i, j] / timesUsedHands[i, j];
                    if (holdDifficultiesFeet[i, j] > 0)
                        holdDifficultiesFeet[i, j] = holdDifficultiesFeet[i, j] / timesUsedFeet[i, j];
                    Console.Write(holdDifficultiesHands[i, j] + " ");
                }
                Console.WriteLine(j);
            }
            Console.WriteLine("0 1 2 3 4 5 6 7 8 9 0 1 2 3 4");
            Console.WriteLine(" ");
        }
        //************************************************************************************

        //These functions show and hide panels************************************************
        public void showHome() { homePanel.Visible = true; }
        public void hideHome() { homePanel.Visible = false; }
        public void showClimbers() { ClimbersPanel.Visible = true; }
        public void hideClimbers() { ClimbersPanel.Visible = false; }
        public void showRoutes() { routesPanel.Visible = true; }
        public void hideRoutes() { routesPanel.Visible = false; }
        public void showAddClimber() { addClimberPanel.Visible = true; }
        public void hideAddClimber() { addClimberPanel.Visible = false; }
        public void showAddRoute() { addRoutePanel.Visible = true; }
        public void hideAddRoute() { addRoutePanel.Visible = false; }
        public void showParty() { partyPanel.Visible = true; }
        public void hideParty() { partyPanel.Visible = false; }

        private void pbClimbers_Click(object sender, EventArgs e)
        {
            hideHome();
            showClimbers();
        }

        private void pBRoutes_Click(object sender, EventArgs e)
        {
            hideHome();
            hideClimbers();
            hideAddRoute();
            showRoutes();

            //WallSelection_ResizeEnd(this, e);
        }

        private void pBAddClimber_Click(object sender, EventArgs e)
        {
            hideClimbers();
            showAddClimber();
        }

        private void pBCloseClimbers_Click(object sender, EventArgs e)
        {
            hideClimbers();
            showHome();
        }

        private void pBCancelAddClimber_Click(object sender, EventArgs e)
        {
            hideAddClimber();
            showClimbers();
        }

        private void pbRoutesBack_Click(object sender, EventArgs e)
        {
            hideRoutes();
            showHome();
        }

        private void pBNewRoute_Click(object sender, EventArgs e)
        {
            updateSetters();
            hideRoutes();
            showAddRoute();
            clearNewRoute();
            if (lblInfoRouteName.Text == "Generated Route")
            {
                string routedifficulty = lblInfoDifficulty.Text.Remove(0,2);

                for (int i = 0; i < 15; i++)
                {
                    for (int j = 0; j < 30; j++)
                    {

                        newRoute[i, j] = randomRoute[i, j];
                        if (randomRoute[i, j] == '1')
                        {
                            Button btn = addRoutePanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                            btn.BackColor = Color.Green;
                        }
                        else if (randomRoute[i, j] == '2')//Start hold/finish hold
                        {
                            Button btn = addRoutePanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                            btn.BackColor = Color.Red;
                        }
                        else if (randomRoute[i, j] == '3')//Foot
                        {
                            Button btn = addRoutePanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                            btn.BackColor = Color.Black;
                        }
                    }
                }
                tBRouteName.Text = "";
                tbRouteDifficulty.Text = routedifficulty;
                cBRouteEditActive.Checked = true;
            }
            //WallSelection_ResizeEnd(this, e);
        }

        public void updateSetters()
        {
            lBSetterNames.Items.Clear();

            for (int i = 0; i < climberList.Count; i++)
            {
                lBSetterNames.Items.Add(climberList[i].ToString());
            }
        }

        private void pBcancelNewRoute_Click(object sender, EventArgs e)
        {
            hideAddRoute();
            clearNewRoute();
            showRoutes();
            tBNotes.Text = "";
            tBRouteName.Text = "";
            tbRouteDifficulty.Text = "";
            //WallSelection_ResizeEnd(this, e);
        }

        private void pBAddClimberDone_Click(object sender, EventArgs e)
        {
            if (tBFirstName.Text != "First Name" && tBFirstName.Text != "" && tBFirstName.Text != "First" && !tBFirstName.Text.Contains("~") && !tBFirstName.Text.Contains("\"") && !tBFirstName.Text.Contains(":"))
            {
                if (tBLastName.Text != "Last Name" && tBLastName.Text != "" && tBLastName.Text != "Last" && !tBLastName.Text.Contains("~") && !tBLastName.Text.Contains("\"") && !tBLastName.Text.Contains(":"))
                {
                    addClimberPanel.Visible = false;
                    ClimbersPanel.Visible = true;
                    addClimber(tBFirstName.Text, tBLastName.Text);
                    tBFirstName.Text = "First Name";
                    tBLastName.Text = "Last Name";
                }
                else
                {
                    MessageBox.Show("Last name has invalid characters or is an invalid name");
                }
            }
            else
            {
                MessageBox.Show("First name has invalid characters or is an invalid name");
            }
        }

        public void addClimber(string first, string last)
        {
            string climber = climberList.Count.ToString();
            climber += "~";
            climber += first;
            climber += "~";
            climber += last;
            climber += "~";
            climber += "0";
            climber += "~";
            climber += "0";
            climber += "~";
            //routes set
            climber += "~";
            //routes climbed

            string path = Application.StartupPath + @"\climbers.climbers";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
            {
                file.WriteLine(climber);
            }

            Climber newClimber = new Climber();
            newClimber.id = climberList.Count;
            newClimber.firstName = first;
            newClimber.lastName = last;
            climberList.Add(newClimber);
            updateClimbers();
        }

        public void updateClimbers()
        {
            lBClimbers.Items.Clear();

            for (int i = 0; i < climberList.Count; i++)
            {
                lBClimbers.Items.Add(climberList[i].ToString());
            }

        }

        private void pbDoneAddRoute_Click(object sender, EventArgs e)
        {
            if (tBRouteName.Text != "" && !tBRouteName.Text.Contains("~") && !tBRouteName.Text.Contains("\"") && !tBRouteName.Text.Contains(":"))
            {
                if (tbRouteDifficulty.Text != "")
                {
                    if (lBSetterNames.SelectedIndex > -1)
                    {
                        Boolean startHold = false;
                        int otherholds = 0;
                        for (int j = 0; j < 30; j++)
                        {
                            for (int i = 0; i < 15; i++)
                            {
                                if (newRoute[i, j] == '2')
                                    startHold = true;
                                else if (newRoute[i, j] == '1')
                                    otherholds++;
                            }
                        }
                        if (startHold)
                        {
                            if (otherholds > 2)
                            {
                                try
                                {
                                    int numDifficulty = Int32.Parse(tbRouteDifficulty.Text.Trim(new char[] { '-', '_', '=', '+', ' ', '`', '~', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '!', '@', '#', '$', '%', '^', '&', '(', ')', ',', '<', '.', '>', '/', '?', ';', ':', '\"', '\'', '\\', '[', '{', ']', '}', '|' }));
                                    if (numDifficulty > 9)
                                    {
                                        string tempDifficulty = tbRouteDifficulty.Text.Trim(new char[] { ' ', '`', '~', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '!', '@', '#', '$', '%', '^', '&', '(', ')', ',', '<', '.', '>', '/', '?', ';', ':', '\"', '\'', '\\', '[', '{', ']', '}', '|' });
                                        char charDifficulty = tempDifficulty[0];

                                        addRoutePanel.Visible = false;
                                        routesPanel.Visible = true;
                                        if (editedRoute == -1)
                                            addRoute(tBRouteName.Text, numDifficulty, charDifficulty, lBSetterNames.SelectedIndex);
                                        else
                                        {
                                            for (int i = 0; i < 15; i++)
                                            {
                                                for (int j = 0; j < 30; j++)
                                                {
                                                    routeList[editedRoute].routeHolds[i, j] = newRoute[i, j];
                                                }
                                            }

                                            routeList[editedRoute].nickname = tBRouteName.Text;
                                            routeList[editedRoute].numDifficulty = numDifficulty;
                                            routeList[editedRoute].charDifficulty = charDifficulty;
                                            routeList[editedRoute].note = tBNotes.Text;
                                            routeList[editedRoute].creator = lBSetterNames.SelectedIndex;
                                            routeList[editedRoute].active = cBRouteEditActive.Checked;
                                            remakeRouteFile();
                                        }
                                        tBNotes.Text = "";

                                        tbRouteDifficulty.Text = "";
                                    }
                                    else
                                    {
                                        addRoutePanel.Visible = false;
                                        routesPanel.Visible = true;
                                        if (editedRoute == -1)
                                            addRoute(tBRouteName.Text, numDifficulty, 'a', lBSetterNames.SelectedIndex);
                                        else
                                        {
                                            for (int i = 0; i < 15; i++)
                                            {
                                                for (int j = 0; j < 30; j++)
                                                {
                                                    routeList[editedRoute].routeHolds[i, j] = newRoute[i, j];
                                                }
                                            }
                                            routeList[editedRoute].nickname = tBRouteName.Text;
                                            routeList[editedRoute].numDifficulty = numDifficulty;
                                            routeList[editedRoute].note = tBNotes.Text;
                                            routeList[editedRoute].creator = lBSetterNames.SelectedIndex;
                                            routeList[editedRoute].active = cBRouteEditActive.Checked;
                                            remakeRouteFile();
                                        }
                                        tBNotes.Text = "";

                                        tbRouteDifficulty.Text = "";
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Difficulty has incorrect format\n5.1-5.9: no additional letter\n5.10 and up: requires a-d");
                                }

                            }
                            else
                            {
                                MessageBox.Show("You must select more holds for this route");
                            }

                        }
                        else
                        {
                            MessageBox.Show("You must specify a start by right clicking");

                        }
                    }
                    else
                    {
                        MessageBox.Show("You must select a route setter!");
                    }

                }
                else
                {
                    MessageBox.Show("Route difficulty is empty");
                }
            }
            else
            {
                MessageBox.Show("Route name is empty or has invalid characters");
            }
            int count = 0;
            for (int x = 0; x < routeList.Count; x++)
            {
                if (routeList[x].active)
                    count++;
                if (routeList[x].nickname == tBRouteName.Text)
                {
                    if (cbActive.Checked)
                        lBRoutes.SelectedItem = lBRoutes.Items[count - 1];
                    else
                        lBRoutes.SelectedItem = lBRoutes.Items[x];
                    lBRouteSelectionClick(lBRoutes.SelectedItem, e);
                }
            }
            editedRoute = -1;
            tBRouteName.Text = "";

        }

        public void addRoute(string name, int numdifficulty, char chardifficulty, int climberIndex)
        {
            string difficulty = numdifficulty.ToString();
            if (numdifficulty > 9)
                difficulty += chardifficulty;
            Route addedRoute = new Route();
            addedRoute.id = routeList.Count;
            addedRoute.nickname = name;
            addedRoute.difficulty = difficulty;
            addedRoute.creator = climberIndex;
            addedRoute.dateCreated = DateTime.Now.ToString();
            addedRoute.active = true;

            addedRoute.numDifficulty = numdifficulty;
            if (numdifficulty > 9)
                addedRoute.charDifficulty = chardifficulty;



            for (int j = 0; j < 30; j++)
            {
                for (int i = 0; i < 15; i++)
                {
                    addedRoute.routeHolds[i, j] = newRoute[i, j];
                }
            }
            addedRoute.note = tBNotes.Text;
            routeList.Add(addedRoute);

            remakeRouteFile();




            updateRoutes();
            clearNewRoute();


        }

        public void remakeRouteFile()
        {

            string path = Application.StartupPath + @"\routes.routes";

            string date = DateTime.Now.ToString();
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            date = rgx.Replace(date, "");
            string backupPath = Application.StartupPath + @"\routes." + date;
            System.IO.File.Copy(path, backupPath);

            using (var fs = new FileStream(Application.StartupPath + @"\routes.routes", FileMode.Truncate))
            {
                fs.Close();
            }

            for (int z = 0; z < 16; z++)//Loop through difficulties
            {
                for (char fake = 'a'; fake < 'e'; fake++)//Loop through difficulties
                {
                    for (int x = 0; x < routeList.Count; x++)//Loop through routes, writing to file
                    {
                        if (routeList[x].numDifficulty == z)
                        {
                            if (routeList[x].charDifficulty == fake || (z < 10 && fake == 'a'))
                            {
                                string route = x.ToString();
                                route += "~";
                                route += routeList[x].nickname;
                                route += "~";
                                route += routeList[x].numDifficulty.ToString();
                                route += "~";

                                if (routeList[x].numDifficulty > 9)
                                    route += routeList[x].charDifficulty;
                                route += "~";
                                route += routeList[x].creator.ToString();
                                route += "~";
                                route += routeList[x].dateCreated;
                                route += "~";
                                route += routeList[x].active.ToString(); //Adding functionality to retire a route
                                route += "~";
                                for (int j = 0; j < 30; j++)
                                {
                                    for (int i = 0; i < 15; i++)
                                    {
                                        route += routeList[x].routeHolds[i, j];
                                    }
                                }
                                route += "~";
                                route += routeList[x].note;
                                path = Application.StartupPath + @"\routes.routes";
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                                {
                                    file.WriteLine(route);
                                }
                            }
                        }
                    }
                }

            }
            routeList.Clear();
            lBRoutes.Items.Clear();
            buildRoutes();
            updateRoutes();
        }

        public void updateRoutes()
        {
            lBRoutes.Items.Clear();

            if (sortByDifficulty)
            {
                for (int i = 0; i < routeList.Count; i++)
                {
                    if ((cbActive.Checked && routeList[i].active) || !cbActive.Checked)
                        lBRoutes.Items.Add(routeList[i].ToString());
                }
            }
            else
            {
                List<Route> dateList = routeList.OrderByDescending(x => DateTime.Parse(x.dateCreated)).ToList();

                for (int i = 0; i < dateList.Count; i++)
                {
                    if ((cbActive.Checked && dateList[i].active) || !cbActive.Checked)
                        lBRoutes.Items.Add(dateList[i].ToString());
                }
            }

        }

        private void WallSelection_ResizeEnd(object sender, EventArgs e)
        {
            int height = this.Height / 31;
            int aheight = this.Height / 31;
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                    btn.Size = new Size(height, height);
                    int locx, locy;
                    locx = (i + 1) * (this.Height / 31);
                    locy = (j + 1) * (this.Height / 31);
                    btn.Location = new Point(locx, locy);

                }
            }
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Button btn = addRoutePanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                    btn.Size = new Size(aheight, aheight);
                    int locx, locy;
                    locx = (i + 1) * (this.Height / 31);
                    locy = (j + 1) * (this.Height / 31);
                    btn.Location = new Point(locx, locy);

                }
            }

            for (int i = 1; i <= 15; i++)//Labels on top
            {
                Label label = routesPanel.Controls.Find("lbl," + i + ",0", true).First() as Label;
                label.Size = new Size(height, height);
                int locx, locy;
                locx = i * (this.Height / 31);
                locy = 0;
                label.Location = new Point(locx, locy);
            }
            for (int i = 1; i <= 30; i++)//Labels on side
            {
                Label label = routesPanel.Controls.Find("lbl,0," + i, true).First() as Label;
                label.Size = new Size(height, height);
                int locx, locy;
                locx = 0;
                locy = i * (this.Height / 31);
                label.Location = new Point(locx, locy);
            }

            for (int i = 1; i <= 15; i++)//Labels on top
            {
                Label label = addRoutePanel.Controls.Find("lbl," + i + ",0", true).First() as Label;
                label.Size = new Size(aheight, aheight);
                int locx, locy;
                locx = i * (this.Height / 31);
                locy = 0;
                label.Location = new Point(locx, locy);
            }
            for (int i = 1; i <= 30; i++)//Labels on side
            {
                Label label = addRoutePanel.Controls.Find("lbl,0," + i, true).First() as Label;
                label.Size = new Size(aheight, aheight);
                int locx, locy;
                locx = 0;
                locy = i * (this.Height / 31);
                label.Location = new Point(locx, locy);
            }

            //label1.Text = WallView.Height.ToString();
            //label2.Text = WallView.Width.ToString();
            //int w = WallView.Height;
            //w = w * 15;
            // w = w / 30;
            // WallView.Width = w;
        }

        private void newHoldHover(object sender, System.EventArgs e)
        {
            //System.EventArgs me = (System.EventArgs)e;
            Button button = (Button)sender;
            int x, y, sIndex, eIndex;
            sIndex = button.Name.IndexOf(',');
            eIndex = button.Name.IndexOf(',', sIndex + 1);
            x = Int32.Parse(button.Name.Substring(sIndex + 1, eIndex - (sIndex + 1)));
            y = Int32.Parse(button.Name.Substring(eIndex + 1, button.Name.Length - (eIndex + 1)));
            //Button btn = addRoutePanel.Controls.Find("btn," + x + "," + y, true).First() as Button;

            for (int i = 0; i < OpcConstants.StrandLength; i++)
            {
                sendLightToSerial(i, 0, 0, 0); //Clear all lights
            }

            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    if (newRoute[i, j] == '1')
                    {
                        sendLightToSerial(lightCrosswalk[j,i], 0, 255, 0); //If it's a regular hold
                    }
                    else if (newRoute[i, j] == '2')
                    {
                        sendLightToSerial(lightCrosswalk[j,i], 255, 0, 0); //If it's a start hold
                    }
                    else if (newRoute[i, j] == '3')
                    {
                        sendLightToSerial(lightCrosswalk[j,i], 255, 255, 255); //If it's a foot only
                    }
                }
            }
            sendLightToSerial(lightCrosswalk[y,x], 255, 0, 255); //Hold being hovered

            wallState.Clear();
        }

        private void sendLightToSerial(int light, int red, int green, int blue)
        {
            byte[] buf = new byte[5];
            byte[] lightBuf = new byte[2];
            ushort checksum = 0;
            lightBuf = BitConverter.GetBytes(light);

            buf[0] = (byte)lightBuf[0];
            buf[1] = (byte)lightBuf[1];
            buf[2] = (byte)red;
            buf[3] = (byte)green;
            buf[4] = (byte)blue;

            checksum = 0;
            foreach (byte b in buf)
            {
                checksum += b;
            }

            serialPort.Write(buf, 0, buf.Length);
            serialPort.Write(BitConverter.GetBytes(checksum), 0, 2);
        }

        private void lBRouteSelectionClick(object sender, EventArgs e)
        {
            startHolds.Clear();
            regularHolds.Clear();
            aboveHolds.Clear();
            dualHolds.Clear();
            wallState.Clear();
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                    btn.BackColor = Color.FromArgb(190, 190, 190);
                }
            }
            int routedifficulty = 1;
            for (int x = 0; x < routeList.Count; x++)
            {
                if (lBRoutes.SelectedItem != null)
                {
                    if (routeList[x].ToString() == lBRoutes.SelectedItem.ToString())
                    {
                        routedifficulty = routeList[x].numDifficulty;
                        for (int i = 0; i < 15; i++)
                        {
                            for (int j = 0; j < 30; j++)
                            {
                                if (routeList[x].routeHolds[i, j] == '1') //If there's a hold above
                                {
                                    if (j < 29)//If it's not the bottom row
                                    {
                                        if (routeList[x].routeHolds[i, j + 1] == '1')//If the hold below is also a hold
                                        {
                                            Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                            if (routedifficulty == 6 || routedifficulty == 7 || routedifficulty == 8)
                                                btn.BackColor = Color.Green;
                                            else if (routedifficulty == 9 || routedifficulty == 10)
                                                btn.BackColor = Color.Yellow;
                                            else
                                                btn.BackColor = Color.Red;
                                            dualHolds.Add(lightCrosswalk[j, i]);
                                        }
                                        else//No hold below, normal hold
                                        {
                                            Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                            if (routedifficulty == 6 || routedifficulty == 7 || routedifficulty == 8)
                                                btn.BackColor = Color.Green;
                                            else if (routedifficulty == 9 || routedifficulty == 10)
                                                btn.BackColor = Color.Yellow;
                                            else
                                                btn.BackColor = Color.Red;
                                            regularHolds.Add(lightCrosswalk[j, i]);
                                        }
                                    }
                                    else//hold above and it's the bottom row
                                    {
                                        Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                        if (routedifficulty == 6 || routedifficulty == 7 || routedifficulty == 8)
                                            btn.BackColor = Color.Green;
                                        else if (routedifficulty == 9 || routedifficulty == 10)
                                            btn.BackColor = Color.Yellow;
                                        else
                                            btn.BackColor = Color.Red;
                                        regularHolds.Add(lightCrosswalk[j, i]);
                                    }
                                }
                                else if (routeList[x].routeHolds[i, j] == '2')//Start hold/finish hold
                                {
                                    Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                    btn.BackColor = Color.Blue;
                                    startHolds.Add(lightCrosswalk[j, i]);
                                }
                                else if (routeList[x].routeHolds[i, j] == '3')//Only Foot
                                {
                                    Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                    btn.BackColor = Color.Black;
                                    aboveHolds.Add(lightCrosswalk[j, i]);
                                }
                                else if (j < 29)//Not the bottom row
                                {
                                    if (routeList[x].routeHolds[i, j + 1] == '1' || routeList[x].routeHolds[i, j + 1] == '2')//If there's a hold below
                                    {
                                        Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                        aboveHolds.Add(lightCrosswalk[j, i]);
                                    }
                                }

                            }
                        }
                        lblInfoRouteName.Text = routeList[x].nickname;
                        if (routedifficulty < 10)
                            lblInfoDifficulty.Text = "5." + routeList[x].numDifficulty.ToString();
                        else
                            lblInfoDifficulty.Text = "5." + routeList[x].numDifficulty.ToString() + routeList[x].charDifficulty;
                        lblInfoCreator.Text = "Set by " + climberList[routeList[x].creator].firstName + " " + climberList[routeList[x].creator].lastName + "\n on " + routeList[x].dateCreated;
                        lblInfoNotes.Text = routeList[x].note;
                        break;
                    }
                }
            }
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < OpcConstants.StrandLength; j++)
                {
                    if (regularHolds.Contains(j))
                    {
                        if (routedifficulty == 6 || routedifficulty == 7 || routedifficulty == 8)
                            wallState.Add(new Pixel(0, 255, 0));
                        else if (routedifficulty == 9 || routedifficulty == 10)
                            wallState.Add(new Pixel(255, 255, 0));
                        else
                            wallState.Add(new Pixel(255, 0, 0));
                    }
                    else if (startHolds.Contains(j))
                    {
                        wallState.Add(new Pixel(0, 0, 255));
                    }
                    else if (aboveHolds.Contains(j))
                    {
                        wallState.Add(new Pixel(100, 100, 88));
                    }
                    else if (dualHolds.Contains(j))
                    {
                        wallState.Add(new Pixel(255, 0, 255));
                    }
                    else
                    {
                        wallState.Add(new Pixel(0, 0, 0));
                    }
                }
            }
            if (wallState.Count > 0)
            {
                //lightControl.WriteFrame(wallState);
            }
        }

        private int convertDifficulty(int numDiff, char charDiff)
        {
            if (numDiff < 10)
                return numDiff - 5;
            else if (numDiff >= 10)
            {
                int charAdd = charDiff - 'a';
                return ((numDiff - 5) + ((numDiff - 10) * 3) + charAdd);
            }
            else
                return 0;
        }

        private string convertDifficultyBack(int numDiff)
        {
            if (numDiff < 5)
                return (numDiff + 5).ToString();
            else if (numDiff >= 5)
            {
                int charSubtract = (numDiff - 1) % 4;
                numDiff -= charSubtract;
                numDiff = (numDiff + 10) / 3;
                char charDiff = Convert.ToChar(charSubtract + 'a');
                return ((numDiff + 5).ToString() + charDiff);
            }
            else
                return "0";
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pBParty_Click(object sender, EventArgs e)
        {
            if (abortLightThread)
            {
                hideHome();
                showParty();
                abortLightThread = false;
                var lightManager = new Thread(ManageParty);
                lightManager.IsBackground = true;
                lightManager.Start();
            }
            else
            {
                abortLightThread = true;
            }

        }

        private void ManageParty()
        {
            var lightThread = new Thread(RainbowLights);
            lightThread.IsBackground = true;
            lightThread.Start();

            while (!abortLightThread)
            {
                Thread.Sleep(50);
            }
            lightThread.Abort();
            lightThread.Join();

        }

        private Color getRGBFromHue(double hue)
        {
            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            double value = 1 * 255;
            var v = Convert.ToInt32(value);
            var p = Convert.ToInt32(value * (1 - 1));
            var q = Convert.ToInt32(value * (1 - f * 1));
            var t = Convert.ToInt32(value * (1 - (1 - f) * 1));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private void RainbowLights()
        {
            //init frame

            //lightControl.SetDitheringAndInterpolation(true);
            var pixels = new Queue<Pixel>();
            List<int> ring1 = new List<int> { 423 };
            List<int> ring2 = new List<int> { 399, 422, 421, 397, 426, 425, 398, 424 };
            List<int> ring3 = new List<int> { 336, 400, 419, 420, 356, 335, 357, 334, 358, 333, 359, 332, 396, 427, 428, 360 };
            List<int> ring4 = new List<int> { 273, 337, 401, 418, 417, 355, 295, 272, 296, 271, 297, 270, 298, 269, 299, 268, 300, 267, 331, 395, 430, 429, 361, 301 };
            List<int> ring5 = new List<int> { 210, 274, 338, 402, 415, 416, 354, 294, 234, 209, 235, 208, 236, 207, 237, 206, 238, 205, 239, 204, 240, 203, 241, 202, 266, 330, 394, 431, 432, 362, 302, 242 };
            List<int> ring6 = new List<int> { 147, 211, 275, 339, 403, 414, 413, 353, 293, 233, 173, 146, 174, 145, 175, 144, 176, 143, 177, 142, 178, 141, 179, 140, 180, 139, 181, 138, 182, 137, 201, 265, 329, 393, 434, 433, 363, 303, 243, 183 };
            List<int> ring7 = new List<int> { 84, 148, 212, 276, 340, 404, 411, 412, 352, 292, 232, 172, 112, 83, 113, 82, 114, 81, 115, 80, 116, 79, 117, 78, 118, 77, 119, 76, 120, 75, 121, 74, 122, 73, 123, 72, 136, 200, 264, 328, 392, 435, 383, 364, 304, 244, 184, 124 };
            List<int> ring8 = new List<int> { 21, 85, 149, 213, 277, 341, 405, 410, 409, 351, 291, 231, 171, 111, 51, 20, 52, 19, 53, 18, 54, 17, 55, 16, 56, 15, 57, 14, 58, 13, 59, 12, 60, 11, 61, 10, 62, 9, 63, 8, 125, 7, 71, 135, 199, 263, 327, 391, 436, 382, 365, 305, 245, 185, 186, 126 };
            List<int> ring9 = new List<int> { 22, 86, 150, 214, 278, 342, 406, 407, 408, 350, 290, 230, 170, 110, 50, 6, 70, 134, 198, 262, 326, 390, 437, 381, 366, 306, 246, 247, 187, 127 };
            List<int> ring10 = new List<int> { 23, 87, 151, 215, 279, 343, 344, 345, 346, 349, 289, 229, 169, 109, 49, 5, 69, 133, 197, 261, 325, 389, 438, 380, 367, 307, 308, 248, 188, 189 };
            List<int> ring11 = new List<int> { 24, 88, 152, 216, 280, 281, 282, 283, 347, 348, 288, 228, 168, 108, 48, 4, 68, 132, 196, 260, 324, 388, 439, 379, 368, 369, 309, 249, 250, 190 };
            List<int> ring12 = new List<int> { 25, 89, 153, 217, 218, 219, 220, 284, 285, 286, 287, 227, 167, 107, 47, 3, 67, 131, 195, 259, 323, 387, 440, 378, 377, 370, 310, 311, 251, 191 };
            List<int> ring13 = new List<int> { 26, 90, 154, 155, 156, 157, 221, 222, 223, 224, 225, 226, 166, 106, 46, 2, 66, 130, 194, 258, 322, 386, 441, 442, 376, 371, 319, 312, 252, 253 };
            List<int> ring14 = new List<int> { 27, 91, 92, 93, 94, 158, 159, 160, 161, 162, 163, 164, 165, 105, 45, 1, 65, 129, 193, 257, 321, 385, 446, 443, 375, 372, 318, 313, 314, 254 };
            List<int> ring15 = new List<int> { 28, 29, 30, 31, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 44, 0, 64, 128, 192, 256, 320, 384, 445, 444, 374, 373, 317, 316, 315, 255 };
            var rings = new List<List<int>> { ring1, ring2, ring3, ring4, ring5, ring6, ring7, ring8, ring9, ring10, ring11, ring12, ring13, ring14, ring15 };
            double hue = 360;
            List<double> hues = new List<double>();
            for (int i = 0; i < rings.Count; i++)
                hues.Add(new double());
            for (int i = 0; i < rings.Count; i++)
            {
                hues[i] = hue - ((i + 1) * (360 / rings.Count));
            }
            Pixel pixel;
            for (int x = 0; x < 447; x++)
            {
                for (int i = 0; i < rings.Count; i++)
                {
                    if (rings[i].Contains(x))
                    {
                        hue = hues[i];
                        break;
                    }
                    if (i == rings.Count - 1)
                        hue = -1;
                }

                if (hue < 0)
                    pixel = new Pixel(0, 0, 0);
                else
                    pixel = Pixel.PixelFromHsv(hue);
                pixels.Enqueue(pixel);
            }

            //loop
            while (true)
            {
                try
                {
                    pixels.Clear();
                    for (int i = 0; i < hues.Count; i++)
                    {
                        hues[i] = (hues[i] + (360 / rings.Count)) % 360;
                    }
                    for (int x = 0; x < 447; x++)
                    {
                        for (int i = 0; i < rings.Count; i++)
                        {
                            if (rings[i].Contains(x))
                            {
                                hue = hues[i];
                                break;
                            }
                            if (i == rings.Count - 1)
                                hue = -1;
                        }

                        if (hue < 0)
                            pixel = new Pixel(0, 0, 0);
                        else
                            pixel = Pixel.PixelFromHsv(hue);
                        pixels.Enqueue(pixel);
                    }
                    //lightControl.WriteFrame(pixels.ToList());
                    //Console.WriteLine("In light thread");
                    Thread.Sleep(135);
                }
                catch
                {
                    Console.WriteLine("Light thread abort");
                    break;
                }
            }
        }

        private void pBExit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void pBEditRoute_Click(object sender, EventArgs e)
        {
            if (lBRoutes.SelectedItem != null)
            {
                for (int x = 0; x < routeList.Count; x++)
                {
                    if (routeList[x].ToString() == lBRoutes.SelectedItem.ToString())
                    {
                        editedRoute = x;
                        clearNewRoute();
                        hideRoutes();
                        showAddRoute();
                        updateSetters();
                        int routedifficulty = routeList[x].numDifficulty;

                        for (int i = 0; i < 15; i++)
                        {
                            for (int j = 0; j < 30; j++)
                            {

                                newRoute[i, j] = routeList[x].routeHolds[i, j];
                                if (routeList[x].routeHolds[i, j] == '1')
                                {
                                    Button btn = addRoutePanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                    btn.BackColor = Color.Green;
                                }
                                else if (routeList[x].routeHolds[i, j] == '2')//Start hold/finish hold
                                {
                                    Button btn = addRoutePanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                    btn.BackColor = Color.Red;
                                }
                                else if (routeList[x].routeHolds[i, j] == '3')//Start hold/finish hold
                                {
                                    Button btn = addRoutePanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                    btn.BackColor = Color.Black;
                                }
                            }
                        }
                        tBRouteName.Text = routeList[x].nickname;
                        if (routedifficulty < 10)
                            tbRouteDifficulty.Text = routeList[x].numDifficulty.ToString();
                        else
                            tbRouteDifficulty.Text = routeList[x].numDifficulty.ToString() + routeList[x].charDifficulty;
                        lBSetterNames.SetSelected(routeList[x].creator, true);
                        tBNotes.Text = routeList[x].note;
                        cBRouteEditActive.Checked = routeList[x].active;
                    }
                }
            }
        }

        private void pBSnake_Click(object sender, EventArgs e)
        {
            hideHome();
            showParty();
            DoSnakeGame();
        }

        private void DoSnakeGame()
        {
            new Settings();

            gameTimer.Interval = 3000 / Settings.Speed;
            if (firstPlay)
            {
                gameTimer.Tick += UpdateScreen;
                firstPlay = false;
            }
            gameTimer.Start();

            Snake.Clear();
            Circle head = new Circle { X = 1, Y = 2 };
            Snake.Add(head);

            GenerateFood();
        }

        //Place random food object
        private void GenerateFood()
        {
            int maxXPos = 15;
            int maxYPos = 30;
            int X = 0;
            int Y = 0;
            bool validLocation = false;
            Random random = new Random();
            while (!validLocation)
            {
                validLocation = true;
                X = random.Next(0, maxXPos);
                Y = random.Next(0, maxYPos);
                for (int i = 0; i < Snake.Count; i++)
                {
                    if (Snake[i].X == X && Snake[i].Y == Y)
                    {
                        validLocation = false;
                        break;
                    }
                }
            }




            food = new Circle { X = X, Y = Y };
            DrawSnake();
        }

        private void UpdateScreen(object sender, EventArgs e)
        {
            //Check for Game Over
            if (Settings.GameOver)
            {
            }
            else
            {
                if (Input.KeyPressed(Keys.D) && Settings.direction != Direction.Left)
                    Settings.direction = Direction.Right;
                else if (Input.KeyPressed(Keys.A) && Settings.direction != Direction.Right)
                    Settings.direction = Direction.Left;
                else if (Input.KeyPressed(Keys.W) && Settings.direction != Direction.Down)
                    Settings.direction = Direction.Up;
                else if (Input.KeyPressed(Keys.S) && Settings.direction != Direction.Up)
                    Settings.direction = Direction.Down;

                MovePlayer();
            }

            //Update Buttons
            DrawSnake();

        }

        private void DrawSnake()
        {
            if (!Settings.GameOver)
            {
                snakeBody.Clear();
                for (int i = 0; i < 15; i++)
                {
                    for (int j = 0; j < 30; j++)
                    {
                        Button btn = partyPanel.Controls.Find("btnParty," + i + "," + j, true).First() as Button;
                        if (i == Snake[0].X)
                        {
                            if (j == Snake[0].Y)
                            {
                                btn.BackColor = Color.Green;
                                snakeHead = lightCrosswalk[j, i];
                            }
                            else if (i == food.X)
                            {
                                if (j == food.Y)
                                {
                                    btn.BackColor = Color.Red;
                                    snakeFood = lightCrosswalk[j, i];
                                }
                                else
                                {
                                    btn.BackColor = Color.White;
                                }
                            }
                            else
                            {
                                btn.BackColor = Color.White;
                            }
                        }
                        else if (i == food.X)
                        {
                            if (j == food.Y)
                            {
                                btn.BackColor = Color.Red;
                                snakeFood = lightCrosswalk[j, i];
                            }
                            else
                            {
                                btn.BackColor = Color.White;
                            }
                        }
                        else
                        {
                            btn.BackColor = Color.White;
                        }
                        for (int x = Snake.Count - 1; x > 0; x--)
                        {
                            if (i == Snake[x].X)
                            {
                                if (j == Snake[x].Y)
                                {
                                    btn.BackColor = Color.Gray;
                                    snakeBody.Add(lightCrosswalk[j, i]);
                                }
                            }
                        }

                    }
                }
                wallState.Clear();

                for (int j = 0; j < OpcConstants.StrandLength; j++)
                {
                    if (snakeHead == j)
                    {
                        wallState.Add(new Pixel(0, 255, 0));
                    }
                    else if (snakeFood == j)
                    {
                        wallState.Add(new Pixel(255, 0, 0));
                    }
                    else if (snakeBody.Contains(j))
                    {
                        wallState.Add(new Pixel(0, 255, 255));
                    }
                    else
                        wallState.Add(new Pixel(0, 0, 0));
                }
                //lightControl.WriteFrame(wallState);
                wallState.Clear();
            }
            else
            {
                string gameOver = "Game over \nYour final score is: " + Settings.Score + "\nPress Enter to try again";
            }
        }

        private void MovePlayer()
        {
            for (int i = Snake.Count - 1; i >= 0; i--)
            {
                //Move head
                if (i == 0)
                {
                    switch (Settings.direction)
                    {
                        case Direction.Right:
                            Snake[i].X++;
                            break;
                        case Direction.Left:
                            Snake[i].X--;
                            break;
                        case Direction.Up:
                            Snake[i].Y--;
                            break;
                        case Direction.Down:
                            Snake[i].Y++;
                            break;
                    }


                    //Get maximum X and Y Pos
                    int maxXPos = 15;
                    int maxYPos = 30;

                    //Detect collission with game borders.
                    if (Snake[i].X < 0 || Snake[i].Y < 0
                        || Snake[i].X >= maxXPos || Snake[i].Y >= maxYPos)
                    {
                        Die();
                    }


                    //Detect collission with body
                    for (int j = 1; j < Snake.Count; j++)
                    {
                        if (Snake[i].X == Snake[j].X &&
                           Snake[i].Y == Snake[j].Y)
                        {
                            Die();
                        }
                    }

                    //Detect collision with food piece
                    if (Snake[0].X == food.X && Snake[0].Y == food.Y)
                    {
                        Eat();
                    }

                }
                else
                {
                    //Move body
                    Snake[i].X = Snake[i - 1].X;
                    Snake[i].Y = Snake[i - 1].Y;
                }
            }
        }

        private void WallSelection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D && Settings.direction != Direction.Left)
                Settings.direction = Direction.Right;
            else if (e.KeyCode == Keys.A && Settings.direction != Direction.Right)
                Settings.direction = Direction.Left;
            else if (e.KeyCode == Keys.W && Settings.direction != Direction.Down)
                Settings.direction = Direction.Up;
            else if (e.KeyCode == Keys.S && Settings.direction != Direction.Up)
                Settings.direction = Direction.Down;
        }

        private void WallSelection_KeyUp(object sender, KeyEventArgs e)
        {
            //Input.ChangeState(e.KeyCode, false);
        }

        private void Eat()
        {
            //Add circle to body
            Circle circle = new Circle
            {
                X = Snake[Snake.Count - 1].X,
                Y = Snake[Snake.Count - 1].Y
            };
            Snake.Add(circle);

            //Update Score
            Settings.Score += Settings.Points;

            GenerateFood();
        }

        private void Die()
        {
            Settings.GameOver = true;
            gameTimer.Stop();
            Settings.Width = 16;
            Settings.Height = 16;
            Settings.Speed = 16;
            Settings.Score = 0;
            Settings.Points = 100;
            Settings.direction = Direction.Down;

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Die();
            hideParty();
            showHome();
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            if (sortByDifficulty)
            {
                sortByDifficulty = false;
                btnSort.Text = "Sort By Difficulty";
                updateRoutes();
            }
            else
            {
                sortByDifficulty = true;
                btnSort.Text = "Sort By Date";
                updateRoutes();
            }
        }

        private void cbActive_CheckedChanged(object sender, EventArgs e)
        {
            updateRoutes();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnRandomRoute_Click(object sender, EventArgs e)
        {
            startHolds.Clear();
            regularHolds.Clear();
            aboveHolds.Clear();
            dualHolds.Clear();
            wallState.Clear();
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                    btn.BackColor = Color.FromArgb(190, 190, 190);
                    randomRoute[i, j] = '0';
                }
            }
            int routedifficulty = 0;
            for (int x = 0; x < routeList.Count; x++)
            {
                if (lBRoutes.SelectedItem != null)
                {
                    if (routeList[x].ToString() == lBRoutes.SelectedItem.ToString())
                    {
                        routedifficulty = convertDifficulty(routeList[x].numDifficulty, routeList[x].charDifficulty);
                        Console.WriteLine("Difficulty: " + routedifficulty);
                        findStartHolds(routedifficulty);
                        addStartFeet(routedifficulty);
                        createRoute(routedifficulty);
                        //findFinishHolds(routedifficulty);

                        for (int i = 0; i < 15; i++)
                        {
                            for (int j = 0; j < 30; j++)
                            {
                                if (randomRoute[i, j] == '1') //If there's a hold above
                                {
                                    if (j < 29)//If it's not the bottom row
                                    {
                                        if (randomRoute[i, j + 1] == '1')//If the hold below is also a hold
                                        {
                                            Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                            if (routedifficulty >= 1 && routedifficulty <= 3)
                                                btn.BackColor = Color.Green;
                                            else if (routedifficulty >= 4 && routedifficulty <= 8)
                                                btn.BackColor = Color.Yellow;
                                            else
                                                btn.BackColor = Color.Red;
                                            dualHolds.Add(lightCrosswalk[j, i]);
                                        }
                                        else//No hold below, normal hold
                                        {
                                            Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                            if (routedifficulty >= 1 && routedifficulty <= 3)
                                                btn.BackColor = Color.Green;
                                            else if (routedifficulty >= 4 && routedifficulty <= 8)
                                                btn.BackColor = Color.Yellow;
                                            else
                                                btn.BackColor = Color.Red;
                                            regularHolds.Add(lightCrosswalk[j, i]);
                                        }
                                    }
                                    else//hold above and it's the bottom row
                                    {
                                        Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                        if (routedifficulty >= 1 && routedifficulty <= 3)
                                            btn.BackColor = Color.Green;
                                        else if (routedifficulty >= 4 && routedifficulty <= 8)
                                            btn.BackColor = Color.Yellow;
                                        else
                                            btn.BackColor = Color.Red;
                                        regularHolds.Add(lightCrosswalk[j, i]);
                                    }
                                }
                                else if (randomRoute[i, j] == '2')//Start hold/finish hold
                                {
                                    Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                    btn.BackColor = Color.Blue;
                                    startHolds.Add(lightCrosswalk[j, i]);
                                }
                                else if (randomRoute[i, j] == '3')//Only Foot
                                {
                                    Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                    btn.BackColor = Color.Black;
                                    aboveHolds.Add(lightCrosswalk[j, i]);
                                }
                                else if (j < 29)//Not the bottom row
                                {
                                    if (randomRoute[i, j + 1] == '1' || randomRoute[i, j + 1] == '2')//If there's a hold below
                                    {
                                        Button btn = routesPanel.Controls.Find("btn," + i + "," + j, true).First() as Button;
                                        aboveHolds.Add(lightCrosswalk[j, i]);
                                    }
                                }

                            }
                        }
                        lblInfoRouteName.Text = "Generated Route";
                        lblInfoDifficulty.Text = "5." + convertDifficultyBack(routedifficulty);
                        lblInfoCreator.Text = "Set by AI";
                        lblInfoNotes.Text = "";
                        break;
                    }
                }
            }
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < OpcConstants.StrandLength; j++)
                {
                    if (regularHolds.Contains(j))
                    {
                        if (routedifficulty >= 1 && routedifficulty <= 3)
                            wallState.Add(new Pixel(0, 255, 0));
                        else if (routedifficulty >= 4 && routedifficulty <= 8)
                            wallState.Add(new Pixel(255, 255, 0));
                        else
                            wallState.Add(new Pixel(255, 0, 0));
                    }
                    else if (startHolds.Contains(j))
                    {
                        wallState.Add(new Pixel(0, 0, 255));
                    }
                    else if (aboveHolds.Contains(j))
                    {
                        wallState.Add(new Pixel(100, 100, 88));
                    }
                    else if (dualHolds.Contains(j))
                    {
                        wallState.Add(new Pixel(255, 0, 255));
                    }
                    else
                    {
                        wallState.Add(new Pixel(0, 0, 0));
                    }
                }
            }
            //lightControl.WriteFrame(wallState);
        }

        public void findStartHolds(int diff)
        {
            bool startHoldSelected = false;
            int i = 0, j = 0;
            Random random = new Random();
            while (!startHoldSelected)
            {
                i = random.Next(0, 14);
                j = random.Next(22, 29);
                if ((Math.Abs(holdDifficultiesHands[i, j] - diff) <= 2) && usedAsStart[i, j])
                {
                    startHolds.Add(lightCrosswalk[j, i]);
                    startHoldSelected = true;
                    randomRoute[i, j] = '2';
                    Console.WriteLine("Start 1 Selected");
                    Console.WriteLine(i + "," + j);
                }
            }
            //Console.WriteLine(i + "," + j);
            int x = 0, y = 0;
            int attempts = 0;
            startHoldSelected = false;
            while (!startHoldSelected)
            {
                x = random.Next(Math.Max(0, i - ((attempts / 600) + 3)), Math.Min(15, i + ((attempts / 600) + 4)));
                y = random.Next(j - 2, Math.Min(30, j + 3));
                if (x != i || y != j)
                {
                    if ((Math.Abs(holdDifficultiesHands[x, y] - diff) <= (2 + attempts/5000)) && usedAsStart[x, y])
                    {
                        if (Math.Abs(x - i) <= 6)
                        {
                            startHolds.Add(lightCrosswalk[y, x]);
                            startHoldSelected = true;
                            randomRoute[x, y] = '2';
                            Console.WriteLine("Start 2 Selected");
                            Console.WriteLine(x + "," + y);
                        }
                    }
                }
                
                attempts++;
                if (attempts % 600 == 0)
                    Console.WriteLine("Attempts: " + attempts);
                else if (attempts % 5000 == 0)
                    Console.WriteLine("Attempts: " + attempts);
            }
            //Console.WriteLine(x + "," + y);
            return;
        }
        public void addStartFeet(int diff)
        {
            int horizontalPlacement = 0, count = 0, verticalPlacement = 0;
            bool footSelected = false;
            int i = 0, j = 0;


            for (j = 0; j < 30; j++)
            {
                for (i = 0; i < 15; i++)
                {
                    if (randomRoute[i, j] == '2')
                    {
                        horizontalPlacement += i;
                        if (j > verticalPlacement)
                            verticalPlacement = j;
                        count++;
                    }
                }
            }
            horizontalPlacement = horizontalPlacement / count;

            Random random = new Random();
            int x = 0, y = 0;
            int attempts = 0;
            while (!footSelected)
            {
                attempts++;
                if (attempts % 600 == 0)
                    Console.WriteLine("Attempts: " + attempts);
                else if (attempts % 5000 == 0)
                    Console.WriteLine("Attempts: " + attempts);
                x = random.Next(Math.Max(0, horizontalPlacement - ((attempts / 600) + 2)), Math.Min(15, horizontalPlacement + ((attempts / 600) + 3)));
                y = random.Next(verticalPlacement + 4, 30);

                if ((Math.Abs(holdDifficultiesHands[x, y] - diff) <= (4 + attempts/5000) &&  //Hand within diff
                    (holdDifficultiesHands[x, y] > 0)) ||//Exist as hand
                    ((Math.Abs(holdDifficultiesFeet[x, y] - diff) <= (4 + attempts / 5000)) && //Foot within diff
                    (holdDifficultiesHands[x, y] > 0)))  //Exist as foot
                {
                    aboveHolds.Add(lightCrosswalk[y, x]);
                    footSelected = true;
                    if (usedAsFoot[x, y])
                        randomRoute[x, y] = '3';
                    else
                        randomRoute[x, y] = '1';
                    Console.WriteLine("Foot 1 Selected");
                    Console.WriteLine(x + "," + y);
                }    
            }
            footSelected = false;
            

            while (!footSelected)
            {
                i = random.Next(Math.Max(0, x - ((attempts / 600) + 3)), Math.Min(15, x + ((attempts / 600) + 4)));
                j = random.Next(y - 3, Math.Min(30, y + 2));
                if ((x != i || y != j) && (!startHolds.Contains(lightCrosswalk[j, i])))
                {
                    if ((Math.Abs(holdDifficultiesHands[i, j] - diff) <= (4 + attempts / 5000) ||  //Hand within diff
                    (Math.Abs(holdDifficultiesFeet[i, j] - diff) <= (4 + attempts / 5000))) && //Foot within diff
                    ((holdDifficultiesHands[i, j] > 0) || //Exist as hand
                    (holdDifficultiesHands[i, j] > 0)))  //Exist as foot
                    {
                        aboveHolds.Add(lightCrosswalk[j, i]);
                        footSelected = true;
                        if (usedAsFoot[i, j])
                            randomRoute[i, j] = '3';
                        else
                            randomRoute[i, j] = '1';
                        Console.WriteLine("Foot 2 Selected");
                        Console.WriteLine(i + "," + j);
                    }
                }
                attempts++;
                if (attempts % 600 == 0)
                    Console.WriteLine("Attempts: " + attempts);
                else if (attempts % 5000 == 0)
                    Console.WriteLine("Attempts: " + attempts);
            }
        }
        public void createRoute(int diff)
        {
            int prev1x = -1, prev1y = -1, prev2x = -1, prev2y = -1;

            for (int j = 0; j < 30; j++)
            {
                for (int i = 0; i < 15; i++)
                {
                    if (randomRoute[i, j] == '2')
                    {
                        if (prev1x == -1) //first start hold
                        {
                            prev1x = i;
                            prev1y = j;
                        }
                        else //second start hold
                        {
                            prev2x = i;
                            prev2y = j;
                        }
                    }
                }
            }

            int higherHold = 0;

            if (prev1y < prev2y) //first hold above second hold
            {
                higherHold = 1;
            }
            else
            {
                higherHold = 2;
            }
            int x = 0, y = 0;
            Random random = new Random();
            bool holdSelected = false, foundFinish = false;
            int attempts = 0;

            while (!foundFinish)
            {
                holdSelected = false;
                attempts = 0;
                if ((higherHold == 1 && prev1x < prev2x) || (higherHold == 2 && prev1x >= prev2x))// left is higher, find right hand
                {
                    Console.WriteLine("Left hand higher, looking for right hand");

                    while (!holdSelected)
                    {
                        //if (attempts > 2000)
                        //return;
                        attempts++;
                        if (attempts % 600 == 0)
                            Console.WriteLine("Attempts: " + attempts);
                        else if (attempts % 5000 == 0)
                            Console.WriteLine("Attempts: " + attempts);
                        if (higherHold == 1)
                        {
                            x = random.Next(Math.Max(0, prev2x - ((attempts / 600) + 3)), Math.Min(15, prev2x + ((attempts / 600) + 4))); //random.Next (prevX - 3, prevX + 4)
                            y = random.Next(Math.Max(0,prev1y - ((attempts / 600) + 2)), Math.Min(30, prev1y + 1));
                        }
                        else
                        {
                            x = random.Next(Math.Max(0, prev1x - ((attempts / 600) + 3)), Math.Min(15, prev1x + ((attempts / 600) + 4))); //random.Next (prevX - 3, prevX + 4)
                            y = random.Next(Math.Max(0, prev2y - ((attempts / 600) + 2)), Math.Min(30, prev2y + 1));
                        }
                        if (!regularHolds.Contains(lightCrosswalk[y, x]) && !startHolds.Contains(lightCrosswalk[y, x]) && !dualHolds.Contains(lightCrosswalk[y,x]))
                        {
                            if (holdDifficultiesHands[x, y] > 0 && (Math.Abs(holdDifficultiesHands[x, y] - diff) <= (2 + (attempts / 5000))))
                            {
                                if (Math.Abs(x - prev1x) <= 6 && Math.Abs(x - prev2x) <= 6)
                                {
                                    if (y <= 2 && !usedAsStart[x, y])
                                        continue;
                                    if (y < 5 && usedAsStart[x, y])
                                    {
                                        startHolds.Add(lightCrosswalk[y, x]);
                                        foundFinish = true;
                                        randomRoute[x, y] = '2';
                                    }
                                    else if (y < 5 && !usedAsStart[x, y])
                                    {
                                        if (regularHolds.Contains(lightCrosswalk[y + 1, x]))
                                            dualHolds.Add(lightCrosswalk[y, x]);
                                        else
                                            regularHolds.Add(lightCrosswalk[y, x]);
                                        randomRoute[x, y] = '1';
                                    }
                                    else
                                    {
                                        if (regularHolds.Contains(lightCrosswalk[y + 1, x]))
                                            dualHolds.Add(lightCrosswalk[y, x]);
                                        else
                                            regularHolds.Add(lightCrosswalk[y, x]);
                                        randomRoute[x, y] = '1';
                                    }
                                    holdSelected = true;
                                    
                                    if (higherHold == 1)
                                    {
                                        prev2x = x;
                                        prev2y = y;
                                        if (prev2y <= prev1y)
                                            higherHold = 2;
                                    }
                                    else
                                    {
                                        prev1x = x;
                                        prev1y = y;
                                        if (prev1y <= prev2y)
                                            higherHold = 1;
                                    }
                                    Console.WriteLine("Right Hand Selected");
                                    Console.WriteLine(x + "," + y);
                                    if (!footFound(x,y,diff))
                                    {
                                        addFoot(x, y, diff);
                                    }
                                }
                            }
                        }
                       
                        //attempts++;
                    }
                }
                else if ((higherHold == 1 && prev1x >= prev2x) || (higherHold == 2 && prev1x < prev2x)) //right is higher, find left hand
                {
                    Console.WriteLine("Right hand higher, looking for Left hand");

                    while (!holdSelected)
                    {
                        //if (attempts > 2000)
                            //return;
                        attempts++;
                        if (attempts % 600 == 0)
                            Console.WriteLine("Attempts: " + attempts);
                        else if (attempts % 5000 == 0)
                            Console.WriteLine("Attempts: " + attempts);
                        if (higherHold == 1)
                        {
                            x = random.Next(Math.Max(0, prev2x - ((attempts / 600) + 3)), Math.Min(15, prev2x + ((attempts / 600) + 4))); //random.Next (prevX - 3, prevX + 4)
                            y = random.Next(Math.Max(0, prev1y - ((attempts / 600) + 2)), Math.Min(30, prev1y + 1));
                        }
                        else
                        {
                            x = random.Next(Math.Max(0, prev1x - ((attempts / 600) + 3)), Math.Min(15, prev1x + ((attempts / 600) + 4))); //random.Next (prevX - 3, prevX + 4)
                            y = random.Next(Math.Max(0, prev2y - ((attempts / 600) + 2)), Math.Min(30, prev2y + 1));
                        }
                        if (!regularHolds.Contains(lightCrosswalk[y, x]) && !startHolds.Contains(lightCrosswalk[y, x]) && !dualHolds.Contains(lightCrosswalk[y, x]))
                        {
                            if (holdDifficultiesHands[x,y] > 0 && (Math.Abs(holdDifficultiesHands[x, y] - diff) <= (2 + (attempts / 5000))))
                            {
                                if (Math.Abs(x - prev1x) <= 6 && Math.Abs(x - prev2x) <= 6)
                                {
                                    if (y < 5 && usedAsStart[x, y])
                                    {
                                        startHolds.Add(lightCrosswalk[y, x]);
                                        foundFinish = true;
                                        randomRoute[x, y] = '2';
                                    }
                                    else if (y < 5 && !usedAsStart[x, y])
                                    {
                                        if (regularHolds.Contains(lightCrosswalk[y + 1, x]))
                                            dualHolds.Add(lightCrosswalk[y, x]);
                                        else
                                            regularHolds.Add(lightCrosswalk[y, x]);
                                        randomRoute[x, y] = '1';
                                    }
                                    else
                                    {
                                        if (regularHolds.Contains(lightCrosswalk[y + 1, x]))
                                            dualHolds.Add(lightCrosswalk[y, x]);
                                        else
                                            regularHolds.Add(lightCrosswalk[y, x]);
                                        randomRoute[x, y] = '1';
                                    }
                                    holdSelected = true;
                                    if (higherHold == 1)
                                    {
                                        prev2x = x;
                                        prev2y = y;
                                        if (prev2y <= prev1y)
                                            higherHold = 2;
                                    }
                                    else
                                    {
                                        prev1x = x;
                                        prev1y = y;
                                        if (prev1y <= prev2y)
                                            higherHold = 1;
                                    }
                                    Console.WriteLine("Left Hand Selected");
                                    Console.WriteLine(x + "," + y);
                                    if (!footFound(x, y, diff))
                                    {
                                        addFoot(x, y, diff);
                                    }
                                }
                            }
                        }
                        
                        //attempts++;
                    }
                }
            }
        }
        bool footFound(int x, int y, int diff)
        {
            for (int i = Math.Max(x-2,0); i <= Math.Min(x+2,14); i++)
            {
                for (int j = Math.Min(y+4,29); j <= Math.Min(y+6,29); j++)
                {
                    if (randomRoute[i, j] != '0')
                    {
                        Console.WriteLine("Found suitable foot");
                        Console.WriteLine(i + "," + j);
                        return true;
                    }
                        
                }
            }
            Console.WriteLine("No suitable foot found!");
            return false;
        }
        void addFoot(int x, int y, int diff)
        {
            Random random = new Random();
            bool footSelected = false;
            int attempts = 0;
            int i, j;

            while (!footSelected)
            {
                i = random.Next(Math.Max(0, x - ((attempts / 2000) + 2)), Math.Min(15, x + ((attempts / 2000) + 3))); //random.Next (prevX - 3, prevX + 4)
                j = random.Next(Math.Min(29, y + ((attempts / 2000) + 4)), Math.Min(30, y + ((attempts / 2000) + 7)));

                if (holdDifficultiesHands[i, j] > 0 && (Math.Abs(holdDifficultiesHands[i, j] - diff) <= (2 + (attempts / 500))) ||
                   (holdDifficultiesFeet [i, j] > 0 && (Math.Abs(holdDifficultiesFeet [i, j] - diff) <= (2 + (attempts / 500))) && usedAsFoot[i, j]))
                {
                    if (usedAsFoot[i, j])
                    {
                        randomRoute[i, j] = '3';
                        aboveHolds.Add(lightCrosswalk[j, i]);
                    }
                    else
                    {
                        randomRoute[i, j-1] = '3';
                        aboveHolds.Add(lightCrosswalk[j-1, i]);
                    }
                    footSelected = true;
                    Console.WriteLine("Added foot");
                    Console.WriteLine(i + "," + j);
                }
                attempts++;
            }
        }
        public class serialSend
        {
            public void WriteFrame(List<Pixel> pixels, int channel = 0)
            {
                for (int i = 0; i < pixels.Count; i++)
                {
                    serialPort.WriteLine(i + "," + pixels[i].Red + "," + pixels[i].Green +","+ pixels[i].Blue + ",");
                }
            }
        };

        private void WallSelection_FormClosed(object sender, FormClosedEventArgs e)
        {
            serialPort.Close();
        }
    }
}

public class ClimbWall
{
    public List<Route> routeList = new List<Route>();
    //public Color[,] holdArray = new Color[15,30]; //SET HOLD NUMBERS HERE************SET HOLD NUMBERS HERE************SET HOLD NUMBERS HERE************SET HOLD NUMBERS HERE************
    public Queue<Climber> climberQueue = new Queue<Climber>();
};

public class Climber
{
    public int id;
    public string firstName;
    public string lastName;
    public bool belayCert;
    public int highestDifficulty;
    public int averageDifficulty;
    public List<Route> routesCreated = new List<Route>();
    public List<Route> routesClimbed = new List<Route>();
    public override string ToString()
    {
        return firstName + " " + lastName;
    }
};

public class Route
{
    public int id;
    public string nickname;
    public string difficulty;
    public int numDifficulty;
    public char charDifficulty;
    public string dateCreated;
    public string type;
    public int creator;
    public bool active;
    public string note;
    public char[,] routeHolds = new char[15, 30]; //SET HOLD NUMBERS HERE************SET HOLD NUMBERS HERE************SET HOLD NUMBERS HERE************SET HOLD NUMBERS HERE************
    public override string ToString()
    {
        if (numDifficulty > 9)
            return "5." + numDifficulty + charDifficulty + " " + nickname;
        else
            return "5." + numDifficulty + "     " + nickname;
    }
};

