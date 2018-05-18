using System;
using System.Collections.Generic;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.IO.Compression;

/// <summary>
/// Application to filter Twitter achive into a slimmer CSV with crud filtered out
/// Licenced under MIT license (see EOF comment)
/// Written by O. Westin http://microsff.com https://twitter.com/MicroSFF
/// </summary>
namespace FilterTwitterCsv
{
    /// <summary>
    /// Class to read the tweets.csv file from a Twitter archive zip file, and filter out:
    ///     - replies to others, 
    ///     - retweets, 
    ///     - minor corrected errors (e.g. previous or replied-to-self tweet has a Levenshtein distance less than or equal to levenshteinDistanceLimit)
    ///     - MicroSFF announcements (e.g. tweets starting with "** ")
    ///     - Some unwanted MicroSFF hashtags (#AdvenTale and #SummerRerun)
    /// </summary>
    public static class TwitterCsvFilterer
    {
        /// <summary>
        /// The Levenshtein distance limit used to identify corrections
        /// </summary>
        static readonly int levenshteinDistanceLimit = 10;

        /// <summary>
        /// The user id, used to identify replies to self
        /// </summary>
        static readonly String ownTwitterId = "1376608884"; // TODO - change to your own - this is MicroSFF

        /// <summary>
        /// The expected headers
        /// </summary>
        static readonly string[] fileHeaders = {"tweet_id","in_reply_to_status_id","in_reply_to_user_id","timestamp","source","text","retweeted_status_id","retweeted_status_user_id","retweeted_status_timestamp","expanded_urls"};

        /// <summary>
        /// Indices to the fields in the Twitter CSV 
        /// </summary>
        enum Fields : int
        {
            tweet_id,
            in_reply_to_status_id,
            in_reply_to_user_id,
            timestamp,
            source,
            text,
            retweeted_status_id,
            retweeted_status_user_id,
            retweeted_status_timestamp,
            expanded_urls
        }

        /// <summary>
        /// Interface of filters to apply to the tweets
        /// </summary>
        interface IFilter
        {
            /// <summary>
            /// Check CSV line to determine whether to filter it out
            /// </summary>
            /// <param name="csv">line to check</param>
            /// <returns>true if line should be filtered out</returns>
            bool Exclude(CsvReader csv);

            /// <summary>
            /// Returns number of filtered-out lines
            /// </summary>
            uint Count
            { get; }
        }
        
        /// <summary>
        /// Filter out replies to others
        /// </summary>
        class ReplyFilter : IFilter
        {
            private uint count = 0;

            public bool Exclude(CsvReader csv)
            {
                if (!string.IsNullOrEmpty(csv[(int)Fields.in_reply_to_user_id]) && csv[(int)Fields.in_reply_to_user_id] != ownTwitterId)
                {
                    count++;
                    return true;
                }
                if (csv[(int)Fields.text].IndexOf("@") == 0)
                {
                    count++;
                    return true;
                }
                return false;
            }
            public uint Count
            { get { return count; } }

            public override string ToString()
            {
                return String.Format("Replies: {0}", count);
            }
        }

        /// <summary>
        /// Filter out retweets
        /// </summary>
        class RetweetFilter : IFilter
        {
            private uint count = 0;

            public bool Exclude(CsvReader csv)
            {
                if ((csv[(int)Fields.retweeted_status_id] != "") || (csv[(int)Fields.text].IndexOf("RT ") == 0) || (csv[(int)Fields.text].IndexOf("MT ") == 0))
                {
                    count++;
                    return true;
                }
                return false;
            }
            public uint Count
            { get { return count; } }

            public override string ToString()
            {
                return String.Format("Retweets: {0}", count);
            }
        }

        /// <summary>
        /// Filter out announcements (tweets starting with ** )
        /// </summary>
        class AnnounceFilter : IFilter
        {
            private uint count = 0;

            public bool Exclude(CsvReader csv)
            {
                if ((csv[(int)Fields.text].IndexOf("** ") == 0))
                {
                    count++;
                    return true;
                }
                return false;
            }
            public uint Count
            { get { return count; } }

            public override string ToString()
            {
                return String.Format("Announcements: {0}", count);
            }
        }

        /// <summary>
        /// Filter out unwanted tags
        /// </summary>
        class TagFilter : IFilter
        {
            private uint count = 0;
            private string[] tags;

            /// <summary>
            /// Constructor takes array of strings to filter out
            /// </summary>
            /// <param name="tags">strings to filter out</param>
            public TagFilter(string[] tags)
            {
                this.tags = tags;
            }

            public bool Exclude(CsvReader csv)
            {
                foreach (string tag in tags)
                {
                    if (csv[(int)Fields.text].IndexOf(tag) != -1)
                    {
                        count++;
                        return true;
                    }
                }
                return false;
            }
            public uint Count
            { get { return count; } }

            public override string ToString()
            {
                return String.Format("Unwanted tags: {0}", count);
            }
        }

