using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Juul
{
    internal class Serialize2
    {

        public static void Serialize(PhotonView pv, RaiseEventOptions options = null, int timeOffset = 0, float delay = 0f)
        {
            if (!PhotonNetwork.InRoom || pv == null)
                return;
            List<object> serializedData = PhotonNetwork.OnSerializeWrite(pv);
            PhotonNetwork.RaiseEventBatch eventBatch = new PhotonNetwork.RaiseEventBatch
            {
                Reliable = pv.Synchronization == ViewSynchronization.ReliableDeltaCompressed || pv.mixedModeIsReliable,
                Group = pv.Group
            };
            IDictionary batchDictionary = PhotonNetwork.serializeViewBatches;
            PhotonNetwork.SerializeViewBatch viewBatch = new PhotonNetwork.SerializeViewBatch(eventBatch, 2);
            if (!batchDictionary.Contains(eventBatch))
                batchDictionary[eventBatch] = viewBatch;
            viewBatch.Add(serializedData);
            RaiseEventOptions defaultOptions = PhotonNetwork.serializeRaiseEvOptions;
            RaiseEventOptions finalOptions = options != null ? new RaiseEventOptions
            {
                CachingOption = defaultOptions.CachingOption,
                Flags = defaultOptions.Flags,
                InterestGroup = defaultOptions.InterestGroup,
                TargetActors = options.TargetActors,
                Receivers = options.Receivers
            } : defaultOptions;
            List<object> updateData = viewBatch.ObjectUpdates;
            updateData[0] = PhotonNetwork.ServerTimestamp + timeOffset;
            updateData[1] = PhotonNetwork.currentLevelPrefix != 0 ? (object)PhotonNetwork.currentLevelPrefix : null;
            bool isReliable = viewBatch.Batch.Reliable;
            byte eventCode = isReliable ? (byte)206 : (byte)201;
            SendOptions sendOptions = isReliable ? SendOptions.SendReliable : SendOptions.SendUnreliable;
            if (delay <= 0f)
                PhotonNetwork.NetworkingClient.OpRaiseEvent(eventCode, updateData, finalOptions, sendOptions);
            else
                Coroutines.instance.StartCoroutine(SerializeDelay(() =>
                    PhotonNetwork.NetworkingClient.OpRaiseEvent(eventCode, updateData, finalOptions, sendOptions), delay));
            viewBatch.Clear();
        }

        public static IEnumerator SerializeDelay(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
