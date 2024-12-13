using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
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

    [HttpPost("process")]
    public IActionResult Process([FromBody] string input)
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

        // 反序列化 JSON 字符串为 int[][] 数组
        int[][] intervalArray = JsonConvert.DeserializeObject<int[][]>(input);

        List<BusSchedule> busScheduleList = [];
        OutputData outputData = new();
        for(int i = 0; i < intervalArray.Length; i++)
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

        if (filePathInstance.ReturnCode == 0)
        {
            string json = JsonConvert.SerializeObject(outputData, Formatting.Indented);       
            return Ok(json);
        }
        else
        {
            string json = JsonConvert.SerializeObject(outputJson, Formatting.Indented);
            return Ok(json);
        }
    }

    private static (FilePath, ReceiveData, TotalBusSchedule) ReadJson(string filePath)
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

    private static void BuildJson(FilePath filePathInstance, ReceiveData receiveDataInstance, TotalBusSchedule totalBusScheduleInstance)
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
