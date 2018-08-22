using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Carat
{
    /// <summary>
    /// Represents a reader for Carat++ files.
    /// </summary>
    public class CaratReader : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaratReader"/> class.
        /// </summary>
        /// <param name="textReader">The TextReader used to feed the data into the document.</param>
        public CaratReader(TextReader textReader) : this(textReader, false) { }

        public void Dispose()
        {
            if (_ownsTextReader)
            {
                _textReader.Dispose();
            }
        }


        /// <summary>
        /// Creates a <see cref="CaratReader"/> to read from a string.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>The <see cref="CaratReader"/> to read the content.</returns>
        public static CaratReader FromText(string text)
        {
            var textReader = new StringReader(text);

            return new CaratReader(textReader, true);
        }

        /// <summary>
        /// Creates a <see cref="CaratReader"/> to read from a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The <see cref="CaratReader"/> to read the content.</returns>
        public static CaratReader FromFile(string path)
        {
            var textReader = new StreamReader(path, Encoding.ASCII);

            return new CaratReader(textReader, true);
        }


        /// <summary>
        /// Gets the current line number.
        /// </summary>
        public int CurrentLineNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public string CurrentToken
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the reader is positioned at the end of the stream.
        /// </summary>
        public bool EOF
        {
            get
            {
                return CurrentToken == null;
            }
        }


        /// <summary>
        /// Ignores the current token.
        /// </summary>
        public void Ignore()
        {
            Move();
        }


        /// <summary>
        /// Checks if the current token is equal to a given expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        /// <returns><code>true</code> if the current token matches the expression, otherwise <code>false</code>.</returns>
        public bool Match(string expression)
        {
            if (CurrentToken == null)
            {
                return false;
            }

            var match = string.Equals(CurrentToken, expression, StringComparison.OrdinalIgnoreCase);

            if (!match)
            {
                return false;
            }

            Move();

            return true;
        }

        /// <summary>
        /// Checks if the current token is equal to any of the given expressions.
        /// </summary>
        /// <param name="expressions">The expressions to check.</param>
        /// <returns><code>true</code> if the current token matches any of the expressions, otherwise <code>false</code>.</returns>
        public bool MatchAny(params string[] expressions)
        {
            var match = expressions.Contains(CurrentToken, StringComparer.OrdinalIgnoreCase);

            if (!match)
            {
                return false;
            }

            Move();

            return true;
        }

        /// <summary>
        /// Checks if the current token is equal to any of the given expressions.
        /// </summary>
        /// <param name="expressions">The expressions to check.</param>
        /// <param name="index">When this method returns, contains the index of the expression that matches.</param>
        /// <returns><code>true</code> if the current token matches any of the expressions, otherwise <code>false</code>.</returns>
        public bool MatchAny(string[] expressions, out int index)
        {
            index = Array.FindIndex(expressions, o => string.Equals(o, CurrentToken, StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                return false;
            }

            Move();

            return true;
        }


        /// <summary>
        /// Throws an error if the current token does not match the given expression.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        public void Expect(string expression)
        {
            var match = CurrentToken.Equals(expression, StringComparison.OrdinalIgnoreCase);

            if (!match)
            {
                throw new ParserException($"'{expression}' expected but found '{CurrentToken}'", CurrentLineNumber);
            }

            Move();
        }

        /// <summary>
        /// Throws an error if the current token does not match any of the given expressions.
        /// </summary>
        /// <param name="expression">The expressions to check.</param>
        /// <returns>When this method returns, contains the index of the matching expression.</returns>
        public int ExpectAny(params string[] expressions)
        {
            for (int i = 0; i < expressions.Length; i++)
            {
                var expression = expressions[i];

                var match = CurrentToken.Equals(expression, StringComparison.OrdinalIgnoreCase);

                if (match)
                {
                    Move();
                    return i;
                }
            }

            var expressionsStr = string.Join(", ", expressions.Select(o => $"'{o}'"));

            throw new ParserException($"'{expressionsStr}' expected but found '{CurrentToken}'", CurrentLineNumber);
        }


        /// <summary>
        /// Parses the current token as a <code>bool</code> and in case of success assigns the value to the given variable.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <returns><code>true</code> if the parsing was successfull, otherwise <code>false</code>.</returns>
        public bool TryRead(ref bool? variable)
        {
            if (!TryParse(CurrentToken, out bool value))
            {
                return false;
            }

            variable = value;

            Move();

            return true;
        }

        /// <summary>
        /// Parses the current token as a <code>int</code> and in case of success assigns the value to the given variable.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <returns><code>true</code> if the parsing was successfull, otherwise <code>false</code>.</returns>
        public bool TryRead(ref int? variable)
        {
            if (variable.HasValue || !TryParse(CurrentToken, out int value))
            {
                return false;
            }

            variable = value;

            Move();

            return true;
        }

        /// <summary>
        /// Parses the current token as a <code>double</code> and in case of success assigns the value to the given variable.
        /// </summary>
        /// <param name="variable">The variable to assign.</param>
        /// <returns><code>true</code> if the parsing was successfull, otherwise <code>false</code>.</returns>
        public bool TryRead(ref double? variable)
        {
            if (variable.HasValue || !TryParse(CurrentToken, out double value))
            {
                return false;
            }

            variable = value;

            Move();

            return true;
        }


        /// <summary>
        /// Parses the current token as a <code>bool</code> and in case of success assigns the value to the given variable.
        /// </summary>
        /// <param name="value">The parsed value.</param>
        /// <returns><code>true</code> if the parsing was successfull, otherwise <code>false</code>.</returns>
        public bool TryRead(out bool value)
        {
            if (!TryParse(CurrentToken, out value))
            {
                return false;
            }

            Move();

            return true;
        }

        /// <summary>
        /// Parses the current token as an <code>int</code> and in case of success assigns the value to the given variable.
        /// </summary>
        /// <param name="value">The parsed value.</param>
        /// <returns><code>true</code> if the parsing was successfull, otherwise <code>false</code>.</returns>
        public bool TryRead(out int value)
        {
            if (!TryParse(CurrentToken, out value))
            {
                return false;
            }

            Move();

            return true;
        }

        /// <summary>
        /// Parses the current token as a <code>double</code> and in case of success assigns the value to the given variable.
        /// </summary>
        /// <param name="value">The parsed value.</param>
        /// <returns><code>true</code> if the parsing was successfull, otherwise <code>false</code>.</returns>
        public bool TryRead(out double value)
        {
            if (!TryParse(CurrentToken, out value))
            {
                return false;
            }

            Move();

            return true;
        }


        /// <summary>
        /// Returns the current token as a <code>bool</code> or the default value if the parsing fails.
        /// </summary>
        /// <param name="defaultValue">The default return value.</param>
        /// <returns>The current token as a <code>bool</code>.</returns>
        public bool ReadBoolean(bool defaultValue)
        {
            if (!TryParse(CurrentToken, out bool value))
            {
                return defaultValue;
            }

            Move();

            return value;
        }

        /// <summary>
        /// Returns the current token as an <code>int</code> or the default value if the parsing fails.
        /// </summary>
        /// <param name="defaultValue">The default return value.</param>
        /// <returns>The current token as an <code>int</code>.</returns>
        public int ReadInteger(int defaultValue)
        {
            if (!TryParse(CurrentToken, out int value))
            {
                return defaultValue;
            }

            Move();

            return value;
        }

        /// <summary>
        /// Returns the current token as a <code>double</code> or the default value if the parsing fails.
        /// </summary>
        /// <param name="defaultValue">The default return value.</param>
        /// <returns>The current token as a <code>double</code>.</returns>
        public double ReadDouble(double defaultValue)
        {
            if (!TryParse(CurrentToken, out double value))
            {
                return defaultValue;
            }

            Move();

            return value;
        }


        /// <summary>
        /// Returns the current token as a <code>string</code>.
        /// </summary>
        /// <returns>The current token as a <code>string</code>.</returns>
        public string ReadString()
        {
            var value = CurrentToken;

            Move();

            return value;
        }

        /// <summary>
        /// Returns the current token as a <code>bool</code>.
        /// </summary>
        /// <returns>The current token as a <code>bool</code>.</returns>
        public bool ReadBoolean()
        {
            if (!TryRead(out bool value))
            {
                throw new ParserException($"Boolean expected but found '{CurrentToken}'", CurrentLineNumber);
            }

            return value;
        }

        /// <summary>
        /// Returns the current token as an <code>int</code>.
        /// </summary>
        /// <returns>The current token as an <code>int</code>.</returns>
        public int ReadInteger()
        {
            if (!TryRead(out int value))
            {
                throw new ParserException($"Integer expected but found '{CurrentToken}'", CurrentLineNumber);
            }

            return value;
        }

        /// <summary>
        /// Returns the current token as a <code>double</code>.
        /// </summary>
        /// <returns>The current token as a <code>double</code></returns>
        public double ReadDouble()
        {
            if (!TryRead(out double value))
            {
                throw new ParserException($"Number expected but found '{CurrentToken}'", CurrentLineNumber);
            }

            return value;
        }


        /// <summary>
        /// Returns a sequence of comma separated tokens as a list of <code>string</code>.
        /// </summary>
        /// <returns>The tokens as a list of <code>string</code></returns>
        public List<string> ReadStringList()
        {
            var values = new List<string>();

            while (!EOF)
            {
                var value = ReadString();

                values.Add(value);

                if (Match(","))
                {
                    continue;
                }

                break;
            }

            return values;
        }

        /// <summary>
        /// Returns a sequence of comma separated tokens as a list of <code>bool</code>.
        /// </summary>
        /// <returns>The tokens as a list of <code>bool</code></returns>
        public List<bool> ReadBooleanList()
        {
            var values = new List<bool>();

            while (!EOF)
            {
                var value = ReadBoolean();

                values.Add(value);

                if (Match(","))
                {
                    continue;
                }

                break;
            }

            return values;
        }

        /// <summary>
        /// Returns a sequence of comma separated tokens as a list of <code>int</code>.
        /// </summary>
        /// <returns>The tokens as a list of <code>int</code></returns>
        public List<int> ReadIntegerList()
        {
            var values = new List<int>();

            while (!EOF)
            {
                var value = ReadInteger();

                values.Add(value);

                if (Match(","))
                {
                    continue;
                }

                break;
            }

            return values;
        }

        /// <summary>
        /// Returns a sequence of comma separated tokens as a list of <code>double</code>.
        /// </summary>
        /// <returns>The tokens as a list of <code>double</code></returns>
        public List<double> ReadDoubleList()
        {
            var values = new List<double>();

            while (!EOF)
            {
                var value = ReadDouble();

                values.Add(value);

                if (Match(","))
                {
                    continue;
                }

                break;
            }

            return values;
        }


        public Exception NewUnexpectedTokenException()
        {
            return new ParserException($"Unexpected '{CurrentToken}'", CurrentLineNumber);
        }

        public Exception NewDuplicateTokenException()
        {
            return new ParserException($"Duplicate '{CurrentToken}'", CurrentLineNumber);
        }


        public void AssertIsSet<T>(T value, string name)
        {
            if (value == null)
                throw new ParserException($"\"{name}\" not specified", CurrentLineNumber);
        }


        private CaratReader(TextReader textReader, bool ownsTextReader)
        {
            _textReader = textReader;
            _ownsTextReader = ownsTextReader;

            Move();
        }


        private readonly TextReader _textReader;

        private readonly bool _ownsTextReader;

        private readonly Queue<string> _tokens = new Queue<string>();

        private string CurrentLine { get; set; }


        private void ReadLine()
        {
            while (true)
            {
                CurrentLine = _textReader.ReadLine();

                if (CurrentLine == null)
                {
                    return;
                }

                CurrentLineNumber += 1;

                var commentStart = CurrentLine.IndexOf('!');

                if (commentStart == 0)
                {
                    continue;
                }

                if (commentStart > 0)
                {
                    CurrentLine = CurrentLine.Substring(0, commentStart);
                }

                CurrentLine = CurrentLine.Trim();

                if (CurrentLine != string.Empty)
                {
                    break;
                }
            }
        }

        private void Move()
        {
            if (_tokens.Count != 0)
            {
                CurrentToken = _tokens.Dequeue();
            }
            else
            {
                ReadLine();

                if (CurrentLine != null)
                {
                    var tokens = CurrentLine.Replace(":", " : ")
                                            .Replace("=", " = ")
                                            .Replace(",", " , ")
                                            .Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                    foreach (var token in tokens)
                    {
                        _tokens.Enqueue(token);
                    }

                    CurrentToken = _tokens.Dequeue();
                }
                else
                {
                    CurrentToken = null;
                }
            }
        }


        private bool ParseBoolean(string s)
        {
            return bool.Parse(s);
        }

        private int ParseInteger(string s)
        {
            return int.Parse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
        }

        private double ParseDouble(string s)
        {
            return double.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
        }


        private bool TryParse(string s, out bool result)
        {
            return bool.TryParse(s, out result);
        }

        private bool TryParse(string s, out int result)
        {
            return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }

        private bool TryParse(string s, out double result)
        {
            return double.TryParse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result);
        }
    }
}
