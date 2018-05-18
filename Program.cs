using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

/// <summary>
/// Application to filter Twitter achive into a slimmer CSV with crud filtered out
/// Licenced under MIT license (see EOF comment)
/// Written by O. Westin http://microsff.com https://twitter.com/MicroSFF
/// </summary>
namespace FilterTwitterCsv
{
    class Program
    {
         /// <summary>
        /// Usage instructions
        /// </summary>
        static readonly string usage = "Usage\nFilterTwitterCsv [archive zip file] -o:<destination file> -d:<lower date limit>";

        /// <summary>
        /// Command-line parameter parsing
        /// </summary>
        class Params
        {
            public string infile { get; }
            public string outfile { get; }
            public DateTime limit { get; }
            public bool valid { get; }

            public Params(string[] args)
            {
                infile = null;
                outfile = null;
                valid = false;

                if ((args.Length >= 1) || (args.Length <= 3))
                {
                    // Must be zip file
                    if (".zip" != System.IO.Path.GetExtension(args[0]).ToLower())
                        return;

                    int c = 1;
                    infile = args[0];
                    for (int i = 1; i < args.Length; ++i)
                    {
                        if (args[i].IndexOf("-o:") == 0)
                        {
                            outfile = args[i].Substring(3, args[i].Length - 3);
                            c++;
                        }
                        if (args[i].IndexOf("-d:") == 0)
                        {
                            limit = DateTime.Parse(args[i].Substring(3, args[i].Length - 3));
                            c++;
                        }
                    }
                    if (String.IsNullOrEmpty(outfile))
                    {
                        outfile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(args[0]), System.IO.Path.GetFileNameWithoutExtension(args[0]) + "_filtered.csv");
                    }
                    valid = c == args.Length;
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Params arguments = new Params(args);
                if (!arguments.valid)
                {
                    Console.WriteLine(usage);
                    return;
                }
                string correctedFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(arguments.outfile), System.IO.Path.GetFileNameWithoutExtension(arguments.outfile) + "_corrected.csv");
                List<Tweet> result = TwitterCsvFilterer.ReadCsv(arguments.infile, arguments.limit, correctedFile);
                TwitterCsvFilterer.WriteCsv(result, arguments.outfile);
                Console.WriteLine("Remaining tweets written to " + arguments.outfile);
            }
            catch (Exception e)
            {
                StringBuilder error = new StringBuilder();
                error.AppendLine("Error: ");
                Exception ex = e;
                while (e != null)
                {
                    error.AppendLine(e.Message);
                    e = e.InnerException;
                }
                Console.WriteLine(error);
            }
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
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
