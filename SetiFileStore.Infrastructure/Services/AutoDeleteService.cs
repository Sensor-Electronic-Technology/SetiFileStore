using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Domain;

public class AutoDeleteService:IHostedService,IDisposable {
    
    private int _executionCount = 0;
    private readonly ILogger<AutoDeleteService> _logger;
    private Timer _timer;
    
    public AutoDeleteService(ILogger<AutoDeleteService> logger) {
        this._logger=logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("Auto delete temp service is running....");
        //this._timer=new Timer()
        return Task.CompletedTask;
    }

    private void DeleteFiles(object state) {
        var count = Interlocked.Increment(ref this._executionCount);
        var specDataTime=DateTime.Now.AddHours(-6);
        string folderPath=Directory.GetCurrentDirectory()+"\\Temp\\";
        DirectoryInfo di=new DirectoryInfo(folderPath);
        var getAllFiles=di.GetFiles("*")
            .Where(file=>file.CreationTime<specDataTime).ToList();

        foreach (var fileFullPath in getAllFiles.Select(fileInfo => fileInfo.DirectoryName + @"\" + fileInfo.Name)) {
            File.Delete(fileFullPath);
        }
        this._logger.LogInformation("Auto delete temp service is working. Count: {Count}",count);
    }
    

    public Task StopAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("Auto delete temp service is stopping....");
        this._timer?.Change(Timeout.Infinite,0);
        return Task.CompletedTask;
    }

    public void Dispose() {
        this._timer?.Dispose();
    }
}