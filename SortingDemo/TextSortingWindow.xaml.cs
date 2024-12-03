﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static SortingDemo.TimeComparisonWindow;

namespace SortingDemo
{
    public partial class TextSortingWindow : Window
    {
        private string inputFilePath;
        private string outputFilePath;
        private SortingResult lastSortingResult;
        private bool isSortingCompleted = false;




        public TextSortingWindow()
        {
            InitializeComponent();
        }

        private void SelectInputFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                inputFilePath = openFileDialog.FileName;
                InputFilePath.Text = inputFilePath;
            }
        }

        private void SelectOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                outputFilePath = saveFileDialog.FileName;
                OutputFilePath.Text = outputFilePath;
            }
        }

        private async void StartSortButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
            {
                MessageBox.Show("Выберите файлы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedMethod = (SortMethodSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selectedMethod == null)
            {
                MessageBox.Show("Выберите алгоритм сортировки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SortedWordsLog.Clear();
            WordCountLog.Clear();

            try
            {
                var text = File.ReadAllText(inputFilePath);
                var words = text.Split(new[] { ' ', '\n', '\r', '\t', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> sortedWords;

                var startTime = DateTime.Now; // Запоминаем время начала

                if (selectedMethod == "QuickSort")
                {
                    sortedWords = await Task.Run(() => QuickSort(words.ToList()));
                }
                else if (selectedMethod == "RadixSort")
                {
                    sortedWords = await Task.Run(() => RadixSort(words.ToList()));
                }
                else
                {
                    throw new Exception("Неизвестный метод сортировки.");
                }

                var endTime = DateTime.Now; // Запоминаем время завершения
                lastSortingResult = new SortingResult
                {
                    Method = selectedMethod,
                    WordCount = words.Length,
                    Time = (long)(endTime - startTime).TotalMilliseconds
                };

                isSortingCompleted = true; // Устанавливаем, что сортировка завершена

                var wordCounts = CountWords(sortedWords);

                File.WriteAllLines(outputFilePath, sortedWords);
                SortedWordsLog.Text = string.Join("\n", sortedWords);
                WordCountLog.Text = string.Join("\n", wordCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

                MessageBox.Show("Сортировка завершена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сортировки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private static List<string> QuickSort(List<string> words)
        {
            if (words.Count <= 1)
                return words;

            var pivot = words[words.Count / 2];
            var less = words.Where(x => string.Compare(x, pivot, StringComparison.Ordinal) < 0).ToList();
            var equal = words.Where(x => x == pivot).ToList();
            var greater = words.Where(x => string.Compare(x, pivot, StringComparison.Ordinal) > 0).ToList();

            return QuickSort(less).Concat(equal).Concat(QuickSort(greater)).ToList();
        }

        private static List<string> RadixSort(List<string> words)
        {
            int maxLength = words.Max(word => word.Length);
            for (int k = maxLength - 1; k >= 0; k--)
            {
                words = words.OrderBy(word => k < word.Length ? word[k] : '\0').ToList();
            }
            return words;
        }

        private static Dictionary<string, int> CountWords(List<string> sortedWords)
        {
            var wordCounts = new Dictionary<string, int>();

            foreach (var word in sortedWords)
            {
                if (wordCounts.ContainsKey(word))
                    wordCounts[word]++;
                else
                    wordCounts[word] = 1;
            }

            return wordCounts;
        }

        private void BackToMainButton_Click(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                Owner.Show();
            }
            this.Close();
        }
        private void SaveResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSortingCompleted)
            {
                MessageBox.Show("Сначала выполните сортировку, чтобы сохранить результат!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var comparisonWindow = Application.Current.Windows.OfType<TimeComparisonWindow>().FirstOrDefault();

            if (comparisonWindow == null)
            {
                comparisonWindow = new TimeComparisonWindow();
                comparisonWindow.Show();
            }

            if (lastSortingResult != null)
            {
                comparisonWindow.AddSortingResult(lastSortingResult);
                MessageBox.Show("Результат добавлен в окно сравнения!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void OpenComparisonWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // Ищем уже открытое окно TimeComparisonWindow
            var comparisonWindow = Application.Current.Windows.OfType<TimeComparisonWindow>().FirstOrDefault();

            if (comparisonWindow == null)
            {
                // Если окно не найдено, создаем новое и открываем
                comparisonWindow = new TimeComparisonWindow();
                comparisonWindow.Show();
            }
            else
            {
                // Если окно уже открыто, активируем его
                comparisonWindow.Activate();
            }
        }




    }
}
