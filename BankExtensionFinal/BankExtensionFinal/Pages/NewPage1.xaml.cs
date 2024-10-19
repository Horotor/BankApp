using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
namespace BankExtensionFinal.Pages;

public partial class NewPage1 : ContentPage
{
    private string selectedCategory = "";
    private string selectedPaymentMethod = "";
    private Dictionary<string, Label> descriptionLabels;

    // Путь к директории внутри внутреннего хранилища приложения
    private string AppDirectory => Path.Combine(FileSystem.Current.AppDataDirectory, "MyAppData");

    public NewPage1()
    {
        InitializeComponent();
        dateLabel.Text = DateTime.Now.ToString("d MMMM yyyy", new CultureInfo("ru-RU"));
        UpdateTotalSpent();
        // Инициализация словаря для сопоставления элементов с описаниями
        descriptionLabels = new Dictionary<string, Label>
            {
                { "МИР БЕЗ ГРАНИЦ", mirBezGranitsDescription },
                { "ЮниХелл МБОО", yunihellDescription },
                { "ЖИЗНИ-ДА", zhizniDaDescription }
            };
    }

    private void UpdateTotalSpent()
    {
        double totalSpent = CalculateTotalSpent();
        totalSpentLabel.Text = $"{totalSpent} BYN";
    }

    private void OnAddButtonClicked(object sender, EventArgs e)
    {
        // Затемняющий фон
        overlayBackground.IsVisible = true;

        // Отображение оверлея для выбора категории
        categoryOverlay.IsVisible = true;
    }

    private void OnOverlayTapped(object sender, EventArgs e)
    {
        // Скрываем оверлей ввода суммы и затемняющий фон при касании затемняющего фона
        overlayBackground.IsVisible = false;
        sumOverlay.IsVisible = false;
        categoryOverlay.IsVisible = false;
        paymentOverlay.IsVisible = false;
    }

    private async void OnMenuButtonClicked(object sender, EventArgs e)
    {
        // Открытие нового окна
        await Navigation.PushAsync(new Pages.NewPage2());
    }

    private void OnCategorySelected(object sender, EventArgs e)
    {
        if (sender is Grid grid)
        {
            var tapGestureRecognizer = grid.GestureRecognizers[0] as TapGestureRecognizer;
            if (tapGestureRecognizer != null && tapGestureRecognizer.CommandParameter is string category)
            {
                // Обработка выбора категории
                selectedCategory = category; // Обновляем переменную selectedCategory
                Debug.WriteLine($"Selected Category: {selectedCategory}");

                categoryOverlay.IsVisible = false;

                // Показываем оверлей для выбора метода оплаты
                paymentOverlay.IsVisible = true;
            }
        }
        else
        {
            Debug.WriteLine("Ошибка при обработке нажатия на категорию.");
        }
    }

    private void OnPaymentMethodSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        selectedPaymentMethod = button.Text;

        // Скрываем затемняющий фон и оверлей для выбора метода оплаты
        overlayBackground.IsVisible = true;
        paymentOverlay.IsVisible = false;

