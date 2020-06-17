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
    [Activity(Label = "IssueActivity", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(IssuesActivity))]
    public class IssueActivity : AppCompatActivity
    {

        private SwipeRefreshLayout mCommentsSwipeContainer;

        RecyclerView mCommentsView;
        RecyclerView.LayoutManager mLayoutManager;
        CommentsListAdapter mAdapter;
        List<Comment> mCommentsList;

        private readonly IApiService apiService = ApiService.GetApiService();
        private string authToken;
        private long projectId;
        private int issueNumber;

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

        void OnItemClick(object sender, int position)
        {
        }
       
    }
    public class CommentViewHolder : RecyclerView.ViewHolder
    {
        public TextView Username { get; set; }
        public TextView CreationTime { get; set; }
        public TextView Body { get; set; }


        public CommentViewHolder(View itemView, Action<int> listener)
            : base(itemView)
        {
            Username = itemView.FindViewById<TextView>(Resource.Id.commentCardUsername);
            CreationTime = itemView.FindViewById<TextView>(Resource.Id.commentCardCreationTime);
            Body = itemView.FindViewById<TextView>(Resource.Id.commentCardBody);

            itemView.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    public class CommentsListAdapter : RecyclerView.Adapter
    {
        public List<Comment> mCommentsList;
        public event EventHandler<int> ItemClick;

        public CommentsListAdapter(List<Comment> commentsList)
        {
            mCommentsList = commentsList;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                Inflate(Resource.Layout.comment_card_view, parent, false);

            CommentViewHolder vh = new CommentViewHolder(itemView, OnClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CommentViewHolder vh = holder as CommentViewHolder;

            vh.Username.Text = mCommentsList[position].User.Username;

            long now = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeMilliseconds();
            long createdAt = ((DateTimeOffset)mCommentsList[position].CreatedAt.ToLocalTime()).ToUnixTimeMilliseconds();

            vh.CreationTime.Text = DateUtils.GetRelativeTimeSpanString(createdAt, now, DateUtils.MinuteInMillis);

            vh.Body.Text = mCommentsList[position].Body;
        }

        public override int ItemCount
        {
            get { return mCommentsList.Count; }
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        public void Clear()
        {
            mCommentsList.Clear();
            NotifyDataSetChanged();
        }

        public void AddAll(List<Comment> comments)
        {
            mCommentsList.AddRange(comments);
            NotifyDataSetChanged();
        }
    }
}