using Grpc.Core;

namespace RemoteDesktopServer.Models
{
    public class User
    {
        public string Id { get; set; } // Unique identifier for the user
        public string Name { get; set; } // Display name of the user

        public IServerStreamWriter<Notification> responseStream { get; set; }   
        public ServerCallContext Context { get; set; } // Connection context for sending messages
    }
}
