/// <summary>
/// 用于存储输入的json数据，换乘乘客系数尽量只要有一位小数，不然有精度误差
/// </summary>
public class BusSchedule
{
    public int[] TimeInterval { get; set; } // 发车间隔
    public int[][] TimeRunning { get; set; } // 行驶时间
    public int[] TimeTransfer { get; set; } // 同站换乘时间
    public int TotalTimeInterval { get; set; } // 总时间
    public double[][][] TransferPassengerFlow { get; set;} // 换乘乘客系数;
    public int TimeLimit { get; set; } // 站点不可通行的设置时间
    public double SolverTimeLimit { get; set; } // 求解器时间限制
    public double P { get; set; } // 模型2求解后的p值
    public int ModelFlag { get; set; } // 模型标志

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