using Cysharp.Threading.Tasks;
using Korion.Achievements;
using Korion.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InitializePSN : MonoBehaviour
{
    [SerializeField]
    private GameObject _achievementInitializer;

    // Start is called before the first frame update
    private async UniTaskVoid Start()
    {
#if UNITY_PS5 && !UNITY_EDITOR
        if (Korion.PS5.PSN.PSNService.IsInitialized)
        {
            await Korion.PS5.PSN.PSNFacade.InitializeAsync();
            await Korion.PS5.PSN.UniversalDataSystemService.AddUserAsync(UnityEngine.PS5.Utility.initialUserId);
            await Korion.IO.IOSystem.Instance.InitializeAsync(destroyCancellationToken);
        }
#elif UNITY_PS4 && !UNITY_EDITOR
            if (!Korion.PS4.PSN.PSNService.IsInitialized)
            {
                Sony.NP.InitToolkit init = new Sony.NP.InitToolkit();
                init.threadSettings.affinity = Sony.NP.Affinity.AllCores;
                init.memoryPools.JsonPoolSize = 6 * 1024 * 1024;
                init.memoryPools.SslPoolSize *= 4;
                init.memoryPools.MatchingSslPoolSize *= 4;
                init.memoryPools.MatchingPoolSize *= 4;

                Sony.NP.AgeRestriction[] ageRestrictions = new Sony.NP.AgeRestriction[2];
                ageRestrictions[0] = new Sony.NP.AgeRestriction(16, new Sony.NP.Core.CountryCode("ru"));
                ageRestrictions[1] = new Sony.NP.AgeRestriction(12, new Sony.NP.Core.CountryCode("de"));
                init.contentRestrictions.AgeRestrictions = ageRestrictions;
                init.contentRestrictions.DefaultAgeRestriction = 16;

            init.SetPushNotificationsFlags(Sony.NP.PushNotificationsFlags.None);
                Korion.PS4.PSN.PSNService.Initialize(init);
            }
#endif
        //Korion
        Instantiate(_achievementInitializer, transform.parent);
    }
}
