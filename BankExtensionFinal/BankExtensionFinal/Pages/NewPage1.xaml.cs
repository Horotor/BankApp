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

    // ���� � ���������� ������ ����������� ��������� ����������
    private string AppDirectory => Path.Combine(FileSystem.Current.AppDataDirectory, "MyAppData");

    public NewPage1()
    {
        InitializeComponent();
        dateLabel.Text = DateTime.Now.ToString("d MMMM yyyy", new CultureInfo("ru-RU"));
        UpdateTotalSpent();
        // ������������� ������� ��� ������������� ��������� � ����������
        descriptionLabels = new Dictionary<string, Label>
            {
                { "��� ��� ������", mirBezGranitsDescription },
                { "������� ����", yunihellDescription },
                { "�����-��", zhizniDaDescription }
            };
    }

    private void UpdateTotalSpent()
    {
        double totalSpent = CalculateTotalSpent();
        totalSpentLabel.Text = $"{totalSpent} BYN";
    }

    private void OnAddButtonClicked(object sender, EventArgs e)
    {
        // ����������� ���
        overlayBackground.IsVisible = true;

        // ����������� ������� ��� ������ ���������
        categoryOverlay.IsVisible = true;
    }

    private void OnOverlayTapped(object sender, EventArgs e)
    {
        // �������� ������� ����� ����� � ����������� ��� ��� ������� ������������ ����
        overlayBackground.IsVisible = false;
        sumOverlay.IsVisible = false;
        categoryOverlay.IsVisible = false;
        paymentOverlay.IsVisible = false;
    }

    private async void OnMenuButtonClicked(object sender, EventArgs e)
    {
        // �������� ������ ����
        await Navigation.PushAsync(new Pages.NewPage2());
    }

    private void OnCategorySelected(object sender, EventArgs e)
    {
        if (sender is Grid grid)
        {
            var tapGestureRecognizer = grid.GestureRecognizers[0] as TapGestureRecognizer;
            if (tapGestureRecognizer != null && tapGestureRecognizer.CommandParameter is string category)
            {
                // ��������� ������ ���������
                selectedCategory = category; // ��������� ���������� selectedCategory
                Debug.WriteLine($"Selected Category: {selectedCategory}");

                categoryOverlay.IsVisible = false;

                // ���������� ������� ��� ������ ������ ������
                paymentOverlay.IsVisible = true;
            }
        }
        else
        {
            Debug.WriteLine("������ ��� ��������� ������� �� ���������.");
        }
    }

    private void OnPaymentMethodSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        selectedPaymentMethod = button.Text;

        // �������� ����������� ��� � ������� ��� ������ ������ ������
        overlayBackground.IsVisible = true;
        paymentOverlay.IsVisible = false;

        // ���������� ������� ��� ����� �����
        sumOverlay.IsVisible = true;
    }

    private async void OnSumEntered(object sender, EventArgs e)
    {
        if (double.TryParse(sumEntry.Text, out double sum) && sum > 0)
        {
            overlayBackground.IsVisible = true;
            // ��������� � ������ ����������
            string transaction = $"Category: {selectedCategory}, Payment Method: {selectedPaymentMethod}, Sum: {sum} BYN, Date: {DateTime.Now:yyyy-MM-ddTHH:mm:ss}";

            // ���������, ��� ���������� ����������
            Directory.CreateDirectory(AppDirectory);

            // ���� � ����� ������ ��������� ����������
            string fileName = Path.Combine(AppDirectory, "transactions.txt");

            // ���������� � ����
            File.AppendAllText(fileName, transaction + Environment.NewLine);

            // �������� ������� ����� ����� � ����������� ���
            sumOverlay.IsVisible = false;
            overlayBackground.IsVisible = false;

            // ����� ��������
            selectedCategory = "";
            selectedPaymentMethod = "";
            sumEntry.Text = "";

            // �����������, �������� ��������� �� �������� ������
            await DisplayAlert("�����", "���������� ������� ��������!", "OK");

            // ���������� ����� ����� ������
            UpdateTotalSpent();

            circularGraph.InvalidateSurface();

        }
        else
        {
            // �������� ��������� �� ������, ���� ����� �����������
            await DisplayAlert("������", "����������, ������� ���������� �����.", "OK");
        }
    }

    private double CalculateTotalSpent()
    {
        double total = 0;

        // ���� � ����� � ������������
        string fileName = Path.Combine(AppDirectory, "transactions.txt");

        // ��������, ���������� �� ����
        if (File.Exists(fileName))
        {
            // ������ ���� ����� �� �����
            var lines = File.ReadAllLines(fileName);

            foreach (var line in lines)
            {
                // ������� ����� � ������� ����� �� ������
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

        // ���� � ����� � ������������
        string fileName = Path.Combine(AppDirectory, "transactions.txt");

        // ��������, ���������� �� ����
        if (File.Exists(fileName))
        {
            // ������ ���� ����� �� �����
            var lines = File.ReadAllLines(fileName);

            foreach (var line in lines)
            {
                // ��������, ��������� �� ������ � ��������� ���������
                if (line.Contains($"Category: {category}"))
                {
                    // ������� ������ Transaction � ��������� ��� ����
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
        // ���� � ����� � ������������
        string fileName = Path.Combine(AppDirectory, "transactions.txt");

        // ��������, ���������� �� ����
        if (File.Exists(fileName))
        {
            // ������ ����������� �����
            string fileContent = File.ReadAllText(fileName);

            // �������� ���������� ����� � ���������� ����
            await DisplayAlert("����������", fileContent, "OK");
        }
        else
        {
            // �������� ���������, ���� ���� �� ������
            await DisplayAlert("������", "���� ���������� �� ������.", "OK");
        }
    }

    // �������� ����� ����������� ������� ��� ������ ������ ����������
    private void OnResetTransactionsClicked(object sender, EventArgs e)
    {
        // ���� � ����� � ������������
        string fileName = Path.Combine(AppDirectory, "transactions.txt");

        // �������� ����� ����������, ���� �� ����������
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        // �����������, �������� UI ��� ���������� ���������
        UpdateTotalSpent();
        DisplayAlert("�����", "���������� ��������.", "OK");
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
        { "��������", SKColors.Blue },
        { "���������", SKColors.Red },
        { "���������", SKColors.Green },
        { "������������", SKColors.Cyan },
        { "������������", SKColors.Purple }
    };

        var categorySpending = new Dictionary<string, double>
    {
        { "��������", GetSpendingPercentage("��������", totalSpent) },
        { "���������", GetSpendingPercentage("���������", totalSpent) },
        { "���������", GetSpendingPercentage("���������", totalSpent) },
        { "������������", GetSpendingPercentage("������������", totalSpent) },
        { "������������", GetSpendingPercentage("������������", totalSpent) }
    };

        // ��������� �����������
        float gapPixels = 4; // ������ ���������� � ��������
        float radius = Math.Min(e.Info.Width, e.Info.Height) / 2;
        float circumference = 2 * (float)Math.PI * radius;
        float gapAngle = (float)(360 * (gapPixels / circumference)); // ���� ���������� � ��������

        float startAngle = 0;
        var rect = new SKRect(10, 10, e.Info.Width - 10, e.Info.Height - 10);

        // ��������� ���� ��� ���� ���������
        float totalSweepAngle = 360 - (gapAngle * (categorySpending.Count + 1)); // ��������� ���������� ����� � ����� ������ �����

        // ������������ ���� ��� ������� ��������
        foreach (var category in categorySpending)
        {
            float segmentPercentage = (float)category.Value;
            float sweepAngle = segmentPercentage * totalSweepAngle;

            if (sweepAngle <= 0) continue; // ����������, ���� ���� ����� ���� ��� �������������

            // ������ ������� �����
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = radius * 0.1f, // ���������������� ������ �����
                Color = categoryColors[category.Key],
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            // ������ ����
            canvas.DrawArc(rect, startAngle, sweepAngle, false, paint);

            // ��������� ��������� ���� ��� ���������� ��������, �������� ����������
            startAngle += sweepAngle + gapAngle;
        }
    }














    private double GetSpendingPercentage(string category, double totalSpent)
    {
        var transactions = GetTransactionsByCategory(category);
        double totalCategorySpent = transactions.Sum(t => t.Sum);
        return totalSpent > 0 ? totalCategorySpent / totalSpent : 0;
    }

    // �������� ����� Transaction ���������
    public class Transaction
    {
        public string Category { get; set; }
        public string PaymentMethod { get; set; }
        public double Sum { get; set; }
        public DateTime Date { get; set; }
    }
}
