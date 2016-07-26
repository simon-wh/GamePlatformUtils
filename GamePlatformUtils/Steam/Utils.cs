using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlatformUtils.Steam
{
    public class KeyValue
    {
        public Dictionary<string, object> Items { get { return _items; } }

        private Dictionary<string, object> _items = new Dictionary<string, object>();
        public KeyValue(Stream instream)
        {
            using (StreamReader sr = new StreamReader(instream))
            {
                while (!sr.EndOfStream)
                {
                    //Attempt to read the item key
                    object keyValue = ReadValue(sr);
                    if (keyValue != null)
                        _items.Add(((string)keyValue).ToLowerInvariant(), ReadValue(sr));

                    //Skip over any whitespace characters to get to next value
                    while (char.IsWhiteSpace((char)sr.Peek()))
                        sr.Read();
                }
            }
            instream.Close();
        }

        private object ReadValue(StreamReader instream)
        {
            object returnValue = null;

            //Skip over any whitespace characters to get to next value
            while (char.IsWhiteSpace((char)instream.Peek()))
                instream.Read();

            char peekchar = (char)instream.Peek();

            if (peekchar.Equals('{'))
                returnValue = ReadSubValues(instream);
            else if (peekchar.Equals('/'))
            {
                //Comment, read until end of line
                instream.ReadLine();
            }
            else
                returnValue = ReadString(instream);

            return returnValue;
        }

        private string ReadString(StreamReader instream)
        {
            StringBuilder builder = new StringBuilder();

            bool isQuote = ((char)instream.Peek()).Equals('"');

            if (isQuote)
                instream.Read();

            for (char chr = (char)instream.Read(); !instream.EndOfStream; chr = (char)instream.Read())
            {

                if (isQuote && chr.Equals('"') ||
                    !isQuote && char.IsWhiteSpace(chr)) //Arrived at end of string
                    break;

                if (chr.Equals('\\')) //Fix up escaped characters
                {
                    char escape = (char)instream.Read();

                    switch(escape)
                    {
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case '\'':
                            builder.Append('\'');
                            break;
                        case '"':
                            builder.Append('"');
                            break;
                        case '\\':
                            builder.Append('\\');
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'v':
                            builder.Append('\v');
                            break;
                    }
                }
                else
                    builder.Append(chr);

            }

            return builder.ToString();
        }

        private Dictionary<string, object> ReadSubValues(StreamReader instream)
        {
            Dictionary<string, object> subValues = new Dictionary<string, object>();

            //Read first {
            instream.Read();

            //Seek to next data
            while (char.IsWhiteSpace((char)instream.Peek()))
                instream.Read();

            while (!((char)instream.Peek()).Equals('}'))
            {
                object keyValue = ReadValue(instream);
                if (keyValue != null)
                    subValues.Add(((string)keyValue).ToLowerInvariant(), ReadValue(instream));

                //Seek to next data
                while (char.IsWhiteSpace((char)instream.Peek()))
                    instream.Read();
            }

            //Read last }
            instream.Read();

            return subValues;
        }
    }

}
