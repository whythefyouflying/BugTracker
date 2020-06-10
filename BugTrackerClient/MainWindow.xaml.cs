using BugTrackerClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BugTrackerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Project> projectsList = new ObservableCollection<Project>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            projectsListView.ItemsSource = projectsList;
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var httpClient = new HttpClient();
            var projectsClient = new ProjectsClient(httpClient);
            var projects = await projectsClient.GetProjectsAsync();
            foreach (GetProjectDto project in projects)
            {
                this.projectsList.Add(new Project { Title = project.Title, Description = project.Description });
            }
        }

        public class Project
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }
    }
}
