////////////////////////////////////////////////////////////////////////////////////////////////////
/// <file>  BugTrackerApp\IssueActivity.cs </file>
///
/// <copyright file="IssueActivity.cs" company="Dawid Osuchowski">
/// Copyright (c) 2020 Dawid Osuchowski. All rights reserved.
/// </copyright>
///
/// <summary>   Implements the issue activity class. </summary>
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
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using BugTrackerApp.Models;
using BugTrackerApp.Services;
using Google.Android.Material.Chip;
using Refit;
using Xamarin.Essentials;

namespace BugTrackerApp
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   An issue activity. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    [Activity(Label = "IssueActivity", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(IssuesActivity))]
    public class IssueActivity : AppCompatActivity
    {

        /// <summary>   The comments swipe container. </summary>
        private SwipeRefreshLayout mCommentsSwipeContainer;

        /// <summary>   The comments view. </summary>
        RecyclerView mCommentsView;
        /// <summary>   Manager for layout. </summary>
        RecyclerView.LayoutManager mLayoutManager;
        /// <summary>   The adapter. </summary>
        CommentsListAdapter mAdapter;
        /// <summary>   List of comments. </summary>
        List<Comment> mCommentsList;

        /// <summary>   The API service. </summary>
        private readonly IApiService apiService = ApiService.GetApiService();
        /// <summary>   The authentication token. </summary>
        private string authToken;
        /// <summary>   Identifier for the project. </summary>
        private long projectId;
        /// <summary>   The issue number. </summary>
        private int issueNumber;

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
                issueNumber = savedInstanceState.GetInt("issue_number");
            }
            else
            {
                authToken = Intent.GetStringExtra("jwt_token");
                projectId = Intent.GetLongExtra("project_id", 0L);
                issueNumber = Intent.GetIntExtra("issue_number", 0);
            }

            mCommentsList = await GetComments(projectId, issueNumber);

            SetContentView(Resource.Layout.activity_issue);

            mCommentsSwipeContainer = FindViewById<SwipeRefreshLayout>(Resource.Id.commentsSwipeContainer);
            mCommentsSwipeContainer.SetColorSchemeResources(Android.Resource.Color.HoloBlueBright,
                Android.Resource.Color.HoloGreenLight,
                Android.Resource.Color.HoloOrangeLight,
                Android.Resource.Color.HoloRedLight);

            mCommentsSwipeContainer.Refresh += async delegate
            {
                var refreshedComments = await GetComments(projectId, issueNumber);
                if (refreshedComments != null)
                {
                    mAdapter.Clear();
                    mAdapter.AddAll(refreshedComments);
                }
                else
                {
                    Toast.MakeText(Application.Context, "Couldn't refresh", ToastLength.Short).Show();
                }
                mCommentsSwipeContainer.Refreshing = false;
            };

            mCommentsView = FindViewById<RecyclerView>(Resource.Id.commentsView);
            mCommentsView.AddItemDecoration(new AndroidX.RecyclerView.Widget.DividerItemDecoration(Application.Context, DividerItemDecoration.Vertical));

            mLayoutManager = new LinearLayoutManager(this);
            mCommentsView.SetLayoutManager(mLayoutManager);

            mAdapter = new CommentsListAdapter(mCommentsList);
            mAdapter.ItemClick += OnItemClick;
            mCommentsView.SetAdapter(mAdapter);

            AndroidX.AppCompat.Widget.Toolbar toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.issueToolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            Title = " ";

            // Create your application here

            Issue issue;

            try
            {
                issue = await GetIssue(projectId, issueNumber);

                TextView issueNumberText = FindViewById<TextView>(Resource.Id.issueNumber);
                Chip issueStatus = FindViewById<Chip>(Resource.Id.issueStatus);
                TextView issueCreationTime = FindViewById<TextView>(Resource.Id.issueCreationTime);
                TextView issueTitle = FindViewById<TextView>(Resource.Id.issueTitleTextView);
                TextView issueBody = FindViewById<TextView>(Resource.Id.issueBodyTextView);

                issueNumberText.Text = $"#{issue.Number}";
                issueStatus.Text = issue.Status;

                long now = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeMilliseconds();
                long createdAt = ((DateTimeOffset)issue.CreatedAt.ToLocalTime()).ToUnixTimeMilliseconds();
                
                issueCreationTime.Text = DateUtils.GetRelativeTimeSpanString(createdAt, now, DateUtils.MinuteInMillis);

                issueTitle.Text = issue.Title;
                issueBody.Text = issue.Body;
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
            }

            Button commentButton = FindViewById<Button>(Resource.Id.createCommentButton);
            EditText commentBody = FindViewById<EditText>(Resource.Id.commentBody);

            commentButton.Click += async (sender, e) =>
            {
                PostComment newComment = new PostComment();
                newComment.Body = commentBody.Text;
                _ = await PostComment(projectId, issueNumber, newComment, authToken);
            };
  
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets an issue. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        ///
        /// <returns>   An asynchronous result that yields the issue. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<Issue> GetIssue(long projectId, int issueNumber)
        {
            try
            {
                return await apiService.GetIssue(projectId, issueNumber);
            }
            catch (ApiException ex)
            {
                var statusCode = ex.StatusCode;
                var error = await ex.GetContentAsAsync<ErrorResponse>();
                Toast.MakeText(Application.Context, error.Message, ToastLength.Short).Show();
                Finish();
                return null;
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
                Finish();
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the comments. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        ///
        /// <returns>   An asynchronous result that yields the comments. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<List<Comment>> GetComments(long projectId, int issueNumber)
        {
            try
            {
                return await apiService.GetComments(projectId, issueNumber);
            }
            catch (ApiException ex)
            {
                var statusCode = ex.StatusCode;
                var error = await ex.GetContentAsAsync<ErrorResponse>();
                Toast.MakeText(Application.Context, error.Message, ToastLength.Short).Show();
                Finish();
                return null;
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(Application.Context, ex.Message, ToastLength.Short).Show();
                Finish();
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Posts a comment. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="projectId">    Identifier for the project. </param>
        /// <param name="issueNumber">  The issue number. </param>
        /// <param name="newComment">   The new comment. </param>
        /// <param name="bearerToken">  The bearer token. </param>
        ///
        /// <returns>   An asynchronous result that yields a Comment. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<Comment> PostComment(long projectId, int issueNumber, PostComment newComment, string bearerToken)
        {
            try
            {
                return await apiService.PostComment(projectId, issueNumber, newComment, bearerToken);
            }
            catch (ApiException ex)
            {
                var statusCode = ex.StatusCode;
                var error = await ex.GetContentAsAsync<ErrorResponse>();
                Toast.MakeText(Application.Context, error.Message, ToastLength.Short).Show();
                return null;
            }
            catch (System.Exception ex)
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
            outState.PutInt("issue_number", issueNumber);
            base.OnSaveInstanceState(outState);
        }

        // No need for a menu currently
        //public override bool OnCreateOptionsMenu(IMenu menu)
        //{
        //    MenuInflater.Inflate(Resource.Menu.menu_issue, menu);
        //    return true;
        //}

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
            if (id == Resource.Id.action_create_comment)
            {
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
        }
       
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A comment view holder. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class CommentViewHolder : RecyclerView.ViewHolder
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the username. </summary>
        ///
        /// <value> The username. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView Username { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the creation time. </summary>
        ///
        /// <value> The creation time. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView CreationTime { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the body. </summary>
        ///
        /// <value> The body. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextView Body { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="itemView"> The item view. </param>
        /// <param name="listener"> The listener. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public CommentViewHolder(View itemView, Action<int> listener)
            : base(itemView)
        {
            Username = itemView.FindViewById<TextView>(Resource.Id.commentCardUsername);
            CreationTime = itemView.FindViewById<TextView>(Resource.Id.commentCardCreationTime);
            Body = itemView.FindViewById<TextView>(Resource.Id.commentCardBody);

            itemView.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   The comments list adapter. </summary>
    ///
    /// <remarks>   Dawid, 18/06/2020. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class CommentsListAdapter : RecyclerView.Adapter
    {
        /// <summary>   List of comments. </summary>
        public List<Comment> mCommentsList;
        /// <summary>   Event queue for all listeners interested in ItemClick events. </summary>
        public event EventHandler<int> ItemClick;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="commentsList"> List of comments. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public CommentsListAdapter(List<Comment> commentsList)
        {
            mCommentsList = commentsList;
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
                Inflate(Resource.Layout.comment_card_view, parent, false);

            CommentViewHolder vh = new CommentViewHolder(itemView, OnClick);
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
            CommentViewHolder vh = holder as CommentViewHolder;

            vh.Username.Text = mCommentsList[position].User.Username;

            long now = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeMilliseconds();
            long createdAt = ((DateTimeOffset)mCommentsList[position].CreatedAt.ToLocalTime()).ToUnixTimeMilliseconds();

            vh.CreationTime.Text = DateUtils.GetRelativeTimeSpanString(createdAt, now, DateUtils.MinuteInMillis);

            vh.Body.Text = mCommentsList[position].Body;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the number of items. </summary>
        ///
        /// <value> The number of items. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override int ItemCount
        {
            get { return mCommentsList.Count; }
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
        /// <summary>   Clears the Comments RecyclerView to its blank/initial state. </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Clear()
        {
            mCommentsList.Clear();
            NotifyDataSetChanged();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds all Comments to the RecyclerView </summary>
        ///
        /// <remarks>   Dawid, 18/06/2020. </remarks>
        ///
        /// <param name="comments"> The comments. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void AddAll(List<Comment> comments)
        {
            mCommentsList.AddRange(comments);
            NotifyDataSetChanged();
        }
    }
}