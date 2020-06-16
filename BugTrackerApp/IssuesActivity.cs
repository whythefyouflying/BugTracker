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
    [Activity(Label = "Issues", Theme = "@style/AppTheme.NoActionBar", ParentActivity = typeof(ProjectActivity))]
    public class IssuesActivity : AppCompatActivity
    {
        private SwipeRefreshLayout mIssuesSwipeContainer;

        RecyclerView mIssuesView;
        RecyclerView.LayoutManager mLayoutManager;
        IssuesListAdapter mAdapter;
        List<Issue> mIssuesList;

        private readonly IApiService apiService = ApiService.GetApiService();
        private string authToken;
        private long projectId;

        AndroidX.AppCompat.Widget.SearchView searchView;

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

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("jwt_token", authToken);
            outState.PutLong("project_id", projectId);
            base.OnSaveInstanceState(outState);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_issues, menu);
            return true;
        }

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

        void OnItemClick(object sender, int position)
        {
            Issue issue = mAdapter.mIssuesList[position];
            Toast.MakeText(Application.Context, $"Opening {issue.Title}", ToastLength.Short).Show();
            //var intent = new Intent(this, typeof(IssueActivity));
            //intent.PutExtra("jwt_token", authToken);
            //intent.PutExtra("project_id", project.Id);
            //StartActivity(intent);
        }
    }

    public class IssueViewHolder : RecyclerView.ViewHolder
    {
        public TextView Number { get; set; }
        public TextView CreationTime { get; set; }
        public TextView Title { get; set; }
        public TextView Status { get; set; }

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

    public class IssuesListAdapter : RecyclerView.Adapter
    {
        public List<Issue> mIssuesList;
        public event EventHandler<int> ItemClick;

        public IssuesListAdapter(List<Issue> issuesList)
        {
            mIssuesList = issuesList;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                Inflate(Resource.Layout.issue_card_view, parent, false);

            IssueViewHolder vh = new IssueViewHolder(itemView, OnClick);
            return vh;
        }

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

        public override int ItemCount
        {
            get { return mIssuesList.Count; }
        }

        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        public void clear()
        {
            mIssuesList.Clear();
            NotifyDataSetChanged();
        }

        public void addAll(List<Issue> issues)
        {
            mIssuesList.AddRange(issues);
            NotifyDataSetChanged();
        }
    }
}