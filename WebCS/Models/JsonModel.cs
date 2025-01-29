// 所需的数据格式
public class GeneralData
{
    public string Type { get; set; } // 数据类型标识，例如 "ClassA" 或 "ClassB"
    public object Data { get; set; } // 数据内容
}

public class ReceiveData
{
    public string status { get; set; } // 数据状态
    public string remark { get; set; } // 数据结构解释
    public string[] lineName { get; set; } // 线路名称
    public string[] period { get; set; } // 时段名称
    public string[] sectionName { get; set; } // 运行区段名称
    public string[] direction { get; set; } // 方向名称
    public int[][] interval { get; set; } // 线路在各个时段上的发车间隔

    public ReceiveData()
    {
        status = "Success";
        remark = "处理的输入数据格式是一个int[][]，表示每条线路在每个时段的发车间隔，4*17的大小，其中17代表线路编号按照下面的lineName属性组织，4代表时段数量按照period属性组织，第一个时段是早高峰,6:30-10:30，后面时段依次为10:30-17:00，17:00-20:30，20:30-24:00";
        lineName = [ "智轨T1主线", "智轨T1支线", "智轨T4主线", "智轨T4支线", "智轨T2线",
                    "智轨T1主线", "智轨T1支线", "智轨T4主线", "智轨T4支线", "智轨T2线",
                    "9路", "15路", "24路", "32路",
                    "南溪9路", "南溪10路", "南溪12路" ];
        period = ["6:30-10:30", "10:30-17:00", "17:00-20:30", "20:30-24:00"];
        sectionName = ["智轨产业园站-高铁西站", "高铁西站-成都工业学院站", "南溪区政府-智轨产业园站", "智轨产业园站-南溪区政府", "银龙广场站-啤酒广场站", "高铁西站-智轨产业园站", "成都工业学院站-高铁西站", "智轨产业园站-南溪区政府", "南溪区政府-智轨产业园站", "啤酒广场站-银龙广场站",
                            "翠屏山-盐坪坝", "丽雅大院-翠屏山(市妇幼保健院)", "东山路-市政广场(云上装饰)", "纵一路口-宜宾七中", "新中医院西门-东门", "客运中心-月亮湾游客中心", "祥和小区-客运中心"];
        direction = ["上行", "上行", "上行", "上行", "上行", "下行", "下行", "下行", "下行", "下行", "上行", "上行", "上行", "上行", "上行", "上行", "上行"];
        interval = [[10, 10, 15, 25, 15, 10, 10, 15, 25, 15, 20, 11, 15, 10, 14, 17, 24], [15, 15, 20, 30, 20, 15, 15, 20, 30, 20, 25, 16, 20, 15, 19, 22, 29], [10, 10, 15, 25, 15, 10, 10, 15, 25, 15, 20, 11, 15, 10, 14, 17, 24], [15, 15, 20, 30, 20, 15, 15, 20, 30, 20, 25, 16, 20, 15, 19, 22, 29]];
    }
}

public class FilePath
{
    public string Path { get; set; } // 文件路径
    public int ReturnCode { get; set; } // 返回码, 0表示默认全信息返回(包括公交首班车和智轨时刻表)，1表示只返回首班车时间

    public FilePath()
    {
        Path = "C:\\Users\\Public\\Downloads\\data.json";

        ReturnCode = 0;
    }
}

public class TotalBusSchedule
{
    public int[][] TimeInteval { get; set; } // 所有时段的发车间隔，该参数为用户输入
    public int[][] TimeRunning { get; set; } // 所有时段的行驶时间
    public int[] TimeTransfer { get; set; } // 同站换乘时间
    public int[] TotalTimeInterval { get; set; } // 总时间
    public double[][][][] TransferPassengerFlow { get; set; } // 换乘乘客系数，按时段清分
    public int TimeLimit { get; set; } // 站点不可通行的设置时间
    public double SolverTimeLimit { get; set; } // 求解器时间限制
    public DateTime LastModified { get; set; } // 最后修改时间
    public int ModelFlag { get; set; } // 模型标志
    public double StopTime { get; set; } // 停车时间
    public int[][] AllStationRunning { get; set; } // 所有站点的行驶时间
    public string[][] AllStationName { get; set; } // 所有站点的名称

