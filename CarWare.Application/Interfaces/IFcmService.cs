using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWare.Application.Interfaces
{
    public interface IFcmService
    {
        Task<string> SendAsync(string token, string title, string body,
            Dictionary<string, string>? data = null);

        Task SendMulticastAsync(List<string> tokens, string title, string body,
            Dictionary<string, string>? data = null);

        Task SendToTopicAsync(string topic, string title, string body);
    }
}