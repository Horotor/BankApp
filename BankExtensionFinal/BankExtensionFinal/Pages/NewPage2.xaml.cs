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
            "������ �����! ������ ������ ������?",
            "����� ���� ��� ����������?",
            "������� ��� �������.",
            "�������� ������ ���������:"
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
                SendButton.Text = "��";
                DisplayMessage(messages[0]);
                _counter++;
            }
            else if (_counter == 1)
            {
                DisplayUserMessage("��");
                _counter++;
                DisplayMessage(messages[1]);
                RadioButtonsFrame.IsVisible = true;
                SendButton.Text = "���������";
            }
            else if (_counter == 2)
            {
                var selectedcateg = ReduceCostsRadioButton.IsChecked ? "�������� ������" : IncreaseRevenueRadioButton.IsChecked ? "��������� ������" : SavingsRecommendationsRadioButton.IsChecked ? "������������ �� �����������" : InvestmentsRadioButton.IsChecked ? "����������" : FinancialGoalRadioButton.IsChecked ? "���������� ����" : CharityRadioButton.IsChecked ? "�������������������" : "�� ������";
                DisplayUserMessage(selectedcateg);
                RadioButtonsFrame.IsVisible = false;
                DisplayMessage(messages[3]);
                Categories.IsVisible = true;
                _counter++;
            }
            else if (_counter == 3)
            {
                var selectedCategories = new List<string>();

                if (ProductsCheckBox.IsChecked) selectedCategories.Add("��������");
                if (RestaurantsCheckBox.IsChecked) selectedCategories.Add("���������");
                if (TransportCheckBox.IsChecked) selectedCategories.Add("���������");
                if (HomeCheckBox.IsChecked) selectedCategories.Add("���");
                if (EquipmentCheckBox.IsChecked) selectedCategories.Add("�������");
                if (ClothingCheckBox.IsChecked) selectedCategories.Add("������ � �����");
                if (HealthCheckBox.IsChecked) selectedCategories.Add("��������");
                if (BeautyCheckBox.IsChecked) selectedCategories.Add("�������");
                if (EntertainmentCheckBox.IsChecked) selectedCategories.Add("�����������");
                if (EducationCheckBox.IsChecked) selectedCategories.Add("�����������");
                if (DebtsCheckBox.IsChecked) selectedCategories.Add("�����");
                if (AllCheckBox.IsChecked) selectedCategories.Add("���");

                var selectedCategoriesText = selectedCategories.Count > 0
                    ? string.Join(", ", selectedCategories)
                    : "�� �������";

                DisplayUserMessage(selectedCategoriesText);
                Categories.IsVisible = false;
                DisplayMessage(messages[4]);
                Interests.IsVisible = true;
                _counter++;


            }
            else if (_counter == 4)
            {
                string message = "��������� ��������:\n" + string.Join(", ", selectedInterests);
                if (!string.IsNullOrEmpty(customCategory))
                {
                    message += "\n������: " + customCategory;
                }
                DisplayUserMessage(message);
                Interests.IsVisible = false;

                SendButton.Text = "���������";
                _counter++;


            }
            else if (_counter == 5)
            {
                // ��������� �� ����� �������� FinalPage
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
                LineBreakMode = LineBreakMode.WordWrap // ������� ���� ��� �������������
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
                HorizontalOptions = LayoutOptions.Start // ����� ������� � ������ ����
            };

            MessagesStackLayout.Children.Add(frame);
        }



        private void DisplayUserMessage(string message)
        {
            // �������� �����
            var label = new Label
            {
                Text = message,
                FontSize = 14,
                TextColor = Colors.White,
                LineBreakMode = LineBreakMode.WordWrap // ������� ���� ��� �������������
            };

            // �������� ������������ ����
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#FF8204"), Offset = 0.0f }); // ����-���������
            gradientBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb("#FFB738"), Offset = 1.0f }); // ������-���������

            // �������� �����
            var frame = new Frame
            {
                BorderColor = Color.FromArgb("#FFA500"),
                Margin = new Thickness(10, 0, 0, 10),
                Background = gradientBrush, // ��������� ������������ ����
                CornerRadius = 15,
                Padding = 10,
                HasShadow = true,
                Content = label,
                HorizontalOptions = LayoutOptions.End // ����� ������� � ������� ����
            };

            // ���������� ����� � StackLayout
            MessagesStackLayout.Children.Add(frame);
        }

        private async void OtherRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                string result = await DisplayPromptAsync("������� ���������", "����������, ������� ���� ���������:", "OK", "Cancel", "���������", -1, Keyboard.Text);
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

            // ������� Label � ������� Grid
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
                    // ���� CheckBox ������, ��������� ������� � ������ ���������
                    if (!selectedInterests.Contains(interest))
                        selectedInterests.Add(interest);
                }
                else
                {
                    // ���� CheckBox ������� �����, ������� ������� �� ������ ���������
                    if (selectedInterests.Contains(interest))
                        selectedInterests.Remove(interest);
                }
            }
            else
            {
                // ��������� ������, ���� Label �� ������ � ������� Grid
                Debug.WriteLine("Label not found in current Grid.");
            }
        }

        private void AllCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            // ������������� ��������� ���� CheckBox � ����������� �� ��������� AllCheckBox
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

            // ���� AllCheckBox �� ������, �� ������� ����� ���� ������ CheckBox
            if (!isChecked)
            {
                AllCheckBox.IsChecked = false;
            }
        }




    }
