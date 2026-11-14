using System; // Подключаем основные типы C#.
using System.Reflection; // Подключаем рефлексию для получения свойств объектов.
using System.IO; // Подключаем работу с файлами.
using System.Collections; // Подключаем интерфейс IEnumerable для перебора коллекций.
using System.Text.RegularExpressions; // Подключаем регулярные выражения.

namespace MiniTemplateEngine; // Указываем пространство имён библиотеки.

/// <summary>
/// Класс для обработки HTML-шаблонов и подстановки в них данных.
/// </summary>
public class HtmlTemplateRenderer : IHtmlTemplateRenderer // Создаём класс, реализующий интерфейс шаблонизатора.
{
    /// <summary>
    /// Обрабатывает шаблон, переданный в виде строки.
    /// </summary>
    /// <param name="htmlTemplate">Текст шаблона.</param>
    /// <param name="dataModel">Объект с данными для подстановки.</param>
    /// <returns>Готовый текст с подставленными данными.</returns>
    public string RenderFromString(string htmlTemplate, object dataModel) // Обрабатываем шаблон из строки.
    {
        return ProcessTemplate(htmlTemplate, dataModel); // Передаём обработку специальному методу.
    } // Закрываем метод RenderFromString.

    /// <summary>
    /// Обрабатывает условия, циклы и переменные в шаблоне.
    /// </summary>
    /// <param name="template">Текст шаблона.</param>
    /// <param name="dataModel">Объект с данными.</param>
    /// <returns>Обработанный текст шаблона.</returns>
    private string ProcessTemplate(string template, object dataModel) // Основной метод обработки конструкций шаблона.
    {
        if(dataModel == null) throw new ArgumentNullException("model");
        
        Regex pattern = new Regex( // Создаём регулярное выражение для поиска условий и циклов.
            @"\$if\s*\((.*?)\)\s*\{|\$foreach\s*\((.*?)\)\s*\{", // Ищем начало $if или $foreach.
            RegexOptions.Singleline); // Разрешаем точке в выражении захватывать переносы строк.

        Match match = pattern.Match(template); // Находим первую конструкцию в шаблоне.

        while (match.Success) // Повторяем, пока находим конструкции.
        {
            int start = match.Index; // Запоминаем позицию начала конструкции.
            int blockStart = match.Index + match.Length - 1; // Находим открывающую фигурную скобку блока.
            int blockEnd = FindBlockEnd(template, blockStart); // Ищем соответствующую закрывающую скобку.

            if (blockEnd == -1) // Проверяем, найдена ли закрывающая скобка.
            {
                break; // Если нет, прекращаем обработку.
            }

            string block = template.Substring( // Получаем содержимое блока без внешних скобок.
                blockStart + 1, // Начинаем после открывающей скобки.
                blockEnd - blockStart - 1); // Вычисляем длину содержимого.

            string replacement = ""; // Создаём строку для результата вместо конструкции.

            if (match.Groups[1].Success) // Проверяем, найдено ли условие $if.
            {
                string condition = match.Groups[1].Value.Trim(); // Получаем текст условия.

                int afterBlock = blockEnd + 1; // Находим позицию после блока условия.
                string remaining = template.Substring(afterBlock); // Получаем оставшуюся часть шаблона.

                Match elseMatch = Regex.Match( // Ищем $else сразу после блока условия.
                    remaining, // Ищем в оставшейся части шаблона.
                    @"^\s*\$else\s*\{"); // Проверяем наличие $else и открывающей скобки.

                int finalEnd = blockEnd; // Сохраняем конец блока, который нужно удалить.

                if (CheckCondition(condition, dataModel)) // Проверяем, истинно ли условие.
                {
                    replacement = ProcessTemplate(block, dataModel); // Обрабатываем содержимое истинной ветки.
                }
                else if (elseMatch.Success) // Если условие ложно и есть ветка $else.
                {
                    int elseStart = afterBlock + elseMatch.Length - 1; // Находим открывающую скобку ветки $else.
                    int elseEnd = FindBlockEnd(template, elseStart); // Ищем конец ветки $else.

                    if (elseEnd == -1) // Проверяем, найдена ли закрывающая скобка.
                    {
                        break; // Если нет, прекращаем обработку.
                    }

                    string elseContent = template.Substring( // Получаем содержимое ветки $else.
                        elseStart + 1, // Начинаем после открывающей скобки.
                        elseEnd - elseStart - 1); // Вычисляем длину содержимого.

                    replacement = ProcessTemplate(elseContent, dataModel); // Обрабатываем выбранную ветку.
                    finalEnd = elseEnd; // Запоминаем конец всей конструкции с $else.
                } // Завершаем проверку ветки $else.

                template = template.Remove( // Удаляем исходную конструкцию условия.
                    start, // Начинаем с позиции $if.
                    finalEnd - start + 1); // Удаляем до конца выбранной конструкции.

                template = template.Insert(start, replacement); // Вставляем результат вместо конструкции.
            } // Завершаем обработку условия.
            else // Если найдена конструкция $foreach.
            {
                string declaration = match.Groups[2].Value.Trim(); // Получаем объявление цикла.

                string[] parts = Regex.Split( // Разделяем объявление по слову in.
                    declaration, // Используем текст объявления цикла.
                    @"\s+in\s+"); // Ищем слово in с пробелами вокруг.

                if (parts.Length != 2) // Проверяем, что получились имя переменной и имя коллекции.
                {
                    break; // Если формат неправильный, прекращаем обработку.
                }

                string itemName = parts[0].Trim(); // Получаем имя переменной цикла.
                object? collection = GetValue(parts[1].Trim(), dataModel); // Получаем коллекцию из объекта данных.

                if (collection == null && dataModel is IEnumerable) // Проверяем, передана ли коллекция напрямую.
                {
                    collection = dataModel; // Используем сам объект данных как коллекцию.
                }

                string result = ""; // Создаём строку для результата цикла.

                if (collection is IEnumerable items && collection is not string) // Проверяем, можно ли перебрать объект.
                {
                    foreach (object? item in items) // Перебираем каждый элемент коллекции.
                    {
                        if (item == null) // Проверяем, что элемент не равен null.
                        {
                            continue; // Пропускаем пустой элемент.
                        }

                        string currentBlock = Regex.Replace( // Заменяем выражения вида ${user.Name}.
                            block, // Обрабатываем содержимое цикла.
                            @"\$\{" + Regex.Escape(itemName) + // Ищем начало выражения с именем переменной цикла.
                            @"\.([A-Za-z_]\w*)\}", // Находим имя свойства после точки.
                            m => GetValue(m.Groups[1].Value, item)?.ToString() ?? ""); // Получаем значение свойства текущего элемента.

                        currentBlock = Regex.Replace( // Заменяем выражения вида user.Name без ${}.
                            currentBlock, // Используем уже обработанный текст блока.
                            @"\b" + Regex.Escape(itemName) + // Ищем имя переменной цикла как отдельное слово.
                            @"\.([A-Za-z_]\w*)\b", // Находим имя свойства.
                            m => GetValue(m.Groups[1].Value, item)?.ToString() ?? ""); // Подставляем значение свойства.

                        result += ProcessTemplate(currentBlock, item); // Обрабатываем вложенные конструкции для текущего элемента и добавляем результат.
                    } // Завершаем перебор элементов коллекции.
                } // Завершаем проверку коллекции.

                template = template.Remove( // Удаляем исходный блок цикла.
                    start, // Начинаем с позиции $foreach.
                    blockEnd - start + 1); // Удаляем до закрывающей скобки цикла.

                template = template.Insert(start, result); // Вставляем результат перебора вместо цикла.
            } // Завершаем обработку цикла.

            match = pattern.Match(template, start); // Ищем следующую конструкцию в изменённом шаблоне.
        } // Завершаем цикл обработки конструкций.

        return ReplaceVariables(template, dataModel); // Подставляем оставшиеся обычные переменные.
    } // Закрываем метод ProcessTemplate.

