using System;
using Sirenix.OdinInspector;

namespace BlueMarble.Server
{
    [Serializable]
    public class User
    {
        [ShowInInspector] public string Id { get; set; }
        [ShowInInspector] public string Nickname { get; set; }

        [ShowInInspector] public bool IsHost { get; set; }

        [ShowInInspector] public bool IsReady { get; set; }
    }
    
    public class PlayerState
    {
        public string Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
    
    public class TypeOnly
    {
        public string Type { get; set; }
    }
    
    public class HelloMessage
    {
        public string Type { get; set; } = "hello";
        public string NickName { get; set; }
    }
    
    public class WelcomeMessage
    {
        public string Type { get; set; } = "welcome";

        public string RoomCode { get; set; }

        public User User { get; set; }
        public User[] Users { get; set; }
    }
    
    public class JoinMessage
    {
        public string Type { get; set; } = "join";
        public User User { get; set; }
    }
    
    public class LeaveMessage
    {
        public string Type { get; set; } = "leave";
        public string Id { get; set; }
    }
    
    public class StateMessage
    {
        public string Type { get; set; } = "state";
        public PlayerState[] States { get; set; }
    }

    public class ReadyMessage
    {
        public string Type { get; set; } = "ready";

        public string Id { get; set; }
        public bool IsReady { get; set; }
    }
}