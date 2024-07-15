using Microsoft.AspNetCore.SignalR;
using OhMyTLU.Helper;
using System.Collections.Generic;
using System.Linq;

namespace OhMyTLU.Hubs
{
    public class RandomChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task JoinRoom(string roomId, string userId)
        {
            UserDic.listPeer.Add(Context.ConnectionId, userId);
            
            Console.WriteLine(UserDic.listPeer.Count);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            //await Clients.Group(roomId).SendAsync("user-connected", userId);
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;

            if (UserDic.waittingRoom.Contains(connectionId))
            {
                UserDic.waittingRoom.Remove(connectionId);
            }

            if (UserDic.listUserId.ContainsKey(connectionId))
            {
                string userId = UserDic.listUserId[connectionId];
                UserDic.listUserId.Remove(connectionId);

                if (UserDic.listPeer.ContainsKey(connectionId))
                {
                    UserDic.listPeer.Remove(connectionId);
                }

                var otherUsers = UserDic.listUserId.Where(x => x.Value != userId).Select(x => x.Key).ToList();
                Console.WriteLine(otherUsers.Count);

                if (otherUsers != null)
                {
                    foreach (var other in otherUsers)
                    {
                        Console.WriteLine(other);
                        await Clients.Client(other).SendAsync("user-disconnected", userId);
                    }
                    
                }
            }

            //Clients.All.SendAsync("user-disconnected", User.list[Context.ConnectionId]);
            await base.OnDisconnectedAsync(exception);
        }

        public void Waiting(string userPeerId, string userId)
        {
            Console.WriteLine("wait");
            if(!UserDic.listPeer.ContainsKey(Context.ConnectionId))
                UserDic.listPeer.Add(Context.ConnectionId, userPeerId);
            if (!UserDic.listUserId.ContainsKey(Context.ConnectionId))
                UserDic.listUserId.Add(Context.ConnectionId, userId);
            //await Groups.AddToGroupAsync(Context.ConnectionId, "waiting");
            if (!UserDic.waittingRoom.Contains(Context.ConnectionId))
                UserDic.waittingRoom.Add(Context.ConnectionId);
        }
        public async Task Match()
        {
            Console.WriteLine(UserDic.listPeer.Count);
            if (UserDic.listPeer.Count >= 2)
            {
                KeyValuePair<string, string>[] randomItems = GetRandomUsers();
                if(UserDic.waittingRoom.Contains(randomItems[0].Key) && UserDic.waittingRoom.Contains(randomItems[1].Key))
                {
                    await Clients.Client(randomItems[0].Key).SendAsync("connecting", randomItems[1].Value, UserDic.listUserId[randomItems[1].Key]);
                    await Clients.Client(randomItems[1].Key).SendAsync("subconnectting", UserDic.listUserId[randomItems[0].Key]);
                    // add 2 client to grooup : "id1,id2" to send friendrequet
                    await Groups.AddToGroupAsync(randomItems[0].Key, UserDic.listUserId[randomItems[0].Key] + "," + UserDic.listUserId[randomItems[1].Key]);
                    await Groups.AddToGroupAsync(randomItems[1].Key, UserDic.listUserId[randomItems[0].Key] + "," + UserDic.listUserId[randomItems[1].Key]);
                    foreach (var item in randomItems)
                    {
                        Console.WriteLine($"Key: {item.Key}, Value: {item.Value}");
                        //await Groups.RemoveFromGroupAsync(item.Key, "waiting");
                        UserDic.waittingRoom.Remove(item.Key);
                        UserDic.listPeer.Remove(item.Key);

                    }
                }
                
            }
            
        }
        public async Task SendFriendRequest(string groupName)
        {
            await Clients.OthersInGroup(groupName).SendAsync("SendFriendRequest");

        }
        public async Task FriendRequestResult(string groupName, int result)
        {
            await Clients.OthersInGroup(groupName).SendAsync("FriendRequestResult", result);

        }
        // chat 1 1 anonymous
        public async Task SendMessage(string userId, string userName, string otherUserId, string message)
        {
            string connectionId = UserDic.listUserId.FirstOrDefault(x => x.Value == otherUserId).Key;
            message = BasicEmojis.ParseEmojis(message);
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", userId, userName, message);
            await Clients.Caller.SendAsync("ReceiveMessage", userId, userName, message);

        }
        public async Task DisMatch(string otherId) 
        {
            Console.WriteLine("close");
            UserDic.listUserId.Remove(Context.ConnectionId);
            UserDic.listPeer.Remove(Context.ConnectionId);
            var otherConnectionId = UserDic.listUserId.FirstOrDefault(x => x.Value == otherId).Key;
            if(otherConnectionId != null)
                await Clients.Client(otherConnectionId).SendAsync("dismatch");

        }
        public void SubDisMatch()
        {
            Console.WriteLine("subclose");
            UserDic.listUserId.Remove(Context.ConnectionId);
            UserDic.listPeer.Remove(Context.ConnectionId);

        }
        public KeyValuePair<string, string>[] GetRandomUsers()
        {
            int count = UserDic.listPeer.Count;
            List<string> keys = UserDic.listPeer.Keys.ToList();

            KeyValuePair<string, string>[] randomItems = new KeyValuePair<string, string>[2];
            Random rand = new Random();
            for (int i = 0; i < 2; i++)
            {
                
                int randomIndex = rand.Next(0, count);

                string randomKey = keys[randomIndex];
                keys.Remove(randomKey);
                count--;

                string randomValue = UserDic.listPeer[randomKey];

                randomItems[i] = new KeyValuePair<string, string>(randomKey, randomValue);
            }

            
            return randomItems;
        }
    }
}
