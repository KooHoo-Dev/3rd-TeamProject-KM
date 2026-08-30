using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Jay;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

namespace BlueMarble.Server
{
    public class RoomClient : SingletonBase<RoomClient>
    {
        [Title("Settings")]
        [SerializeField] private string url = "ws://34.50.63.204:5005/room";
        
        [field: Title("States")]
        [field: ShowInInspector, ReadOnly] public bool IsBusy { get; private set; }

        [field: PropertySpace]
        [field: ShowInInspector, ReadOnly] public User Me { get; private set; }
        [field: ShowInInspector, ReadOnly] public string Nickname { get; private set; }
        [field: ShowInInspector, ReadOnly] public string RoomCode { get; private set; }
        
        [field: PropertySpace]
        [field: ShowInInspector, ReadOnly] private readonly Dictionary<string, User> users = new();
        
        private ClientWebSocket socket;
        private readonly SemaphoreSlim sendLock = new(1, 1);

        public event Action<User> OnUserJoined;
        public event Action<User> OnUserLeft;
        public event Action<User, bool> OnUserReadyChanged;

        public IReadOnlyDictionary<string, User> Users => users;

        protected override void OnInitialize()
        {
            Application.runInBackground = true;
        }

        #region PUBLIC_METHODS
        
        public void Join(string nickname, string roomCode)
        {
            if (IsBusy) return;
            
            Nickname = nickname;
            RoomCode = roomCode;
            users.Clear();
            
            RunAsync().Forget();
        }
        
        public void RequestReady(bool isReady)
        {
            ReadyMessage msg = new ReadyMessage { Id = Me.Id, IsReady = isReady };
            Debug.Log($"[RoomClient] {Me.Nickname}({Me.Id}) is ready: {isReady}");
            
            SendAsync(msg).Forget();
        }
        
        #endregion

        private async UniTaskVoid RunAsync()
        {
            IsBusy = true;
            
            socket = new ClientWebSocket();
            string uriStr = $"{url}?code={UnityWebRequest.EscapeURL(RoomCode)}";

            try
            {
                await socket.ConnectAsync(new Uri(uriStr), destroyCancellationToken).AsUniTask();

                Debug.Log($"[RoomClient] Connected to \"{uriStr}\"");

                await SendAsync(new HelloMessage { NickName = Nickname });
                await ReceiveLoopAsync();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Debug.LogError($"[RoomClient] {ex}"); }
            finally
            {
                Me = null;
                IsBusy = false;
            }
        }

        public async UniTask LeaveAsync()
        {
            if (socket == null) return;

            await sendLock.WaitAsync();

            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, 
                        "bye", 
                        destroyCancellationToken).AsUniTask();
                }
            }
            catch (WebSocketException) { }
            finally
            {
                socket.Abort();
                sendLock.Release();
            }

            await UniTask.WaitUntil(this, self => self.IsBusy == false);

            socket.Dispose();
            socket = null;

            users.Clear();
            Nickname = null;
            RoomCode = null;
            
            Debug.Log("[RoomClient] Disconnected");
        }

        #region SEND_AND_RECEIVE_MESSAGE

        private async UniTask SendAsync(object message)
        {
            await sendLock.WaitAsync();
            
            try
            {
                if (socket is not { State: WebSocketState.Open }) return;
                
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text, 
                    true, 
                    destroyCancellationToken).AsUniTask();
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException) { }
            finally
            {
                sendLock.Release();
            }
        }

        private async UniTask ReceiveLoopAsync()
        {
            StringBuilder stringBuilder = new();
            
            byte[] buffer = new byte[4096];

            while (true)
            {
                if (socket.State != WebSocketState.Open) break;

                WebSocketReceiveResult result;

                try
                {
                    result = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), 
                        destroyCancellationToken).AsUniTask();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (WebSocketException)
                {
                    Debug.LogWarning("[RoomClient] RoomClient socket closed unexpectedly");
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Close) return;
                
                stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                if (result.EndOfMessage == false) continue;
                
                string json = stringBuilder.ToString();
                stringBuilder.Clear();

                HandleReceived(json);
            }
        }

        #endregion
        
        #region HANDLE_RECEIVED_MESSAGE
        
        private void HandleReceived(string json)
        {
            string type = JsonConvert.DeserializeObject<TypeOnly>(json).Type;

            switch (type)
            {
                case "welcome": OnWelcome(JsonConvert.DeserializeObject<WelcomeMessage>(json)); break;
                case "join": OnJoin(JsonConvert.DeserializeObject<JoinMessage>(json)); break;
                case "leave": OnLeave(JsonConvert.DeserializeObject<LeaveMessage>(json)); break;
                case "state": OnState(JsonConvert.DeserializeObject<StateMessage>(json)); break;
                case "ready": OnReadyChanged(JsonConvert.DeserializeObject<ReadyMessage>(json)); break;
                default: Debug.Log($"[방] 등록되지 않은 종류(type) : {type}"); break;
            }
        }

        private void OnWelcome(WelcomeMessage message)
        {
            Me = message.User;
            RoomCode = message.RoomCode;

            foreach (User user in message.Users)
                users[user.Id] = user;
            
            Debug.Log($"[RoomClient] 방 입장 완료: {RoomCode}. 내 ID: {Me.Id}, 참가자: {users.Count + 1}명");
        }

        private void OnJoin(JoinMessage message)
        {
            User user = message.User;
            
            users[user.Id] = user;
            OnUserJoined?.Invoke(user);
            
            Debug.Log($"[RoomClient] 방 입장: {user.Nickname}({user.Id})");
        }

        private void OnLeave(LeaveMessage message)
        {
            if (users.Remove(message.Id, out User user) == false) return;

            OnUserLeft?.Invoke(user);
            Debug.Log($"[RoomClient] 방 퇴장: {user.Nickname}({user.Id})");
        }

        private void OnState(StateMessage message)
        {
            
        }

        private void OnReadyChanged(ReadyMessage message)
        {
            User user = message.Id == Me.Id ? Me : users[message.Id];
            
            user.IsReady = message.IsReady;
            OnUserReadyChanged?.Invoke(user, message.IsReady);
        }
        
        #endregion
    }
}