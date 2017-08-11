using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Siotrix.Discord
{
    public static class StringTableExtensions
    {
        public static string ToStringTable<T>(this IEnumerable<T> values,
            params Expression<Func<T, object>>[] valueSelectors)
        {
            var headers = valueSelectors.Select(func => GetProperty(func).Name).ToArray();
            var selectors = valueSelectors.Select(exp => exp.Compile()).ToArray();
            return ToStringTable(values, headers, selectors);
        }

        public static string ToStringTable<T>(this IEnumerable<T> values, string[] columnHeaders,
            params Func<T, object>[] valueSelectors)
        {
            return ToStringTable(values.ToArray(), columnHeaders, valueSelectors);
        }

        public static string ToStringTable<T>(this T[] values, string[] columnHeaders,
            params Func<T, object>[] valueSelectors)
        {
            Debug.Assert(columnHeaders.Length == valueSelectors.Length);

            var arrValues = new string[values.Length + 1, valueSelectors.Length];

            // Fill headers
            for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                arrValues[0, colIndex] = columnHeaders[colIndex];

            // Fill table rows
            for (var rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++)
            for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                arrValues[rowIndex, colIndex] = valueSelectors[colIndex]
                    .Invoke(values[rowIndex - 1]).ToString();

            return ToStringTable(arrValues);
        }

        public static string ToStringTable(this string[,] arrValues)
        {
            var maxColumnsWidth = GetMaxColumnsWidth(arrValues);
            var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

            var sb = new StringBuilder();
            for (var rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    // Print cell
                    var cell = arrValues[rowIndex, colIndex];
                    cell = cell.PadRight(maxColumnsWidth[colIndex]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print end of line
                sb.Append(" | ");
                sb.AppendLine();

                // Print splitter
                if (rowIndex == 0)
                {
                    sb.AppendFormat(" |{0}| ", headerSpliter);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static int[] GetMaxColumnsWidth(string[,] arrValues)
        {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            for (var rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                var newLength = arrValues[rowIndex, colIndex].Length;
                var oldLength = maxColumnsWidth[colIndex];

                if (newLength > oldLength)
                    maxColumnsWidth[colIndex] = newLength;
            }

            return maxColumnsWidth;
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expresstion)
        {
            if (expresstion.Body is UnaryExpression)
                if ((expresstion.Body as UnaryExpression).Operand is MemberExpression)
                    return ((expresstion.Body as UnaryExpression).Operand as MemberExpression).Member as PropertyInfo;

            if (expresstion.Body is MemberExpression)
                return (expresstion.Body as MemberExpression).Member as PropertyInfo;
            return null;
        }

        public interface ITextRow
        {
            object Tag { get; set; }
            string Output();
            void Output(StringBuilder sb);
        }

        public class TableBuilder : IEnumerable<ITextRow>
        {
            protected string _fmtString;
            protected List<int> colLength = new List<int>();

            protected List<ITextRow> rows = new List<ITextRow>();

            public TableBuilder()
            {
                Separator = "  ";
            }

            public TableBuilder(string separator)
                : this()
            {
                Separator = separator;
            }

            public string Separator { get; set; }

            public string FormatString
            {
                get
                {
                    if (_fmtString == null)
                    {
                        var format = "";
                        var i = 0;
                        foreach (var len in colLength)
                            format += string.Format("{{{0},-{1}}}{2}", i++, len, Separator);
                        format += "\r\n";
                        _fmtString = format;
                    }
                    return _fmtString;
                }
            }

            public ITextRow AddRow(params object[] cols)
            {
                var row = new TextRow(this);
                foreach (var o in cols)
                {
                    var str = o.ToString().Trim();
                    row.Add(str);
                    if (colLength.Count >= row.Count)
                    {
                        var curLength = colLength[row.Count - 1];
                        if (str.Length > curLength) colLength[row.Count - 1] = str.Length;
                    }
                    else
                    {
                        colLength.Add(str.Length);
                    }
                }
                rows.Add(row);
                return row;
            }

            public string Output()
            {
                var sb = new StringBuilder();
                foreach (TextRow row in rows)
                    row.Output(sb);
                return sb.ToString();
            }

            protected class TextRow : List<string>, ITextRow
            {
                protected TableBuilder owner;

                public TextRow(TableBuilder Owner)
                {
                    owner = Owner;
                    if (owner == null) throw new ArgumentException("Owner");
                }

                public string Output()
                {
                    var sb = new StringBuilder();
                    Output(sb);
                    return sb.ToString();
                }

                public void Output(StringBuilder sb)
                {
                    sb.AppendFormat(owner.FormatString, ToArray());
                }

                public object Tag { get; set; }
            }

            #region IEnumerable Members

            public IEnumerator<ITextRow> GetEnumerator()
            {
                return rows.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return rows.GetEnumerator();
            }

            #endregion
        }
    }
}