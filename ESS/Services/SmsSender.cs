using ESS.Services;
using System.Text;

namespace ESS.Services
{
    public class SmsSender : ISmsSender
    {
        public Task SendSmsAsync(string number, string message)
        {
            using (var web = new System.Net.WebClient())
            {
                try
                {
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return Task.CompletedTask;
                }
            }
        }
    }
}
