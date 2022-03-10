using System.Diagnostics.CodeAnalysis;

namespace AppodealStack.Mediation.Common
{
    /// <summary>
    /// <para>
    /// Interface containing signatures of Appodeal Rewarded video callback methods.
    /// </para>
    /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    public interface IRewardedVideoAdListener
    {
        /// <summary>
        /// <para>
        /// Fires when Rewarded Video is loaded.
        /// </para>
        /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
        /// </summary>
        /// <param name="isPrecache">true if loaded ad is precache, otherwise - false.</param>
        void onRewardedVideoLoaded(bool isPrecache);

        /// <summary>
        /// <para>
        /// Fires when Rewarded Video fails to load after passing the waterfall.
        /// </para>
        /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
        /// </summary>
        /// <remarks>If auto cache is enabled, the next attempt to load ad will start automatically, after some delay.</remarks>
        void onRewardedVideoFailedToLoad();

        /// <summary>
        /// <para>
        /// Fires when attempt to show Rewarded Video fails for some reason.
        /// </para>
        /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
        /// </summary>
        void onRewardedVideoShowFailed();

        /// <summary>
        /// <para>
        /// Fires a few seconds after Rewarded Video is displayed on the screen.
        /// </para>
        /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
        /// </summary>
        void onRewardedVideoShown();

        /// <summary>
        /// <para>
        /// Fires when Rewarded Video has been viewed to the end.
        /// </para>
        /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
        /// </summary>
        /// <param name="amount">amount of reward.</param>
        /// <param name="name">reward currency.</param>
        void onRewardedVideoFinished(double amount, string name);

        /// <summary>
        /// <para>
        /// Fires when user closes Rewarded Video.
        /// </para>
        /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
        /// </summary>
        /// <param name="finished">true if video has been fully watched, otherwise - false.</param>
        void onRewardedVideoClosed(bool finished);

        /// <summary>
        /// <para>
        /// Fires when Rewarded Video expires and should not be used.
        /// </para>
        /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
        /// </summary>
        /// <remarks>This callback won't be fired, unless you are loading and not showing ad creative for hours or even days.</remarks>
        void onRewardedVideoExpired();

        /// <summary>
        /// <para>
        /// Fires when user clicks on Rewarded Video ad.
        /// </para>
        /// See <see href="https://wiki.appodeal.com/en/unity/get-started/ad-types/rewarded-video#id-[Development]UnitySDK.Rewardedvideo-RewardedVideoCallbacks"/> for more details.
        /// </summary>
        void onRewardedVideoClicked();
    }
}
