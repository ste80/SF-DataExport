﻿using PuppeteerSharp;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SF_DataExport.Interceptor
{
    public class ImageInterceptor : InterceptorBase
    {
        ResourceManager Resource { get; }
        AppStateManager AppState { get; }

        public ImageInterceptor(ResourceManager resource, AppStateManager appState)
        {
            Resource = resource;
            AppState = appState;
        }

        public override async Task<bool> RequestAsync(Page appPage, Request request)
        {
            if (Resource.IsLoginUrl(request.Url) && request.Url.Contains(".salesforce.com/assets/images/"))
            {
                var imgPath = "images/" + request.Url.Split(".salesforce.com/assets/images/", 2).Last().Split('?').First();
                var img = Resource.GetResourceBytes(imgPath);
                if (img != null)
                    await AppState.IntercepObservable(appPage, request, () => request.RespondAsync(new ResponseData
                    {
                        Status = HttpStatusCode.OK,
                        ContentType = Resource.GetContentType(imgPath),
                        BodyData = img
                    }));
                else
                    await AppState.IntercepObservable(appPage, request, () => request.ContinueAsync());
                return true;
            }
            return false;
        }
    }
}