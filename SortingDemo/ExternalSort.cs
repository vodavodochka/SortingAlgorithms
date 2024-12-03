namespace SortingDemo
{
    public static class ExternalSort
    {
        public static void Sort(
            Table table,
            string sortKey,
            string sortingMethod,
            Action<string> log,
            Action<string> stepLog,
            int delay)
        {
            log("Начало сортировки...");

            // Сортируем данные по ключу sortKey перед применением метода сортировки
            var rows = table.Rows.OrderBy(row => row[sortKey], StringComparer.OrdinalIgnoreCase).ToList();
            log($"Данные отсортированы по ключу '{sortKey}'.");

            List<Dictionary<string, string>> sortedRows = sortingMethod switch
            {
                "Прямое слияние" => DirectMergeSort(rows, sortKey, log, stepLog, delay),
                "Естественное слияние" => NaturalMergeSort(rows, sortKey, log, stepLog, delay),
                "Многопутевое слияние" => MultiwayMergeSort(rows, sortKey, log, stepLog, delay),
                _ => throw new ArgumentException($"Неизвестный метод сортировки: {sortingMethod}")
            };

            table.Rows = sortedRows;
            log("Сортировка завершена.");
        }

        private static List<Dictionary<string, string>> DirectMergeSort(
            List<Dictionary<string, string>> rows,
            string sortKey,
            Action<string> log,
            Action<string> stepLog,
            int delay)
        {
            log("Запуск прямого слияния...");
            if (rows.Count <= 1)
            {
                log("Данные уже отсортированы.");
                return rows;
            }

            int mid = rows.Count / 2;
            var left = DirectMergeSort(rows.Take(mid).ToList(), sortKey, log, stepLog, delay);
            var right = DirectMergeSort(rows.Skip(mid).ToList(), sortKey, log, stepLog, delay);

            stepLog($"Шаг: Разделение на две части. Левая часть: {string.Join(", ", left.Select(r => r[sortKey]))}");
            stepLog($"Шаг: Правая часть: {string.Join(", ", right.Select(r => r[sortKey]))}");

            List<Dictionary<string, string>> merged = Merge(left, right, sortKey, log, stepLog, delay);

            stepLog($"Шаг: Слияние этих частей в одну: {string.Join(", ", merged.Select(r => r[sortKey]))}");

            log("Прямое слияние завершено.");
            return merged;
        }

        private static List<Dictionary<string, string>> NaturalMergeSort(
            List<Dictionary<string, string>> rows,
            string sortKey,
            Action<string> log,
            Action<string> stepLog,
            int delay)
        {
            log("Запуск естественного слияния...");
            if (rows.Count <= 1)
            {
                log("Данные уже отсортированы.");
                return rows;
            }

            log("Шаг 1: Поиск естественных последовательностей...");
            var runs = FindNaturalRuns(rows, sortKey, log);

            log($"Шаг 2: Найдено {runs.Count} естественных последовательностей.");

            int step = 3;
            do
            {
                log($"Шаг {step}: Слияние последовательностей...");
                var mergedRuns = new List<List<Dictionary<string, string>>>();

                for (int i = 0; i < runs.Count; i += 2)
                {
                    if (i + 1 < runs.Count)
                    {
                        var merged = Merge(runs[i], runs[i + 1], sortKey, log, stepLog, delay);
                        mergedRuns.Add(merged);

                        stepLog($"Шаг {step}: Слияние последовательности {i} и {i + 1} завершено: {string.Join(", ", merged.Select(r => r[sortKey]))}");
                    }
                    else
                    {
                        mergedRuns.Add(runs[i]);
                        stepLog($"Шаг {step}: Последовательность {i} осталась без изменений: {string.Join(", ", runs[i].Select(r => r[sortKey]))}");
                    }
                }

                runs = mergedRuns;
                step++;

            } while (runs.Count > 1); // Повторять, пока не останется только одна последовательность

            log("Естественная сортировка завершена.");
            return runs[0];
        }

        private static List<List<Dictionary<string, string>>> FindNaturalRuns(
            List<Dictionary<string, string>> rows,
            string sortKey,
            Action<string> log)
        {
            var runs = new List<List<Dictionary<string, string>>>();
            var currentRun = new List<Dictionary<string, string>> { rows[0] };

            for (int i = 1; i < rows.Count; i++)
            {
                if (CompareValues(rows[i - 1][sortKey], rows[i][sortKey]) <= 0)
                {
                    // Добавляем элемент в текущую последовательность
                    currentRun.Add(rows[i]);
                }
                else
                {
                    // Закрываем текущую последовательность и начинаем новую
                    runs.Add(currentRun);
                    currentRun = new List<Dictionary<string, string>> { rows[i] };
                }
            }

            if (currentRun.Count > 0)
            {
                runs.Add(currentRun);
            }

            log($"Сформированы {runs.Count} естественных последовательностей.");
            return runs;
        }

        private static List<Dictionary<string, string>> MultiwayMergeSort(
             List<Dictionary<string, string>> rows,
             string sortKey,
             Action<string> log,
             Action<string> stepLog,
             int delay)
        {
            log("Запуск многопутевого слияния...");
            if (rows.Count <= 1)
            {
                log("Данные уже отсортированы.");
                return rows;
            }

            int chunkSize = 2;
            List<List<Dictionary<string, string>>> chunks = new List<List<Dictionary<string, string>>>();

            while (rows.Count > 0)
            {
                var chunk = rows.Take(chunkSize).ToList();
                chunks.Add(chunk);
                rows = rows.Skip(chunkSize).ToList();
            }

            log($"Шаг 1: Разбиение на чанки. Количество чанков: {chunks.Count}");
            foreach (var chunk in chunks)
            {
                stepLog($"Шаг 1: Чанк: {string.Join(", ", chunk.Select(r => r[sortKey]))}");
            }

            while (chunks.Count > 1)
            {
                log("Шаг 2: Слияние чанков...");
                List<List<Dictionary<string, string>>> mergedChunks = new List<List<Dictionary<string, string>>>();

                for (int i = 0; i < chunks.Count; i += 2)
                {
                    if (i + 1 < chunks.Count)
                    {
                        var merged = Merge(chunks[i], chunks[i + 1], sortKey, log, stepLog, delay);
                        mergedChunks.Add(merged);

                        stepLog($"Шаг 2: Слияние чанков {i} и {i + 1} завершено: {string.Join(", ", merged.Select(r => r[sortKey]))}");
                    }
                    else
                    {
                        mergedChunks.Add(chunks[i]);
                        stepLog($"Шаг 2: Чанк {i} остался без изменений: {string.Join(", ", chunks[i].Select(r => r[sortKey]))}");
                    }
                }

                chunks = mergedChunks;
            }

            log("Многопутевое слияние завершено.");
            return chunks[0];
        }

        private static List<List<Dictionary<string, string>>> SplitIntoChunks(
            List<Dictionary<string, string>> rows,
            int chunkSize,
            string sortKey,
            Action<string> log)
        {
            var chunks = new List<List<Dictionary<string, string>>>();

            // Разделяем на чанки
            for (int i = 0; i < rows.Count; i += chunkSize)
            {
                var chunk = rows.Skip(i).Take(chunkSize).OrderBy(row => row[sortKey], StringComparer.OrdinalIgnoreCase).ToList();
                chunks.Add(chunk);
                log($"Создан чанк: {string.Join(", ", chunk.Select(row => row[sortKey]))}");
            }

            // Убедитесь, что все чанки добавляются в список
            log($"Всего создано чанков: {chunks.Count}");
            return chunks;
        }

        private static List<Dictionary<string, string>> KWayMerge(
            List<List<Dictionary<string, string>>> chunks,
            string sortKey,
            Action<string> log, // Основные логи
            int delay,          // Задержка
            Action<string> stepLog // Логи пошагового выполнения
        )
        {
            var result = new List<Dictionary<string, string>>();

            // Используем приоритетную очередь
            // Сравниватель для числовых и строковых значений
            // Сравниватель для числовых и строковых значений с учетом индекса чанка
            var comparer = Comparer<(Dictionary<string, string> row, int chunkIndex)>.Create(
                (a, b) =>
                {
                    string aValue = a.row[sortKey];
                    string bValue = b.row[sortKey];

                    // Попробуем распарсить значения как числа
                    bool isANumber = double.TryParse(aValue, out double aNumericValue);
                    bool isBNumber = double.TryParse(bValue, out double bNumericValue);

                    if (isANumber && isBNumber)
                    {
                        // Если оба значения числовые, сравниваем как числа
                        return aNumericValue.CompareTo(bNumericValue);
                    }
                    else
                    {
                        // Если оба значения строки, сравниваем как строки
                        int stringComparison = string.Compare(aValue, bValue, StringComparison.OrdinalIgnoreCase);
                        if (stringComparison != 0)
                        {
                            return stringComparison;
                        }
                        else
                        {
                            // Если строки одинаковые, сравниваем по индексу чанка, чтобы избежать дублирования
                            return a.chunkIndex.CompareTo(b.chunkIndex);
                        }
                    }
                }
            );

            var priorityQueue = new SortedSet<(Dictionary<string, string> row, int chunkIndex)>(comparer);
            var indices = new int[chunks.Count]; // Массив для индексов текущих элементов в каждом чанке

            // Инициализация очереди
            stepLog("Инициализация очереди:");
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].Count > 0) // Только если чанк не пустой
                {
                    priorityQueue.Add((chunks[i][0], i)); // Добавляем первый элемент каждого чанка в очередь
                    indices[i] = 1; // Устанавливаем индекс для следующего элемента в этом чанке
                    stepLog($"Добавлен в очередь первый элемент из чанка {i + 1}: {FormatElement(chunks[i][0])}");
                }
            }

            stepLog($"Начальное состояние очереди: {priorityQueue.Count} элементов.");

            // Основной цикл обработки
            stepLog("Начинаем обработку:");
            while (priorityQueue.Count > 0)
            {
                // Логируем текущее состояние очереди
                stepLog("Текущее состояние очереди:");
                foreach (var item in priorityQueue)
                {
                    stepLog($"Элемент из чанка {item.chunkIndex + 1}: {FormatElement(item.row)}");
                }

                // Извлекаем минимальный элемент
                var smallest = priorityQueue.Min;
                priorityQueue.Remove(smallest); // Удаляем минимальный элемент

                stepLog($"Выбран минимальный элемент из чанка {smallest.chunkIndex + 1}: {FormatElement(smallest.row)}");
                result.Add(smallest.row); // Добавляем его в результат

                log($"Добавлено: {smallest.row[sortKey]} из чанка {smallest.chunkIndex + 1}");
                System.Threading.Thread.Sleep(delay);

                // Добавляем следующий элемент из того же чанка, если он есть
                int chunkIndex = smallest.chunkIndex;
                if (indices[chunkIndex] < chunks[chunkIndex].Count)
                {
                    var nextElement = chunks[chunkIndex][indices[chunkIndex]];
                    stepLog($"Добавляем следующий элемент из чанка {chunkIndex + 1}: {FormatElement(nextElement)}");
                    priorityQueue.Add((nextElement, chunkIndex)); // Добавляем следующий элемент в очередь
                    indices[chunkIndex]++; // Обновляем индекс для этого чанка
                }
                else
                {
                    stepLog($"Чанк {chunkIndex + 1} полностью обработан.");
                }
            }

            stepLog("Слияние завершено. Результаты готовы.");
            return result;
        }

        private static string FormatElement(Dictionary<string, string> row)
        {
            return string.Join(", ", row.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        private static List<Dictionary<string, string>> Merge(
            List<Dictionary<string, string>> run1,
            List<Dictionary<string, string>> run2,
            string sortKey,
            Action<string> log,
            Action<string> stepLog,
            int delay)
        {
            List<Dictionary<string, string>> merged = new List<Dictionary<string, string>>();
            int i = 0, j = 0;

            // Сливаем две последовательности
            while (i < run1.Count && j < run2.Count)
            {
                if (CompareValues(run1[i][sortKey], run2[j][sortKey]) <= 0)
                {
                    merged.Add(run1[i]);
                    i++;
                }
                else
                {
                    merged.Add(run2[j]);
                    j++;
                }
            }

            // Добавляем оставшиеся элементы из обеих последовательностей
            while (i < run1.Count)
            {
                merged.Add(run1[i]);
                i++;
            }
            while (j < run2.Count)
            {
                merged.Add(run2[j]);
                j++;
            }

            return merged;
        }

        private static int CompareValues(string value1, string value2)
        {
            // Попробуем преобразовать строки в числа
            bool isNumeric1 = double.TryParse(value1, out double num1);
            bool isNumeric2 = double.TryParse(value2, out double num2);

            // Если оба значения можно преобразовать в числа
            if (isNumeric1 && isNumeric2)
            {
                // Сравниваем как числа
                return num1.CompareTo(num2);
            }

            // Если оба значения не являются числами, то сравниваем как строки
            return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
