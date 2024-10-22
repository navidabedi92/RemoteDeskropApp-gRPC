using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDesktopClient
{

    class Client
    {
        private readonly RemoteDesktopService.RemoteDesktopServiceClient client;
        ConnectUserRequest request;
      
        public Client(string serverAddress)
        {
            var channel = GrpcChannel.ForAddress(serverAddress);
            client = new RemoteDesktopService.RemoteDesktopServiceClient(channel);
           
        }

        public async Task ConnectToServer(string userId, string userName)
        {
            request = new ConnectUserRequest
            {
                UserId = userId,
                UserName = userName
            };
            var response = await client.ConnectUserAsync(request);
            if (response.Success)
            {
                Console.WriteLine($"Connected as {userName}.");
            }
            else
            {
                Console.WriteLine("Failed to connect.");
            }
        }

        private async Task DisconnectFromServer(string userId)
        {
            var request = new DisconnectUserRequest
            {
                UserId = userId
            };

            var response = await client.DisconnectUserAsync(request);
            if (response.Success)
            {
                Console.WriteLine($"Disconnected user {userId}.");
            }
            else
            {
                Console.WriteLine("Failed to disconnect.");
            }
        }
        public async Task<byte[]> CaptureScreen()
        {
            return File.ReadAllBytes(@"C:\Users\Notebook\Desktop\monkey.jpg");
        }


        public async void RequestPermission(string targetClientId)
        {
            var permissionRequest = new PermissionRequest { RequesterId =this.request.UserId,
                TargetId = targetClientId };
            client.SendPermissionRequest(permissionRequest);
        }

        public async Task ListenToServerNotifications()
        {
            using var call = client.ListenForNotifications(request);
            try
            {
                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var notification = call.ResponseStream.Current;
                    Console.WriteLine($"Notification from {notification.SenderId}: {notification.Message}");
                    switch(notification.Message)
                    {
                        case "Start Sending":
                            //string[] images = Directory.GetFiles(@"D:\BigProject\RemoteGrpc\RemoteDesktopApp\RemoteDesktopClient\Photos");
                            for (int i = 0; i <20; i++)
                            {
                                CaptureScreenshot(300, 300);
                                byte[] image = File.ReadAllBytes(@"D:\BigProject\RemoteGrpc\RemoteDesktopApp\RemoteDesktopClient\Photos\screenshot.png");
                                ScreenCaptureRequest screenCapture = new ScreenCaptureRequest()
                                { RequesterId = request.UserId, TargetId = notification.SenderId, ImageData = Google.Protobuf.ByteString.CopyFrom(image) };
                                client.SendScreenCapture(screenCapture);
                                Task.Delay(1000).Wait();
                            }
                            break;
                        default:
                            byte[] imageData=notification.ImageData.ToByteArray();
                            System.IO.File.WriteAllBytes(@"D:\BigProject\RemoteGrpc\RemoteDesktopApp\RemoteDesktopClient\NewPhotos\" + Guid.NewGuid().ToString() + ".jpg", imageData);
                            break;

                    }


                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Listening to server notifications cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listening to notifications: {ex.Message}");
            }
        }

        private void CaptureScreenshot(int width, int height)
        {
            // Create a bitmap to hold the screenshot
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                // Create a graphics object from the bitmap
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    // Capture the screenshot
                    graphics.CopyFromScreen(0, 0, 0, 0, new Size(width, height));
                }

                // Save the screenshot as a PNG file
                bitmap.Save(@"D:\BigProject\RemoteGrpc\RemoteDesktopApp\RemoteDesktopClient\Photos\screenshot.png", ImageFormat.Png);
            }
        }
    }
        
    
}
