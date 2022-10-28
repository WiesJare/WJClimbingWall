using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wungle
{
    /// <summary>
    /// Interaction logic for HomeScreen.xaml
    /// </summary>
    public partial class HomeScreen : Page
    {
        public HomeScreen()
        {
            InitializeComponent();
        }

        public static ClimbWall LightWall = new ClimbWall();
        public List<Climber> climberList = new List<Climber>();
        public List<Route> routeList = new List<Route>();
        //OpcClient lightControl = new OpcClient();
        //List<Pixel> wallState = new List<Pixel>();
        List<int> regularHolds = new List<int>();
        List<int> startHolds = new List<int>();
        List<int> aboveHolds = new List<int>();
        List<int> dualHolds = new List<int>();
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


        private void BtnRoutes_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RouteScreen());
        }

        private void BtnParty_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PartyScreen());
        }

        private void BtnClimber_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ClimberScreen());
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