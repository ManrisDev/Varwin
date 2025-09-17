using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.Public
{
    public interface IPlayerObject
    {
        void SetNodes(PlayerNodes playerNodes);

        void SetPlayerInfo(PlayerInfo playerInfo);
    }

    public class PlayerInfo
    {
        public string NickName;
        public string UserId;
    }

    public class PlayerNodes
    {
        public Transform Head;
        public Transform RightHand;
        public Transform LeftHand;
    }

}