using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    [HttpPost("process")]
    public IActionResult ProcessData([FromBody] DataModel data)
    {
        // 对接收到的数据进行处理
        string processedData = $"Received: {data.Content}, Processed at: {DateTime.Now}";
        return Ok(new { Result = processedData });
    }
}

public class DataModel
{
    public string Content { get; set; }
}
