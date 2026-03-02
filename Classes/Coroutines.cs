using System;
using System.Collections;
using UnityEngine;

namespace Juul
{
    public class Coroutines : MonoBehaviour
    {
        private void Awake()
        {
            Coroutines.instance = this;
        }

        public static Coroutine RunCoroutine(IEnumerator enumerator)
        {
            return Coroutines.instance.StartCoroutine(enumerator);
        }

        public static void EndCoroutine(Coroutine enumerator)
        {
            Coroutines.instance.StopCoroutine(enumerator);
        }

        public static Coroutines instance;
    }
}