public class BusSchedule
{
    public int[] timeInterval { get; set; } // 使用属性而非字段
    public int[][] timeRunning { get; set; } // 使用属性而非字段
    public int[] timeTransfer { get; set; } // 使用属性而非字段
    public int totalTimeInterval { get; set; } // 使用属性而非字段
    public int timeLimit { get; set; } // 使用属性而非字段

    // 无参数构造函数
    public BusSchedule() { }

    // 可选：带参数的构造函数
    public BusSchedule(int[] timeInterval, int[][] timeRunning, int[] timeTransfer, int totalTimeInterval, int timeLimit)
    {
        this.timeInterval = timeInterval;
        this.timeRunning = timeRunning;
        this.timeTransfer = timeTransfer;
        this.totalTimeInterval = totalTimeInterval;
        this.timeLimit = timeLimit;
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