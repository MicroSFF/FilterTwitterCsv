# FilterTwitterCsv
Application to filter Twitter achive into a slimmer CSV with crud filtered out

Written by O. Westin http://microsff.com https://twitter.com/MicroSFF

Usage:
	FilterTwitterCsv [archive zip file] -o:<destination file> -d:<date limit>

	If no output file is given in -o, "_filtered.csv" is appended to the 
	archive zip file name.
	
	If no date limit is given, all tweets in the archive are processed. 
	If a date is given, no tweets earlier than this date are processed. 
	Format of date must be parseable into a DateTime.
	
	If any corrected tweets are filtered out, they are written to a file in the
	same location as the output file, with "_corrected.csv" appended to the 
	output file name.

References:
        https://github.com/phatcher/CsvReader
	
Files:
	LevenshteinDistance.cs
		- contains an implementation of the Levenshtein distance algorithm to 
		calculate difference between strings. Copied from StackOverflow.

	Program.cs
		- contains the main application class. Checks arguments, displays 
		  errors, and writes output file.

	Tweet.cs
		- contains a class representing a single tweet. Contains functions and
		  properties not used in this application, but which are used in 
		  another application written to read the output file from this and 
		  process tweets further.

	TwitterCsvFilterer.cs
		- contains a class doing all filtering and processing. 
		- The internal classes derived from the IFilter interface do most of the 
		  simple filtering; change these to suit your own preferences.
		- Uses static string ownTwitterId which holds the Twitter user id of the 
		  user whose archive is processed. Change to your own.

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
