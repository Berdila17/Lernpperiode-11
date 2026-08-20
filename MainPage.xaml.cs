using System.Collections.ObjectModel;
using System.Text.Json;

namespace WorkoutTracker;

public partial class MainPage : ContentPage
{
    public ObservableCollection<string> Workouts { get; set; }

    private readonly string filePath;

    public MainPage()
    {
        InitializeComponent();

        Workouts = new ObservableCollection<string>();

        BindingContext = this;

        // Speicherort für unsere Datei
        filePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "workouts.json"
        );
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadWorkouts();
    }

    private async void OnCreateWorkoutClicked(object sender, EventArgs e)
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

        // Workouts nach jeder Änderung speichern
        await SaveWorkouts();
    }

    private async Task SaveWorkouts()
    {
        string json = JsonSerializer.Serialize(Workouts);

        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task LoadWorkouts()
    {
        // Prüfen, ob bereits eine Datei existiert
        if (!File.Exists(filePath))
        {
            return;
        }

        string json = await File.ReadAllTextAsync(filePath);

        ObservableCollection<string>? savedWorkouts =
            JsonSerializer.Deserialize<ObservableCollection<string>>(json);

        if (savedWorkouts == null)
        {
            return;
        }

        Workouts.Clear();

        foreach (string workout in savedWorkouts)
        {
            Workouts.Add(workout);
        }
    }
}