        /// <summary>
        /// Reads CSV file from Twitter, filtering out announcements, replies and duplicates
        /// </summary>
        /// <param name="input">Pathname of CSV file to read</param>
        /// <param name="limit">Date at which to break</param>
        /// <param name="filteredCorrectionsFile">Pathname of file in which to store filtered-out corrections</param>
        /// <returns>Filtered tweets</returns>
        public static List<Tweet> ReadCsv(string input, DateTime limit, string filteredCorrectionsFile)
        {
            using (ZipArchive archive = ZipFile.OpenRead(input))
            {
                // Extract the tweets, which is a CSV file with header
                ZipArchiveEntry tweetEntry = archive.GetEntry("tweets.csv");
                
                using (CsvReader csv = new CsvReader(new StreamReader(tweetEntry.Open()), true))
                {
                    // Make sure it has the correct content (at least up to retweeted_status_id)
                    string[] headers = csv.GetFieldHeaders();
                    for (int h = 0; h <= (int)Fields.retweeted_status_id; ++h)
                    {
                        if (h >= headers.Length)
                            throw new ArgumentException("Too few header fields");
                        if (headers[h] != fileHeaders[h])
                            throw new ArgumentException(String.Format("Mismatching header fields in column {0}. Expected \"{1}\", found \"{2}\"", h + 1, fileHeaders[h], headers[h]));
                    }

                    string prevId = null;
                    Dictionary<string, Tweet> result = new Dictionary<string, Tweet>();

                    // Set up filters
                    string[] tags = { "#AdvenTale", "#SummerRerun" };
                    List<IFilter> filters = new List<IFilter>();
                    filters.Add(new RetweetFilter());
                    filters.Add(new AnnounceFilter());
                    filters.Add(new ReplyFilter());
                    filters.Add(new TagFilter(tags));
                    int corrections = 0;
                    int tweets = 0;

                    List<Tweet> filteredCorrections = new List<Tweet>();

                    while (csv.ReadNextRecord())
                    {
                        tweets++;

                        string currId = csv[(int)Fields.tweet_id];
                        string text = (string)csv[(int)Fields.text];
                        Tweet current = new Tweet(currId, csv[(int)Fields.in_reply_to_status_id], csv[(int)Fields.timestamp], text);

                        // Check date
                        if (current.TimeStamp < limit)
                            break;

                        // Apply filters
                        bool exclude = false;
                        foreach (IFilter filter in filters)
                        {
                            if (exclude = filter.Exclude(csv))
                                break;
                        }
                        if (exclude)
                            continue;

                        // Assume we'll add it, unless it is a minor correction to the previous tweet
                        bool add = true;

                        if (!string.IsNullOrEmpty(prevId) && result.ContainsKey(prevId))
                        {
                            // Compare to previous
                            int distance = LevenshteinDistance.Compute(current.Text, result[prevId].Text);
                            if (distance <= levenshteinDistanceLimit)
                            {
                                add = false;
                                corrections++;
                                result[prevId].ReplyToId = "";
                                filteredCorrections.Add(current);
                                filteredCorrections.Add(result[prevId]);
                            }
                        }
                        if (add)
                        {
                            prevId = currId;
                            result[prevId] = current;
                        }

                    }
                    // Flag replies
                    List<string> toRemove = new List<string>();
                    foreach (var t in result)
                    {
                        if (t.Value.IsReply && result.ContainsKey(t.Value.ReplyToId))
                        {
                            int distance = LevenshteinDistance.Compute(t.Value.Text, result[t.Value.ReplyToId].Text);
                            if (distance <= levenshteinDistanceLimit)
                            {
                                toRemove.Add(t.Value.ReplyToId);
                                t.Value.ReplyToId = "";
                                filteredCorrections.Add(t.Value);
                                filteredCorrections.Add(result[t.Value.ReplyToId]);
                                corrections++;
                            }
                            else
                            {
                                result[t.Value.ReplyToId].AddReply(t.Key);
                            }
                        }
                    }
                    foreach (string i in toRemove)
                    {
                        result.Remove(i);
                    }
                    Console.WriteLine(String.Format("Tweets: {0}", tweets));
                    foreach (IFilter filter in filters)
                    {
                        Console.WriteLine(filter.ToString());
                    }
                    Console.WriteLine(String.Format("Corrections: {0}", corrections));
                    Console.WriteLine(String.Format("Remaining stories: {0}", result.Count));

                    // Take a backup of the filtered corrections
                    if (!String.IsNullOrWhiteSpace(filteredCorrectionsFile))
                    {
                        WriteCsv(filteredCorrections, filteredCorrectionsFile);
                        Console.WriteLine("Corrected tweets written to " + filteredCorrectionsFile);
                    }
                    return new List<Tweet>(result.Values);
                }
            }
        }

        /// <summary>
        /// Write tweets to CSV 
        /// </summary>
        /// <param name="tweets">content to write</param>
        /// <param name="filename">pathname of file to write to</param>
        static public void WriteCsv(List<Tweet> tweets, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine(Tweet.Headers);
                foreach (var t in tweets)
                {
                    sw.WriteLine(t.ToString());
                }
            }
        }
    }
}

/*
Copyright 2018 O. Westin 

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in 
the Software without restriction, including without limitation the rights to 
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE.
*/
