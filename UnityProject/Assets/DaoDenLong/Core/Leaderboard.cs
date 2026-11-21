using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DaoDenLong.Core
{
    /// <summary>
    /// v1.3 (TICKET-04) — the local leaderboard, pure core so the rules can run headless and be
    /// tested. Top 10, score descending; on a tie whoever achieved it first stands above (entry
    /// order is the achievement order). Names are [A-Za-z0-9_]{3,12}; Vietnamese diacritics are
    /// accepted as a bonus (any Unicode letter/digit plus underscore, same length window).
    /// The file lives on disk; reading and writing it is the host's job (no System.IO in the core).
    /// </summary>
    public sealed class Leaderboard
    {
        public sealed class Entry
        {
            public string Name = "";
            public long Score;
            public int NightsWon;                       // 0..3
            public string AchievedAtIso = "";           // ISO 8601, as written by the clock at achievement
            public long Seed;
            internal long AchievedSeq;                  // the moment it joined the list (the tie-break)
        }

        public const int MaxEntries = 10;
        private static readonly Regex AsciiName = new Regex("^[A-Za-z0-9_]{3,12}$");
        private static readonly Regex UnicodeName = new Regex(@"^[\p{L}\p{N}_]{3,12}$");   // the diacritics bonus

        private readonly List<Entry> _entries = new List<Entry>();

        public static bool ValidName(string name)
        {
            if (name == null) return false;
            return AsciiName.IsMatch(name) || UnicodeName.IsMatch(name);
        }

        public IReadOnlyList<Entry> Entries { get { return _entries; } }

        /// <summary>Add an achieved score and re-sort. Returns false when the name is rejected.
        /// The list keeps entries in achievement order before each sort so the tie rule —
        /// earlier achieved_at first — is the insertion order itself.</summary>
        public bool Add(string name, long score, int nightsWon, string achievedAtIso, long seed)
        {
            if (!ValidName(name)) return false;
            if (nightsWon < 0 || nightsWon > 3) throw new ArgumentOutOfRangeException("nightsWon");

            _entries.Add(new Entry { Name = name, Score = score, NightsWon = nightsWon, AchievedAtIso = achievedAtIso, Seed = seed, AchievedSeq = _entries.Count });
            Sort();
            while (_entries.Count > MaxEntries)
                _entries.RemoveAt(_entries.Count - 1);       // the list is sorted; the tail is the weakest
            return true;
        }

        private void Sort()
        {
            _entries.Sort(delegate (Entry a, Entry b)
            {
                int c = b.Score.CompareTo(a.Score);           // score descending...
                if (c != 0) return c;
                c = string.CompareOrdinal(a.AchievedAtIso, b.AchievedAtIso);   // ...ties: earlier achieved_at first...
                if (c != 0) return c;
                return a.AchievedSeq.CompareTo(b.AchievedSeq);                 // ...same instant: whoever joined first
            });
        }

        /// <summary>The export document: {"entries":[...]} in leaderboard order.</summary>
        public JsonValue ToJson()
        {
            JsonValue arr = JsonValue.Arr();
            foreach (Entry e in _entries)
                arr.Add(JsonValue.Obj()
                    .Set("name", JsonValue.Str(e.Name))
                    .Set("score", JsonValue.Int(e.Score))
                    .Set("nights_won", JsonValue.Int(e.NightsWon))
                    .Set("achieved_at", JsonValue.Str(e.AchievedAtIso))
                    .Set("seed", JsonValue.Int(e.Seed)));
            return JsonValue.Obj().Set("entries", arr);
        }

        /// <summary>Load a previously saved document (the same shape as ToJson). Invalid names are
        /// skipped — a file edited by hand never breaks the game.</summary>
        public static Leaderboard FromJson(JsonValue doc)
        {
            Leaderboard lb = new Leaderboard();
            JsonValue arr = doc.Get("entries");
            if (arr == null) return lb;
            foreach (JsonValue v in arr.Arr_)
            {
                string name = v.Get("name").Text(null);
                if (!ValidName(name)) continue;
                int nw = (int)v.Get("nights_won").Num(0);
                if (nw < 0 || nw > 3) nw = 0;
                lb._entries.Add(new Entry
                {
                    Name = name,
                    Score = (long)v.Get("score").Num(0),
                    NightsWon = nw,
                    AchievedAtIso = v.Get("achieved_at").Text(""),
                    Seed = (long)v.Get("seed").Num(0),
                    AchievedSeq = lb._entries.Count       // file order is achievement order
                });
            }
            lb.Sort();
            while (lb._entries.Count > MaxEntries) lb._entries.RemoveAt(lb._entries.Count - 1);
            return lb;
        }
    }
}
