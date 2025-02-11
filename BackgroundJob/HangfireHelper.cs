using Hangfire;
using MachineTrading.Services;

namespace MachineTrading.BackgroundJob;

public static class HangfireHelper
{
    private const string UpdateArticles = "UpdateArticles";
    private const string Every30MinuteCron = "*/30 * * * *";
    
    public static void SetUpdateArticleJobId()
    {
        RecurringJob.AddOrUpdate<PlaywrightService>(
            UpdateArticles,
            job => job.StartScrapping(default),
            Every30MinuteCron);
    }
}