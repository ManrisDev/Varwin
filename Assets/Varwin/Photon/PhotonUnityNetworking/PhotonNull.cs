using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun
{

    public class PhotonTransformViewClassic : MonoBehaviour
    {
        public PositionModel m_PositionModel;
        public RotationModel m_RotationModel;
        public ScaleModel m_ScaleModel;
    }

    public class PhotonEmpty : MonoBehaviour
    {

    }

    public class PhotonAnimatorView : MonoBehaviourPunCallbacks
    {

    }

    public class PhotonRigidbodyView : MonoBehaviourPunCallbacks
    {
        public bool m_TeleportEnabled;
        public bool m_SynchronizeVelocity;
        public bool m_SynchronizeAngularVelocity;
    }

    public class PositionModel
    {
        public bool SynchronizeEnabled;
        public object InterpolateOption;
        public float InterpolateLerpSpeed;
    }

    public class RotationModel
    {
        public bool SynchronizeEnabled;
        public object InterpolateOption;
        public float InterpolateLerpSpeed;
    }

    public class ScaleModel
    {
        public bool SynchronizeEnabled;
        public object InterpolateOption;
        public float InterpolateLerpSpeed;
    }

    public class PhotonTransformViewScaleModel
    {
        public class InterpolateOptions
        {
            public static object Lerp;
        }
    }

    public class MonoBehaviourPunCallbacks : MonoBehaviour
    {
        public PhotonView photonView;

        public virtual void OnDisconnected(DisconnectCause cause)
        {

        }

        public virtual void OnConnectedToMaster()
        {

        }

        public virtual void OnJoinedLobby()
        {
        }

        public virtual void OnRoomListUpdate(List<RoomInfo> roomList)
        {
        }

        public virtual void OnJoinRandomFailed(short returnCode, string message)
        {

        }

        public virtual void OnJoinedRoom()
        {

        }

        public virtual void OnPlayerLeftRoom(Player player)
        {

        }

        public virtual void OnPlayerEnteredRoom(Player other)
        {

        }

        public virtual void OnMasterClientSwitched(Player newMasterClient)
        {

        }

        public virtual void OnLeftRoom()
        {

        }

        public virtual void OnCreatedRoom()
        {

        }
        
        public virtual void OnDisable()
        {
            
        }

        public virtual void OnEnable()
        {
            
        }
    }

    public class DisconnectCause
    {

    }


    public class PhotonView : MonoBehaviour
    {
        public List<Component> ObservedComponents = new List<Component>();
        public bool IsMine;
        public Player Owner;
        public int ViewID;
        public object Synchronization;

        public void OnPhotonPlayerConnected(Player other)
        {

        }

        public void RPC(string methodName, PhotonTargets all, params object[] objects)
        {

        }


        public void TransferOwnership(dynamic any)
        {

        }



        public void RPC(string methodName, RpcTarget masterClient, params object[] objects)
        {

        }
    }


    public class ViewSynchronization
    {
        public static object UnreliableOnChange { get; set; }
    }

    public enum PhotonTargets
    {
        All
    }

    public enum RpcTarget
    {
        MasterClient,
        All,
        Others,
        OthersBuffered
    }

    public class LocalPlayer
    {
        public string UserId;
        public string NickName { get; set; }
    }

    public static class PhotonNetwork
    {
        public static bool AutoJoinLobby;
        public static bool AutomaticallySyncScene;
        public static bool InRoom;
        public static bool Connected;
        public static bool IsMasterClient;
        public static bool OfflineMode { get; set; }
        public static bool IsConnected { get; set; }
        public static string GameVersion { get; set; }

        public static LocalPlayer LocalPlayer;

        public static void Instantiate(string opwPlayerRig, Vector3 transformPosition, Quaternion identity, int i)
        {

        }

        public static class NetworkingClient
        {
            public static string AppId { get; set; }
        }

        public class PhotonServerSettings
        {
            public class AppSettings
            {
                public static string Server { get; set; }
                public static int Port { get; set; }
                public static string AppIdRealtime { get; set; }
            }
        }

        public static int AllocateViewID()
        {
            return 0;
        }

        public class PhotonPlayer
        {

        }

        public class playerList
        {
            public static bool Contains(object owner)
            {
                return false;
            }
        }

        public static void LeaveRoom(bool destroy = true)
        {

        }

        public static void JoinLobby()
        {
        }

        public static void GetCustomRoomList(TypedLobby typedLobby, string sqlLobbyFilter)
        {
        }

        public static void Destroy(GameObject leftHand)
        {

        }

        public static void ConnectUsingSettings()
        {

        }

        public static void CreateRoom(object o, RoomOptions roomOptions)
        {

        }

        public class CurrentRoom
        {
            public static string Name;
            public static int PlayerCount { get; set; }
        }

        public static GameObject Instantiate(string opwPlayerRig, Vector3 transformPosition, Quaternion identity)
        {
            return null;
        }

        public static void JoinOrCreateRoom(string roomName, RoomOptions roomOptions, object o)
        {

        }

        public static int AllocateViewID(bool b)
        {
            return 0;
        }

        public static void Disconnect()
        {

        }
    }

    public class TypedLobby
    {
        public static readonly TypedLobby Default = new TypedLobby();
    }

    public class RoomInfo
    {
        public Hashtable CustomProperties = new Hashtable();
    }

    public class RoomOptions
    {
        public byte MaxPlayers { get; set; }
        public bool CleanupCacheOnLeave { get; set; }
        public bool PublishUserId { get; set; }
        public Hashtable CustomRoomProperties { get; set; }
        public string[] CustomRoomPropertiesForLobby { get; set; }
    }

    public interface IPunObservable
    {

    }

    public interface IPunOwnershipCallbacks
    {

    }

    public class PunRPCAttribute : Attribute
    {
    }

    public class PhotonStream
    {
        public bool IsWriting { get; set; }

        public void SendNext(object head)
        {

        }

        public object ReceiveNext()
        {
            return null;
        }
    }

    public class PhotonMessageInfo
    {

    }

    public class PhotonTransformView : MonoBehaviour
    {
        public NullModel m_PositionModel;
        public NullModel2 m_RotationModel;
    }

    public class NullModel2
    {
        public bool SynchronizeEnabled { get; set; }
        public object InterpolateOption { get; set; }
        public int InterpolateLerpSpeed { get; set; }
    }

    public class NullModel
    {
        public bool SynchronizeEnabled { get; set; }
        public object InterpolateOption { get; set; }
        public int InterpolateLerpSpeed { get; set; }
    }

    public static class PhotonTransformViewRotationModel
    {
        public class InterpolateOptions
        {
            public static object Lerp { get; set; }
        }
    }

    public static class PhotonTransformViewPositionModel
    {
        public class InterpolateOptions
        {
            public static object Lerp { get; set; }
        }
    }
}

namespace ExitGames.Client.Photon
{
    public static class PhotonPeer
    {
        public static void RegisterType(Type type, int i, Func<object, byte[]> serialize, Func<byte[], object> deserialize)
        {

        }
    }

    public class Hashtable : Dictionary<string, string>
    {
    }
}
