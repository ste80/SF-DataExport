﻿using PuppeteerSharp;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SF_DataExport.Interceptor
{
    public class IconInterceptor : InterceptorBase
    {
        ResourceManager Resource { get; }
        AppStateManager AppState { get; }

        public IconInterceptor(ResourceManager resource, AppStateManager appState)
        {
            Resource = resource;
            AppState = appState;
        }

        public override async Task<bool> RequestAsync(Page appPage, Request request)
        {
            if (Resource.IsLoginUrl(request.Url) && request.Url.Contains(".salesforce.com/assets/icons/"))
            {
                var iconPath = "icons/" + request.Url.Split(".salesforce.com/assets/icons/", 2).Last().Split('?').First();
                var icon = Resource.GetResourceBytes(iconPath);
                if (icon?.LongLength > 0)
                    await AppState.InterceptAsync(appPage, request, req => req.RespondAsync(new ResponseData
                    {
                        Status = HttpStatusCode.OK,
                        ContentType = Resource.GetContentType(iconPath),
                        BodyData = icon
                    })).GoOn();
                else
                    await AppState.InterceptAsync(appPage, request, req => req.ContinueAsync()).GoOn();

                return true;
            }
            return false;
        }
    }
}
