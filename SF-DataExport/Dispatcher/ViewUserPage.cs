﻿using DotNetForce;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Unit = System.Reactive.Unit;

namespace SF_DataExport.Dispatcher
{
    public class ViewUserPage : IDispatcher
    {
        ResourceManager Resource { get; }
        AppSettingsConfig AppSettings { get; }
        OrgSettingsConfig OrgSettings { get; }

        public ViewUserPage(ResourceManager resource, AppSettingsConfig appSettings, OrgSettingsConfig orgSettings)
        {
            Resource = resource;
            AppSettings = appSettings;
            OrgSettings = orgSettings;
        }

        public Task<JToken> DispatchAsync(JToken payload)
        {
            var instanceUrl = (string)payload["instanceUrl"] ?? "";
            var userId = (string)payload["userId"] ?? "";
            var accessToken = (string)OrgSettings.Get(o => o[instanceUrl]?[OAuth.ACCESS_TOKEN]) ?? "";
            var targetUrl = instanceUrl + "/" + userId + "?noredirect=1";
            var urlWithAccessCode = Resource.GetUrlViaAccessToken(instanceUrl, accessToken, targetUrl);
            Resource.OpenIncognitoBrowser(urlWithAccessCode, AppSettings.GetString(AppConstants.PATH_CHROME));
            return Task.FromResult<JToken>(null);
        }
    }
}