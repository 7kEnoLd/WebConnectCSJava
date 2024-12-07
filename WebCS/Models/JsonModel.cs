// 所需的数据格式
public class GeneralData
{
    public string Type { get; set; } // 数据类型标识，例如 "ClassA" 或 "ClassB"
    public object Data { get; set; } // 数据内容
}

public class ReceiveData
{
    public string Status { get; set; } // 数据状态
    public string Remark { get; set; } // 数据结构解释
    public string[] LineName { get; set; } // 线路名称
    public int[][] Interval { get; set; } // 线路在各个时段上的发车间隔

    public ReceiveData()
    {
        Status = "Success";
        Remark = "The input data structure is as follows: Interval: int[][] as json format, only need to enter the departure intervals of each line in each time period. The time periods are divided into morning peak, off-peak 1, evening peak and off-peak 2";
        LineName = [ "A1-智轨T1主线-高铁西站至智轨产业园站", "A2-智轨T1支线-成都工业学院站至高铁西站", "A3-智轨T4主线-智轨产业园站至南溪区政府", "A4-智轨T4支线-南溪区政府至智轨产业园站",
                                       "B1-9路-翠屏山至盐坪坝", "B2-15路-丽雅大院至翠屏山(市妇幼保健院)", "B3-24路-东山路至市政广场(云上装饰)", "B4-32路-纵一路口至宜宾七中",
                                       "B5-南溪9路-新中医院西门至东门", "B6-南溪10路-客运中心至月亮湾游客中心", "B7-南溪12路-祥和小区至客运中心" ];
        Interval = [[10, 10, 15, 25, 20, 11, 15, 10, 14, 17, 24], [15, 15, 20, 30, 25, 16, 20, 15, 19, 22, 29], [10, 10, 15, 25, 20, 11, 15, 10, 14, 17, 24], [15, 15, 20, 30, 25, 16, 20, 15, 19, 22, 29]];
    }
}

public class FilePath
{
    public string Path { get; set; } // 文件路径

    public FilePath()
    {
        Path = "C:\\Users\\Public\\Downloads\\data.json";
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
        TimeInteval = [[10, 10, 15, 25, 20, 11, 15, 10, 14, 17, 24], [15, 15, 20, 30, 25, 16, 20, 15, 19, 22, 29], [10, 10, 15, 25, 20, 11, 15, 10, 14, 17, 24], [15, 15, 20, 30, 25, 16, 20, 15, 19, 22, 29]];
        TimeRunning = [[10000, 10000, 19, 24, 10000, 32, 10000, 36, 10000, 45, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                             [10000, 10000, 42, 37, 10000, 29, 10000, 25, 23, 10000, 1000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                             [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 18, 42, 58, 10000, 10000, 10000, 10000, 51],
                             [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 90, 72, 10000, 14, 26, 22, 10000, 49, 10000],
                             [10000, 17, 22, 10000, 34, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                             [37, 17, 26, 31, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                             [10000, 10000, 10000, 10000, 61, 53, 51, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                             [10000, 10000, 10000, 10000, 10000, 10000, 55, 10000, 48, 10000, 21, 10000, 10000, 10000, 10000, 10000, 10000, 10000],
                             [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 16, 20, 32, 10000, 10000],
                             [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 20, 10000, 10000],
                             [10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 20, 28, 10000, 10000, 10000, 10000, 10000]];
        TimeTransfer = [4, 3, 2, 6, 0, 2, 7, 0, 6, 0, 1, 1, 0, 0, 1, 12, 11, 3];
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
                TransferPassengerFlow[i][5][4][0] = 1.2;   //S1  B2-B1
                TransferPassengerFlow[i][5][4][1] = 0.4;   //S2  B2-B1
                TransferPassengerFlow[i][4][5][1] = 0.4;   //S2  B1-B2
                TransferPassengerFlow[i][5][4][2] = 0.4;   //S3  B2-B1
                TransferPassengerFlow[i][4][5][2] = 0.4;   //S3  B1-B2
                TransferPassengerFlow[i][0][4][2] = 0.4;   //S3  A1-B1
                TransferPassengerFlow[i][0][5][2] = 0.4;   //S3  A1-B2
                TransferPassengerFlow[i][4][0][2] = 0.8;   //S3  B1-A1
                TransferPassengerFlow[i][4][1][2] = 0.8;   //S3  B1-A2
                TransferPassengerFlow[i][5][0][2] = 0.8;   //S3  B2-A1
                TransferPassengerFlow[i][5][1][2] = 0.8;   //S3  B2-A2
                TransferPassengerFlow[i][1][5][3] = 0.8;   //S4  A2-B2
                TransferPassengerFlow[i][6][4][4] = 0.4;   //S5  B3-B1
                TransferPassengerFlow[i][6][0][5] = 0.4;   //S6  B3-A1
                TransferPassengerFlow[i][6][1][5] = 0.8;   //S6  B3-A2
                TransferPassengerFlow[i][7][6][6] = 0.4;   //S7  B4-B3
                TransferPassengerFlow[i][1][0][7] = 6;     //S8  A2-A1
                TransferPassengerFlow[i][1][7][8] = 0.4;   //S9  A2-B4
                TransferPassengerFlow[i][0][2][9] = 12;    //S10 A1-A3
                TransferPassengerFlow[i][7][2][10] = 0.4;  //S11 B4-A3
                TransferPassengerFlow[i][3][7][10] = 1;    //S11 A4-B4
                TransferPassengerFlow[i][2][10][11] = 0.4; //S12 A3-B7
                TransferPassengerFlow[i][10][2][11] = 0.8; //S12 B7-A3
                TransferPassengerFlow[i][10][9][12] = 1.2; //S13 B7-B6
                TransferPassengerFlow[i][10][3][12] = 1.6; //S13 B7-A4
                TransferPassengerFlow[i][2][9][12] = 0.8;  //S13 A3-B6
                TransferPassengerFlow[i][8][3][13] = 0.4;  //S14 B5-A4
                TransferPassengerFlow[i][3][8][14] = 1.2;  //S15 A4-B5
                TransferPassengerFlow[i][8][9][15] = 0.4;  //S16 B5-B6
                TransferPassengerFlow[i][3][10][16] = 2.4; //S17 A4-B7
                TransferPassengerFlow[i][2][8][17] = 0.8;  //S18 A3-B5
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
        SolverTimeLimit = 600.0;
        ModelFlag = 1;
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