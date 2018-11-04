using System;

namespace UploadEeUploader
{
    public static class StringExtensions
    {
        /// <summary>
        /// Вырезает одну строку между двумя подстроками. Если совпадений нет, вернет <paramref name="notFoundValue"/> или по-умолчанию <keyword>null</keyword>.
        /// <remarks>
        /// Создана для удобства, для написания исключений через ?? тернарный оператор.</remarks>
        /// <example>
        /// str.Between("<tag>","</tag>") ?? throw new Exception("Не найдена строка");
        /// </example>
        /// </summary>
        /// <param name="self">Строка где следует искать подстроки</param>
        /// <param name="left">Начальная подстрока</param>
        /// <param name="right">Конечная подстрока</param>
        /// <param name="startIndex">Искать начиная с индекса</param>
        /// <param name="comparsion">Метод сравнения строк</param>
        /// <param name="notFoundValue">Значение в случае если подстрока не найдена</param>
        /// <returns>Возвращает строку между двумя подстроками или <paramref name="notFoundValue"/> (по-умолчанию <keyword>null</keyword>).</returns>
        public static string Between(this string self, string left, string right,
            int startIndex = 0, StringComparison comparsion = StringComparison.Ordinal, string notFoundValue = null)
        {
            if (string.IsNullOrEmpty(self) || string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right) ||
                startIndex < 0 || startIndex >= self.Length)
                return notFoundValue;

            // Ищем начало позиции левой подстроки.
            int leftPosBegin = self.IndexOf(left, startIndex, comparsion);
            if (leftPosBegin == -1)
                return notFoundValue;

            // Вычисляем конец позиции левой подстроки.
            int leftPosEnd = leftPosBegin + left.Length;
            // Ищем начало позиции правой строки.
            int rightPos = self.IndexOf(right, leftPosEnd, comparsion);

            return rightPos != -1 ? self.Substring(leftPosEnd, rightPos - leftPosEnd) : notFoundValue;
        }
    }
}
