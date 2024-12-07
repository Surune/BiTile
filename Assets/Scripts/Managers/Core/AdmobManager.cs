using System;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AdmobManager : MonoBehaviour
{
    private bool TEST_MODE = true;
    
    const string TEST_APP_ID = "ca-app-pub-3940256099942544~3347511713";
    const string TEST_ANDROID_INTERSTITIAL = "ca-app-pub-3940256099942544/1033173712";
    const string TEST_ANDROID_BANNER = "ca-app-pub-3940256099942544/6300978111";
    const string TEST_ANDROID_REWARD = "ca-app-pub-3940256099942544/5224354917";
    const string TEST_IOS_INTERSTITIAL = "ca-app-pub-3940256099942544/4411468910";
    const string TEST_IOS_BANNER = "ca-app-pub-3940256099942544/2934735716";
    const string TEST_IOS_REWARD = "ca-app-pub-3940256099942544/5224354917";

    // 실제 출시용 ID들
    const string ANDROID_APP_ID = "ca-app-pub-7307437131366489~2822302594";
    const string IOS_APP_ID = "ca-app-pub-7307437131366489~2042533383";

	
#if UNITY_ANDROID
	string interstitial = "ca-app-pub-7307437131366489/9190848284"; // Android_Interstitial
    string banner = "ca-app-pub-7307437131366489/1503929958"; //ANDROID_Banner
    string reward = "ca-app-pub-7307437131366489/9812034502"; //ANDROID_Rewarded
	string interstitialTest = TEST_ANDROID_INTERSTITIAL;
    string bannerTest = TEST_ANDROID_BANNER;
    string rewardTest = TEST_ANDROID_REWARD;
#else
    string interstitial = "ca-app-pub-7307437131366489/6044276008"; // IOS_Interstitial
    string banner = "ca-app-pub-7307437131366489/3399847082"; // IOS_Banner
    string reward = "ca-app-pub-7307437131366489/3912936686"; //IOS_Rewarded
    string interstitialTest = TEST_IOS_INTERSTITIAL;
    string bannerTest = TEST_IOS_BANNER;
    string rewardTest = TEST_IOS_REWARD;
#endif
    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    
    public void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            Debug.Log("광고 기능 켜짐");
        });
        LoadBannerAD();
        LoadInterstitial();
        LoadRewarded();
    }

    #region Banner

    public void LoadBannerAD()
    {
        //create banner AD
        CreateBannerView();
        //listen to event
        ListenToBannerEvents();
        //load the banner
        if (bannerView == null)
        {
            CreateBannerView();
        }
        
        var adRequest = new AdRequest();
        //adRequest.Keywords.Add("unity-admob-sample");
        Debug.Log("Loading banner ad.");
        bannerView.LoadAd(adRequest);
    }

    void CreateBannerView()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        bannerView = new BannerView(TEST_MODE ? bannerTest : banner, AdSize.Banner, AdPosition.Bottom);
    }

    void DestroyBannerView()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
    }

    void ListenToBannerEvents()
    {
        // Raised when an ad is loaded into the banner view.
        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                      + bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                           + error);
            if (FailedCounterBanner())
            {
                LoadBannerAD();
            }
        };
        // Raised when the ad is estimated to have earned money.
        bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
            LoadBannerAD();
        };
    }
    #endregion

    #region Interstitial

    public void LoadInterstitial()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");
        Debug.Log("Loading interstitial ad.");
        InterstitialAd.Load(TEST_MODE ? interstitialTest : interstitial,adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.Log("전면광고 로딩 실패");
                }
                else
                {
                    Debug.Log("전면 광고 로딩 성공! " + ad.GetResponseInfo());

                    interstitialAd = ad;
                    InterstitialEvent(interstitialAd);
                }
            });
    
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("광고준비안됨");
            LoadInterstitial();
        }
    }
    void InterstitialEvent(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitial();
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            // Reload the ad so that we can show another as soon as possible.
            LoadInterstitial();
        };
    }

    #endregion

    #region Reward

    public void LoadRewarded()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");
        
        RewardedAd.Load(TEST_MODE ? rewardTest : reward,adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.Log("리워드광고 로딩 실패");
                }
                else
                {
                    Debug.Log("리워드 광고 로딩 성공! " + ad.GetResponseInfo());

                    rewardedAd = ad;
                    RewardedAdEvents(rewardedAd);
                }
            });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward _reward) =>
            {
                Debug.Log("플레이어에게 보상");
                
                PuzzleManager.Instance.ShowHint();
            });
        }
        else
        {
            PuzzleManager.Instance.ShowHint();

            Debug.Log("리워드 광고 준비 안됨");
        }
    }

    public void RewardedAdEvents(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewarded();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            LoadRewarded();
        };
    }
    
    #endregion
    
    private int failedInterstitialNum;
    private int failedBannerNum;
    bool FailedCounterInterstitial()
    {
        failedInterstitialNum++;
        if (failedInterstitialNum>4)
        {
            failedInterstitialNum = 0;
            return false;
        }

        return true;
    }
    bool FailedCounterBanner()
    {
        failedBannerNum++;
        if (failedBannerNum>4)
        {
            failedBannerNum = 0;
            return false;
        }

        return true;
    }
}