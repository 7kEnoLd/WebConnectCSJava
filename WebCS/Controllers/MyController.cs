using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private static Dictionary<string, ProcessingState> sessionStates = [];

    // 结果储存地址
    private static string resultDirectory = "C:\\Users\\Public\\Documents";

    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }

    [HttpPost("receive")]
    public IActionResult Receive([FromBody] string input)
    {
        // 使用日志记录接收到的原始字符串数据
        _logger.LogInformation("Received string data: {Input}", input);

        // 检查是否为 null 或空
        if (string.IsNullOrWhiteSpace(input))
        {
            return BadRequest("Invalid string received.");
        }

        // 创建固定格式的 JSON 数据模板
        FilePath filePath = new();

        (FilePath filePathInstance, ReceiveData receiveDataInstance, TotalBusSchedule totalBusScheduleInstance) = ReadJson(filePath.Path);

        BuildJson(filePathInstance, receiveDataInstance, totalBusScheduleInstance);

        string json = JsonConvert.SerializeObject(receiveDataInstance, Formatting.Indented);

        // 返回 JSON 数据
        return Ok(json);
    }

    /// <summary>
    /// 分阶段方法
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [HttpPost("process-step")]
    public IActionResult ProcessStep([FromBody] ProcessRequest request)
    {
        _logger.LogInformation("Received request: {Input}", request.Input);

        // 反序列化 JSON 字符串为 int[][] 数组
        if (string.IsNullOrWhiteSpace(request.Input))
        {
            return BadRequest("Invalid input data.");
        }

        if (request.SessionId == "0")
        {
            sessionStates.Clear();
            sessionStates = null; // 释放旧引用
            sessionStates = [];
        }

        int[][] intervalArray = JsonConvert.DeserializeObject<int[][]>(request.Input);

        // 获取或初始化当前会话的状态
        // 获取或初始化当前会话的状态
        if (!sessionStates.TryGetValue(request.SessionId, out var state))
        {
            // 获取当前会话在 sessionStates 中的索引位置，作为 currentIndex
            int currentIdx = sessionStates.Count;  // 当前 sessionStates 中已有的元素个数，作为索引

            // 初始化新的 ProcessingState，并将 currentIndex 显式设置为当前的位置
            state = new ProcessingState(intervalArray) { CurrentIndex = currentIdx };

            // 将 state 加入到 sessionStates 字典
            sessionStates[request.SessionId] = state;

            _logger.LogInformation("Initialized new state for session: {SessionId} with CurrentIndex: {CurrentIndex}", request.SessionId, currentIdx);
        }

        _logger.LogInformation("Current index: {CurrentIndex}, IsCompleted: {IsCompleted}", state.CurrentIndex, state.IsCompleted);

        // 添加对前一个状态的拷贝
        int crtIdx = sessionStates.Count - 1; // 前一个状态索引
        if (crtIdx != 0)
        {
            // 深拷贝 BusScheduleList
            state.BusScheduleList = sessionStates[(sessionStates.Count - 2).ToString()].BusScheduleList
                .Select(schedule => new BusSchedule
                {
                    TotalTimeInterval = schedule.TotalTimeInterval,
                    TimeInterval = (int[])schedule.TimeInterval.Clone()
                }).ToList();

            // 深拷贝 OutputData.outputBusSchedules
            state.OutputData.outputBusSchedules = sessionStates[(sessionStates.Count - 2).ToString()].OutputData.outputBusSchedules
                .Select(output => new OutputBusSchedule
                {
                    ModelStatus = output.ModelStatus,
                    ObjectiveValue = output.ObjectiveValue,
                    FirstCar = (int[])output.FirstCar.Clone()
                }).ToList();
        }

        int currentIndex = state.CurrentIndex;
        int[] lastCar = new int[state.IntervalArray[currentIndex].Length];

        if (currentIndex == 0)
        {
            Array.Fill(lastCar, 0);
        }
        else
        {
            if (currentIndex - 1 < 0 || currentIndex - 1 >= state.BusScheduleList.Count)
            {
                throw new InvalidOperationException($"Invalid currentIndex: {currentIndex - 1}. List Count: {state.BusScheduleList.Count}");
            }

            for (int j = 0; j < lastCar.Length; j++)
            {
                if (j >= state.BusScheduleList[currentIndex - 1].TimeInterval.Length)
                {
                    throw new InvalidOperationException($"Invalid index {j} for TimeInterval. Length: {state.BusScheduleList[currentIndex - 1].TimeInterval.Length}");
                }

                int idx = (int)Math.Ceiling((double)state.BusScheduleList[currentIndex - 1].TotalTimeInterval /
                                            (double)state.BusScheduleList[currentIndex - 1].TimeInterval[j]);

                lastCar[j] = state.BusScheduleList[currentIndex - 1].TotalTimeInterval -
                             state.OutputData.outputBusSchedules[currentIndex - 1].FirstCar[j] -
                             (idx - 1) * state.BusScheduleList[currentIndex - 1].TimeInterval[j];
            }
        }


        BusSchedule busSchedule = new();
        busSchedule.BuildBusSchedule(state.IntervalArray[currentIndex], currentIndex, state.TotalBusScheduleInstance, lastCar);
        state.BusScheduleList.Add(busSchedule);

        OutputBusSchedule result = BusScheduleOptimization.OptimizationOrtools(busSchedule);
        state.OutputData.outputBusSchedules.Add(result);

        // 更新索引并检查是否完成
        state.CurrentIndex++;

        _logger.LogInformation("Updated CurrentIndex: {UpdatedIndex}", state.CurrentIndex);

        if (state.CurrentIndex >= state.IntervalArray.Length)
        {
            state.IsCompleted = true;
        }

        // 检查是否已完成
        if (state.IsCompleted)
        {
            if (state.FilePathInstance.ReturnCode == 1)
            {
                // 构建最终 JSON 结果
                OutputJson outputJson = new(state.OutputData, state.TotalBusScheduleInstance, state.ReceiveDataInstance);
                string finalJson = JsonConvert.SerializeObject(outputJson, Formatting.Indented);
                return Ok(new { Code = 200, Status = "Completed", Result = finalJson });
            }
            else
            {
                string finalJson = JsonConvert.SerializeObject(state.OutputData, Formatting.Indented);
                return Ok(new { Code = 200, Status = "Completed", Result = finalJson });
            }
        }

        // 返回部分结果
        return Ok(new
        {
            Code = 100, // 表示处理中
            Status = "Processing",
            CurrentIndex = state.CurrentIndex,
            PartialResult = result
        });
    }

    [HttpPost("process-request")]
    public IActionResult StartLongRunningTask([FromBody] string input)
    {
        var taskId = Guid.NewGuid().ToString(); // 生成唯一标识符
        Task.Run(() => PerformLongRunningTask(taskId, input)); // 启动后台任务
        return Ok(new { taskId });
    }

    private void PerformLongRunningTask(string taskId, string input)
    {
        // 使用日志记录接收到的原始字符串数据
        _logger.LogInformation("Received string data: {Input}", input);

        // 创建固定格式的 JSON 数据模板
        FilePath filePath = new();
        (FilePath filePathInstance, ReceiveData receiveDataInstance, TotalBusSchedule totalBusScheduleInstance) = ReadJson(filePath.Path);

        // 反序列化 JSON 字符串为 int[][] 数组
        int[][] intervalArray = JsonConvert.DeserializeObject<int[][]>(input);

        List<BusSchedule> busScheduleList = [];
        OutputData outputData = new();
        for (int i = 0; i < intervalArray.Length; i++)
        {
            // 初始化最后一辆车的位置
            int[] lastCar = new int[intervalArray[i].Length];
            if (i == 0)
            {
                for (int j = 0; j < intervalArray[i].Length; j++)
                {
                    lastCar[j] = 0;
                }
            }
            else
            {
                for (int j = 0; j < intervalArray[i].Length; j++)
                {
                    // 计算上一次的最后一辆车到下一个时段开始的时间
                    int idx = (int)Math.Ceiling((double)busScheduleList[i - 1].TotalTimeInterval / (double)busScheduleList[i - 1].TimeInterval[j]);
                    lastCar[j] = busScheduleList[i - 1].TotalTimeInterval - outputData.outputBusSchedules[i - 1].FirstCar[j] - (idx - 1) * busScheduleList[i - 1].TimeInterval[j];
                }
            }
            BusSchedule busSchedule = new();
            busSchedule.BuildBusSchedule(intervalArray[i], i, totalBusScheduleInstance, lastCar);
            busScheduleList.Add(busSchedule);
            OutputBusSchedule result = BusScheduleOptimization.OptimizationOrtools(busScheduleList[i]);
            outputData.outputBusSchedules.Add(result);
        }

        OutputJson outputJson = new(outputData, totalBusScheduleInstance, receiveDataInstance);

        // 转化为数据表的格式
        DataTable dtBusSchedule = GetBusScheduleTable(outputJson);
        DataTable dtTrainSchedule = GetTrainScheduleTable(outputJson, receiveDataInstance);

        var busScheduleFilePath = Path.Combine(resultDirectory, taskId + "-BusSchedule" + ".txt");
        var trainScheduleFilePath = Path.Combine(resultDirectory, taskId + "-TrainSchedule" + ".txt");

        // 保存数据表到文本文件
        SaveDataTableToTxt(dtBusSchedule, busScheduleFilePath);
        SaveDataTableToTxt(dtTrainSchedule, trainScheduleFilePath);

        // 记录日志
        _logger.LogInformation("Task {TaskId} completed.", taskId);
    }

    private static DataTable GetBusScheduleTable(OutputJson outputJson)
    {
        DataTable dt = new();
        dt.Columns.Add("序号", typeof(int));       // 数据序号
        dt.Columns.Add("线路", typeof(string));    // 线路名称
        dt.Columns.Add("车次号", typeof(int));     // 时间段
        dt.Columns.Add("始发站时间", typeof(string)); // 始发站时间

        int idx = 1;
        // 添加数据行
        for (int i = 0; i < outputJson.BusFirstCar.Length; i++)
        {

            for (int j = 0; j < outputJson.BusFirstCar[i].Length; j++)
            {
                DataRow dr = dt.NewRow();
                dr["序号"] = idx;
                dr["线路"] = outputJson.BusLineName[i];
                dr["车次号"] = j + 1;
                dr["始发站时间"] = ConvertToTimeString(outputJson.BusFirstCar[i][j]);
                dt.Rows.Add(dr);
                idx++;
            }
        }

        return dt;
    }

    private static DataTable GetTrainScheduleTable(OutputJson outputJson, ReceiveData receiveData)
    {
        DataTable dt = new();
        dt.Columns.Add("序号", typeof(int));        // 数据序号
        dt.Columns.Add("线路", typeof(string));     // 线路名称
        dt.Columns.Add("车次号", typeof(int));      // 车次号
        dt.Columns.Add("车站", typeof(string));     // 车站
        dt.Columns.Add("车站编号", typeof(int));    // 车站编号
        dt.Columns.Add("到站时间", typeof(string)); // 到站时间
        dt.Columns.Add("发车时间", typeof(string)); // 发车时间

        int idx = 1;
        // 添加数据行
        for (int i = 0; i < outputJson.RailTimetable.Length; i++)  // 线路
        {
            for (int j = 0; j < outputJson.RailTimetable[i].Length; j++)  // 车次
            {
                for (int k = 0; k < outputJson.RailStationName[i].Length; k++)  // 车站
                {
                    DataRow dr = dt.NewRow();
                    dr["序号"] = idx;
                    dr["线路"] = receiveData.LineName[i];
                    dr["车次号"] = j + 1;
                    dr["车站"] = outputJson.RailStationName[i][k];
                    dr["车站编号"] = k + 1;
                    
                    // 计算到发时间
                    if (k == 0)
                    {
                        dr["到站时间"] = " ";
                        dr["发车时间"] = ConvertToTimeString(outputJson.RailTimetable[i][j][k]);
                    }
                    else if (k == outputJson.RailStationName[i].Length - 1)
                    {
                        dr["到站时间"] = ConvertToTimeString(outputJson.RailTimetable[i][j][2 * (outputJson.RailStationName[i].Length - 1) - 1]);
                        dr["发车时间"] = " ";
                    }
                    else
                    {
                        dr["到站时间"] = ConvertToTimeString(outputJson.RailTimetable[i][j][2 * k - 1]);
                        dr["发车时间"] = ConvertToTimeString(outputJson.RailTimetable[i][j][2 * k]);
                    }

                    dt.Rows.Add(dr);
                    idx++;
                }
            }
        }

        return dt;
    }

    public static void SaveDataTableToTxt(DataTable dt, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // 写入列标题（列名）
            string columnHeaders = string.Join(",", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            writer.WriteLine(columnHeaders);

            // 写入每一行数据
            foreach (DataRow row in dt.Rows)
            {
                string rowData = string.Join(",", row.ItemArray.Select(item => item.ToString()));
                writer.WriteLine(rowData);
            }
        }
    }

    private static string ConvertToTimeString(double timeValue)
    {
        // 基准时间是6:00:00，转换为分钟
        int baseHours = 6;  // 基准时间为6点
        int baseMinutes = 0;  // 基准时间为00分钟
        int baseSeconds = 0;  // 基准时间为00秒

        // 计算经过的分钟数和秒数
        int totalMinutes = (int)timeValue;
        int totalSeconds = (int)((timeValue - totalMinutes) * 60);

        // 总小时和分钟
        int hours = baseHours + totalMinutes / 60;  // 基于6:00:00开始，计算总小时数
        int minutes = totalMinutes % 60;  // 计算分钟
        int seconds = totalSeconds;  // 计算秒数

        // 处理小时数超过24小时的情况
        hours %= 24;

        // 创建TimeSpan对象
        TimeSpan time = new(hours, minutes, seconds);

        // 格式化为所需的字符串
        return time.ToString(@"hh\:mm\:ss");
    }

    [HttpGet("process-request")]
    public IActionResult ProcessRequest([FromQuery] string taskId)
    {
        // 使用日志记录接收到的原始字符串数据
        _logger.LogInformation("Received string taskId: {TaskId}", taskId);

        // 检查是否为 null 或空
        if (string.IsNullOrWhiteSpace(taskId))
        {
            return BadRequest("Invalid string received.");
        }

        // 查询任务执行状态
        var busScheduleFilePath = Path.Combine(resultDirectory, taskId + "-BusSchedule" + ".txt");
        var trainScheduleFilePath = Path.Combine(resultDirectory, taskId + "-TrainSchedule" + ".txt");

        if (System.IO.File.Exists(busScheduleFilePath) && System.IO.File.Exists(trainScheduleFilePath))
        {
            var busScheduleResult = System.IO.File.ReadAllText(busScheduleFilePath);
            var trainScheduleResult = System.IO.File.ReadAllText(trainScheduleFilePath);
            return Ok(new { Code = 200, Status = "Completed", BusScheduleResult = busScheduleResult, TrainScheduleResult = trainScheduleResult });
        }
        else
        {
            return Ok(new { Code = 100, Status = "Processing" });
        }
    }

    public static (FilePath, ReceiveData, TotalBusSchedule) ReadJson(string filePath)
    {
        try
        {
            // 读取文件内容
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);

                // 反序列化为通用数据列表
                var dataList = JsonConvert.DeserializeObject<List<GeneralData>>(json);
                var filePathInstance = null as FilePath;
                var receiveDataInstance = null as ReceiveData;
                var totalBusScheduleInstance = null as TotalBusSchedule;

                // 根据类型解析具体数据
                foreach (var item in dataList)
                {
                    switch (item.Type)
                    {
                        case "FilePath":
                            filePathInstance = JsonConvert.DeserializeObject<FilePath>(item.Data.ToString());
                            break;
                        case "ReceiveData":
                            receiveDataInstance = JsonConvert.DeserializeObject<ReceiveData>(item.Data.ToString());
                            break;
                        case "TotalBusSchedule":
                            totalBusScheduleInstance = JsonConvert.DeserializeObject<TotalBusSchedule>(item.Data.ToString());
                            break;
                        default:
                            break;
                    }
                }

                return (filePathInstance, receiveDataInstance, totalBusScheduleInstance);
            }
            else
            {
                return (null, null, null);
            }
        }
        catch (Exception ex)
        {
            return (null, null, null);
        }
    }

    public static void BuildJson(FilePath filePathInstance, ReceiveData receiveDataInstance, TotalBusSchedule totalBusScheduleInstance)
    {
        totalBusScheduleInstance.GetTime(filePathInstance.Path);

        // 将实例包装为通用数据对象
        var dataList = new List<GeneralData>
            {
                new() { Type = "FilePath", Data = filePathInstance },
                new() { Type = "ReceiveData", Data = receiveDataInstance },
                new() { Type = "TotalBusSchedule", Data = totalBusScheduleInstance }
            };

        // 序列化为 JSON 并写入文件
        string json = JsonConvert.SerializeObject(dataList, Formatting.Indented);

        try
        {
            System.IO.File.WriteAllText(filePathInstance.Path, json);
            Console.WriteLine($"Data has been written to {filePathInstance.Path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}

public class ProcessRequest
{
    public string SessionId { get; set; } // 用于区分不同会话
    public string Input { get; set; }     // 输入数据（JSON 格式的 int[][]）
}

public class ProcessingState
{
    public int CurrentIndex { get; set; }
    public int[][] IntervalArray { get; }
    public List<BusSchedule> BusScheduleList { get; set; } = new();
    public OutputData OutputData { get; set; } = new();
    public TotalBusSchedule TotalBusScheduleInstance { get; }
    public ReceiveData ReceiveDataInstance { get; }
    public bool IsCompleted { get; set; }
    public FilePath FilePathInstance { get; set; }

    public ProcessingState(int[][] intervalArray)
    {
        IntervalArray = intervalArray;

        // 初始化 TotalBusSchedule 和 ReceiveData
        FilePath filePath = new();
        (FilePathInstance, ReceiveDataInstance, TotalBusScheduleInstance) = MyController.ReadJson(filePath.Path);
    }
}