        // Показываем оверлей для ввода суммы
        sumOverlay.IsVisible = true;
    }

    private async void OnSumEntered(object sender, EventArgs e)
    {
        if (double.TryParse(sumEntry.Text, out double sum) && sum > 0)
        {
            overlayBackground.IsVisible = true;
            // Валидация и запись транзакции
            string transaction = $"Category: {selectedCategory}, Payment Method: {selectedPaymentMethod}, Sum: {sum} BYN, Date: {DateTime.Now:yyyy-MM-ddTHH:mm:ss}";

            // Убедитесь, что директория существует
            Directory.CreateDirectory(AppDirectory);

            // Путь к файлу внутри созданной директории
            string fileName = Path.Combine(AppDirectory, "transactions.txt");

            // Сохранение в файл
            File.AppendAllText(fileName, transaction + Environment.NewLine);

            // Скрываем оверлей ввода суммы и затемняющий фон
            sumOverlay.IsVisible = false;
            overlayBackground.IsVisible = false;

            // Сброс значений
            selectedCategory = "";
            selectedPaymentMethod = "";
            sumEntry.Text = "";

            // Опционально, показать сообщение об успешной записи
            await DisplayAlert("Успех", "Транзакция успешно записана!", "OK");

            // Обновление общей суммы затрат
            UpdateTotalSpent();

            circularGraph.InvalidateSurface();

        }
        else
        {
            // Показать сообщение об ошибке, если сумма недопустима
            await DisplayAlert("Ошибка", "Пожалуйста, введите допустимую сумму.", "OK");
        }
    }

    private double CalculateTotalSpent()
    {
        double total = 0;

        // Путь к файлу с транзакциями
        string fileName = Path.Combine(AppDirectory, "transactions.txt");

        // Проверка, существует ли файл
        if (File.Exists(fileName))
        {
            // Чтение всех строк из файла
            var lines = File.ReadAllLines(fileName);

            foreach (var line in lines)
            {
                // Попытка найти и извлечь сумму из строки
                var parts = line.Split(',');
                foreach (var part in parts)
                {
                    if (part.Trim().StartsWith("Sum:"))
                    {
                        var sumPart = part.Split(':')[1].Trim().Split(' ')[0];
                        if (double.TryParse(sumPart, out double sum))
                        {
                            total += sum;
                        }
                    }
                }
            }
        }

        return total;
    }

    private List<Transaction> GetTransactionsByCategory(string category)
    {
        var transactions = new List<Transaction>();

        // Путь к файлу с транзакциями
        string fileName = Path.Combine(AppDirectory, "transactions.txt");

        // Проверка, существует ли файл
        if (File.Exists(fileName))
        {
            // Чтение всех строк из файла
            var lines = File.ReadAllLines(fileName);

            foreach (var line in lines)
            {
                // Проверка, относится ли строка к выбранной категории
                if (line.Contains($"Category: {category}"))
                {
                    // Создаем объект Transaction и заполняем его поля
                    var parts = line.Split(',');
                    var transaction = new Transaction();

                    foreach (var part in parts)
                    {
                        var trimmedPart = part.Trim();
                        if (trimmedPart.StartsWith("Category:"))
                        {
                            transaction.Category = trimmedPart.Split(':')[1].Trim();
                        }
                        else if (trimmedPart.StartsWith("Payment Method:"))
                        {
                            transaction.PaymentMethod = trimmedPart.Split(':')[1].Trim();
                        }
                        else if (trimmedPart.StartsWith("Sum:"))
                        {
                            var sumPart = trimmedPart.Split(':')[1].Trim().Split(' ')[0];
                            if (double.TryParse(sumPart, out double sum))
                            {
                                transaction.Sum = sum;
                            }
                        }
                        else if (trimmedPart.StartsWith("Date:"))
                        {
                            var datePart = trimmedPart.Substring(6).Trim();
                            if (DateTime.TryParseExact(datePart, "yyyy-MM-ddTHH:mm:ss", null, DateTimeStyles.None, out DateTime date))
                            {
                                transaction.Date = date;
                            }
                            else
                            {
                                Debug.WriteLine($"Failed to parse date: {datePart}");
                            }
                        }
                    }

                    transactions.Add(transaction);
                }
            }
        }

        return transactions;
    }


    private async void OnCategoryButtonClicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var category = button?.CommandParameter as string;

        if (category != null)
        {
            var transactions = GetTransactionsByCategory(category);
            double totalCategorySpent = transactions.Sum(t => t.Sum);
            double totalSpent = CalculateTotalSpent();
            double percentageSpent = totalSpent > 0 ? (totalCategorySpent / totalSpent) * 100 : 0;

            string transactionsDetails = string.Join(Environment.NewLine, transactions.Select(t => $"Date: {t.Date}, Sum: {t.Sum} BYN, Payment Method: {t.PaymentMethod}"));

            await DisplayAlert("Selected Category",
                $"You selected: {category}\n" +
                $"Total spent: {totalCategorySpent} BYN\n" +
                $"Percentage of total spent: {percentageSpent:F2}%\n\nDetails:\n{transactionsDetails}", "OK");
        }
    }


    private void OnLabelTapped(object sender, EventArgs e)
    {
        var label = sender as Label;
        if (label != null && descriptionLabels.TryGetValue(label.Text, out var descriptionLabel))
        {
            descriptionLabel.IsVisible = !descriptionLabel.IsVisible;
        }
    }

    private async void OnShowTransactionsButtonClicked(object sender, EventArgs e)
    {
        // Путь к файлу с транзакциями
        string fileName = Path.Combine(AppDirectory, "transactions.txt");

        // Проверка, существует ли файл
        if (File.Exists(fileName))
        {
            // Чтение содержимого файла
            string fileContent = File.ReadAllText(fileName);

            // Показать содержимое файла в диалоговом окне
            await DisplayAlert("Транзакции", fileContent, "OK");
        }
        else
        {
            // Показать сообщение, если файл не найден
            await DisplayAlert("Ошибка", "Файл транзакций не найден.", "OK");
        }
    }

    // Добавьте метод обработчика событий для кнопки сброса транзакций
    private void OnResetTransactionsClicked(object sender, EventArgs e)
    {
        // Путь к файлу с транзакциями
        string fileName = Path.Combine(AppDirectory, "transactions.txt");

        // Удаление файла транзакций, если он существует
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        // Опционально, обновите UI или отобразите сообщение
        UpdateTotalSpent();
        DisplayAlert("Сброс", "Транзакции сброшены.", "OK");
        circularGraph.InvalidateSurface();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        double totalSpent = CalculateTotalSpent();
        if (totalSpent == 0) return;

        var categoryColors = new Dictionary<string, SKColor>
    {
        { "Продукты", SKColors.Blue },
        { "Транспорт", SKColors.Red },
        { "Рестораны", SKColors.Green },
        { "Недвижимость", SKColors.Cyan },
        { "Обслуживание", SKColors.Purple }
    };

        var categorySpending = new Dictionary<string, double>
    {
        { "Продукты", GetSpendingPercentage("Продукты", totalSpent) },
        { "Транспорт", GetSpendingPercentage("Транспорт", totalSpent) },
        { "Рестораны", GetSpendingPercentage("Рестораны", totalSpent) },
        { "Недвижимость", GetSpendingPercentage("Недвижимость", totalSpent) },
        { "Обслуживание", GetSpendingPercentage("Обслуживание", totalSpent) }
    };

        // Настройка промежутков
        float gapPixels = 4; // Ширина промежутка в пикселях
        float radius = Math.Min(e.Info.Width, e.Info.Height) / 2;
        float circumference = 2 * (float)Math.PI * radius;
        float gapAngle = (float)(360 * (gapPixels / circumference)); // Угол промежутка в градусах

        float startAngle = 0;
        var rect = new SKRect(10, 10, e.Info.Width - 10, e.Info.Height - 10);

        // Суммарный угол для всех сегментов
        float totalSweepAngle = 360 - (gapAngle * (categorySpending.Count + 1)); // Учитываем промежутки перед и после каждой линии

        // Распределяем угол для каждого сегмента
        foreach (var category in categorySpending)
        {
            float segmentPercentage = (float)category.Value;
            float sweepAngle = segmentPercentage * totalSweepAngle;

            if (sweepAngle <= 0) continue; // Пропускаем, если угол равен нулю или отрицательный

            // Рисуем цветную линию
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = radius * 0.1f, // Пропорциональная ширина линии
                Color = categoryColors[category.Key],
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            // Рисуем дугу
            canvas.DrawArc(rect, startAngle, sweepAngle, false, paint);

            // Обновляем начальный угол для следующего сегмента, учитывая промежутки
            startAngle += sweepAngle + gapAngle;
        }
    }














    private double GetSpendingPercentage(string category, double totalSpent)
    {
        var transactions = GetTransactionsByCategory(category);
        double totalCategorySpent = transactions.Sum(t => t.Sum);
        return totalSpent > 0 ? totalCategorySpent / totalSpent : 0;
    }

    // Сделайте класс Transaction публичным
    public class Transaction
    {
        public string Category { get; set; }
        public string PaymentMethod { get; set; }
        public double Sum { get; set; }
        public DateTime Date { get; set; }
    }
}
