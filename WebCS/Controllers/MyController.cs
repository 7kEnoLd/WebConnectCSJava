using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly ILogger<MyController> _logger;

    public MyController(ILogger<MyController> logger)
    {
        _logger = logger;
    }

    [HttpPost("process")]
    public IActionResult Process([FromBody] BusSchedule busSchedule)
    {
        // 使用日志记录接收到的 JSON 数据
        _logger.LogInformation("Received JSON data: {@BusSchedule}", busSchedule);
        
        // 或者使用 Console.WriteLine 直接输出
        // Console.WriteLine("Received JSON data: " + Newtonsoft.Json.JsonConvert.SerializeObject(busSchedule));

        // 检查是否为 null
        if (busSchedule == null)
        {
            return BadRequest("Invalid data received.");
        }

        // 对传入的 JSON 数据进行操作
        OutputBusSchedule result = BusScheduleOptimization.OptimizationOrtools(busSchedule);

        // 处理逻辑...
        return Ok(new { message = "Processing successful", data = result });
    }
}
