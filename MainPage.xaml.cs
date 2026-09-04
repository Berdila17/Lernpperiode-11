using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;

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

        public ObservableCollection<string> ProgressItems { get; set; }
            = new ObservableCollection<string>();

        private Workout? selectedWorkout;
        private Exercise? selectedExercise;

        private readonly string filePath;

        private IDispatcherTimer timer;

        private int timerDurationSeconds = 60;
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
                DetailErrorLabel.Text =
                    "Bitte einen Workout-Namen eingeben.";

                return;
            }

            selectedWorkout.Name = newName;

            DetailWorkoutNameLabel.Text = newName;
            DetailErrorLabel.Text = "";

            RefreshWorkoutList();

            await SaveData();
        }


   
        // WORKOUT LÖSCHEN

        private async void OnDeleteWorkoutClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            Workout workout = (Workout)button.CommandParameter;

            Workouts.Remove(workout);

            if (selectedWorkout == workout)
            {
                selectedWorkout = null;
                SelectedExercises.Clear();
            }

            await SaveData();
        }


       
        // NEU: WORKOUT DUPLIZIEREN
    

        private async void OnDuplicateWorkoutClicked(
            object sender,
            EventArgs e)
        {
            Button button = (Button)sender;

            Workout original =
                (Workout)button.CommandParameter;


            Workout copy = new Workout
            {
                Name = original.Name + " Kopie"
            };


            foreach (Exercise exercise in original.Exercises)
            {
                Exercise exerciseCopy = new Exercise
                {
                    Name = exercise.Name,
                    Sets = exercise.Sets,
                    Reps = exercise.Reps,
                    Weight = exercise.Weight
                };

                copy.Exercises.Add(exerciseCopy);
            }


            Workouts.Add(copy);

            ErrorLabel.Text =
                $"{original.Name} wurde dupliziert.";

            await SaveData();
        }



        // ÜBUNG HINZUFÜGEN / BEARBEITEN
        

        private async void OnSaveExerciseClicked(
            object sender,
            EventArgs e)
        {
            if (selectedWorkout == null)
            {
                DetailErrorLabel.Text =
                    "Kein Workout ausgewählt.";

                return;
            }

            string name = ExerciseNameEntry.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                DetailErrorLabel.Text =
                    "Bitte Übungsname eingeben.";

                return;
            }


            if (!int.TryParse(SetsEntry.Text, out int sets))
            {
                DetailErrorLabel.Text =
                    "Sätze müssen eine Zahl sein.";

                return;
            }


            if (!int.TryParse(RepsEntry.Text, out int reps))
            {
                DetailErrorLabel.Text =
                    "Wiederholungen müssen eine Zahl sein.";

                return;
            }


            if (!double.TryParse(
                WeightEntry.Text,
                out double weight))
            {
                DetailErrorLabel.Text =
                    "Gewicht muss eine Zahl sein.";

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

            SaveExerciseButton.Text =
                "Übung hinzufügen";

            await SaveData();
        }


        private void OnEditExerciseClicked(
            object sender,
            EventArgs e)
        {
            Button button = (Button)sender;

            selectedExercise =
                (Exercise)button.CommandParameter;

            ExerciseNameEntry.Text =
                selectedExercise.Name;

            SetsEntry.Text =
                selectedExercise.Sets.ToString();

            RepsEntry.Text =
                selectedExercise.Reps.ToString();

            WeightEntry.Text =
                selectedExercise.Weight.ToString();

            SaveExerciseButton.Text =
                "Änderungen speichern";
        }


       
        // ÜBUNG LÖSCHEN
    

        private async void OnDeleteExerciseClicked(
            object sender,
            EventArgs e)
        {
            if (selectedWorkout == null)
                return;

            Button button = (Button)sender;

            Exercise exercise =
                (Exercise)button.CommandParameter;


            selectedWorkout.Exercises.Remove(exercise);

            SelectedExercises.Remove(exercise);


            if (selectedExercise == exercise)
            {
                selectedExercise = null;

                ClearExerciseFields();

                SaveExerciseButton.Text =
                    "Übung hinzufügen";
            }


            DetailErrorLabel.Text =
                $"{exercise.Name} wurde gelöscht.";

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
      

        private async void OnFinishTrainingClicked(
            object sender,
            EventArgs e)
        {
            if (selectedWorkout == null)
                return;


            TrainingSession session =
                new TrainingSession
                {
                    WorkoutName = selectedWorkout.Name,

                    Date = DateTime.Now,

                    ExerciseCount =
                        selectedWorkout.Exercises.Count
                };


            // Werte der Übungen für die Historie kopieren

            foreach (Exercise exercise
                     in selectedWorkout.Exercises)
            {
                ExerciseSnapshot snapshot =
                    new ExerciseSnapshot
                    {
                        Name = exercise.Name,
                        Sets = exercise.Sets,
                        Reps = exercise.Reps,
                        Weight = exercise.Weight
                    };

                session.ExerciseResults.Add(snapshot);
            }


            History.Insert(0, session);

            DetailErrorLabel.Text =
                "Training wurde gespeichert.";

            await SaveData();
        }


       
        // NEU: FORTSCHRITT ANZEIGEN
  

        private void OnShowProgressClicked(
            object sender,
            EventArgs e)
        {
            ProgressItems.Clear();

            string exerciseName =
                ProgressExerciseEntry.Text;


            if (string.IsNullOrWhiteSpace(exerciseName))
            {
                ProgressItems.Add(
                    "Bitte eine Übung eingeben.");

                return;
            }


            foreach (TrainingSession session
                     in History.OrderBy(x => x.Date))
            {
                foreach (ExerciseSnapshot exercise
                         in session.ExerciseResults)
                {
                    if (exercise.Name.Equals(
                        exerciseName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        string text =
                            $"{session.Date:dd.MM.yyyy}: " +
                            $"{exercise.Sets} x {exercise.Reps} " +
                            $"mit {exercise.Weight} kg";

                        ProgressItems.Add(text);
                    }
                }
            }


            if (ProgressItems.Count == 0)
            {
                ProgressItems.Add(
                    "Für diese Übung wurden noch keine Trainings gefunden.");
            }
        }


    
        // NAVIGATION

        private void OnOverviewClicked(
            object sender,
            EventArgs e)
        {
            ShowOverview();
        }


        private void OnDetailClicked(
            object sender,
            EventArgs e)
        {
            ShowDetail();
        }


        private void OnTimerPageClicked(
            object sender,
            EventArgs e)
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


        // TIMER-DAUER FESTLEGEN

        private void OnSetTimerClicked(
            object sender,
            EventArgs e)
        {
            if (!int.TryParse(
                TimerSecondsEntry.Text,
                out int seconds))
            {
                TimerErrorLabel.Text =
                    "Bitte eine Zahl eingeben.";

                return;
            }


            if (seconds <= 0)
            {
                TimerErrorLabel.Text =
                    "Die Zeit muss grösser als 0 sein.";

                return;
            }


            timer.Stop();

            timerDurationSeconds = seconds;
            remainingSeconds = seconds;

            TimerLabel.Text =
                remainingSeconds.ToString();

            TimerErrorLabel.Text = "";
        }


        private void OnStartTimerClicked(
            object sender,
            EventArgs e)
        {
            if (remainingSeconds <= 0)
            {
                remainingSeconds =
                    timerDurationSeconds;

                TimerLabel.Text =
                    remainingSeconds.ToString();
            }

            timer.Start();
        }


        private void OnPauseTimerClicked(
            object sender,
            EventArgs e)
        {
            timer.Stop();
        }


        private void OnResetTimerClicked(
            object sender,
            EventArgs e)
        {
            timer.Stop();

            remainingSeconds =
                timerDurationSeconds;

            TimerLabel.Text =
                remainingSeconds.ToString();
        }


        private void OnTimerTick(
            object sender,
            EventArgs e)
        {
            remainingSeconds--;

            TimerLabel.Text =
                remainingSeconds.ToString();


            if (remainingSeconds <= 0)
            {
                timer.Stop();

                remainingSeconds = 0;

                TimerLabel.Text =
                    "Pause beendet!";
            }
        }


        // DATEN SPEICHERN


        private async Task SaveData()
        {
            AppData data = new AppData
            {
                Workouts = Workouts.ToList(),

                History = History.ToList()
            };


            string json =
                JsonSerializer.Serialize(data);


            await File.WriteAllTextAsync(
                filePath,
                json);
        }


        // DATEN LADEN
     

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

            foreach (Workout workout
                     in data.Workouts)
            {
                Workouts.Add(workout);
            }


            History.Clear();

            foreach (TrainingSession session
                     in data.History)
            {
                History.Add(session);
            }
        }


        private void RefreshWorkoutList()
        {
            BindableLayout.SetItemsSource(
                WorkoutList,
                null);

            BindableLayout.SetItemsSource(
                WorkoutList,
                Workouts);
        }


        private void RefreshExerciseList()
        {
            BindableLayout.SetItemsSource(
                ExerciseList,
                null);

            BindableLayout.SetItemsSource(
                ExerciseList,
                SelectedExercises);
        }
    }



    // KLASSEN

    public class Workout
    {
        public string Name { get; set; } = "";


        public ObservableCollection<Exercise>
            Exercises { get; set; }
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
                return
                    $"{Name} - {Sets} x {Reps} - {Weight} kg";
            }
        }
    }


    // Kopie der Trainingswerte für Historie

    public class ExerciseSnapshot
    {
        public string Name { get; set; } = "";

        public int Sets { get; set; }

        public int Reps { get; set; }

        public double Weight { get; set; }
    }


    public class TrainingSession
    {
        public string WorkoutName { get; set; } = "";

        public DateTime Date { get; set; }

        public int ExerciseCount { get; set; }


        public List<ExerciseSnapshot>
            ExerciseResults { get; set; }
            = new List<ExerciseSnapshot>();


        public string DisplayText
        {
            get
            {
                return
                    $"{Date:dd.MM.yyyy HH:mm} - " +
                    $"{WorkoutName} " +
                    $"({ExerciseCount} Übungen)";
            }
        }
    }


    public class AppData
    {
        public List<Workout>
            Workouts { get; set; }
            = new List<Workout>();


        public List<TrainingSession>
            History { get; set; }
            = new List<TrainingSession>();
    }
}
