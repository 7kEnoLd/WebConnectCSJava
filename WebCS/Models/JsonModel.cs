public class BusSchedule
{
    public int[] timeInterval { get; set; } // 发车间隔
    public int[][] timeRunning { get; set; } // 行驶时间
    public int[] timeTransfer { get; set; } // 同站换乘时间
    public int totalTimeInterval { get; set; } // 总时间
    public int timeLimit { get; set; } // 站点不可通行的设置时间
    public double solverTimeLimit { get; set; } // 求解器时间限制

    // 无参数构造函数
    public BusSchedule() { }

    // 可选：带参数的构造函数
    public BusSchedule(int[] timeInterval, int[][] timeRunning, int[] timeTransfer, int totalTimeInterval, int timeLimit, double solverTimeLimit)
    {
        this.timeInterval = timeInterval;
        this.timeRunning = timeRunning;
        this.timeTransfer = timeTransfer;
        this.totalTimeInterval = totalTimeInterval;
        this.timeLimit = timeLimit;
        this.solverTimeLimit = solverTimeLimit;
    }
}


public class OutputBusSchedule
{
    public int objectiveValue { get; set; }
    public int[] firstCar { get; set; }

    public OutputBusSchedule(int objectiveValue, int[] firstCar)
    {
        this.objectiveValue = objectiveValue;
        this.firstCar = firstCar;
    }
}