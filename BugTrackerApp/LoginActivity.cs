using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Refit;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class LoginActivity : AppCompatActivity
    {
        private readonly IApiService apiService = ApiService.GetApiService();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_login);

            // If token is already present, proceed to MainActivity
            try
            {
                var token = await SecureStorage.GetAsync("jwt_token");
                if (token != null)
                {
                    var intent = new Intent(this, typeof(MainActivity));
                    intent.PutExtra("jwt_token", token);
                    StartActivity(intent);
                    Finish();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            }


            // Create your application here
            EditText usernameText = FindViewById<EditText>(Resource.Id.usernameText);
            EditText passwordText = FindViewById<EditText>(Resource.Id.passwordText);

            Button loginButton = FindViewById<Button>(Resource.Id.loginButton);
            Button registerButton = FindViewById<Button>(Resource.Id.registerButton);

            loginButton.Click += async (sender, e) =>
            {
                try
                {
                    var response = await apiService.PostLogin(new AuthAccountDetails { Username = usernameText.Text, Password = passwordText.Text });
                    await SecureStorage.SetAsync("jwt_token", response.Token);

                    var intent = new Intent(this, typeof(MainActivity));
                    var token = $"Bearer {response.Token}";
                    intent.PutExtra("jwt_token", token);
                    StartActivity(intent);
                    Finish();
                } 
                catch (ApiException ex)
                {
                    var statusCode = ex.StatusCode;
                    var error = await ex.GetContentAsAsync<ErrorResponse>();
                    Toast.MakeText(Application.Context, error.Message, ToastLength.Short).Show();
                }
                catch (Exception ex)
                {
                    Toast.MakeText(Application.Context, "Device doesn't support secure storage: " + ex.Message, ToastLength.Short).Show();
                }
            };

            registerButton.Click += async (sender, e) =>
            {
                try
                {
                    var response = await apiService.PostRegister(new AuthAccountDetails { Username = usernameText.Text, Password = passwordText.Text });
                    Toast.MakeText(Application.Context, "Register successful!", ToastLength.Short).Show();
                }
                catch (ApiException ex)
                {
                    var statusCode = ex.StatusCode;
                    var error = await ex.GetContentAsAsync<ErrorResponse>();
                    Toast.MakeText(Application.Context, error.Message, ToastLength.Short).Show();
                }
            };
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }
    }
}