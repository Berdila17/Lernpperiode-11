using System.Collections.ObjectModel;

namespace WorkoutTracker;

public partial class MainPage : ContentPage
{
    public ObservableCollection<string> Workouts { get; set; }

    public MainPage()
    {
        InitializeComponent();

        Workouts = new ObservableCollection<string>();

        BindingContext = this;
    }

    private void OnCreateWorkoutClicked(object sender, EventArgs e)
    {
        string workoutName = WorkoutNameEntry.Text;

        if (string.IsNullOrWhiteSpace(workoutName))
        {
            ErrorLabel.Text = "Bitte geben Sie einen Namen für das Workout ein.";
            ErrorLabel.IsVisible = true;
            return;
        }

        Workouts.Add(workoutName);

        WorkoutNameEntry.Text = "";

        ErrorLabel.IsVisible = false;
    }
}
