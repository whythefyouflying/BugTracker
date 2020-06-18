////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\MainActivity.cs </file>
///
/// <copyright file="MainActivity.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the main activity class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;
using Org.Apache.Http.Authentication;
using BugTrackerApp.Services;
using BugTrackerApp.Models;
using AndroidX.RecyclerView.Widget;
using Android.Webkit;
using AndroidX.SwipeRefreshLayout.Widget;
using Android.Graphics;
using System.Threading.Tasks;
using Google.Android.Material.FloatingActionButton;

namespace BugTrackerApp
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A main activity. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Activity(Label = "@string/projects", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        /// <summary>   The projects swipe container. </summary>
        private SwipeRefreshLayout mProjectsSwipeContainer;

        /// <summary>   The projects view. </summary>
        RecyclerView mProjectsView;
        /// <summary>   Manager for layout. </summary>
        RecyclerView.LayoutManager mLayoutManager;
        /// <summary>   The adapter. </summary>
        ProjectsListAdapter mAdapter;
        /// <summary>   List of projects. </summary>
        List<Project> mProjectsList;

        /// <summary>   The API service. </summary>
        private readonly IApiService apiService = ApiService.GetApiService();
        /// <summary>   The authentication token. </summary>
        private string authToken;

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
            }
            else
            {
                authToken = Intent.GetStringExtra("jwt_token");
            }

            mProjectsList = await getProjects();

            SetContentView(Resource.Layout.activity_main);

            mProjectsSwipeContainer = FindViewById<SwipeRefreshLayout>(Resource.Id.projectsSwipeContainer);
            mProjectsSwipeContainer.SetColorSchemeResources(Android.Resource.Color.HoloBlueBright,
                Android.Resource.Color.HoloGreenLight,
                Android.Resource.Color.HoloOrangeLight,
                Android.Resource.Color.HoloRedLight);

            mProjectsSwipeContainer.Refresh += async delegate
            {
                var refreshedProjects = await getProjects();
                if (refreshedProjects != null)
                {
                    mAdapter.clear();
                    mAdapter.addAll(refreshedProjects);
                }
                else
                {
                    Toast.MakeText(Application.Context, "Couldn't refresh", ToastLength.Short).Show();
                }
                mProjectsSwipeContainer.Refreshing = false;
            };

            mProjectsView = FindViewById<RecyclerView>(Resource.Id.projectsView);
            mProjectsView.AddItemDecoration(new DividerItemDecoration(Application.Context, DividerItemDecoration.Vertical));

            mLayoutManager = new LinearLayoutManager(this);
            mProjectsView.SetLayoutManager(mLayoutManager);

            mAdapter = new ProjectsListAdapter(mProjectsList);
            mAdapter.ItemClick += OnItemClick;
            mProjectsView.SetAdapter(mAdapter);
            
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            Google.Android.Material.FloatingActionButton.FloatingActionButton fab = FindViewById<Google.Android.Material.FloatingActionButton.FloatingActionButton>(Resource.Id.addProjectFab);

            fab.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(CreateProjectActivity));
                intent.PutExtra("jwt_token", authToken);
                StartActivity(intent);
            };
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
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
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
            if (id == Resource.Id.action_logout)
            {
                SecureStorage.Remove("jwt_token");
                var intent = new Intent(this, typeof(LoginActivity));
                StartActivity(intent);
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
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
            base.OnSaveInstanceState(outState);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the item click action. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="position"> The position. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        void OnItemClick(object sender, int position)
        {
            Project project = mAdapter.mProjectsList[position];
            var intent = new Intent(this, typeof(ProjectActivity));
            intent.PutExtra("jwt_token", authToken);
            intent.PutExtra("project_id", project.Id);
            StartActivity(intent);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the projects. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <returns>   An asynchronous result that yields the projects. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<List<Project>> getProjects()
        {
            try
            {
                return await apiService.GetProjects();
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
                return null;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A project view holder. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class ProjectViewHolder : RecyclerView.ViewHolder
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the title. </summary>
        ///
        /// <value> The title. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView Title { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the description. </summary>
        ///
        /// <value> The description. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView Description { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="itemView"> The item view. </param>
        /// <param name="listener"> The listener. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public ProjectViewHolder (View itemView, Action<int> listener)
            : base (itemView)
        {
            Title = itemView.FindViewById<TextView>(Resource.Id.projectCardTitleTextView);
            Description = itemView.FindViewById<TextView>(Resource.Id.projectCardDescriptionTextView);

            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   The projects list adapter. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class ProjectsListAdapter : RecyclerView.Adapter
    {
        /// <summary>   List of projects. </summary>
        public List<Project> mProjectsList;
        /// <summary>   Event queue for all listeners interested in ItemClick events. </summary>
        public event EventHandler<int> ItemClick;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectsList"> List of projects. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public ProjectsListAdapter (List<Project> projectsList)
        {
            mProjectsList = projectsList;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the create view holder action. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="parent">   The parent. </param>
        /// <param name="viewType"> Type of the view. </param>
        ///
        /// <returns>   A RecyclerView.ViewHolder. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                Inflate(Resource.Layout.project_card_view, parent, false);

            ProjectViewHolder vh = new ProjectViewHolder(itemView, OnClick);
            return vh;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the bind view holder action. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="holder">   The holder. </param>
        /// <param name="position"> The position. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ProjectViewHolder vh = holder as ProjectViewHolder;

            vh.Title.Text = (mProjectsList[position].Title);

            vh.Description.Text = (mProjectsList[position].Description);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the number of items. </summary>
        ///
        /// <value> The number of items. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override int ItemCount
        {
            get { return mProjectsList.Count;  }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Executes the click action. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="position"> The position. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Clears this object to its blank/initial state. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void clear()
        {
            mProjectsList.Clear();
            NotifyDataSetChanged();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds all. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projects"> The projects. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void addAll(List<Project> projects)
        {
            mProjectsList.AddRange(projects);
            NotifyDataSetChanged();
        }
    }
}
