using HutongGames.PlayMaker;
using ItemChanger.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LoreMaster
{
    public static class FsmHelper
    {
        #region FSM

        public static PlayMakerFSM GetFSM(string gameObjectName, string fsmName)
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject == null)
                return null;

            return GetFSM(gameObject, fsmName);
        }

        public static PlayMakerFSM GetFSM(GameObject gameObject, string fsmName)
        {
            try
            {
                PlayMakerFSM playMakerFSM = gameObject.LocateMyFSM(fsmName);
                return playMakerFSM;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static PlayMakerFSM GetFSMByIndex(string gameObjectName, int index)
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject == null)
                return null;

            return GetFSMByIndex(gameObject, index);
        }

        public static PlayMakerFSM GetFSMByIndex(GameObject gameObject, int index)
        {
            try
            {
                return gameObject.GetComponents<PlayMakerFSM>()[index];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<PlayMakerFSM> GetFSMs(string gameObjectName)
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject == null)
                return null;

            return GetFSMs(gameObject);
        }

        public static List<PlayMakerFSM> GetFSMs(GameObject gameObject)
        {
            try
            {
                return gameObject.GetComponents<PlayMakerFSM>().ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region States

        public static FsmState GetState(PlayMakerFSM playMakerFSM, string stateName)
        => playMakerFSM.GetState(stateName);

        #endregion
    }
}
