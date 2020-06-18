////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\CreateIssueActivity.cs </file>
///
/// <copyright file="CreateIssueActivity.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the create issue activity class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using BugTrackerApp.Helpers;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A create issue activity. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Activity(Label = "@string/create_issue", Theme ="@style/AppTheme.NoActionBar", ParentActivity = typeof(IssuesActivity), WindowSoftInputMode = SoftInput.StateVisible)]
    public class CreateIssueActivity : AppCompatActivity
    {
        /// <summary>   The API service. </summary>
        private readonly IApiService apiService = ApiService.GetApiService();
        /// <summary>   The authentication token. </summary>
        private string authToken;
        /// <summary>   Identifier for the project. </summary>
        private long projectId;

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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            if (savedInstanceState != null)
            {
                authToken = savedInstanceState.GetString("jwt_token", null);
                projectId = savedInstanceState.GetLong("projectId");
            }
            else
            {
                authToken = Intent.GetStringExtra("jwt_token");
                projectId = Intent.GetLongExtra("project_id", 0L);
            }

            SetContentView(Resource.Layout.activity_createissue);
            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.createIssueToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            EditText issueTitle = FindViewById<EditText>(Resource.Id.createIssueTitleEditText);

            _ = issueTitle.RequestFocus();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Initialize the contents of the Activity's standard options menu. </summary>
        ///
        /// <remarks>
        /// <para>Portions of this page are modifications based on work created and shared by the
        /// <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android
        /// Open Source Project</a></format> and used according to terms described in
        /// the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative
        /// Commons 2.5 Attribution License.</a></format></para>
        /// </remarks>
        ///
        /// <param name="menu"> The options menu in which you place your items. </param>
        ///
        /// <returns>   To be added. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_createissue, menu);
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   This hook is called whenever an item in your options menu is selected. </summary>
        ///
        /// <remarks>
        /// <para>Portions of this page are modifications based on work created and shared by the
        /// <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android
        /// Open Source Project</a></format> and used according to terms described in
        /// the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative
        /// Commons 2.5 Attribution License.</a></format></para>
        /// </remarks>
        ///
        /// <param name="item"> The menu item that was selected. </param>
        ///
        /// <returns>   To be added. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_createIssue)
            {
                CreateIssue();
                return true;
            }
            else if (id == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates the issue. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void CreateIssue()
        {
            EditText issueTitle = FindViewById<EditText>(Resource.Id.createIssueTitleEditText);
            EditText issueBody = FindViewById<EditText>(Resource.Id.createIssueBodyEditText);
            apiService.PostIssue(projectId, new PostIssue { Title = issueTitle.Text, Body = issueBody.Text }, authToken)
                .ContinueWithSuccess(issue => {
                    var intent = new Intent(this, typeof(IssueActivity));
                    intent.PutExtra("jwt_token", authToken);
                    intent.PutExtra("project_id", projectId);
                    intent.PutExtra("issue_number", issue.Number);
                    StartActivity(intent);
                    Finish();
                })
                .ContinueWithFailure(ex =>
                {
                    /*if(ex.InnerException is ApiException apiEx)
                    {
                        var statusCode = apiEx.StatusCode;
                        var error = apiEx.GetContentAs<ErrorResponse>();
                        RunOnUiThread(() => Toast.MakeText(context, error?.Message ?? "XD", ToastLength.Long).Show());
                    }*/
                    RunOnUiThread(() => Toast.MakeText(Application.Context, ex.Message, ToastLength.Long).Show());
                });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   To be added. </summary>
        ///
        /// <remarks>
        /// <para>Portions of this page are modifications based on work created and shared by the
        /// <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android
        /// Open Source Project</a></format> and used according to terms described in
        /// the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative
        /// Commons 2.5 Attribution License.</a></format></para>
        /// </remarks>
        ///
        /// <param name="requestCode">  To be added. </param>
        /// <param name="permissions">  To be added. </param>
        /// <param name="grantResults"> To be added. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Called to retrieve per-instance state from an activity before being killed so that the state
        /// can be restored in <see cref="M:Android.App.Activity.OnCreate(Android.OS.Bundle)" /> or
        /// <see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" /> (the
        /// <see cref="T:Android.OS.Bundle" /> populated by this method
        /// will be passed to both).
        /// </summary>
        ///
        /// <remarks>
        /// <para>Portions of this page are modifications based on work created and shared by the
        /// <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android
        /// Open Source Project</a></format> and used according to terms described in
        /// the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative
        /// Commons 2.5 Attribution License.</a></format></para>
        /// </remarks>
        ///
        /// <param name="outState"> Bundle in which to place your saved state. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("jwt_token", authToken);
            outState.PutLong("project_id", projectId);
            base.OnSaveInstanceState(outState);
        }
    }
}