    public void GetTime(string filePath)
    {
        if (File.Exists(filePath))
        {
            // 获取最后修改时间，假设文件系统时间是本地时间
            LastModified = File.GetLastWriteTime(filePath);

            // 转换为北京时间（CST）
            TimeZoneInfo chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            LastModified = TimeZoneInfo.ConvertTime(LastModified, chinaTimeZone);
        }
        else
        {
            LastModified = DateTime.MinValue;
        }
    }

    public TotalBusSchedule()
    {
        TimeInteval = [[10, 10, 15, 25, 15, 10, 10, 15, 25, 15, 20, 11, 15, 10, 14, 17, 24], [15, 15, 20, 30, 20, 15, 15, 20, 30, 20, 25, 16, 20, 15, 19, 22, 29], [10, 10, 15, 25, 15, 10, 10, 15, 25, 15, 20, 11, 15, 10, 14, 17, 24], [15, 15, 20, 30, 20, 15, 15, 20, 30, 20, 25, 16, 20, 15, 19, 22, 29]];
        TimeRunning = [[10000, 10000, 45, 26, 10000, 21, 10000, 13, 10000, 9, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                           [10000, 10000, 19, 24, 10000, 32, 10000, 36, 38, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 54, 30, 14, 10000, 10000, 10000, 10000, 21, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 0, 18, 10000, 76, 64, 68, 10000, 41, 10000, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 3],

                           [10000, 10000, 19, 24, 10000, 32, 10000, 36, 10000, 45, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                           [10000, 10000, 42, 37, 10000, 29, 10000, 25, 23, 10000, 1000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 18, 42, 58, 10000, 10000, 10000, 10000, 51, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 90, 72, 10000, 14, 26, 22, 10000, 49, 10000, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 21],

                           [10000, 17, 22, 10000, 34, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 3],
                           [37, 17, 26, 31, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                           [10000, 10000, 10000, 10000, 61, 53, 51, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 55, 10000, 48, 10000, 21, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 16, 20, 32, 10000, 10000, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 20, 10000, 10000, 10000],
                           [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 20, 28, 10000, 10000, 10000, 10000, 10000, 10000]];
        TimeTransfer = [4, 3, 2, 6, 0, 2, 7, 0, 6, 0, 1, 1, 0, 0, 1, 12, 11, 3, 13];
        TotalTimeInterval = [240, 390, 210, 210]; // 6:30-10:30, 10:30-17:00, 17:00-20:30, 20:30-24:00
        TransferPassengerFlow = new double[TotalTimeInterval.Length][][][];
        for (int i = 0; i < TotalTimeInterval.Length; i++)
        {
            TransferPassengerFlow[i] = new double[TimeInteval[0].Length][][];
            if (i == 0)
            {
                for (int j = 0; j < TimeInteval[0].Length; j++)
                {
                    TransferPassengerFlow[i][j] = new double[TimeInteval[0].Length][];
                    for (int k = 0; k < TimeInteval[0].Length; k++)
                    {
                        TransferPassengerFlow[i][j][k] = new double[TimeRunning[0].Length];
                        for (int l = 0; l < TimeRunning[0].Length; l++)
                        {
                            TransferPassengerFlow[i][j][k][l] = 0;
                        }
                    }
                }
                // 新的方向
                TransferPassengerFlow[i][0][10][2] = 0.4;   //S3  A1u-B1
                TransferPassengerFlow[i][0][11][2] = 0.4;   //S3  A1u-B2
                TransferPassengerFlow[i][10][0][2] = 0.8;   //S3  B1-A1u
                TransferPassengerFlow[i][10][1][2] = 0.8;   //S3  B1-A2u
                TransferPassengerFlow[i][11][0][2] = 0.8;   //S3  B2-A1u
                TransferPassengerFlow[i][11][1][2] = 0.8;   //S3  B2-A2u
                TransferPassengerFlow[i][1][11][3] = 0.8;   //S4  A2u-B2
                TransferPassengerFlow[i][12][0][5] = 0.4;  //S6  B3-A1u
                TransferPassengerFlow[i][12][1][5] = 0.8;  //S6  B3-A2u
                TransferPassengerFlow[i][6][0][7] = 6;     //S8  A2o-A1u
                TransferPassengerFlow[i][1][13][8] = 0.4;  //S9  A2u-B4
                TransferPassengerFlow[i][5][2][9] = 12;    //S10 A1o-A3u
                TransferPassengerFlow[i][13][2][10] = 0.4; //S11 B4-A3u
                TransferPassengerFlow[i][3][13][10] = 1;   //S11 A4u-B4
                TransferPassengerFlow[i][2][16][11] = 0.4; //S12 A3u-B7
                TransferPassengerFlow[i][16][2][11] = 0.8; //S12 B7-A3u
                TransferPassengerFlow[i][16][3][12] = 1.6; //S13 B7-A4u
                TransferPassengerFlow[i][2][15][12] = 0.8; //S13 A3u-B6
                TransferPassengerFlow[i][14][3][13] = 0.4; //S14 B5-A4u
                TransferPassengerFlow[i][3][14][14] = 1.2; //S15 A4u-B5
                TransferPassengerFlow[i][3][16][16] = 2.4; //S17 A4u-B7
                TransferPassengerFlow[i][2][14][17] = 0.8; //S18 A3u-B5
                TransferPassengerFlow[i][4][10][18] = 0.8; //S19 A5u-B1


                // 原方向
                TransferPassengerFlow[i][11][10][0] = 1.2;   //S1  B2-B1
                TransferPassengerFlow[i][11][10][1] = 0.4;   //S2  B2-B1
                TransferPassengerFlow[i][10][11][1] = 0.4;   //S2  B1-B2
                TransferPassengerFlow[i][11][10][2] = 0.4;   //S3  B2-B1
                TransferPassengerFlow[i][10][11][2] = 0.4;   //S3  B1-B2
                TransferPassengerFlow[i][5][10][2] = 0.4;   //S3  A1o-B1
                TransferPassengerFlow[i][5][11][2] = 0.4;   //S3  A1o-B2
                TransferPassengerFlow[i][10][5][2] = 0.8;   //S3  B1-A1o
                TransferPassengerFlow[i][10][6][2] = 0.8;   //S3  B1-A2o
                TransferPassengerFlow[i][11][5][2] = 0.8;   //S3  B2-A1o
                TransferPassengerFlow[i][11][6][2] = 0.8;   //S3  B2-A2o
                TransferPassengerFlow[i][7][11][3] = 0.8;   //S4  A2o-B2
                TransferPassengerFlow[i][12][10][4] = 0.4;  //S5  B3-B1
                TransferPassengerFlow[i][12][5][5] = 0.4;  //S6  B3-A1o
                TransferPassengerFlow[i][12][6][5] = 0.8;  //S6  B3-A2o
                TransferPassengerFlow[i][13][12][6] = 0.4; //S7  B4-B3
                TransferPassengerFlow[i][1][5][7] = 6;     //S8  A2u-A1o
                TransferPassengerFlow[i][6][13][8] = 0.4;  //S9  A2o-B4
                TransferPassengerFlow[i][0][7][9] = 12;    //S10 A1u-A3o
                TransferPassengerFlow[i][13][7][10] = 0.4; //S11 B4-A3o
                TransferPassengerFlow[i][8][13][10] = 1;   //S11 A4o-B4
                TransferPassengerFlow[i][7][16][11] = 0.4; //S12 A3o-B7
                TransferPassengerFlow[i][16][7][11] = 0.8; //S12 B7-A3o
                TransferPassengerFlow[i][16][15][12] = 1.2;//S13 B7-B6
                TransferPassengerFlow[i][16][8][12] = 1.6; //S13 B7-A4o
                TransferPassengerFlow[i][7][15][12] = 0.8; //S13 A3o-B6
                TransferPassengerFlow[i][14][8][13] = 0.4; //S14 B5-A4o
                TransferPassengerFlow[i][8][14][14] = 1.2; //S15 A4o-B5
                TransferPassengerFlow[i][14][15][15] = 0.4;//S16 B5-B6
                TransferPassengerFlow[i][8][16][16] = 2.4; //S17 A4o-B7
                TransferPassengerFlow[i][7][14][17] = 0.8; //S18 A3o-B5
                TransferPassengerFlow[i][9][10][18] = 0.8; //S19 A5o-B1

            }
            else if (i == 1)
            {
                TransferPassengerFlow[i] = new double[TimeInteval[0].Length][][];
                for (int j = 0; j < TimeInteval[0].Length; j++)
                {
                    TransferPassengerFlow[i][j] = new double[TimeInteval[0].Length][];
                    for (int k = 0; k < TimeInteval[0].Length; k++)
                    {
                        TransferPassengerFlow[i][j][k] = new double[TimeRunning[0].Length];
                        for (int l = 0; l < TimeRunning[0].Length; l++)
                        {
                            if (TransferPassengerFlow[0][j][k][l] == 0)
                            {
                                TransferPassengerFlow[i][j][k][l] = 0;
                            }
                            else
                            {
                                TransferPassengerFlow[i][j][k][l] = TransferPassengerFlow[0][j][k][l] / 2;
                            }
                        }
                    }
                }
            }
            else if (i == 2)
            {
                TransferPassengerFlow[i] = new double[TimeInteval[0].Length][][];
                for (int j = 0; j < TimeInteval[0].Length; j++)
                {
                    TransferPassengerFlow[i][j] = new double[TimeInteval[0].Length][];
                    for (int k = 0; k < TimeInteval[0].Length; k++)
                    {
                        TransferPassengerFlow[i][j][k] = new double[TimeRunning[0].Length];
                        for (int l = 0; l < TimeRunning[0].Length; l++)
                        {
                            if (TransferPassengerFlow[0][j][k][l] == 0)
                            {
                                TransferPassengerFlow[i][j][k][l] = 0;
                            }
                            else
                            {
                                TransferPassengerFlow[i][j][k][l] = TransferPassengerFlow[0][j][k][l];
                            }
                        }
                    }
                }
            }
            else
            {
                TransferPassengerFlow[i] = new double[TimeInteval[0].Length][][];
                for (int j = 0; j < TimeInteval[0].Length; j++)
                {
                    TransferPassengerFlow[i][j] = new double[TimeInteval[0].Length][];
                    for (int k = 0; k < TimeInteval[0].Length; k++)
                    {
                        TransferPassengerFlow[i][j][k] = new double[TimeRunning[0].Length];
                        for (int l = 0; l < TimeRunning[0].Length; l++)
                        {
                            if (TransferPassengerFlow[0][j][k][l] == 0)
                            {
                                TransferPassengerFlow[i][j][k][l] = 0;
                            }
                            else
                            {
                                TransferPassengerFlow[i][j][k][l] = TransferPassengerFlow[0][j][k][l] / 2;
                            }
                        }
                    }
                }
            }
        }

        TimeLimit = 10000;
        SolverTimeLimit = 60.0;
        ModelFlag = 1; // 默认对应模型1

        StopTime = 0.5;
        AllStationRunning = new int[5][]; // 所有站点的行驶时间

        AllStationRunning[0] = [2, 2, 2, 3, 2, 2, 3, 3, 2, 5, 9, 6, 4];
        AllStationRunning[1] = [4, 6, 9, 5, 2, 3, 3, 2, 2, 2, 7, 4, 8, 4];
        AllStationRunning[2] = [14, 2, 2, 3, 4, 5, 3, 5, 7, 9, 6, 6, 6];
        AllStationRunning[3] = [6, 6, 6, 9, 7, 3, 4, 6, 5, 7, 5, 4, 5, 3, 14];
        AllStationRunning[4] = [4, 3, 3, 5, 3, 3, 3];

        AllStationName = new string[5][];

        AllStationName[0] = ["高铁宜宾西站", "天璇路站", "大湾路站", "时代广场站", "新世纪广场站", "酒都路站", "长江桥南站", "学堂路站", "二医院林港站", "长翠路站", "紫金城站", "西南交通大学站", "牌坊路站", "智轨产业园站"];
        AllStationName[1] = ["成都工业学院站", "成都理工大学站", "成都外国语学院站", "四川轻化工大学站", "新楼路站", "长翠路站", "二医院临港站", "学堂路站", "长江桥南站", "酒都路站", "新世纪广场站", "时代广场站", "大湾路站", "天璇路站", "高铁宜宾西站"];
        AllStationName[2] = ["智轨产业园站", "临港车辆段站", "磅礴路站", "汽车产业园站", "石鼓快速路站", "南溪经开区站", "罗龙站", "朝阳洞站", "高职院站", "仙源长江大桥站", "南溪古街站", "钻石城站", "客运中心站", "南溪区政府站"];
        AllStationName[3] = ["南溪区政府站", "客运中心站", "钻石城站", "文体中心站", "未来之门站", "漂海楼站", "长江第一湾站", "欢乐田园站", "三块石站", "医用基地", "启航路站", "石鼓快速路站", "汽车产业园站", "磅礴路站", "临港车辆段站", "智轨产业园站"];
        AllStationName[4] = ["啤酒广场站", "天池站", "天池家园站", "半岛大院站", "春江盛景站", "金帝庄园站", "新业街站", "银龙广场站"];

    }
}

/// <summary>
/// 用于存储输入的json数据，换乘乘客系数尽量只要有一位小数，不然有精度误差
/// </summary>
public class BusSchedule
{
    public int[] TimeInterval { get; set; } // 发车间隔
    public int[][] TimeRunning { get; set; } // 行驶时间
    public int[] TimeTransfer { get; set; } // 同站换乘时间
    public int TotalTimeInterval { get; set; } // 总时间
    public double[][][] TransferPassengerFlow { get; set; } // 换乘乘客系数;
    public int TimeLimit { get; set; } // 站点不可通行的设置时间
    public double SolverTimeLimit { get; set; } // 求解器时间限制
    public double P { get; set; } // 模型2求解后的p值
    public int ModelFlag { get; set; } // 模型标志
    public int[] LastCar { get; set; } // 上一个时段模型的最后第一辆车的发车时间

    // 可选：带参数的构造函数
    public BusSchedule(int[] timeInterval, int[][] timeRunning, int[] timeTransfer, int totalTimeInterval, double[][][] transferPassengerFlow, int timeLimit, double solverTimeLimit, double p, int modelFlag)
    {
        this.TimeInterval = timeInterval;
        this.TimeRunning = timeRunning;
        this.TimeTransfer = timeTransfer;
        this.TotalTimeInterval = totalTimeInterval;
        this.TransferPassengerFlow = transferPassengerFlow;
        this.TimeLimit = timeLimit;
        this.SolverTimeLimit = solverTimeLimit;
        this.P = p;
        this.ModelFlag = modelFlag;
    }

    public BusSchedule()
    {
    }

    public void BuildBusSchedule(int[] timeInterval, int i, TotalBusSchedule totalBusSchedule, int[] lastCar)
    {
        TimeInterval = timeInterval;
        TimeRunning = totalBusSchedule.TimeRunning;
        TimeTransfer = totalBusSchedule.TimeTransfer;
        TotalTimeInterval = totalBusSchedule.TotalTimeInterval[i];
        TransferPassengerFlow = totalBusSchedule.TransferPassengerFlow[i];
        TimeLimit = totalBusSchedule.TimeLimit;
        SolverTimeLimit = totalBusSchedule.SolverTimeLimit;
        ModelFlag = totalBusSchedule.ModelFlag;
        LastCar = lastCar;
    }
}

public class OutputBusSchedule
{
    public int ModelStatus { get; set; }  // 模型状态，0表示最优解，1表示因时间达限获取的局部最优解，2表示求解器时间到达限制也无可行解
    public double ObjectiveValue { get; set; } // 目标函数值
    public int[] FirstCar { get; set; } // 第一辆车的发车时间

    public OutputBusSchedule()
    {

    }

    public OutputBusSchedule(int modelStatus, double objectiveValue, int[] firstCar)
    {
        this.ModelStatus = modelStatus;
        this.ObjectiveValue = objectiveValue;
        this.FirstCar = firstCar;
    }
}

public class OutputData
{
    public List<OutputBusSchedule> outputBusSchedules { get; set; }

    public OutputData()
    {
        outputBusSchedules = [];
    }
}

public class OutputJson
{
    public int[] Status { get; set; } // 各个时段数据状态
    public double[] ObjectiveValue { get; set; } // 目标函数值
    public string[] BusLineName { get; set; } // 公交：对应的线路名称
    public int[][] BusFirstCar { get; set; } // 公交：第一辆车的发车时间
    public string[][] RailStationName { get; set; } // 智轨：线路站点名称
    public double[][][] RailTimetable { get; set; } // 智轨：时刻表

    public OutputJson(OutputData outputData, TotalBusSchedule totalBusSchedule, ReceiveData receiveData)
    {
        // 求解状态和目标函数值
        Status = new int[outputData.outputBusSchedules.Count];
        ObjectiveValue = new double[outputData.outputBusSchedules.Count];
        for (int i = 0; i < outputData.outputBusSchedules.Count; i++)
        {
            Status[i] = outputData.outputBusSchedules[i].ModelStatus;
            ObjectiveValue[i] = outputData.outputBusSchedules[i].ObjectiveValue;
        }

        // 公交线路名称及始发站时间
        BusLineName = new string[totalBusSchedule.TimeInteval[0].Length - totalBusSchedule.AllStationName.Length * 2]; // 要计算上下行两个方向
        for (int i = 0; i < BusLineName.Length; i++)
        {
            BusLineName[i] = receiveData.lineName[totalBusSchedule.AllStationName.Length * 2 + i];
        }
        BusFirstCar = new int[BusLineName.Length][];
        for (int i = 0; i < BusLineName.Length; i++)
        {
            int[] carCount = new int[totalBusSchedule.TotalTimeInterval.Length];
            for (int j = 0; j < totalBusSchedule.TotalTimeInterval.Length; j++)
            {
                carCount[j] = (int)Math.Ceiling((double)totalBusSchedule.TotalTimeInterval[j] / (double)totalBusSchedule.TimeInteval[j][i + totalBusSchedule.AllStationName.Length * 2]);
            }
            int count = carCount.Sum();
            int index = 0;
            int time = 0;
            BusFirstCar[i] = new int[count];
            for (int j = 0; j < carCount.Length; j++)
            {
                for (int k = 0; k < carCount[j]; k++)
                {
                    BusFirstCar[i][index] = time + outputData.outputBusSchedules[j].FirstCar[i] + k * totalBusSchedule.TimeInteval[j][i + totalBusSchedule.AllStationName.Length * 2];
                    index++;
                }
                time += totalBusSchedule.TotalTimeInterval[j];
            }
        }

        // 智轨线路站点名称及时刻表
        RailStationName = new string[totalBusSchedule.AllStationName.Length * 2][];
        RailTimetable = new double[totalBusSchedule.AllStationName.Length * 2][][];
        for (int i = 0; i < totalBusSchedule.AllStationName.Length * 2; i++)
        {
            // 第一个方向，和对应AllStationName站点顺序相反
            if (i < totalBusSchedule.AllStationName.Length)
            {
                RailStationName[i] = new string[totalBusSchedule.AllStationName[i].Length];
                for (int j = 0; j < totalBusSchedule.AllStationName[i].Length; j++)
                {
                    int idx = totalBusSchedule.AllStationName[i].Length - 1 - j;
                    RailStationName[i][j] = totalBusSchedule.AllStationName[i][idx];
                }

                // 计算TimeTable的长度
                int[] carCount = new int[totalBusSchedule.TotalTimeInterval.Length];
                for (int j = 0; j < totalBusSchedule.TotalTimeInterval.Length; j++)
                {
                    carCount[j] = (int)Math.Ceiling((double)totalBusSchedule.TotalTimeInterval[j] / (double)totalBusSchedule.TimeInteval[j][i]);
                }
                int count = carCount.Sum();
                int index = 0;
                int time = 0;

                double[][] ints = new double[count][];
                for (int j = 0; j < carCount.Length; j++)
                {
                    for (int k = 0; k < carCount[j]; k++)
                    {
                        ints[index] = new double[2 * (totalBusSchedule.AllStationName[i].Length - 1)]; // 始发终到站只有一次
                        for (int l = 0; l < 2 * (totalBusSchedule.AllStationName[i].Length - 1); l++)
                        {
                            if (l == 0)
                            {
                                ints[index][l] = time + outputData.outputBusSchedules[j].FirstCar[i] + k * totalBusSchedule.TimeInteval[j][i];

                            }
                            else if (l == 1)
                            {
                                ints[index][l] = ints[index][l - 1] + totalBusSchedule.AllStationRunning[i][^1]; // 要倒着算
                            }
                            else
                            {
                                if (l % 2 == 0)
                                {
                                    ints[index][l] = ints[index][l - 1] + totalBusSchedule.StopTime; // 出发
                                }
                                else
                                {
                                    int idx = totalBusSchedule.AllStationRunning[i].Length - 1 - (l / 2);
                                    ints[index][l] = ints[index][l - 2] + totalBusSchedule.AllStationRunning[i][idx]; // 到达
                                }
                            }
                        }
                        index++;
                    }
                    time += totalBusSchedule.TotalTimeInterval[j];
                }

                RailTimetable[i] = ints;
            }
            // 第二个方向，和对应AllStationName站点顺序相同
            else
            {
                RailStationName[i] = new string[totalBusSchedule.AllStationName[i - totalBusSchedule.AllStationName.Length].Length];
                for (int j = 0; j < totalBusSchedule.AllStationName[i - totalBusSchedule.AllStationName.Length].Length; j++)
                {
                    RailStationName[i][j] = totalBusSchedule.AllStationName[i - totalBusSchedule.AllStationName.Length][j];
                }

                // 计算TimeTable的长度
                int[] carCount = new int[totalBusSchedule.TotalTimeInterval.Length];
                for (int j = 0; j < totalBusSchedule.TotalTimeInterval.Length; j++)
                {
                    carCount[j] = (int)Math.Ceiling((double)totalBusSchedule.TotalTimeInterval[j] / (double)totalBusSchedule.TimeInteval[j][i]);
                }
                int count = carCount.Sum();
                int index = 0;
                int time = 0;

                double[][] ints = new double[count][];
                for (int j = 0; j < carCount.Length; j++)
                {
                    for (int k = 0; k < carCount[j]; k++)
                    {
                        ints[index] = new double[2 * (totalBusSchedule.AllStationName[i - totalBusSchedule.AllStationName.Length].Length - 1)]; // 始发终到站只有一次
                        for (int l = 0; l < 2 * (totalBusSchedule.AllStationName[i - totalBusSchedule.AllStationName.Length].Length - 1); l++)
                        {
                            if (l == 0)
                            {
                                ints[index][l] = time + outputData.outputBusSchedules[j].FirstCar[i] + k * totalBusSchedule.TimeInteval[j][i];

                            }
                            else if (l == 1)
                            {
                                ints[index][l] = ints[index][l - 1] + totalBusSchedule.AllStationRunning[i - totalBusSchedule.AllStationName.Length][0]; // 要正着算
                            }
                            else
                            {
                                if (l % 2 == 0)
                                {
                                    ints[index][l] = ints[index][l - 1] + totalBusSchedule.StopTime; // 出发
                                }
                                else
                                {
                                    ints[index][l] = ints[index][l - 2] + totalBusSchedule.AllStationRunning[i - totalBusSchedule.AllStationName.Length][l / 2]; // 到达
                                }
                            }
                        }
                        index++;
                    }
                    time += totalBusSchedule.TotalTimeInterval[j];
                }
                RailTimetable[i] = ints;
            }
        }
    }
}