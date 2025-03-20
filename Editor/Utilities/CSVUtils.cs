//From: https://stackoverflow.com/questions/60444221/c-sharp-ignore-split-character-if-its-inside-parentheses

using System.Collections.Generic;
using System.Linq;

namespace Editor.Utilities
{
    /// <summary>
    ///     Provides utility methods for parsing CSV (Comma-Separated Values) formatted strings.
    /// </summary>
    public static class CSVUtils
    {
        /// <summary>
        ///     Represents the opening parenthesis character used for grouping.
        /// </summary>
        private const string GroupOpen = "(";

        /// <summary>
        ///     Represents the closing parenthesis character used for grouping.
        /// </summary>
        private const string GroupClose = ")";

        /// <summary>
        ///     Returns a substring from the beginning of the source string with specified length.
        /// </summary>
        /// <param name="source">The source string to peek into.</param>
        /// <param name="peek">The number of characters to peek.</param>
        /// <returns>A substring of specified length or null if source is null or peek is negative.</returns>
        private static string Peek(this string source, int peek)
        {
            return source == null || peek < 0
                ? null
                : source.Substring(0, source.Length < peek ? source.Length : peek);
        }

        /// <summary>
        ///     Splits a string into two parts: the first 'pop' characters and the remainder.
        /// </summary>
        /// <param name="source">The source string to split.</param>
        /// <param name="pop">The number of characters to include in the first part.</param>
        /// <returns>A tuple containing the popped substring and the remaining string.</returns>
        private static (string, string) Pop(this string source, int pop)
        {
            return source == null || pop < 0
                ? (null, source)
                : (source.Substring(0, source.Length < pop ? source.Length : pop),
                    source.Length < pop ? string.Empty : source.Substring(pop));
        }

        /// <summary>
        ///     Parses a CSV line into an array of fields.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <returns>An array of strings representing the fields in the CSV line.</returns>
        public static string[] ParseLine(this string line)
        {
            return ParseLineImpl(line).ToArray();

            IEnumerable<string> ParseLineImpl(string l)
            {
                var remainder = line;
                string field;
                while (remainder.Peek(1) != "")
                {
                    (field, remainder) = ParseField(remainder);
                    yield return field;
                }
            }
        }

        /// <summary>
        ///     Parses a single field from a CSV line.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <returns>A tuple containing the parsed field and the remaining string.</returns>
        private static (string field, string remainder) ParseField(string line)
        {
            if (line.Peek(1) == GroupOpen)
            {
                var (_, split) = line.Pop(1);
                return ParseFieldQuoted(split);
            }

            var field = "";
            var (head, tail) = line.Pop(1);
            while (head != "," && head != "")
            {
                field += head;
                (head, tail) = tail.Pop(1);
            }

            return (field, tail);
        }

        /// <summary>
        ///     Parses a quoted field from a CSV line.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <returns>A tuple containing the parsed quoted field and the remaining string.</returns>
        private static (string field, string remainder) ParseFieldQuoted(string line)
        {
            return ParseFieldQuoted(line, false);
        }

        /// <summary>
        ///     Parses a quoted field from a CSV line with support for nested quotes.
        /// </summary>
        /// <param name="line">The CSV line to parse.</param>
        /// <param name="isNested">Indicates whether this is a nested quoted field.</param>
        /// <returns>A tuple containing the parsed quoted field and the remaining string.</returns>
        private static (string field, string remainder) ParseFieldQuoted(string line, bool isNested)
        {
            var field = "";
            var head = "";
            var tail = line;
            while (tail.Peek(1) != "" && tail.Peek(1) != GroupClose)
                if (tail.Peek(1) == GroupOpen)
                {
                    (head, tail) = tail.Pop(1);
                    (head, tail) = ParseFieldQuoted(tail, true);
                    field += GroupOpen + head + GroupClose;
                }
                else
                {
                    (head, tail) = tail.Pop(1);
                    field += head;
                }

            if (tail.Peek(2) == GroupClose + ",")
                (head, tail) = tail.Pop(isNested ? 1 : 2);
            else if (tail.Peek(1) == GroupClose) (head, tail) = tail.Pop(1);

            return (field, tail);
        }
    }
}