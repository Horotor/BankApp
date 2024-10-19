using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BankExtensionFinal.Pages;

public partial class NewPage2 : ContentPage
{
        private List<string> selectedInterests = new List<string>();
        private string customCategory = string.Empty;
        private int _counter = 0;
        private readonly string[] messages = new string[]
        {
            "Добрый вечер! Хотите задать вопрос?",
            "Какая тема вас интересует?",
            "Укажите ваш возраст.",
            "Выберете нужную категорию:"
        };


        public NewPage2()
        {
            InitializeComponent();
        }

        private async Task ScrollToBottomAsync()
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                var lastChild = MessagesStackLayout.Children.LastOrDefault() as Microsoft.Maui.Controls.Element;
                if (lastChild != null)
                {
                    await ChatScrollView.ScrollToAsync(lastChild, ScrollToPosition.End, false);
                }
            }).ConfigureAwait(true);
        }

        private async void OnSendButtonClicked(object sender, EventArgs e)
        {
            if (_counter == 0)
            {
                SendButton.Text = "Да";
                DisplayMessage(messages[0]);
                _counter++;
            }
            else if (_counter == 1)
            {
                DisplayUserMessage("Да");
                _counter++;
                DisplayMessage(messages[1]);
                RadioButtonsFrame.IsVisible = true;
                SendButton.Text = "Отправить";
            }
            else if (_counter == 2)
            {
                var selectedcateg = ReduceCostsRadioButton.IsChecked ? "Снижение затрат" : IncreaseRevenueRadioButton.IsChecked ? "Повышение дохода" : SavingsRecommendationsRadioButton.IsChecked ? "Рекомендации по сбережениям" : InvestmentsRadioButton.IsChecked ? "Инвестиции" : FinancialGoalRadioButton.IsChecked ? "Финансовая цель" : CharityRadioButton.IsChecked ? "Благотворительность" : "Не выбран";
                DisplayUserMessage(selectedcateg);
                RadioButtonsFrame.IsVisible = false;
                DisplayMessage(messages[3]);
                Categories.IsVisible = true;
                _counter++;
            }
            else if (_counter == 3)
            {
                var selectedCategories = new List<string>();

                if (ProductsCheckBox.IsChecked) selectedCategories.Add("Продукты");
                if (RestaurantsCheckBox.IsChecked) selectedCategories.Add("Рестораны");
                if (TransportCheckBox.IsChecked) selectedCategories.Add("Транспорт");
                if (HomeCheckBox.IsChecked) selectedCategories.Add("Дом");
                if (EquipmentCheckBox.IsChecked) selectedCategories.Add("Техника");
                if (ClothingCheckBox.IsChecked) selectedCategories.Add("Одежда и обувь");
                if (HealthCheckBox.IsChecked) selectedCategories.Add("Здоровье");
                if (BeautyCheckBox.IsChecked) selectedCategories.Add("Красота");
                if (EntertainmentCheckBox.IsChecked) selectedCategories.Add("Развлечения");
                if (EducationCheckBox.IsChecked) selectedCategories.Add("Образование");
                if (DebtsCheckBox.IsChecked) selectedCategories.Add("Долги");
                if (AllCheckBox.IsChecked) selectedCategories.Add("Все");

                var selectedCategoriesText = selectedCategories.Count > 0
                    ? string.Join(", ", selectedCategories)
                    : "Не выбрано";

                DisplayUserMessage(selectedCategoriesText);
                Categories.IsVisible = false;
                DisplayMessage(messages[4]);
                Interests.IsVisible = true;
                _counter++;


            }
            else if (_counter == 4)
            {
                string message = "Выбранные интересы:\n" + string.Join(", ", selectedInterests);
                if (!string.IsNullOrEmpty(customCategory))
                {
                    message += "\nДругое: " + customCategory;
                }
                DisplayUserMessage(message);
                Interests.IsVisible = false;

                SendButton.Text = "Завершить";
                _counter++;


            }
            else if (_counter == 5)
            {
                // Навигация на новую страницу FinalPage
                await Navigation.PushAsync(new Pages.NewPage1());
            }

            await ScrollToBottomAsync();
        }


        private void DisplayMessage(string message)
        {
            var label = new Label
            {
                Text = message,
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap // Перенос слов при необходимости
            };

            var frame = new Frame
            {
                BorderColor = Color.FromArgb("#D3D3D3"),
                Margin = new Thickness(0, 0, 100, 10),
                BackgroundColor = Colors.White,
                CornerRadius = 15,
                Padding = 10,
                HasShadow = true,
                Content = label,
                HorizontalOptions = LayoutOptions.Start // Чтобы прижать к левому краю
            };

            MessagesStackLayout.Children.Add(frame);
        }



        private void DisplayUserMessage(string message)
        {
            // Создание метки
            var label = new Label
            {
                Text = message,
                FontSize = 14,
                TextColor = Colors.White,
                LineBreakMode = LineBreakMode.WordWrap // Перенос слов при необходимости
            };

            // Создание градиентного фона
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#FF8204"), Offset = 0.0f }); // Ярко-оранжевый
            gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#FFB738"), Offset = 1.0f }); // Светло-оранжевый

            // Создание рамки
            var frame = new Frame
            {
                BorderColor = Color.FromArgb("#FFA500"),
                Margin = new Thickness(10, 0, 0, 10),
                Background = gradientBrush, // Установка градиентного фона
                CornerRadius = 15,
                Padding = 10,
                HasShadow = true,
                Content = label,
                HorizontalOptions = LayoutOptions.End // Чтобы прижать к правому краю
            };

            // Добавление рамки в StackLayout
            MessagesStackLayout.Children.Add(frame);
        }

        private async void OtherRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                string result = await DisplayPromptAsync("Введите категорию", "Пожалуйста, введите вашу категорию:", "OK", "Cancel", "Категория", -1, Keyboard.Text);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    customCategory = result;
                }
                else
                {
                    OtherRadioButton.IsChecked = false;
                }
            }
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            Grid parentGrid = checkBox.Parent as Grid;

            // Находим Label в текущем Grid
            Label currentLabel = null;

            foreach (var child in parentGrid.Children)
            {
                if (child is Label label && Grid.GetRow(label) == Grid.GetRow(checkBox))
                {
                    currentLabel = label;
                    break;
                }
            }

            if (currentLabel != null)
            {
                string interest = currentLabel.Text;

                if (e.Value)
                {
                    // Если CheckBox выбран, добавляем интерес в список выбранных
                    if (!selectedInterests.Contains(interest))
                        selectedInterests.Add(interest);
                }
                else
                {
                    // Если CheckBox отменен выбор, удаляем интерес из списка выбранных
                    if (selectedInterests.Contains(interest))
                        selectedInterests.Remove(interest);
                }
            }
            else
            {
                // Обработка случая, если Label не найден в текущем Grid
                Debug.WriteLine("Label not found in current Grid.");
            }
        }

        private void AllCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // Устанавливаем состояние всех CheckBox в зависимости от состояния AllCheckBox
            bool isChecked = e.Value;

            ProductsCheckBox.IsChecked = isChecked;
            RestaurantsCheckBox.IsChecked = isChecked;
            TransportCheckBox.IsChecked = isChecked;
            HomeCheckBox.IsChecked = isChecked;
            EquipmentCheckBox.IsChecked = isChecked;
            ClothingCheckBox.IsChecked = isChecked;
            HealthCheckBox.IsChecked = isChecked;
            BeautyCheckBox.IsChecked = isChecked;
            EntertainmentCheckBox.IsChecked = isChecked;
            EducationCheckBox.IsChecked = isChecked;
            DebtsCheckBox.IsChecked = isChecked;

            // Если AllCheckBox не выбран, то очищаем выбор всех других CheckBox
            if (!isChecked)
            {
                AllCheckBox.IsChecked = false;
            }
        }




    }
