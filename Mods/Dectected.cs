using ExitGames.Client.Photon;
using Juul;
using Photon.Pun;
using Photon.Realtime;
using POpusCodec.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace juul_v2_dev_build.Mods
{
    public class Dectected
    {

        public static void UnlimitLobby()
        {
            if (IsModded() || PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
            {
                Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
                dictionary.Add(251, new Hashtable() { { 249, false } });
                dictionary.Add(250, true);
                dictionary.Add(231, null);
                PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer.SendOperation(252, dictionary, SendOptions.SendReliable);
                PhotonNetwork.CurrentRoom.SuppressPlayerInfo = true;
            }
        }
        public static void PCLagAll()
        {
            if (Time.time > Overpowered.delay)
            {
                Overpowered.delay = Time.time + 1;
                for (int i = 0; i < 400; i++)
                {
                    PhotonNetwork.SendDestroyOfAll();
                }
                Safety.AntiRPCKick();
            }
        }
        public static void PCLagGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (Time.time > Overpowered.delay)
                {
                    Overpowered.delay = Time.time + 1;
                    for (int i = 0; i < 400; i++)
                    {
                        PhotonNetwork.SendDestroyOfPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
                    }
                    Safety.AntiRPCKick();
                }
            }, true);
        }
        public static void PCLagAura()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f)
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 1;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            for (int i = 0; i < 400; i++)
                            {
                                PhotonNetwork.SendDestroyOfPlayer(targetPlayer.ActorNumber);
                            }
                        }
                        Safety.AntiRPCKick();
                    }
                }
            }
        }

        public static void PCLagOnTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.transform.position) < 0.25f ||
                     Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.transform.position) < 0.25f))
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 1;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            for (int i = 0; i < 400; i++)
                            {
                                PhotonNetwork.SendDestroyOfPlayer(targetPlayer.ActorNumber);
                            }
                        }
                        Safety.AntiRPCKick();
                    }
                }
            }
        }

        public static void PCLagOnYourTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig &&
                    (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 1;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            for (int i = 0; i < 400; i++)
                            {
                                PhotonNetwork.SendDestroyOfPlayer(targetPlayer.ActorNumber);
                            }
                        }
                        Safety.AntiRPCKick();
                    }
                }
            }
        }
        public static void DestroyObjectAura()
        {
            if (!IsModded()) return;

            float auraRadius = 3.54f;
            float searchRadius = 2f;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= auraRadius)
                {
                    PhotonView nearestPhotonView = null;
                    float minDistance = float.MaxValue;

                    foreach (var photonView in PhotonNetwork.PhotonViewCollection)
                    {
                        float distance = Vector3.Distance(vrrig.transform.position, photonView.transform.position);
                        if (distance < searchRadius && distance < minDistance)
                        {
                            minDistance = distance;
                            nearestPhotonView = photonView;
                        }
                    }

                    if (nearestPhotonView != null)
                    {
                        ExitGames.Client.Photon.Hashtable even = new ExitGames.Client.Photon.Hashtable();
                        even[0] = nearestPhotonView.ViewID;

                        RaiseEventOptions options = new RaiseEventOptions
                        {
                            CachingOption = nearestPhotonView.isRuntimeInstantiated ? EventCaching.RemoveFromRoomCache : EventCaching.DoNotCache
                        };
                        SendOptions sendOptions = SendOptions.SendReliable;

                        for (int i = 0; i < 5; i++)
                        {
                            PhotonNetwork.CurrentRoom.LoadBalancingClient.OpRaiseEvent(204, even, options, sendOptions);
                        }
                        Safety.AntiRPCKick();
                    }
                }
            }
        }

        public static void DestroyObjectOnTouch()
        {
            if (!IsModded()) return;

            float searchRadius = 2f;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.transform.position) < 0.25f ||
                     Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.transform.position) < 0.25f))
                {
                    PhotonView nearestPhotonView = null;
                    float minDistance = float.MaxValue;

                    foreach (var photonView in PhotonNetwork.PhotonViewCollection)
                    {
                        float distance = Vector3.Distance(vrrig.transform.position, photonView.transform.position);
                        if (distance < searchRadius && distance < minDistance)
                        {
                            minDistance = distance;
                            nearestPhotonView = photonView;
                        }
                    }

                    if (nearestPhotonView != null)
                    {
                        ExitGames.Client.Photon.Hashtable even = new ExitGames.Client.Photon.Hashtable();
                        even[0] = nearestPhotonView.ViewID;

                        RaiseEventOptions options = new RaiseEventOptions
                        {
                            CachingOption = nearestPhotonView.isRuntimeInstantiated ? EventCaching.RemoveFromRoomCache : EventCaching.DoNotCache
                        };
                        SendOptions sendOptions = SendOptions.SendReliable;

                        for (int i = 0; i < 5; i++)
                        {
                            PhotonNetwork.CurrentRoom.LoadBalancingClient.OpRaiseEvent(204, even, options, sendOptions);
                        }
                        Safety.AntiRPCKick();
                    }
                }
            }
        }

        public static void DestroyObjectOnYourTouch()
        {
            if (!IsModded()) return;

            float searchRadius = 2f;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig &&
                    (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    PhotonView nearestPhotonView = null;
                    float minDistance = float.MaxValue;

                    foreach (var photonView in PhotonNetwork.PhotonViewCollection)
                    {
                        float distance = Vector3.Distance(vrrig.transform.position, photonView.transform.position);
                        if (distance < searchRadius && distance < minDistance)
                        {
                            minDistance = distance;
                            nearestPhotonView = photonView;
                        }
                    }

                    if (nearestPhotonView != null)
                    {
                        ExitGames.Client.Photon.Hashtable even = new ExitGames.Client.Photon.Hashtable();
                        even[0] = nearestPhotonView.ViewID;

                        RaiseEventOptions options = new RaiseEventOptions
                        {
                            CachingOption = nearestPhotonView.isRuntimeInstantiated ? EventCaching.RemoveFromRoomCache : EventCaching.DoNotCache
                        };
                        SendOptions sendOptions = SendOptions.SendReliable;

                        for (int i = 0; i < 5; i++)
                        {
                            PhotonNetwork.CurrentRoom.LoadBalancingClient.OpRaiseEvent(204, even, options, sendOptions);
                        }
                        Safety.AntiRPCKick();
                    }
                }
            }
        }
        public static void DestroyObjectGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                RaycastHit ray;
                if (Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out ray))
                {
                    float searchRadius = 2f;
                    PhotonView nearestPhotonView = null;
                    float minDistance = float.MaxValue;

                    foreach (var photonView in PhotonNetwork.PhotonViewCollection)
                    {
                        float distance = Vector3.Distance(ray.point, photonView.transform.position);
                        if (distance < searchRadius && distance < minDistance)
                        {
                            minDistance = distance;
                            nearestPhotonView = photonView;
                        }
                    }

                    if (nearestPhotonView != null && IsModded())
                    {
                        ExitGames.Client.Photon.Hashtable even = new ExitGames.Client.Photon.Hashtable();
                        even[0] = nearestPhotonView.ViewID;

                        RaiseEventOptions options = new RaiseEventOptions
                        {
                            CachingOption = nearestPhotonView.isRuntimeInstantiated ? EventCaching.RemoveFromRoomCache : EventCaching.DoNotCache
                        };
                        SendOptions sendOptions = SendOptions.SendReliable;

                        for (int i = 0; i < 5; i++)
                        {
                            PhotonNetwork.CurrentRoom.LoadBalancingClient.OpRaiseEvent(204, even, options, sendOptions);
                        }
                        Safety.AntiRPCKick();
                    }
                }
            }, false);
        }
      
        public static void SetMaster()
        {
            if (IsModded() && PhotonNetwork.IsConnected && !PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
            }
        }
        public static void SetMasterAll()
        {
            if (!IsModded() || !PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null) return;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (Time.time > Overpowered.delay)
                {
                    Overpowered.delay = Time.time + 0.1f;
                    PhotonNetwork.SetMasterClient(player);
                }
                Safety.AntiRPCKick();
            }
        }
        public static void SetMasterAura()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f)
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 0.1f;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            PhotonNetwork.SetMasterClient(targetPlayer);
                        }
                    }
                    Safety.AntiRPCKick();
                }
            }
        }

        public static void SetMasterOnTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.transform.position) < 0.25f ||
                     Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.transform.position) < 0.25f))
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 0.1f;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            PhotonNetwork.SetMasterClient(targetPlayer);
                        }
                    }
                    Safety.AntiRPCKick();
                }
            }
        }

        public static void SetMasterOnYourTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig &&
                    (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 0.1f;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            PhotonNetwork.SetMasterClient(targetPlayer);
                        }
                    }
                    Safety.AntiRPCKick();
                }
            }
        }
        public static void KickAll()
        {
            if (!IsModded() || !PhotonNetwork.IsConnected) return;
            foreach (Player player in PhotonNetwork.PlayerListOthers)
            {
                if (Time.time > Overpowered.delay)
                {
                    Overpowered.delay = Time.time + 0.1f;
                    PhotonNetwork.CloseConnection(player);
                    Hashtable removeProps = new Hashtable();
                    removeProps[byte.MaxValue] = null;
                    player.SetCustomProperties(removeProps);

                    Safety.AntiRPCKick();
                }
            }
        }
        public static void KickGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null && Time.time > Overpowered.delay)
                {
                    Overpowered.delay = Time.time + 0.1f;
                    PhotonNetwork.CloseConnection(GunLib.LockedPlayer.OwningNetPlayer.GetPlayerRef());
                    Safety.AntiRPCKick();
                }
            }, true);
        }
        public static void ExileGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!IsModded() || GunLib.LockedPlayer == null) return;
                Player target = GunLib.LockedPlayer.OwningNetPlayer.GetPlayerRef();
                PhotonNetwork.DestroyPlayerObjects(target.ActorNumber);
                PhotonNetwork.OpCleanActorRpcBuffer(target.ActorNumber);
                PhotonNetwork.OpRemoveCompleteCacheOfPlayer(target.ActorNumber);
                PhotonNetwork.CloseConnection(target);
                Safety.AntiRPCKick();
            }, true);
        }

        public static void ExileAll()
        {
            if (!IsModded() || !PhotonNetwork.IsMasterClient) return;
            foreach (Player player in PhotonNetwork.PlayerListOthers)
            {
                PhotonNetwork.DestroyPlayerObjects(player.ActorNumber);
                PhotonNetwork.OpCleanActorRpcBuffer(player.ActorNumber);
                PhotonNetwork.OpRemoveCompleteCacheOfPlayer(player.ActorNumber);
                PhotonNetwork.CloseConnection(player);
                Safety.AntiRPCKick();
            }
        }
        public static void ClearPlayerProperties(Player target)
        {
            if (!IsModded() || target == null) return;
            Hashtable props = new Hashtable();
            foreach (object key in target.CustomProperties.Keys)
            {
                props[(string)key] = null;
            }
            target.SetCustomProperties(props);
        }

        public static void ClearPropertiesGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!IsModded() || GunLib.LockedPlayer == null) return;
                Player target = GunLib.LockedPlayer.OwningNetPlayer.GetPlayerRef();
                ClearPlayerProperties(target);
                Safety.AntiRPCKick();
            }, true);
        }









        public static void ChangeNameAll()
        {
            if (!IsModded()) return;
            foreach (Player player in PhotonNetwork.PlayerListOthers)
            {
                if (Time.time > Overpowered.delay)
                {
                    Overpowered.delay = Time.time + 0.1f;
                    player.CustomProperties[byte.MaxValue] = badwordlist[Random.Range(0, badwordlist.Length)];
                    player.InternalCacheProperties(new Hashtable { { byte.MaxValue, badwordlist[Random.Range(0, badwordlist.Length)] } });
                    player.SetPlayerNameProperty();
                }
                Safety.AntiRPCKick();
            }
        }
        public static void ChangeNameGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!IsModded()) return;

                if (Time.time > Overpowered.delay)
                {
                    Overpowered.delay = Time.time + 0.1f;
                    GunLib.LockedPlayer.OwningNetPlayer.GetPlayerRef().CustomProperties[byte.MaxValue] = badwordlist[Random.Range(0, badwordlist.Length)];
                    GunLib.LockedPlayer.OwningNetPlayer.GetPlayerRef().InternalCacheProperties(new Hashtable { { byte.MaxValue, badwordlist[Random.Range(0, badwordlist.Length)] } });
                    GunLib.LockedPlayer.OwningNetPlayer.GetPlayerRef().SetPlayerNameProperty();
                }
                Safety.AntiRPCKick();
            }, true);
        }
        public static void GhostAll()
        {
            foreach (VRRig plyer in VRRigCache.ActiveRigs)
            {
                PhotonView view = Rigs.GetPhotonViewFromVRRig(plyer);
                if (view != null && IsModded())
                {
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(204, new Hashtable() { { 0, view.ViewID }, }, new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(p => p != view.Owner).Select(p => p.ActorNumber).ToArray() }, SendOptions.SendReliable);
                    Safety.AntiRPCKick();
                }
            }
        }
        public static void GhostGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.raycastHit.collider.GetComponentInParent<VRRig>())
                {
                    VRRig plyer = GorillaGameManager.instance.FindPlayerVRRig(GunLib.raycastHit.collider.GetComponentInParent<VRRig>().OwningNetPlayer);
                    PhotonView view = Rigs.GetPhotonViewFromVRRig(plyer);
                    if (view != null && IsModded())
                    {
                        PhotonNetwork.NetworkingClient.OpRaiseEvent(204, new Hashtable() { { 0, view.ViewID }, }, new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(p => p != view.Owner).Select(p => p.ActorNumber).ToArray() }, SendOptions.SendReliable);
                        Safety.AntiRPCKick();
                    }
                }
            }, false);
        }
        public static void IsolateAll()
        {
            foreach (VRRig plyer in VRRigCache.ActiveRigs)
            {
                if (plyer != GorillaTagger.Instance.offlineVRRig)
                {
                    PhotonView view = Rigs.GetPhotonViewFromVRRig(plyer);
                    if (view != null && IsModded())
                    {
                        PhotonNetwork.NetworkingClient.OpRaiseEvent(204,
                            new Hashtable()
                            {
                            {
                                0,
                                view.ViewID
                            },
                            }, new RaiseEventOptions()
                            {
                                TargetActors = PhotonNetwork.PlayerList.Where(q => q.ActorNumber != view.Owner.ActorNumber).Select(q => q.ActorNumber).ToArray()
                            }, SendOptions.SendReliable);
                        Safety.AntiRPCKick();
                    }
                }
            }
        }
        public static void IsolateGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.raycastHit.collider.GetComponentInParent<VRRig>())
                {
                    VRRig plyer = GorillaGameManager.instance.FindPlayerVRRig(GunLib.raycastHit.collider.GetComponentInParent<VRRig>().OwningNetPlayer);
                    foreach (VRRig vRRig in VRRigCache.ActiveRigs)
                    {
                        if (vRRig != GorillaTagger.Instance.offlineVRRig)
                        {
                            PhotonView view = Rigs.GetPhotonViewFromVRRig(vRRig);
                            if (view != null && IsModded())
                            {
                                PhotonNetwork.NetworkingClient.OpRaiseEvent(204, new Hashtable() { { 0, view.ViewID }, }, new RaiseEventOptions { TargetActors = new int[] { plyer.OwningNetPlayer.GetPlayerRef().ActorNumber } }, SendOptions.SendReliable);
                                Safety.AntiRPCKick();
                            }
                        }
                    }
                }
            }, false);
        }
        public static void ChangeNameAura()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f)
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 0.1f;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            targetPlayer.CustomProperties[byte.MaxValue] = badwordlist[Random.Range(0, badwordlist.Length)];
                            targetPlayer.InternalCacheProperties(new Hashtable { { byte.MaxValue, badwordlist[Random.Range(0, badwordlist.Length)] } });
                            targetPlayer.SetPlayerNameProperty();
                        }
                    }
                    Safety.AntiRPCKick();
                }
            }
        }

        public static void ChangeNameOnTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.transform.position) < 0.25f ||
                     Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.transform.position) < 0.25f))
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 0.1f;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            targetPlayer.CustomProperties[byte.MaxValue] = badwordlist[Random.Range(0, badwordlist.Length)];
                            targetPlayer.InternalCacheProperties(new Hashtable { { byte.MaxValue, badwordlist[Random.Range(0, badwordlist.Length)] } });
                            targetPlayer.SetPlayerNameProperty();
                        }
                    }
                    Safety.AntiRPCKick();
                }
            }
        }

        public static void ChangeNameOnYourTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig &&
                    (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    if (Time.time > Overpowered.delay)
                    {
                        Overpowered.delay = Time.time + 0.1f;
                        Player targetPlayer = vrrig.OwningNetPlayer?.GetPlayerRef();
                        if (targetPlayer != null)
                        {
                            targetPlayer.CustomProperties[byte.MaxValue] = badwordlist[Random.Range(0, badwordlist.Length)];
                            targetPlayer.InternalCacheProperties(new Hashtable { { byte.MaxValue, badwordlist[Random.Range(0, badwordlist.Length)] } });
                            targetPlayer.SetPlayerNameProperty();
                        }
                    }
                    Safety.AntiRPCKick();
                }
            }
        }
        public static void GhostAura()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f)
                {
                    PhotonView view = Rigs.GetPhotonViewFromVRRig(vrrig);
                    if (view != null)
                    {
                        PhotonNetwork.NetworkingClient.OpRaiseEvent(204,
                            new Hashtable() { { 0, view.ViewID } },
                            new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(p => p != view.Owner).Select(p => p.ActorNumber).ToArray() },
                            SendOptions.SendReliable);
                        Safety.AntiRPCKick();
                    }
                }
            }
        }

        public static void GhostOnTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.transform.position) < 0.25f ||
                     Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.transform.position) < 0.25f))
                {
                    PhotonView view = Rigs.GetPhotonViewFromVRRig(vrrig);
                    if (view != null)
                    {
                        PhotonNetwork.NetworkingClient.OpRaiseEvent(204,
                            new Hashtable() { { 0, view.ViewID } },
                            new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(p => p != view.Owner).Select(p => p.ActorNumber).ToArray() },
                            SendOptions.SendReliable);
                        Safety.AntiRPCKick();
                    }
                }
            }
        }

        public static void GhostOnYourTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig &&
                    (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    PhotonView view = Rigs.GetPhotonViewFromVRRig(vrrig);
                    if (view != null)
                    {
                        PhotonNetwork.NetworkingClient.OpRaiseEvent(204,
                            new Hashtable() { { 0, view.ViewID } },
                            new RaiseEventOptions { TargetActors = PhotonNetwork.PlayerList.Where(p => p != view.Owner).Select(p => p.ActorNumber).ToArray() },
                            SendOptions.SendReliable);
                        Safety.AntiRPCKick();
                    }
                }
            }
        }
        public static void IsolateAura()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f)
                {
                    foreach (VRRig targetRig in VRRigCache.ActiveRigs)
                    {
                        if (targetRig != GorillaTagger.Instance.offlineVRRig)
                        {
                            PhotonView view = Rigs.GetPhotonViewFromVRRig(targetRig);
                            if (view != null)
                            {
                                PhotonNetwork.NetworkingClient.OpRaiseEvent(204,
                                    new Hashtable() { { 0, view.ViewID } },
                                    new RaiseEventOptions() { TargetActors = new int[] { vrrig.OwningNetPlayer?.GetPlayerRef().ActorNumber ?? -1 } },
                                    SendOptions.SendReliable);
                                Safety.AntiRPCKick();
                            }
                        }
                    }
                }
            }
        }

        public static void IsolateOnTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig &&
                    (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.transform.position) < 0.25f ||
                     Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.transform.position) < 0.25f))
                {
                    foreach (VRRig targetRig in VRRigCache.ActiveRigs)
                    {
                        if (targetRig != GorillaTagger.Instance.offlineVRRig)
                        {
                            PhotonView view = Rigs.GetPhotonViewFromVRRig(targetRig);
                            if (view != null)
                            {
                                PhotonNetwork.NetworkingClient.OpRaiseEvent(204,
                                    new Hashtable() { { 0, view.ViewID } },
                                    new RaiseEventOptions() { TargetActors = new int[] { vrrig.OwningNetPlayer?.GetPlayerRef().ActorNumber ?? -1 } },
                                    SendOptions.SendReliable);
                                Safety.AntiRPCKick();
                            }
                        }
                    }
                }
            }
        }

        public static void IsolateOnYourTouch()
        {
            if (!IsModded()) return;

            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig &&
                    (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5 ||
                     Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    foreach (VRRig targetRig in VRRigCache.ActiveRigs)
                    {
                        if (targetRig != GorillaTagger.Instance.offlineVRRig)
                        {
                            PhotonView view = Rigs.GetPhotonViewFromVRRig(targetRig);
                            if (view != null)
                            {
                                PhotonNetwork.NetworkingClient.OpRaiseEvent(204,
                                    new Hashtable() { { 0, view.ViewID } },
                                    new RaiseEventOptions() { TargetActors = new int[] { vrrig.OwningNetPlayer?.GetPlayerRef().ActorNumber ?? -1 } },
                                    SendOptions.SendReliable);
                                Safety.AntiRPCKick();
                            }
                        }
                    }
                }
            }
        }
        public static void SSMutePlayer(VRRig p)
        {
            if (!IsModded()) return;

            if (p == null) return;

            NetworkView view = p.netView;
            view.OwnerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
            view.ControllerActorNr = PhotonNetwork.LocalPlayer.ActorNumber;
            if (Time.time > Overpowered.delay)
            {
                Overpowered.delay = Time.time + 0.15f;
                NetworkSystem.Instance.NetDestroy(view.gameObject);
            }
            Safety.AntiRPCKick();
        }
        public static void SSMuteAll()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs)
            {
                SSMutePlayer(p);
            }
        }
        public static void SSMuteGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                SSMutePlayer(GunLib.LockedPlayer);
            }, true);
        }
        public static void SSMuteOnTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    SSMutePlayer(vrrig);
                }
            }
        }
        public static void SSMuteAura()
        {
            List<VRRig> vrriglist = new List<VRRig>();
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if ((Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f && vrrig != GorillaTagger.Instance.offlineVRRig))
                {
                    vrriglist.Add(vrrig);
                }
                foreach (VRRig rigs in vrriglist)
                {
                    SSMutePlayer(rigs);
                }
            }
        }
        public static void SSMuteOnYourTouch()
        {
            foreach (VRRig vrrig in VRRigCache.ActiveRigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    SSMutePlayer(vrrig);
                }
            }
        }










        public static string[] badwordlist = new string[]{
    "asshole", // 0
    "bitch", // 1
    "cock", // 2
    "cunt", // 3
    "dick", // 4
    "fag", // 5
    "faggot", // 6
    "fuck", // 7
    "fucker", // 8
    "motherfucker", // 9
    "nigger", // 10
    "nigga", // 11
    "pussy", // 12
    "shit", // 13
    "shithead", // 14
    "slut", // 15
    "tits", // 16
    "twat", // 17
    "wanker", // 18
    "whore", // 19
    "bastard", // 20
    "douche", // 21
    "douchebag", // 22
    "dumbass", // 23
    "faggot", // 24
    "fucking", // 25
    "goddamn", // 26
    "hell", // 27
    "jerk", // 28
    "piss", // 29
    "pissed", // 30
    "pissed off", // 31
    "shitface", // 32
    "shitty", // 33
    "son of a bitch", // 34
    "turd", // 35
    "ass", // 36
    "bastard", // 37
    "bitch", // 38
    "bullshit", // 39
    "damn", // 40
    "dickhead", // 41
    "douche", // 42
    "fuck", // 43
    "fucking", // 44
    "hell", // 45
    "motherfucker", // 46
    "pussy", // 47
    "shit", // 48
    "shitty", // 49
    "tits", // 50
    "twat", // 51
    "wanker", // 52
    "whore", // 53
    "bitch", // 54
    "bullshit", // 55
    "dick", // 56
    "douchebag", // 57
    "fag", // 58
    "faggot", // 59
    "fuck", // 60
    "fucker", // 61
    "hell", // 62
    "motherfucker", // 63
    "nigger", // 64
    "pussy", // 65
    "shit", // 66
    "slut", // 67
    "tits", // 68
    "twat", // 69
    "wanker", // 70
    "whore", // 71
    "asshole", // 72
    "bitch", // 73
    "cock", // 74
    "cunt", // 75
    "dick", // 76
    "fag", // 77
    "faggot", // 78
    "fuck", // 79
    "fucker", // 80
    "motherfucker", // 81
    "nigger", // 82
    "nigga", // 83
    "pussy", // 84
    "shit", // 85
    "shithead", // 86
    "slut", // 87
    "tits", // 88
    "twat", // 89
    "wanker", // 90
    "whore" // 91
};
        public static bool IsModded()
        {
            string gameMode = PhotonNetwork.CurrentRoom.CustomProperties["gameMode"] as string;
            if (gameMode.Contains("MODDED"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }










    }
}