    /// <summary>
    /// Находит закрывающую скобку блока с учётом вложенности.
    /// </summary>
    /// <param name="template">Текст шаблона.</param>
    /// <param name="blockStart">Позиция открывающей скобки.</param>
    /// <returns>Позиция закрывающей скобки или -1, если она не найдена.</returns>
    private int FindBlockEnd(string template, int blockStart) // Ищем конец блока.
    {
        int depth = 0; // Считаем уровень вложенности фигурных скобок.

        for (int i = blockStart; i < template.Length; i++) // Перебираем символы от начала блока до конца текста.
        {
            if (template[i] == '{') // Если встретили открывающую скобку.
            {
                depth++; // Увеличиваем уровень вложенности.
            }
            else if (template[i] == '}') // Если встретили закрывающую скобку.
            {
                depth--; // Уменьшаем уровень вложенности.

                if (depth == 0) // Проверяем, закрыт ли исходный блок.
                {
                    return i; // Возвращаем позицию его закрывающей скобки.
                } // Завершаем проверку уровня вложенности.
            } // Завершаем проверку символа.
        } // Завершаем перебор символов.

        return -1; // Возвращаем -1, если подходящая скобка не найдена.
    } // Закрываем метод FindBlockEnd.

    /// <summary>
    /// Заменяет переменные в формате ${Property}.
    /// </summary>
    /// <param name="template">Текст шаблона.</param>
    /// <param name="dataModel">Объект с данными.</param>
    /// <returns>Текст с подставленными значениями.</returns>
    private string ReplaceVariables(string template, object dataModel) // Заменяем обычные переменные.
    {
        return Regex.Replace( // Находим все выражения ${...} и заменяем их.
            template, // Исходный текст шаблона.
            @"\$\{([^}]+)\}", // Ищем текст между ${ и }.
            match => // Выполняем замену для каждого найденного выражения.
            {
                object? value = GetValue( // Получаем значение свойства.
                    match.Groups[1].Value.Trim(), // Берём имя свойства из выражения.
                    dataModel); // Ищем свойство в переданном объекте.

                return value?.ToString() ?? ""; // Возвращаем значение или пустую строку.
            }); // Завершаем регулярную замену.
    } // Закрываем метод ReplaceVariables.

