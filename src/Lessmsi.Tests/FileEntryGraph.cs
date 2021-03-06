﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace LessMsi.Tests
{
    /// <summary>
    /// Used in testing to represent the graph/directory tree of files extracted from an MSI.
    /// </summary>
    public class FileEntryGraph
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FileEntryGraph"/>.
        /// </summary>
        /// <param name="forFileName">The initial value for <see cref="FileEntryGraph.ForFileName"/></param>
        public FileEntryGraph(string forFileName)
        {
            this.ForFileName = forFileName;
        }

        /// <summary>
        /// The file name that this graph is for.
        /// </summary>
        public string ForFileName { get; private set; }
        
        /// <summary>
        /// The entries of the file graph.
        /// </summary>
        public List<FileEntry> Entries
        { 
            get { return _entries; }
            set { _entries = value; } 
        } private  List<FileEntry> _entries = new List<FileEntry>();

        public void Add(FileEntry entry)
        {
            Entries.Add(entry);
        }

        /// <summary>
        /// Saves this <see cref="FileEntryGraph"/> to the specified file.
        /// </summary>
        /// <param name="file">The file that this graph will be saved to.</param>
        public void Save(FileInfo file)
        {
            //save it as csv:
            if (file.Exists)
                file.Delete();
            using (var f = file.CreateText())
            {
                f.WriteLine("Path,Size");

                foreach (var e in this.Entries)
                {
                    f.Write(e.Path);
                    f.Write(",");
                    f.Write(e.Size);
                    f.WriteLine();
                }
            }
        }

        /// <summary>
        /// Loads a <see cref="FileEntryGraph"/> from the specified file.
        /// </summary>
        /// <param name="file">The file to load a new <see cref="FileEntryGraph"/> from.</param>
        /// <param name="forFileName">The initial value for the returned objects <see cref="FileEntryGraph.ForFileName"/></param>
        /// <returns>The newly loaded <see cref="FileEntryGraph"/>.</returns>
        public static FileEntryGraph Load(FileInfo file, string forFileName)
        {
            var graph = new FileEntryGraph(forFileName);
            using (var f = file.OpenText())
            {
                f.ReadLine();//headings
                while (!f.EndOfStream)
                {
                    var line = f.ReadLine().Split(',');
                    if (line.Length != 2)
                        throw new IOException("Expected two fields!");
                    graph.Add(new FileEntry(line[0], Int64.Parse(line[1])));
                }
            }
            return graph;
        }

        public static bool CompareEntries(FileEntryGraph a, FileEntryGraph b, out string errorMessage)
        {
            if (a.Entries.Count != b.Entries.Count)
            {
                errorMessage = string.Format("Entries for '{0}' and '{1}' have a different number of file entries ({2}, {3} respectively).", a.ForFileName, b.ForFileName, a.Entries.Count, b.Entries.Count);
                return false;
            }

            for (int i = 0; i < Math.Max(a.Entries.Count, b.Entries.Count); i++)
            {
                if (!a.Entries[i].Equals(b.Entries[i]))
                {
                    errorMessage = string.Format("'{0}'!='{1}' at index '{2}'.", a.Entries[i].Path, b.Entries[i].Path, i);
                    return false;
                }
            }
            errorMessage = "";
            return true;
        }
    }
}