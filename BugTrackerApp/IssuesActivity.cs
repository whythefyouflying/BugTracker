////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\IssuesActivity.cs </file>
///
/// <copyright file="IssuesActivity.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the issues activity class. </summary>
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Refit;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   The issues activity. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Activity(Label = "Issues", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(ProjectActivity))]
    public class IssuesActivity : AppCompatActivity
    {
        /// <summary>   The issues swipe container. </summary>
        private SwipeRefreshLayout mIssuesSwipeContainer;

        /// <summary>   The issues view. </summary>
        RecyclerView mIssuesView;
        /// <summary>   Manager for layout. </summary>
        RecyclerView.LayoutManager mLayoutManager;
        /// <summary>   The adapter. </summary>
        IssuesListAdapter mAdapter;
        /// <summary>   List of issues. </summary>
        List<Issue> mIssuesList;

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
                projectId = savedInstanceState.GetLong("projectId");
            }
            else
            {
                authToken = Intent.GetStringExtra("jwt_token");
                projectId = Intent.GetLongExtra("project_id", 0L);
            }

            mIssuesList = await GetIssues(projectId);

            SetContentView(Resource.Layout.activity_issues);

            mIssuesSwipeContainer = FindViewById<SwipeRefreshLayout>(Resource.Id.issuesSwipeContainer);
            mIssuesSwipeContainer.SetColorSchemeResources(Android.Resource.Color.HoloBlueBright,
                Android.Resource.Color.HoloGreenLight,
                Android.Resource.Color.HoloOrangeLight,
                Android.Resource.Color.HoloRedLight);

            mIssuesSwipeContainer.Refresh += async delegate
            {
                var refreshedIssues = await GetIssues(projectId);
                if (refreshedIssues != null)
                {
                    mAdapter.clear();
                    mAdapter.addAll(refreshedIssues);
                }
                else
                {
                    Toast.MakeText(Application.Context, "Couldn't refresh", ToastLength.Short).Show();
                }
                mIssuesSwipeContainer.Refreshing = false;
            };

            mIssuesView = FindViewById<RecyclerView>(Resource.Id.issuesView);
            mIssuesView.AddItemDecoration(new DividerItemDecoration(Application.Context, DividerItemDecoration.Vertical));

            mLayoutManager = new LinearLayoutManager(this);
            mIssuesView.SetLayoutManager(mLayoutManager);

            mAdapter = new IssuesListAdapter(mIssuesList);
            mAdapter.ItemClick += OnItemClick;
            mIssuesView.SetAdapter(mAdapter);


            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.issuesToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);


            // Create your application here
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the issues. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        ///
        /// <returns>   An asynchronous result that yields the issues. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<List<Issue>> GetIssues(long projectId)
        {
            try
            {
                return await apiService.GetIssues(projectId);
            }
            catch (ApiException ex)
            {
                var statusCode = ex.StatusCode;
                var error = await ex.GetContentAsAsync<string>();
                Toast.MakeText(Application.Context, error, ToastLength.Short).Show();
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
            MenuInflater.Inflate(Resource.Menu.menu_issues, menu);
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
            if (id == Resource.Id.action_open_create_issue)
            {
                var intent = new Intent(this, typeof(CreateIssueActivity));
                intent.PutExtra("jwt_token", authToken);
                intent.PutExtra("project_id", projectId);
                StartActivity(intent);
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
        /// <summary>   Executes the item click action. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="position"> The position. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        void OnItemClick(object sender, int position)
        {
            Issue issue = mAdapter.mIssuesList[position];
            var intent = new Intent(this, typeof(IssueActivity));
            intent.PutExtra("jwt_token", authToken);
            intent.PutExtra("project_id", projectId);
            intent.PutExtra("issue_number", issue.Number);
            StartActivity(intent);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An issue view holder. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class IssueViewHolder : RecyclerView.ViewHolder
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the number of.  </summary>
        ///
        /// <value> The number. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView Number { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the creation time. </summary>
        ///
        /// <value> The creation time. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView CreationTime { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the title. </summary>
        ///
        /// <value> The title. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView Title { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the status. </summary>
        ///
        /// <value> The status. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView Status { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="itemView"> The item view. </param>
        /// <param name="listener"> The listener. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public IssueViewHolder(View itemView, Action<int> listener)
            : base(itemView)
        {
            Number = itemView.FindViewById<TextView>(Resource.Id.issueCardNumber);
            CreationTime = itemView.FindViewById<TextView>(Resource.Id.issueCardCreationTime);
            Title = itemView.FindViewById<TextView>(Resource.Id.issueCardTitleTextView);
            Status = itemView.FindViewById<TextView>(Resource.Id.issueCardStatusTextView);

            itemView.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   The issues list adapter. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class IssuesListAdapter : RecyclerView.Adapter
    {
        /// <summary>   List of issues. </summary>
        public List<Issue> mIssuesList;
        /// <summary>   Event queue for all listeners interested in ItemClick events. </summary>
        public event EventHandler<int> ItemClick;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="issuesList">   List of issues. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public IssuesListAdapter(List<Issue> issuesList)
        {
            mIssuesList = issuesList;
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
                Inflate(Resource.Layout.issue_card_view, parent, false);

            IssueViewHolder vh = new IssueViewHolder(itemView, OnClick);
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
            IssueViewHolder vh = holder as IssueViewHolder;

            vh.Number.Text = $"#{mIssuesList[position].Number}";

            long now = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeMilliseconds();
            long createdAt = ((DateTimeOffset)mIssuesList[position].CreatedAt.ToLocalTime()).ToUnixTimeMilliseconds();

            vh.CreationTime.Text = DateUtils.GetRelativeTimeSpanString(createdAt, now, DateUtils.MinuteInMillis);

            vh.Title.Text = mIssuesList[position].Title;

            vh.Status.Text = mIssuesList[position].Status;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the number of items. </summary>
        ///
        /// <value> The number of items. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override int ItemCount
        {
            get { return mIssuesList.Count; }
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
            ItemClick?.Invoke(this, position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Clears the RecyclerView to its blank/initial state. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void clear()
        {
            mIssuesList.Clear();
            NotifyDataSetChanged();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds all Issues to the RecyclerView. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="issues">   The issues. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void addAll(List<Issue> issues)
        {
            mIssuesList.AddRange(issues);
            NotifyDataSetChanged();
        }
    }
}