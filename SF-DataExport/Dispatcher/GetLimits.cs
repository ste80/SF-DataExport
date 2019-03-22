﻿using CsvHelper;
using CsvHelper.Configuration;
using DotNetForce;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF_DataExport.Dispatcher
{
    public class GetLimits : IDispatcher
    {
        AppStateManager AppState { get; }
        ResourceManager Resource { get; }
        JsonConfig OrgSettings { get; }

        public GetLimits(AppStateManager appState, ResourceManager resource, OrgSettingsConfig orgSettings)
        {
            AppState = appState;
            Resource = resource;
            OrgSettings = orgSettings;
        }

        public async Task<JToken> DispatchAsync(JToken payload)
        {
            var instanceUrl = (string)payload?["instanceUrl"] ?? "";
            var showLimitsModal = !string.IsNullOrEmpty(instanceUrl);
            AppState.Commit(new JObject { ["showLimitsModal"] = showLimitsModal });
            if (showLimitsModal)
            {
                var orgName = Resource.OrgName(instanceUrl);

                if (!string.IsNullOrEmpty(orgName))
                {
                    var accessToken = (string)OrgSettings.Get(o => o[instanceUrl]?[OAuth.ACCESS_TOKEN]);
                    var client = new DNFClient(instanceUrl, accessToken);

                    var request = new BatchRequest();
                    request.Limits();
                    request.Query("SELECT Name,UsedLicenses,TotalLicenses FROM UserLicense WHERE Status = 'Active' AND TotalLicenses > 0 ORDER BY Name");
                    var result = await client.Composite.BatchAsync(request);

                    var orgLimits = new JArray();

                    var limits = (JObject)result.Results("0") ?? new JObject();
                    var limitsKeys = limits.Properties().Select(p => p.Name).ToList();
                    foreach (var limitsKey in limitsKeys)
                    {
                        var limit = (JObject)limits[limitsKey];
                        orgLimits.Add(new JObject
                        {
                            ["Name"] = limitsKey,
                            ["Remaining"] = limit["Remaining"],
                            ["Max"] = limit["Max"],
                        });
                    }
                    foreach (var userLicense in client.GetEnumerable(result.Queries("1")))
                    {
                        orgLimits.Add(new JObject
                        {
                            ["Name"] = userLicense["Name"],
                            ["Remaining"] = (double)userLicense["TotalLicenses"] - (double)userLicense["UsedLicenses"],
                            ["Max"] = userLicense["TotalLicenses"],
                        });
                    }

                    var orgLimitsTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                    AppState.Commit(new JObject
                    {
                        ["orgLimits"] = orgLimits,
                    });

                    var header = string.Join('\t', "Name", "Remaining", "Max", "Time");
                    var exists = false;
                    var file = new FileInfo(Path.Combine(Resource.DefaultDirectory, orgName + ".limits.csv"));
                    if (file.Exists)
                    {
                        using (var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (var streamReader = new StreamReader(fileStream, true))
                            {
                                if (await streamReader.ReadLineAsync().GoOn() == header)
                                {
                                    exists = true;
                                }
                            }
                        }
                        if (!exists)
                        {
                            file.Delete();
                        }
                    }
                    var csvConfig = new Configuration { Delimiter = "\t", Encoding = Encoding.Unicode };
                    using (var fileStream = file.Open(exists ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        using (var streamWriter = new StreamWriter(fileStream, csvConfig.Encoding))
                        {
                            using (var writer = new CsvWriter(streamWriter, csvConfig))
                            {
                                if (!exists)
                                {
                                    writer.WriteField("Name");
                                    writer.WriteField("Remaining");
                                    writer.WriteField("Max");
                                    writer.WriteField("Time");
                                    await writer.NextRecordAsync().GoOn();
                                    await writer.FlushAsync().GoOn();
                                }

                                for (var i = 0; i < orgLimits.Count; i++)
                                {
                                    var orgLimit = orgLimits[i];
                                    writer.WriteField((string)orgLimit["Name"]);
                                    writer.WriteField((double)orgLimit["Remaining"]);
                                    writer.WriteField((double)orgLimit["Max"]);
                                    writer.WriteField(orgLimitsTime);
                                    await writer.NextRecordAsync().GoOn();
                                    await writer.FlushAsync().GoOn();
                                }
                            }
                        }
                    }

                    AppState.Commit(new JObject
                    {
                        ["orgLimitsLog"] = await Resource.GetOrgLimitsLogAsync(orgName)
                    });
                }
            }
            return null;
        }
    }
}