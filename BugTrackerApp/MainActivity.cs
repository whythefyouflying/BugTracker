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
    [Activity(Label = "@string/projects", Theme = "@style/AppTheme.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        private SwipeRefreshLayout mProjectsSwipeContainer;

        RecyclerView mProjectsView;
        RecyclerView.LayoutManager mLayoutManager;
        ProjectsListAdapter mAdapter;
        List<Project> mProjectsList;

        private readonly IApiService apiService = ApiService.GetApiService();
        private string authToken;

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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("jwt_token", authToken);
            base.OnSaveInstanceState(outState);
        }

        void OnItemClick(object sender, int position)
        {
            Project project = mAdapter.mProjectsList[position];
            var intent = new Intent(this, typeof(ProjectActivity));
            intent.PutExtra("jwt_token", authToken);
            intent.PutExtra("project_id", project.Id);
            StartActivity(intent);
        }

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

    public class ProjectViewHolder : RecyclerView.ViewHolder
    {
        public TextView Title { get; set; }
        public TextView Description { get; set; }

        public ProjectViewHolder (View itemView, Action<int> listener)
            : base (itemView)
        {
            Title = itemView.FindViewById<TextView>(Resource.Id.projectCardTitleTextView);
            Description = itemView.FindViewById<TextView>(Resource.Id.projectCardDescriptionTextView);

            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }

    public class ProjectsListAdapter : RecyclerView.Adapter
    {
        public List<Project> mProjectsList;
        public event EventHandler<int> ItemClick;

        public ProjectsListAdapter (List<Project> projectsList)
        {
            mProjectsList = projectsList;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                Inflate(Resource.Layout.project_card_view, parent, false);

            ProjectViewHolder vh = new ProjectViewHolder(itemView, OnClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ProjectViewHolder vh = holder as ProjectViewHolder;

            vh.Title.Text = (mProjectsList[position].Title);

            vh.Description.Text = (mProjectsList[position].Description);
        }

        public override int ItemCount
        {
            get { return mProjectsList.Count;  }
        }

        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }

        public void clear()
        {
            mProjectsList.Clear();
            NotifyDataSetChanged();
        }

        public void addAll(List<Project> projects)
        {
            mProjectsList.AddRange(projects);
            NotifyDataSetChanged();
        }
    }
}
