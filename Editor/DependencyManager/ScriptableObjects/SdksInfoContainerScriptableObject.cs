using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace AppodealInc.Mediation.DependencyManager.Editor
{
#if APPODEAL_DEV
    [CreateAssetMenu(menuName = "Appodeal/SDKs Info Container (Scriptable Object)", fileName = "New Container")]
#endif
    internal class SdksInfoContainerScriptableObject : ScriptableObject
    {
        [SerializeField] private List<SdkInfoScriptableObject> sdksInfo;

        public List<SdkInfoScriptableObject> SdksInfo { get => sdksInfo; }
    }
}
