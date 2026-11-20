using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DaoDenLong.Core
{
    /// <summary>
    /// Minimal JSON value tree used by both the Unity player and the .NET headless runner.
    /// Dependency free (no System.Text.Json) so both targets behave identically, and so
    /// serialization order and number formatting stay fully deterministic.
    /// </summary>
    public sealed class JsonValue
    {
        public enum Kind { Null, Bool, NumInt, NumReal, Str, Arr, Obj }

        public Kind Type;
        public bool B;
        public long I;
        public double D;
        public string S;
        public List<JsonValue> Arr_ = new List<JsonValue>();
        public List<KeyValuePair<string, JsonValue>> Members = new List<KeyValuePair<string, JsonValue>>();

        // Number formatting used for output documents (Test Contract: coordinates get 3 decimals).
        public int Decimals = 0;

        public static JsonValue Null() { return new JsonValue { Type = Kind.Null }; }
        public static JsonValue Bool(bool v) { return new JsonValue { Type = Kind.Bool, B = v }; }
        public static JsonValue Int(long v) { return new JsonValue { Type = Kind.NumInt, I = v }; }
        public static JsonValue Real(double v, int decimals = 3) { return new JsonValue { Type = Kind.NumReal, D = v, Decimals = decimals }; }
        public static JsonValue Str(string v) { return new JsonValue { Type = Kind.Str, S = v }; }
        public static JsonValue Arr() { return new JsonValue { Type = Kind.Arr }; }
        public static JsonValue Obj() { return new JsonValue { Type = Kind.Obj }; }

        public JsonValue Add(JsonValue v) { Arr_.Add(v); return this; }

        public JsonValue Set(string key, JsonValue v)
        {
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].Key == key) { Members[i] = new KeyValuePair<string, JsonValue>(key, v); return this; }
            }
            Members.Add(new KeyValuePair<string, JsonValue>(key, v));
            return this;
        }

        public bool Has(string key)
        {
            if (Type != Kind.Obj) return false;
            for (int i = 0; i < Members.Count; i++) if (Members[i].Key == key) return true;
            return false;
        }

        public JsonValue Get(string key)
        {
            if (Type != Kind.Obj) return null;
            for (int i = 0; i < Members.Count; i++) if (Members[i].Key == key) return Members[i].Value;
            return null;
        }

        public double Num(double fallback) { return (Type == Kind.NumInt) ? I : (Type == Kind.NumReal ? D : fallback); }
        public bool Flag(bool fallback) { return Type == Kind.Bool ? B : fallback; }
        public string Text(string fallback) { return Type == Kind.Str ? S : fallback; }

        public double[] Vec(int n)
        {
            if (Type != Kind.Arr || Arr_.Count < n) return null;
            double[] r = new double[n];
            for (int i = 0; i < n; i++) r[i] = Arr_[i].Num(double.NaN);
            return r;
        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            Write(sb);
            return sb.ToString();
        }

        private static void Esc(string s, StringBuilder sb)
        {
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        private void Write(StringBuilder sb)
        {
            switch (Type)
            {
                case Kind.Null: sb.Append("null"); break;
                case Kind.Bool: sb.Append(B ? "true" : "false"); break;
                case Kind.NumInt: sb.Append(I.ToString(CultureInfo.InvariantCulture)); break;
                case Kind.NumReal:
                    double v = D;
                    if (v == 0.0) v = 0.0; // collapse -0.0 so two runs produce the same text
                    sb.Append(v.ToString("F" + Decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
                    break;
                case Kind.Str: Esc(S, sb); break;
                case Kind.Arr:
                    sb.Append('[');
                    for (int i = 0; i < Arr_.Count; i++) { if (i > 0) sb.Append(','); Arr_[i].Write(sb); }
                    sb.Append(']');
                    break;
                case Kind.Obj:
                    sb.Append('{');
                    for (int i = 0; i < Members.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        Esc(Members[i].Key, sb);
                        sb.Append(':');
                        Members[i].Value.Write(sb);
                    }
                    sb.Append('}');
                    break;
            }
        }

        /// <summary>Parse JSON text. Throws FormatException on malformed input (runner maps that to exit code 2).</summary>
        public static JsonValue Parse(string text)
        {
            int pos = 0;
            JsonValue v = ParseValue(text, ref pos);
            SkipWs(text, ref pos);
            if (pos != text.Length) throw new FormatException("trailing characters in JSON");
            return v;
        }

        private static void SkipWs(string s, ref int p)
        {
            while (p < s.Length && (s[p] == ' ' || s[p] == '\t' || s[p] == '\n' || s[p] == '\r')) p++;
        }

        private static JsonValue ParseValue(string s, ref int p)
        {
            SkipWs(s, ref p);
            if (p >= s.Length) throw new FormatException("unexpected end of JSON");
            char c = s[p];
            if (c == '{')
            {
                JsonValue o = Obj(); p++;
                SkipWs(s, ref p);
                if (p < s.Length && s[p] == '}') { p++; return o; }
                while (true)
                {
                    SkipWs(s, ref p);
                    if (p >= s.Length || s[p] != '"') throw new FormatException("expected string key");
                    string key = ParseString(s, ref p);
                    SkipWs(s, ref p);
                    if (p >= s.Length || s[p] != ':') throw new FormatException("expected ':'");
                    p++;
                    o.Members.Add(new KeyValuePair<string, JsonValue>(key, ParseValue(s, ref p)));
                    SkipWs(s, ref p);
                    if (p < s.Length && s[p] == ',') { p++; continue; }
                    if (p < s.Length && s[p] == '}') { p++; return o; }
                    throw new FormatException("expected ',' or '}'");
                }
            }
            if (c == '[')
            {
                JsonValue a = Arr(); p++;
                SkipWs(s, ref p);
                if (p < s.Length && s[p] == ']') { p++; return a; }
                while (true)
                {
                    a.Arr_.Add(ParseValue(s, ref p));
                    SkipWs(s, ref p);
                    if (p < s.Length && s[p] == ',') { p++; continue; }
                    if (p < s.Length && s[p] == ']') { p++; return a; }
                    throw new FormatException("expected ',' or ']'");
                }
            }
            if (c == '"') return Str(ParseString(s, ref p));
            if (c == 't' || c == 'f')
            {
                bool val = c == 't';
                string lit = val ? "true" : "false";
                if (p + lit.Length > s.Length || s.Substring(p, lit.Length) != lit) throw new FormatException("bad literal");
                p += lit.Length;
                return Bool(val);
            }
            if (c == 'n')
            {
                if (p + 4 > s.Length || s.Substring(p, 4) != "null") throw new FormatException("bad literal");
                p += 4;
                return Null();
            }
            int start = p;
            while (p < s.Length && "+-.eE0123456789".IndexOf(s[p]) >= 0) p++;
            string num = s.Substring(start, p - start);
            if (num.Length == 0) throw new FormatException("unexpected character in JSON");
            bool isInt = num.IndexOf('.') < 0 && num.IndexOf('e') < 0 && num.IndexOf('E') < 0;
            if (isInt)
            {
                long lv;
                if (!long.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out lv)) throw new FormatException("bad integer " + num);
                return Int(lv);
            }
            double dv;
            if (!double.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out dv)) throw new FormatException("bad number " + num);
            return Real(dv);
        }

        private static string ParseString(string s, ref int p)
        {
            p++; // opening quote
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (p >= s.Length) throw new FormatException("unterminated string");
                char c = s[p++];
                if (c == '"') return sb.ToString();
                if (c == '\\')
                {
                    if (p >= s.Length) throw new FormatException("bad escape");
                    char e = s[p++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (p + 4 > s.Length) throw new FormatException("bad \\u escape");
                            sb.Append((char)int.Parse(s.Substring(p, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            p += 4;
                            break;
                        default: throw new FormatException("bad escape");
                    }
                }
                else sb.Append(c);
            }
        }
    }
}