    /// <summary>
    /// Получает значение свойства объекта, в том числе вложенного.
    /// </summary>
    /// <param name="expression">Имя свойства или путь через точки.</param>
    /// <param name="dataModel">Объект, в котором ищется свойство.</param>
    /// <returns>Значение свойства или null, если оно не найдено.</returns>
    private object? GetValue(string expression, object dataModel) // Получаем значение через рефлексию.
    {
        string[] parts = expression.Split('.'); // Разделяем путь к свойству по точкам.
        object? current = dataModel; // Начинаем с исходного объекта.

        foreach (string part in parts) // Перебираем части пути к свойству.
        {
            if (current == null) // Проверяем, не оказался ли текущий объект пустым.
            {
                return null; // Если пустой, дальше искать нельзя.
            } // Завершаем проверку на null.

            PropertyInfo? property = current.GetType().GetProperty( // Ищем свойство текущего объекта.
                part, // Используем имя свойства.
                BindingFlags.Public | // Ищем публичные свойства.
                BindingFlags.Instance | // Ищем свойства экземпляра объекта.
                BindingFlags.IgnoreCase); // Не учитываем регистр букв в имени.

            if (property == null) // Проверяем, найдено ли свойство.
            {
                return null; // Если не найдено, возвращаем null.
            } // Завершаем проверку свойства.

            current = property.GetValue(current); // Получаем значение свойства и переходим к нему.
        } // Завершаем перебор частей пути.

        return current; // Возвращаем найденное значение.
    } // Закрываем метод GetValue.

    /// <summary>
    /// Проверяет простое условие сравнения через ==.
    /// </summary>
    /// <param name="condition">Текст условия.</param>
    /// <param name="dataModel">Объект с данными для проверки.</param>
    /// <returns>true, если значения совпадают; иначе false.</returns>
    private bool CheckCondition(string condition, object dataModel) // Проверяем условие.
    {
        string[] parts = condition.Split( // Разделяем условие на две части.
            new string[] { "==" }, // Используем оператор сравнения.
            StringSplitOptions.None); // Сохраняем обе части, даже если одна пустая.

        if (parts.Length != 2) // Проверяем, что есть две части условия.
        {
            return false; // Если формат неправильный, считаем условие ложным.
        } // Завершаем проверку формата.

        string propertyName = parts[0].Trim(); // Получаем имя свойства.
        string expectedValue = parts[1].Trim(); // Получаем значение для сравнения.

        object? actualValue = GetValue(propertyName, dataModel); // Получаем фактическое значение свойства.
        string actualText = actualValue?.ToString() ?? ""; // Преобразуем его в строку.

        return actualText.Equals( // Сравниваем фактическое и ожидаемое значения.
            expectedValue, // Значение, написанное в условии.
            StringComparison.OrdinalIgnoreCase); // Не учитываем регистр букв.
    } // Закрываем метод CheckCondition.

    /// <summary>
    /// Читает шаблон из файла и обрабатывает его.
    /// </summary>
    /// <param name="filePath">Путь к файлу шаблона.</param>
    /// <param name="dataModel">Объект с данными.</param>
    /// <returns>Обработанный текст файла.</returns>
    public string RenderFromFile(string filePath, object dataModel) // Обрабатываем шаблон из файла.
    {
        string htmlTemplate = File.ReadAllText(filePath); // Читаем весь файл в строку.

        return RenderFromString(htmlTemplate, dataModel); // Обрабатываем прочитанный текст.
    } // Закрываем метод RenderFromFile.

    /// <summary>
    /// Читает шаблон, обрабатывает его и записывает результат в файл.
    /// </summary>
    /// <param name="inputFilePath">Путь к исходному файлу.</param>
    /// <param name="outputFilePath">Путь к файлу для результата.</param>
    /// <param name="dataModel">Объект с данными.</param>
    /// <returns>Обработанный текст шаблона.</returns>
    public string RenderToFile( // Объявляем метод для записи результата в файл.
        string inputFilePath, // Получаем путь к исходному шаблону.
        string outputFilePath, // Получаем путь для сохранения результата.
        object dataModel) // Получаем объект с данными.
    {
        string result = RenderFromFile(inputFilePath, dataModel); // Читаем и обрабатываем исходный файл.

        File.WriteAllText(outputFilePath, result); // Записываем результат в файл.

        return result; // Возвращаем обработанный текст.
    } // Закрываем метод RenderToFile.
} // Закрываем класс HtmlTemplateRenderer.
