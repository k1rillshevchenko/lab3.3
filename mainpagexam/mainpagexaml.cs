using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace LAB_3._2;

public partial class MainPage : ContentPage
{
    private List<Student> _allStudents = new();
    public ObservableCollection<Student> DisplayedStudents { get; set; } = new();
    private string _currentFilePath = Path.Combine(FileSystem.AppDataDirectory, "students.json");

    public MainPage()
    {
        InitializeComponent();
        StudentsList.ItemsSource = DisplayedStudents;
        _ = InitializeData();
    }

    private async Task InitializeData()
    {
        try
        {
            if (!File.Exists(_currentFilePath))
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("students.json");
                using var fileStream = File.Create(_currentFilePath);
                await stream.CopyToAsync(fileStream);
            }
            await LoadData(_currentFilePath);
        }
        catch { }
    }

    private async Task LoadData(string path)
    {
        string json = await File.ReadAllTextAsync(path);
        _allStudents = JsonSerializer.Deserialize<List<Student>>(json) ?? new();
        UpdateUI(_allStudents);
    }

    private void UpdateUI(List<Student> list)
    {
        DisplayedStudents.Clear();
        foreach (var s in list) DisplayedStudents.Add(s);
    }

    private void OnFilterChanged(object sender, TextChangedEventArgs e)
    {
        var filtered = _allStudents.Where(s =>
            (string.IsNullOrEmpty(SearchName.Text) || s.FullName.Contains(SearchName.Text, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(SearchFaculty.Text) || s.Faculty.Contains(SearchFaculty.Text, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(SearchYear.Text) || s.Year.ToString() == SearchYear.Text)
        ).ToList();

        UpdateUI(filtered);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        string json = JsonSerializer.Serialize(_allStudents, options);
        await File.WriteAllTextAsync(_currentFilePath, json);
        await DisplayAlert("Успіх", "Дані збережено у локальне сховище!", "OK");
    }

    private async void OnOpenClicked(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync();
        if (result != null) await LoadData(result.FullPath);
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync("Новий студент", "Введіть ПІБ:");
        if (string.IsNullOrEmpty(name)) return;

        _allStudents.Add(new Student { FullName = name, Faculty = "Не вказано", Year = 1 });
        UpdateUI(_allStudents);
    }

    private async void OnItemTapped(object sender, TappedEventArgs e)
    {
        var student = e.Parameter as Student;
        string action = await DisplayActionSheet("Дія", "Відміна", "Видалити", "Редагувати");

        if (action == "Видалити")
        {
            _allStudents.Remove(student);
            UpdateUI(_allStudents);
        }
        else if (action == "Редагувати")
        {
            string newName = await DisplayPromptAsync("Редагування", "Змінити ПІБ:", initialValue: student.FullName);
            if (!string.IsNullOrEmpty(newName))
            {
                student.FullName = newName;
                UpdateUI(_allStudents);
            }
        }
    }

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Про програму", "ПІБ:Шевченко КИрило Олександрович \nКурс: 2\nГрупа: К-16\nРік: 2026\nДиспетчер JSON для гуртожитку.", "OK");
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Вихід", "Дійсно вийти?", "Так", "Ні")) Application.Current.Quit();
    }
}
