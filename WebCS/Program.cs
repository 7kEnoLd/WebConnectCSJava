using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 配置Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() // 输出到控制台
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day) // 输出到文件
            .CreateLogger();

        builder.Host.UseSerilog(); // 告诉应用使用 Serilog

        // 配置日志记录
        builder.Logging.ClearProviders(); // 清除默认的日志提供程序
        builder.Logging.AddSerilog(); // 添加 Serilog

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}


// launchSettings注释

//   "profiles": {
//     "http": {
//       "commandName": "Project",
//       "dotnetRunMessages": true,
//       "launchBrowser": true,
//       "applicationUrl": "http://localhost:5162",
//       "environmentVariables": {
//         "ASPNETCORE_ENVIRONMENT": "Development"
//       }
//     },
//     "https": {
//       "commandName": "Project",
//       "dotnetRunMessages": true,
//       "launchBrowser": true,
//       "applicationUrl": "https://localhost:7189;http://localhost:5162",
//       "environmentVariables": {
//         "ASPNETCORE_ENVIRONMENT": "Development"
//       }
//     },
