////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\ProjectActivity.cs </file>
///
/// <copyright file="ProjectActivity.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the project activity class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Refit;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A project activity. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Activity(Label = "ProjectActivity", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(MainActivity))]
    public class ProjectActivity : AppCompatActivity
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

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            if (savedInstanceState != null)
            {
                authToken = savedInstanceState.GetString("jwt_token", null);
                projectId = savedInstanceState.GetLong("project_id");
            }
            else
            {
                authToken = Intent.GetStringExtra("jwt_token");
                projectId = Intent.GetLongExtra("project_id", 0L);
            }

            SetContentView(Resource.Layout.activity_project);

            // Create your application here

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.projectToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Title = " ";

            Project project;
            try
            {
                project = await GetProject(projectId);
                TextView projectOwner = FindViewById<TextView>(Resource.Id.projectOwnerTextView);
                TextView projectTitle = FindViewById<TextView>(Resource.Id.projectTitleTextView);
                TextView projectDescription = FindViewById<TextView>(Resource.Id.projectDescriptionTextView);
                TextView projectIssuesCount = FindViewById<TextView>(Resource.Id.projectIssuesCount);
                LinearLayout projectIssuesButton = FindViewById<LinearLayout>(Resource.Id.projectIssuesButton);

                projectOwner.Text = project.User.Username;
                projectTitle.Text = project.Title;
                projectDescription.Text = project.Description;
                projectIssuesCount.Text = project.Issues.ToString();


                projectIssuesButton.Click += (sender, e) =>
                {
                    var intent = new Intent(this, typeof(IssuesActivity));
                    intent.PutExtra("jwt_token", authToken);
                    intent.PutExtra("project_id", project.Id);
                    StartActivity(intent);
                };
            }
            catch (ApiException ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            }

        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a project. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        ///
        /// <returns>   An asynchronous result that yields the project. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<Project> GetProject(long projectId)
        {
            try
            {
                return await apiService.GetProject(projectId);
            }
            catch (ApiException ex)
            {
                var statusCode = ex.StatusCode;
                var error = await ex.GetContentAsAsync<ErrorResponse>();
                Toast.MakeText(Application.Context, error.Message, ToastLength.Short).Show();
                return null;
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
                return null;
            }
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
            if (id == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}