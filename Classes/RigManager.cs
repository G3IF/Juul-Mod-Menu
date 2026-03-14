using Photon.Pun;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Photon.Realtime;

namespace Juul
{
    public class Rigs
    {

        public static VRRig GetVRRigFromPlayer(Photon.Realtime.Player p)
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs) if (rig.netView.GetView.Owner == p) return rig;
            return null;
        }
        public static VRRig GetVRRigFromNetPlayer(NetPlayer p)
        {
            return GorillaGameManager.instance.FindPlayerVRRig(p);
        }
        public static Photon.Realtime.Player NetPlayerToPlayer(NetPlayer p)
        {
            return p.GetPlayerRef();
        }
        public static Photon.Realtime.Player GetPlayer(VRRig r)
        {
            return r.Creator.GetPlayerRef();
        }
        public static VRRig GetRandomVRRig(bool includeSelf)
        {
            VRRig random = VRRigCache.ActiveRigs[UnityEngine.Random.Range(0, VRRigCache.ActiveRigs.Count - 1)];
            if (includeSelf)
            {
                return random;
            }
            else
            {
                if (random != GorillaTagger.Instance.offlineVRRig)
                {
                    return random;
                }
                else
                {
                    return GetRandomVRRig(includeSelf);
                }
            }
        }

        public static NetworkView GetNetworkViewFromVRRig(VRRig p)
        {
            return (NetworkView)Traverse.Create(p).Field("netView").GetValue();
        }
        public static PhotonView GetPhotonViewFromVRRig(VRRig p)
        {
            NetworkView view = p.netView;
            return Rigs.NetView2PhotonView(view);
        }
        public static PhotonView NetView2PhotonView(NetworkView view)
        {
            PhotonView result;
            if (view == null)
            {
                Debug.Log("null netview passed to converter");
                result = null;
            }
            else
            {
                result = view.GetView;
            }
            return result;
        }
        public static VRRig GetOwnVRRig()
        {
            return GorillaTagger.Instance.offlineVRRig;
        }
        public static NetPlayer GetNetPlayerFromVRRig(VRRig p)
        {
            return Rigs.ToNetPlayer(Rigs.GetPhotonViewFromVRRig(p).Owner);
        }
        public static NetPlayer ToNetPlayer(Photon.Realtime.Player player)
        {
            foreach (NetPlayer netPlayer in NetworkSystem.Instance.AllNetPlayers)
            {
                if (netPlayer.GetPlayerRef() == player)
                {
                    return netPlayer;
                }
            }
            return null;
        }

        public static VRRig GetClosestVRRig()
        {
            float num = float.MaxValue;
            VRRig outRig = null;
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position) < num)
                {
                    num = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position);
                    outRig = vrrig;
                }
            }
            return outRig;
        }

        public static Photon.Realtime.Player GetRandomPlayer(bool includeSelf)
        {
            if (includeSelf)
            {
                return PhotonNetwork.PlayerList[UnityEngine.Random.Range(0, PhotonNetwork.PlayerList.Length - 1)];
            }
            else
            {
                return PhotonNetwork.PlayerListOthers[UnityEngine.Random.Range(0, PhotonNetwork.PlayerListOthers.Length - 1)];
            }
        }

        public static NetPlayer GetPlayerFromVRRig(VRRig p)
        {
            return p.Creator;
        }

        public static Photon.Realtime.Player GetPlayerFromVRRig1(VRRig p)
        {
            return GetPhotonViewFromVRRig(p).Owner;
        }

        public static Photon.Realtime.Player GetPlayerFromID(string id)
        {
            Photon.Realtime.Player found = null;
            foreach (Photon.Realtime.Player target in PhotonNetwork.PlayerList)
            {
                if (target.UserId == id)
                {
                    found = target;
                    break;
                }
            }
            return found;
        }
        public static bool RigIsInfected(VRRig vrrig)
        {
            bool result;
            if (vrrig == null || vrrig.mainSkin == null || vrrig.mainSkin.material == null)
            {
                result = false;
            }
            else
            {
                string name = vrrig.mainSkin.material.name;
                result = (name.Contains("fected") || name.Contains("It"));
            }
            return result;
        }

        public static bool IsOtherPlayer(VRRig rig)
        {
            return rig != null && rig != GorillaTagger.Instance.offlineVRRig && !rig.isOfflineVRRig && !rig.isMyPlayer;
        }
    }
}