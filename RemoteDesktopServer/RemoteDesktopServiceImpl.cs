using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RemoteDesktopServer.Models;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using static RemoteDesktopService;

namespace RemoteDesktopServer
{

    public class RemoteDesktopServiceImpl : RemoteDesktopServiceBase
    {
        private static readonly List<User> ConnectedUsers = new List<User>();

        public override Task<ConnectUserResponse> ConnectUser(ConnectUserRequest request, ServerCallContext context)
        {
            // Add user to the list of connected users
            var user = new User
            {
                Id = request.UserId,
                Name = request.UserName,
                Context = context // Store context for sending messages
            };

            ConnectedUsers.Add(user);

            // Return response indicating success
            return Task.FromResult(new ConnectUserResponse { Success = true });
        }

        // Handle user disconnection
        public override Task<DisconnectUserResponse> DisconnectUser(DisconnectUserRequest request, ServerCallContext context)
        {
            // Remove user from the list of connected users
            RemoveUser(request.UserId);

            // Return response indicating success
            return Task.FromResult(new DisconnectUserResponse { Success = true });
        }

        // Method to remove users on disconnection
   
        public override Task<Empty> SendScreenCapture(ScreenCaptureRequest request, ServerCallContext context)
        {
            SendNotification(request.ImageData, request.RequesterId, request.TargetId);
            return Task.FromResult(new Empty());
        }


        public override Task<Empty> SendPermissionRequest(PermissionRequest request, ServerCallContext context)
        {
            SendNotification("Start Sending", request.RequesterId,request.TargetId);
            return Task.FromResult(new Empty());
        }

   
        public void RemoveUser(string userId)
        {
            var userToRemove = ConnectedUsers.FirstOrDefault(u => u.Id == userId);
            if (userToRemove != null)
            {
                ConnectedUsers.Remove(userToRemove);
            }
        }


        public override async Task ListenForNotifications(ConnectUserRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            //// Add the response stream to the list of listeners
            User user= ConnectedUsers.FirstOrDefault(f => f.Id == request.UserId);
            if(user != null)
            {
                user.responseStream = responseStream;
            }

            // Keep the stream open until the client disconnects
            try
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000); // Prevents blocking
                }
            }
            finally
            {
                // Remove the stream when done
                user.responseStream=null;
            }
        }

        private void SendNotification(object message, string senderId,string targetId)
        {
            var notification = new Notification
            {
                Message =message is string? message.ToString():"",
                ImageData= message is Google.Protobuf.ByteString ? (Google.Protobuf.ByteString)message : Google.Protobuf.ByteString.CopyFrom(new byte[0]),
                SenderId = senderId,
                TargetId=targetId
            };

            User user = ConnectedUsers.FirstOrDefault(f => f.Id == targetId);
            if (user != null)
            {
                user.responseStream.WriteAsync(notification);
            }
        }


    }
}
