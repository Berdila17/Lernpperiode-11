using System.Collections.ObjectModel;
using System.Text.Json;

namespace Lernperiode_10
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Workout> Workouts { get; set; }

        private readonly string filePath;

        private Workout? selectedWorkout;

        private IDispatcherTimer timer;
        private int remainingSeconds = 60;

        public MainPage()
        {
            InitializeComponent();

            Workouts = new ObservableCollection<Workout>();

            BindingContext = this;

            filePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "workouts.json"
            );

            timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += OnTimerTick;
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
                ErrorLabel.Text = "Bitte geben Sie einen Workout-Namen ein.";
                ErrorLabel.IsVisible = true;
                return;
            }

            Workout workout = new Workout();
            workout.Name = workoutName;

            Workouts.Add(workout);

            WorkoutNameEntry.Text = "";
            ErrorLabel.IsVisible = false;

            await SaveWorkouts();
        }

        private void OnSelectWorkoutClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            selectedWorkout = (Workout)button.CommandParameter;

            SelectedWorkoutLabel.Text =
                $"Ausgewählt: {selectedWorkout.Name}";
        }

        private async void OnAddExerciseClicked(object sender, EventArgs e)
        {
            if (selectedWorkout == null)
            {
                ErrorLabel.Text = "Bitte zuerst ein Workout auswählen.";
                ErrorLabel.IsVisible = true;
                return;
            }

            string exerciseName = ExerciseNameEntry.Text;

            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                return;
            }

            selectedWorkout.Exercises.Add(exerciseName);

            ExerciseNameEntry.Text = "";

            ErrorLabel.IsVisible = false;

            await SaveWorkouts();
        }

        private async void OnDeleteWorkoutClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            Workout workout = (Workout)button.CommandParameter;

            Workouts.Remove(workout);

            if (selectedWorkout == workout)
            {
                selectedWorkout = null;
                SelectedWorkoutLabel.Text = "Kein Workout ausgewählt";
            }

            await SaveWorkouts();
        }

        private void OnStartTimerClicked(object sender, EventArgs e)
        {
            remainingSeconds = 60;

            TimerLabel.Text = remainingSeconds.ToString();

            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            remainingSeconds--;

            TimerLabel.Text = remainingSeconds.ToString();

            if (remainingSeconds <= 0)
            {
                timer.Stop();

                TimerLabel.Text = "Pause beendet!";
            }
        }

        private async Task SaveWorkouts()
        {
            string json = JsonSerializer.Serialize(Workouts);

            await File.WriteAllTextAsync(filePath, json);
        }

        private async Task LoadWorkouts()
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            string json = await File.ReadAllTextAsync(filePath);

            ObservableCollection<Workout>? savedWorkouts =
                JsonSerializer.Deserialize<ObservableCollection<Workout>>(json);

            if (savedWorkouts == null)
            {
                return;
            }

            Workouts.Clear();

            foreach (Workout workout in savedWorkouts)
            {
                Workouts.Add(workout);
            }
        }
    }

    public class Workout
    {
        public string Name { get; set; } = "";

        public ObservableCollection<string> Exercises { get; set; }
            = new ObservableCollection<string>();
    }
}
