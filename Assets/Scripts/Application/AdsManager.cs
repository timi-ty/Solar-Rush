using UnityEngine;
using Yodo1.MAS;

public class AdsManager : MonoBehaviour
{
    #region Singleton
    public static AdsManager instance { get; private set; }
    #endregion

    #region Properties
    private static bool ShowAds { get; set; }
    public static bool IsRewardedAdReady => true;
    public static bool ShowingFullScreenAd { get; set; }
    #endregion


    private void Awake()
    {
        #region Singleton
        if (!instance)
        {
            instance = this;

            Initialize();
        }
        else if (!instance.Equals(this))
        {
            Destroy(gameObject);
        }
        #endregion
    }

    public void Initialize()
    {
        ShowAds = true;

        Yodo1U3dMas.SetInitializeDelegate((bool success, Yodo1U3dAdError error) => {
            if (success)
            {
                Debug.Log("Yodo1 Initialized Successfully.");
                SetUpInterstitial();
                SetUpRewarded();
            }
            else
            {
                Debug.Log("Yodo1 Failed to Initialize.");
            }
        });
    }

    private void Start()
    {
        Yodo1U3dMas.InitializeSdk();
    }

    private void SetUpInterstitial()
    {
        Yodo1U3dMas.SetInterstitialAdDelegate((Yodo1U3dAdEvent adEvent, Yodo1U3dAdError error) => {
            Debug.Log("[Yodo1 Mas] InterstitialAdDelegate:" + adEvent.ToString() + "\n" + error.ToString());
            switch (adEvent)
            {
                case Yodo1U3dAdEvent.AdClosed:
                    HandleOnInterstitialClosed();
                    break;
                case Yodo1U3dAdEvent.AdOpened:
                    HandleOnInterstitialOpened();
                    break;
                case Yodo1U3dAdEvent.AdError:
                    HandleAdFailedToLoad(error.Message);
                    break;
            }
        });
    }

    private void SetUpRewarded()
    {
        Yodo1U3dMas.SetRewardedAdDelegate((Yodo1U3dAdEvent adEvent, Yodo1U3dAdError error) => {
            Debug.Log("[Yodo1 Mas] RewardVideoDelegate:" + adEvent.ToString() + "\n" + error.ToString());
            switch (adEvent)
            {
                case Yodo1U3dAdEvent.AdClosed:
                    HandleOnRewardedClosed();
                    break;
                case Yodo1U3dAdEvent.AdOpened:
                    HandleOnRewardedOpened();
                    break;
                case Yodo1U3dAdEvent.AdError:
                    HandleAdFailedToLoad(error.Message);
                    break;
                case Yodo1U3dAdEvent.AdReward:
                    HandleUserEarnedReward();
                    break;
            }

        });
    }

    public static void ShowInterstitial()
    {
        if (!ShowAds || GameManager.gameInstance.disableInterstitialAds) return;

        if (Yodo1U3dMas.IsInterstitialAdLoaded())
        {
            Yodo1U3dMas.ShowInterstitialAd();

            GameManager.HandleFullScreenAdOpened();
        }
    }

    public void ShowRewarded()
    {
        if (GameManager.gameInstance.rewardsWithoutAds)
        {
            GameManager.HandleUserEarnedReward();
            GameManager.HandleRewardedAdClosed();

            return;
        }

        if (Yodo1U3dMas.IsRewardedAdLoaded())
        {
            Yodo1U3dMas.ShowRewardedAd();

            Debug.Log("Rewarded Ad attempting to show...");

            GameManager.HandleFullScreenAdOpened();
        }
    }

    #region Event Handlers
    public static void HandleOnInterstitialOpened()
    {
        ShowingFullScreenAd = true;
    }

    public static void HandleOnInterstitialClosed()
    {
        ShowingFullScreenAd = false;
    }

    public void HandleRewardedAdLoaded()
    {
        //IsRewardedAdReady = true;
    }

    public void HandleOnRewardedOpened()
    {
        ShowingFullScreenAd = true;
        //IsRewardedAdReady = false;
    }

    public void HandleUserEarnedReward()
    {
        GameManager.HandleUserEarnedReward();
    }

    public void HandleOnRewardedClosed()
    {

        ShowingFullScreenAd = false;

        GameManager.HandleRewardedAdClosed();
    }

    public void HandleAdFailedToLoad(string error)
    {
        Debug.Log("Ad Failed: " + error);
    }
    #endregion
}