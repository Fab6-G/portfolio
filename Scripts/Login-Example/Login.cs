using System;
using MoodleAPI.Operations.CoreUserUpdateUsers;
using MoodleAPI.Operations.WebserviceGetSiteInfo;
using MoodleAPI.Token;
using Scripts.MoodleAPI.Operations.User_Progress.Get;
using StarkAST;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace MoodleAPI.Operations
{
    public class Login : MonoBehaviour
    {
        [SerializeField] private TMP_InputField username = default;
        [SerializeField] private TMP_InputField password = default;

        [SerializeField] private LoginHandler loginHandler = default;

        public static StarkAppCallback OnLoginSuccess;
        public static Action<CoreUsersGetUserByFieldResponse[]> LoginSuccessUpdateUserInformation;

        [SerializeField] private GameObject loginButton = default;
        private Image loginImage = default;
        private Button loginB = default;

        [SerializeField] private GameObject page = default;
        [SerializeField] private Animator loginAnimator = default;


        private bool autoLoginBool = false;

        void Start()
        {
            page.SetActive(false);

            loginB = loginButton.GetComponent<Button>();
            loginImage = loginButton.GetComponent<Image>();
        }

        public void OnLogin()
        {
            EnableLoginButton(false);

            //if (username.text == null || password.text == null)
            //    username.;
            //else
            UsernamePasswordInput(username.text, password.text, false);
        }

        public void OnAutoLogin(string savedUsername, string savedPassword)
        {
            if (GameManager.Instance.Internet())
            {
                loginAnimator.SetBool("AutoLogin", true);
                autoLoginBool = true;

                loginHandler.loginIntro.SetActive(false);
                EnableLoginButton(false);

                UsernamePasswordInput(savedUsername, savedPassword, true);
            }
            else
            {
                Debug.Log("Error. Check internet connection!");
            }
        }

        public void UsernamePasswordInput(string username, string password, bool autoLogin)
        {
            TokenGetRequest tokenGetRequest = new TokenGetRequest(username, password);

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log($"no internet");
                loginHandler.ErrorLogingIn(true);
                EnableLoginButton(true);
                return;
            }

            if (autoLogin)
                GameManager.Instance.moodleAPI.SendGetRequest<TokenResponse>(tokenGetRequest, OnAutoTokenSuccess,
                    OnErrorAutoLogin);
            else
                GameManager.Instance.moodleAPI.SendGetRequest<TokenResponse>(tokenGetRequest, OnTokenSuccess, OnError);
        }

        private void OnErrorAutoLogin(ErrorResponse obj)
        {
            Debug.Log($"Error AUTO logging in: {obj.error}");
            loginHandler.ErrorLogingIn();

            EnableLoginButton(true);
        }

        private void OnError(ErrorResponse obj)
        {
            Debug.Log($"Error logging in: {obj.error}");
            loginHandler.ErrorLogingIn();

            EnableLoginButton(true);
        }


        private void OnTokenSuccess(TokenResponse response)
        {
            // Save the username and password.
            GameManager.Username = username.text;
            GameManager.Password = password.text;
            SaveFileManager.SaveToDisk(true);

            // Alpha hacky code for notification scheduling
            bool displayNotifications = Convert.ToBoolean(PlayerPrefs.GetInt("displayNotifications"));
            if (!displayNotifications)
            {
                PlayerPrefs.SetInt("displayNotifications", 1);
                LocalNotificationManager.Instance.ScheduleOnFirstLoginNotifications();
            }

            // Continue as if auto login.
            OnAutoTokenSuccess(response);
            loginHandler.GoToMain();
        }

        private void OnAutoTokenSuccess(TokenResponse response)
        {
            Debug.Log($"Logged in with token: {response.token}");
            GameManager.Instance.moodleAPI.token = response.token;
            GameManager.Instance.moodleAPI.privateToken = response.privatetoken;
            GameManager.Instance.moodleAPI.SendGetRequest<CoreWebserviceGetSiteInfoResponse>(
                new CoreWebserviceGetSiteInfoRequest(),
                OnSiteInfoSuccess);
            //loginHandler.GoToMain();
            EnableLoginButton(true);
        }

        private void OnSiteInfoSuccess(CoreWebserviceGetSiteInfoResponse response)
        {
            Debug.Log($"Retrieved site info with user id: {response.userID}");
            GameManager.Instance.moodleAPI.userID = response.userID;
            GameManager.Instance.moodleAPI.fullname = response.fullname;
            GameManager.Instance.moodleAPI.userPhoto = response.userpictureurl;
            Debug.Log("STARTED GOT STAR AND SCORE INFO FROM USER");
            GameManager.Instance.moodleAPI.SendGetRequest<GetUserProgressResponse>(new GetUserProgressRequest(),
                OnGetUserProgress);
            OnLoginSuccess?.Invoke();
            GameManager.Instance.moodleAPI.SendGetRequest<CoreUsersGetUserByFieldResponse[]>(
                new CoreUserGetUsersByField(),
                (secondResponse) => { LoginSuccessUpdateUserInformation?.Invoke(secondResponse); }, OnError);
        }

        private void OnGetUserProgress(GetUserProgressResponse obj)
        {
            GameManager.Instance.GetManager<UserStatusManager>()
                .GetStatusCacheFromMoodle(obj.stars, obj.points, obj.level);
            Debug.Log("GOT STAR AND SCORE INFO FROM USER");
        }

        public void ClearPassword()
        {
            password.text = "";
        }

        /// <summary>
        /// Disable or enable the button. Will also change the button colour.
        /// </summary>
        /// <param name="state">Enable or disable the button.</param>
        private void EnableLoginButton(bool state)
        {
            // Disable Login Button.
            loginB.enabled = state;

            // Yellow Colour: FFC53A
            loginImage.color = state ? new Color32(0xFF, 0xC5, 0x3A, 0xFF) : new Color32(0x63, 0x63, 0x63, 0xFF);
        }
    }
}