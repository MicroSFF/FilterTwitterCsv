using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Application to filter Twitter achive into a slimmer CSV with crud filtered out
/// Licenced under MIT license (see EOF comment)
/// Written by O. Westin http://microsff.com https://twitter.com/MicroSFF
/// </summary>
namespace FilterTwitterCsv
{
    /// <summary>
    /// Class representing a single tweet
    /// </summary>
    public class Tweet
    {
        private static readonly string headers = "\"id\",\"replyToId\",\"timestamp\",\"text\",\"replies\"";
        private string id;
        private string replyToId;
        private DateTime timestamp;
        private string text;
        private List<string> replies = null;

        /// <summary>
        /// CVS header
        /// </summary>
        public static string Headers
        {
            get { return headers; }
        }

        /// <summary>
        /// Column indices to CSV
        /// </summary>
        public enum Fields : int
        {
            id,
            replyToId,
            timestamp,
            text,
            replies,
        }

        /// <summary>
        /// Constructor. Escapes reserved characters in text.
        /// </summary>
        /// <param name="id">id string</param>
        /// <param name="replyToId">id of tweet this replies to</param>
        /// <param name="timeStamp">timestamp of tweet</param>
        /// <param name="text">text of tweet</param>
        public Tweet(string id, string replyToId, string timeStamp, string text)
        {
            this.id = id;
            this.replyToId = replyToId;
            text = text.Replace("&", "&amp;");
            text = text.Replace("\"", "&quot;");
            text = text.Replace("'", "&apos;");
            text = text.Replace("\r\n", "<br>");
            this.text = text.Replace("\n", "<br>");
            this.timestamp = DateTime.Parse(timeStamp);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">id string</param>
        /// <param name="replyToId">id of tweet this replies to</param>
        /// <param name="timeStamp">timestamp of tweet</param>
        /// <param name="text">text of tweet</param>
        /// <param name="replies">comma-separated list of ids of tweets replying to this</param>
        public Tweet(string id, string replyToId, string timeStamp, string text, string replies)
        {
            this.id = id;
            this.replyToId = replyToId;
            this.text = text;
            this.timestamp = DateTime.Parse(timeStamp);
            if (!String.IsNullOrWhiteSpace(replies))
            {
                this.replies = new List<string>();
                this.replies.AddRange(replies.Split(','));
            }
        }

        /// <summary>
        /// True if tweet is a reply
        /// </summary>
        public bool IsReply
        {
            get { return !string.IsNullOrEmpty(this.replyToId); }
        }

        /// <summary>
        /// Tweet id
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Tweet text, HTMLised
        /// </summary>
        public string Text
        {
            get { return this.text; }
        }

        /// <summary>
        /// Id of tweet this replies to, if any
        /// </summary>
        public string ReplyToId
        {
            get { return this.replyToId; }
            set { this.replyToId = value; }
        }

        /// <summary>
        /// Timestamp of tweet
        /// </summary>
        public string Time
        {
            get { return this.timestamp.ToString(); }
        }

        /// <summary>
        /// Timestamp of tweet
        /// </summary>
        public DateTime TimeStamp
        {
            get { return this.timestamp; }
        }

        /// <summary>
        /// True if this tweet has replies
        /// </summary>
        public bool HasReply
        {
            get { return (this.replies != null) && (this.replies.Count > 0); }
        }

        /// <summary>
        /// Add reply to tweet, inserting it first in the list
        /// (Since they are read from last to first)
        /// </summary>
        /// <param name="id">Id of tweet replying to this</param>
        public void AddReply(string id)
        {
            if (!HasReply)
                this.replies = new List<string>();
            this.replies.Insert(0, id);
        }

        /// <summary>
        /// Return the replies to this tweet
        /// </summary>
        public List<string> Replies
        {
            get { return this.replies; }
        }

        /// <summary>
        /// Render as a line of comma-separated text
        /// </summary>
        /// <returns>Contents of tweet</returns>
        public override string ToString()
        {
            string result = String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",", id, replyToId, timestamp.ToString(), text);
            if (HasReply)
            {
                StringBuilder replyList = new StringBuilder();
                foreach (string r in this.replies)
                {
                    replyList.AppendFormat("{0},", r);
                }
                // Remove last comma
                if (replyList.Length > 0)
                    replyList.Remove(replyList.Length - 1, 1);
                return String.Format("{0}\"{1}\"", result, replyList.ToString());
            }
            else
            {
                return String.Format("{0}\"\"",result);
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
