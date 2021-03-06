﻿using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SF_DataExport.Reducers
{
    public class FetchDirPath : IDispatcher
    {
        public Task<JToken> DispatchAsync(JToken payload)
        {
            var search = TrimDir((string)payload?["search"]);

            if (search != "")
            {
                try
                {
                    var searchDir = new DirectoryInfo(search);
                    if (searchDir.Exists)
                    {
                        return Task.FromResult<JToken>(new JArray(new[] { TrimDir(searchDir.FullName), TrimDir(searchDir.Parent?.FullName) }
                            .Concat(searchDir.Parent?.GetDirectories().Where(d => MatchDir(d, searchDir.Name)).Select(d => TrimDir(d.FullName)) ?? new string[0])
                            .Concat(searchDir.GetDirectories().Select(d => TrimDir(d.FullName)))
                                .Concat(Directory.GetLogicalDrives().Select(d => TrimDir(d)))
                            .Where(s => s != "").Distinct()));
                    }
                    else
                    {
                        var fi = new FileInfo(search);
                        if (fi.Directory?.Exists == true)
                        {
                            return Task.FromResult<JToken>(new JArray(new[] { TrimDir(fi.Directory.FullName) }
                                .Concat(fi.Directory.GetDirectories().Where(d => MatchDir(d, fi.Name)).Select(d => TrimDir(d.FullName)))
                                .Concat(Directory.GetLogicalDrives().Select(d => TrimDir(d)))
                                .Where(s => s != "").Distinct()));
                        }
                    }
                }
                catch { }
            }
            return Task.FromResult<JToken>(new JArray(Directory.GetLogicalDrives().Select(d => TrimDir(d))));

            string TrimDir(string dir)
            {
                return dir?.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? "";
            }

            bool MatchDir(DirectoryInfo di, string filterText)
            {
                return !string.IsNullOrEmpty(filterText) && di.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }
}