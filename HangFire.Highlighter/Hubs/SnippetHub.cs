using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace HangFire.Highlighter.Hubs
{
    public class SnippetHub : Hub
    {
        public Task Subscribe(int snippetId)
        {
            return Groups.Add(Context.ConnectionId, GetGroup(snippetId));
        }

        public static string GetGroup(int snippetId)
        {
            return "snippet:" + snippetId;
        }
    }
}