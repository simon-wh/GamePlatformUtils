using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePlatformUtils.Steam.Utils
{
    public interface IKeyValue
    {
        string Key { get; set; }
    }

    public class KeyValueAttribute : IKeyValue
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public KeyValueAttribute(string key, string val)
        {
            this.Key = key;
            this.Value = val;
        }
    }

    public class KeyValueTable : IKeyValue
    {
        public string Key { get; set; }

        public Dictionary<string, KeyValueAttribute> Attributes { get; set; }

        public Dictionary<string, KeyValueTable> SubTables { get; set; }

        public KeyValueTable()
        {
            this.Attributes = new Dictionary<string, KeyValueAttribute>();
            this.SubTables = new Dictionary<string, KeyValueTable>();
        }

        public KeyValueTable(StreamReader str, bool sub = false) : this()
        {
            this.ReadTable(str);
        }

        private void EatWhiteSpace(StreamReader str)
        {
            //Skip over any whitespace characters to get to next value
            while (char.IsWhiteSpace((char)str.Peek()))
                str.Read();
        }

        public void Read(StreamReader str)
        {
            this.ReadItem(str);
        }

        private object ReadValue(StreamReader str)
        {
            object returnValue = null;

            this.EatWhiteSpace(str);

            char peekchar = (char)str.Peek();

            if (peekchar.Equals('{'))
                returnValue = new KeyValueTable(str);
            else if (peekchar.Equals('/'))
            {
                //Comment, read until end of line
                str.ReadLine();
            }
            else
                returnValue = ReadString(str);

            return returnValue;
        }

        private Dictionary<char, char> escape_characters = new Dictionary<char, char>{
            { 'r', '\r' },
            { 'n', '\n' },
            { 't', '\t' },
            { '\'', '\'' },
            {'"', '"' },
            {'\\', '\\' },
            {'b', '\b' },
            {'f', '\f' },
            {'v', '\v' }
        };

        private string ReadString(StreamReader str)
        {
            StringBuilder builder = new StringBuilder();

            bool isQuote = ((char)str.Peek()).Equals('"');

            if (isQuote)
                str.Read();

            for (char chr = (char)str.Read(); !str.EndOfStream; chr = (char)str.Read())
            {

                if ((isQuote && chr.Equals('"')) || (!isQuote && char.IsWhiteSpace(chr))) //Arrived at end of string
                    break;

                if (chr.Equals('\\')) //Fix up escaped characters
                {
                    char escape = (char)str.Read();

                    if (this.escape_characters.ContainsKey(escape))
                        builder.Append(this.escape_characters[escape]);
                }
                else
                    builder.Append(chr);

            }

            return builder.ToString();
        }

        private void ReadTable(StreamReader str)
        {
            //Read first {
            str.Read();

            this.EatWhiteSpace(str);

            while (!((char)str.Peek()).Equals('}'))
            {
                this.ReadItem(str);
            }

            //Read last }
            str.Read();
        }

        private void ReadItem(StreamReader str)
        {
            string key = ReadValue(str) as string;
            if (key != null)
            {
                key = key.ToLowerInvariant();
                object val = ReadValue(str);

                IKeyValue key_val;

                if (val is string)
                    key_val = new KeyValueAttribute(key, (string)val);
                else
                    key_val = val as IKeyValue;

                if (key_val != null)
                {
                    key_val.Key = key;
                    if (key_val is KeyValueTable)
                        this.SubTables.Add(key, (KeyValueTable)key_val);
                    else if (key_val is KeyValueAttribute)
                        this.Attributes.Add(key, (KeyValueAttribute)key_val);
                }
            }
            this.EatWhiteSpace(str);
        }

        public bool TryGetAttribute(string key, out KeyValueAttribute att)
        {
            if (this.Attributes.ContainsKey(key))
            {
                att = this.Attributes[key];
                return true;
            }

            att = null;
            return false;
        }

        public IKeyValue Child(string name)
        {
            if (this.SubTables.ContainsKey(name))
                return this.SubTables[name];
            else if (this.Attributes.ContainsKey(name))
                return this.Attributes[name];


            return null;
        }
    }

    public class KeyValue
    {
        public KeyValueTable RootNode { get; set; }

        public KeyValue(string path)
        {
            if (File.Exists(path))
            {
                using (StreamReader str = new StreamReader(path))
                    this.Read(str);
            }
            else
            {
                throw new FileNotFoundException("The passed path does not exist!");
            }
        }

        public KeyValue(Stream instream)
        {
            using (StreamReader str = new StreamReader(instream))
                this.Read(str);
        }

        private void Read(StreamReader str)
        {
            this.RootNode = new KeyValueTable();
            while (!str.EndOfStream)
            {
                //Attempt to read the item key
                this.RootNode.Read(str);
            }
        }
    }

}
