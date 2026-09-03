using System.Collections.ObjectModel;
using System.Text.Json;

namespace Lernperiode_10
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Workout> Workouts { get; set; }
            = new ObservableCollection<Workout>();

        public ObservableCollection<Exercise> SelectedExercises { get; set; }
            = new ObservableCollection<Exercise>();

        public ObservableCollection<TrainingSession> History { get; set; }
            = new ObservableCollection<TrainingSession>();

        private Workout? selectedWorkout;
        private Exercise? selectedExercise;

        private readonly string filePath;

        private IDispatcherTimer timer;
        private int remainingSeconds = 60;

        private bool dataLoaded = false;


        public MainPage()
        {
            InitializeComponent();

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

            if (!dataLoaded)
            {
                await LoadData();
                dataLoaded = true;
            }
        }


        // WORKOUT ERSTELLEN

        private async void OnCreateWorkoutClicked(object sender, EventArgs e)
        {
            string workoutName = WorkoutNameEntry.Text;

            if (string.IsNullOrWhiteSpace(workoutName))
            {
                ErrorLabel.Text = "Bitte Workout-Name eingeben.";
                return;
            }

            Workout workout = new Workout
            {
                Name = workoutName
            };

            Workouts.Add(workout);

            WorkoutNameEntry.Text = "";
            ErrorLabel.Text = "";

            await SaveData();
        }


        // WORKOUT ÖFFNEN

        private void OnOpenWorkoutClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            selectedWorkout = (Workout)button.CommandParameter;

            DetailWorkoutNameLabel.Text = selectedWorkout.Name;
            EditWorkoutNameEntry.Text = selectedWorkout.Name;

            LoadSelectedExercises();

            ShowDetail();
        }


        private void LoadSelectedExercises()
        {
            SelectedExercises.Clear();

            if (selectedWorkout == null)
                return;

            foreach (Exercise exercise in selectedWorkout.Exercises)
            {
                SelectedExercises.Add(exercise);
            }
        }


        // WORKOUT BEARBEITEN

        private async void OnUpdateWorkoutClicked(object sender, EventArgs e)
        {
            if (selectedWorkout == null)
                return;

            string newName = EditWorkoutNameEntry.Text;

            if (string.IsNullOrWhiteSpace(newName))
            {
                DetailErrorLabel.Text = "Bitte einen Namen eingeben.";
                return;
            }

            selectedWorkout.Name = newName;

            DetailWorkoutNameLabel.Text = newName;
            DetailErrorLabel.Text = "";

            RefreshWorkoutList();

            await SaveData();
        }


        // ÜBUNG HINZUFÜGEN / BEARBEITEN

        private async void OnSaveExerciseClicked(object sender, EventArgs e)
        {
            if (selectedWorkout == null)
            {
                DetailErrorLabel.Text = "Kein Workout ausgewählt.";
                return;
            }

            string name = ExerciseNameEntry.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                DetailErrorLabel.Text = "Bitte Übungsname eingeben.";
                return;
            }

            if (!int.TryParse(SetsEntry.Text, out int sets))
            {
                DetailErrorLabel.Text = "Sätze müssen eine Zahl sein.";
                return;
            }

            if (!int.TryParse(RepsEntry.Text, out int reps))
            {
                DetailErrorLabel.Text = "Wiederholungen müssen eine Zahl sein.";
                return;
            }

            if (!double.TryParse(WeightEntry.Text, out double weight))
            {
                DetailErrorLabel.Text = "Gewicht muss eine Zahl sein.";
                return;
            }


            if (selectedExercise == null)
            {
                Exercise exercise = new Exercise
                {
                    Name = name,
                    Sets = sets,
                    Reps = reps,
                    Weight = weight
                };

                selectedWorkout.Exercises.Add(exercise);
                SelectedExercises.Add(exercise);
            }
            else
            {
                selectedExercise.Name = name;
                selectedExercise.Sets = sets;
                selectedExercise.Reps = reps;
                selectedExercise.Weight = weight;

                selectedExercise = null;

                RefreshExerciseList();
            }


            ClearExerciseFields();

            DetailErrorLabel.Text = "";
            SaveExerciseButton.Text = "Übung hinzufügen";

            await SaveData();
        }


        private void OnEditExerciseClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            selectedExercise = (Exercise)button.CommandParameter;

            ExerciseNameEntry.Text = selectedExercise.Name;
            SetsEntry.Text = selectedExercise.Sets.ToString();
            RepsEntry.Text = selectedExercise.Reps.ToString();
            WeightEntry.Text = selectedExercise.Weight.ToString();

            SaveExerciseButton.Text = "Änderungen speichern";
        }


        //  ÜBUNG LÖSCHEN

        private async void OnDeleteExerciseClicked(object sender, EventArgs e)
        {
            if (selectedWorkout == null)
                return;

            Button button = (Button)sender;

            Exercise exercise = (Exercise)button.CommandParameter;

            
            selectedWorkout.Exercises.Remove(exercise);

            
            SelectedExercises.Remove(exercise);


          
            if (selectedExercise == exercise)
            {
                selectedExercise = null;

                ClearExerciseFields();

                SaveExerciseButton.Text = "Übung hinzufügen";
            }

            DetailErrorLabel.Text =
                $"Übung {exercise.Name} wurde gelöscht.";

           
            await SaveData();
        }


        private void ClearExerciseFields()
        {
            ExerciseNameEntry.Text = "";
            SetsEntry.Text = "";
            RepsEntry.Text = "";
            WeightEntry.Text = "";
        }


        // TRAINING ABSCHLIESSEN

        private async void OnFinishTrainingClicked(object sender, EventArgs e)
        {
            if (selectedWorkout == null)
                return;

            TrainingSession session = new TrainingSession
            {
                WorkoutName = selectedWorkout.Name,
                Date = DateTime.Now,
                ExerciseCount = selectedWorkout.Exercises.Count
            };

            History.Insert(0, session);

            DetailErrorLabel.Text = "Training wurde gespeichert.";

            await SaveData();
        }


        // NAVIGATION

        private void OnOverviewClicked(object sender, EventArgs e)
        {
            ShowOverview();
        }


        private void OnDetailClicked(object sender, EventArgs e)
        {
            ShowDetail();
        }


        private void OnTimerPageClicked(object sender, EventArgs e)
        {
            OverviewView.IsVisible = false;
            DetailView.IsVisible = false;
            TimerView.IsVisible = true;
        }


        private void ShowOverview()
        {
            OverviewView.IsVisible = true;
            DetailView.IsVisible = false;
            TimerView.IsVisible = false;
        }


        private void ShowDetail()
        {
            OverviewView.IsVisible = false;
            DetailView.IsVisible = true;
            TimerView.IsVisible = false;
        }


        // TIMER

        private void OnStartTimerClicked(object sender, EventArgs e)
        {
            timer.Start();
        }


        private void OnPauseTimerClicked(object sender, EventArgs e)
        {
            timer.Stop();
        }


        private void OnResetTimerClicked(object sender, EventArgs e)
        {
            timer.Stop();

            remainingSeconds = 60;

            TimerLabel.Text = "60";
        }


        private void OnTimerTick(object sender, EventArgs e)
        {
            remainingSeconds--;

            TimerLabel.Text = remainingSeconds.ToString();

            if (remainingSeconds <= 0)
            {
                timer.Stop();

                TimerLabel.Text = "Pause beendet!";

                remainingSeconds = 60;
            }
        }


        // SPEICHERN

        private async Task SaveData()
        {
            AppData data = new AppData
            {
                Workouts = Workouts.ToList(),
                History = History.ToList()
            };

            string json = JsonSerializer.Serialize(data);

            await File.WriteAllTextAsync(filePath, json);
        }


        private async Task LoadData()
        {
            if (!File.Exists(filePath))
                return;

            string json =
                await File.ReadAllTextAsync(filePath);

            AppData? data =
                JsonSerializer.Deserialize<AppData>(json);

            if (data == null)
                return;


            Workouts.Clear();

            foreach (Workout workout in data.Workouts)
            {
                Workouts.Add(workout);
            }


            History.Clear();

            foreach (TrainingSession session in data.History)
            {
                History.Add(session);
            }
        }


        private void RefreshWorkoutList()
        {
            BindableLayout.SetItemsSource(WorkoutList, null);
            BindableLayout.SetItemsSource(WorkoutList, Workouts);
        }


        private void RefreshExerciseList()
        {
            BindableLayout.SetItemsSource(ExerciseList, null);
            BindableLayout.SetItemsSource(
                ExerciseList,
                SelectedExercises
            );
        }
    }


    public class Workout
    {
        public string Name { get; set; } = "";

        public ObservableCollection<Exercise> Exercises { get; set; }
            = new ObservableCollection<Exercise>();
    }


    public class Exercise
    {
        public string Name { get; set; } = "";

        public int Sets { get; set; }

        public int Reps { get; set; }

        public double Weight { get; set; }


        public string DisplayText
        {
            get
            {
                return $"{Name} - {Sets} x {Reps} - {Weight} kg";
            }
        }
    }


    public class TrainingSession
    {
        public string WorkoutName { get; set; } = "";

        public DateTime Date { get; set; }

        public int ExerciseCount { get; set; }


        public string DisplayText
        {
            get
            {
                return $"{Date:dd.MM.yyyy HH:mm} - {WorkoutName} ({ExerciseCount} Übungen)";
            }
        }
    }


    public class AppData
    {
        public List<Workout> Workouts { get; set; }
            = new List<Workout>();

        public List<TrainingSession> History { get; set; }
            = new List<TrainingSession>();
    }
}
