module Farss.Server.CachedHttpClient

open System
open System.Net
open System.Net.Http
open Domain
open Persistence

//
// public static async Task<byte[]> DownloadBytesAsync(
//   string url,
//   CancellationToken cancellationToken,
//   bool autoRedirect = true,
//   string userAgent = "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0")
// {
//   url = WebUtility.UrlDecode(url);
//   HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
//   HttpResponseMessage httpResponseMessage;
//   try
//   {
//     request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
//     request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
//     httpResponseMessage = await Helpers._httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
//   }
//   finally
//   {
//     request?.Dispose();
//   }
//   request = (HttpRequestMessage) null;
//   if (!httpResponseMessage.IsSuccessStatusCode)
//   {
//     int statusCode = (int) httpResponseMessage.StatusCode;
//     if (autoRedirect && (statusCode == 301 || statusCode == 302 || statusCode == 308))
//       url = httpResponseMessage.Headers?.Location?.AbsoluteUri ?? url;
//     request = new HttpRequestMessage(HttpMethod.Get, url);
//     try
//     {
//       httpResponseMessage = await Helpers._httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
//     }
//     finally
//     {
//       request?.Dispose();
//     }
//     request = (HttpRequestMessage) null;
//   }
//   return await httpResponseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
// }

type Response =
    | Ok of {| Content: string; ETag: string option; LastModified: DateTimeOffset option |}
    | NotModified
    | Error

//Doesnt follow redirects, no timeouts nor cancellation tokens, error handling
let get (url: string, etag: string option, lastModified: DateTimeOffset option) =
    task {
        use httpClient = new HttpClient()

        let request =
            let userAgent = "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0"
            let url = WebUtility.UrlDecode(url)

            let request = new HttpRequestMessage(HttpMethod.Get, url)

            match etag with
            | Some etag -> request.Headers.IfNoneMatch.ParseAdd(etag)
            | None -> ()

            match lastModified with
            | Some lastModified -> request.Headers.Add("If-Modified-Since", lastModified.ToString("R"))
            | None -> ()

            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
            request.Headers.Add("User-Agent", userAgent)

            request

        let! response = httpClient.SendAsync(request)

        match response.StatusCode with
        | HttpStatusCode.OK ->
            let etag = response.Headers.ETag.Tag |> Option.ofObj
            let lastModified = response.Content.Headers.LastModified |> Option.ofNullable
            let! content = response.Content.ReadAsStringAsync()
            return Ok {| Content = content; ETag = etag; LastModified = lastModified |}
        | HttpStatusCode.NotModified ->
            return NotModified
        | _ ->
            return Error
    }

let getCacheHeadersImpl (repository: HttpCacheRepository) (url: string): Domain.CacheHeaders option =
    repository.getCacheHeaders url

let getCached (getCacheHeaders: string -> Domain.CacheHeaders option) (url: string) =
    task {
        let cacheHeaders = getCacheHeaders url

        let etag =
            cacheHeaders
            |> Option.bind (_.ETag)
        let lastModifiedDate =
            cacheHeaders
            |> Option.bind (_.LastModified)
        
        let! response = get (url, etag, lastModifiedDate)
     
        //Store if cache miss
        //Retrieve if cache hit
           
        return ()
    }
