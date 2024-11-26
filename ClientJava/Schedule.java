public class Schedule {
    /*
    * 字段1：timeInterval，发车间隔数组，一个整数数组，取值范围为(0,最大时间范围)，表示每条线路的发车间隔，例如第一个数据10表示第一条线路的发车间隔为10分钟
    * 字段2：timeRunning，运行时间数组，一个二维整数数组，取值范围为(0,最大时间范围)U(无法联通参数)，表示每条线路的运行到某个换乘站的运行时间，例如第一个数据10000表示第一条线路无法到达第一个换乘站，第三个数据19表示第一条线路到达第三个换乘站的时间为19分钟
    * 字段3：timeTransfer，换乘时间数组，一个整数数组，取值范围为【0,最大时间范围)，表示每个换乘站的同站换乘时间，例如第一个数据4表示第一个换乘站的同站换乘时间为4分钟
    * 字段4：totalTimeInterval，最大时间范围，一个整数，取值范围大于0，表示分钟数，是问题研究的公交车开行时间范围，例如240表示公交车开行时间范围为240分钟
    * 字段5：transferPassengerFlow，换乘乘客系数，一个三维交错的浮点数数组，用于表示第三个元素代表的站点的第一个元素指示的线路换乘第二元素代表的线路的乘客系数，例如第一个数据0.5表示第一个站点的第一条线路换乘第一条线路的乘客系数为0.5
    * 字段6：timeLimit，无法联通参数，一个整数，取值范围大于0，这里固定为一个大数10000，用于条件判断，确定网络中线路无法到达的换乘站
    * 字段7：solverTimeLimit，求解器时间限制，一个浮点数，取值范围大于0，表示求解器的时间限制，单位为秒
    * 字段8：p，第2个模型的结果，指示最大的换乘乘客系数的换乘值，需要代入第3，4个模型中求解，在大算例中为3756
    * 字段9：modelStatus，模型状态，一个整数，取值范围为0,1,2。0表示最优解，1表示因时间达限获取的局部最优解，2表示求解器时间到达限制也无可行解
    */

    public int[] TimeInterval;
    public int[][] TimeRunning;
    public int[] TimeTransfer;
    public int TotalTimeInterval;
    public double[][][] TransferPassengerFlow;
    public int TimeLimit;
    public double SolverTimeLimit;
    public double P;
    public int ModelFlag;

    // Constructor
    public Schedule(int[] timeInterval, int[][] timeRunning, int[] timeTransfer, int totalTimeInterval, double[][][] transferPassengerFlow, int timeLimit, double solverTimeLimit, double p, int modelFlag) {
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
