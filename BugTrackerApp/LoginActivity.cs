////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\LoginActivity.cs </file>
///
/// <copyright file="LoginActivity.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the login activity class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A login activity. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class LoginActivity : AppCompatActivity
    {
        /// <summary>   The API service. </summary>
        private readonly IApiService apiService = ApiService.GetApiService();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Called when the activity is starting. </summary>
        ///
        /// <remarks>
        /// <para>Portions of this page are modifications based on work created and shared by the
        /// <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android
        /// Open Source Project</a></format> and used according to terms described in
        /// the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative
        /// Commons 2.5 Attribution License.</a></format></para>
        /// </remarks>
        ///
        /// <param name="savedInstanceState">   If the activity is being re-initialized after previously
        ///                                     being shut down then this Bundle contains the data it
        ///                                     most recently supplied in
        ///                                     <see cref="M:Android.App.Activity.OnSaveInstanceState(Android.OS.Bundle)" />.
        ///                                     <format type="text/html"><b><i>Note: Otherwise it is
        ///                                     null.</i></b></format> </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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
                if(ValidateUsername(usernameText))
                {
                    try
                    {
                        var response = await apiService.PostLogin(new AuthAccountDetails { Username = usernameText.Text, Password = passwordText.Text });
                        var token = $"Bearer {response.Token}";
                        await SecureStorage.SetAsync("jwt_token", token);

                        var intent = new Intent(this, typeof(MainActivity));
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
                }
                
            };

            registerButton.Click += async (sender, e) =>
            {
                if(ValidateUsername(usernameText))
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
                }
            };
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Validates the username described by usernameField. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="usernameField">    The username field. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool ValidateUsername(EditText usernameField)
        {
            string pattern = @"^[a-zA-Z0-9_]+$";
            if (usernameField.Text.Length <= 0)
            {
                usernameField.Error = "Field can't be empty";
                return false;
            }
            else if (!Regex.Match(usernameField.Text, pattern).Success)
            {
                usernameField.Error = "Username can only contain letters, digits and underscore sign.";
                return false;
            }
            else
            {
                usernameField.Error = null;
                return true;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Called when the activity has detected the user's press of the back key. </summary>
        ///
        /// <remarks>
        /// <para>Portions of this page are modifications based on work created and shared by the
        /// <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android
        /// Open Source Project</a></format> and used according to terms described in
        /// the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative
        /// Commons 2.5 Attribution License.</a></format></para>
        /// </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }
    }
}