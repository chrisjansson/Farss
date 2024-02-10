module Farss.Server.CachedHttpClient

open System
open System.Net
open System.Net.Http

open System.Threading.Tasks
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
    | Ok of
        {|
            Content: string
            ETag: string option
            LastModified: DateTimeOffset option
        |}
    | NotModified
    | Error

//Doesnt follow redirects, no timeouts nor cancellation tokens, error handling
let get (url: string, etag: string option, lastModified: DateTimeOffset option) =
    task {
        printfn $"Url request to %A{url}"
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

            return
                Ok {|
                    Content = content
                    ETag = etag
                    LastModified = lastModified
                |}
        | HttpStatusCode.NotModified -> return NotModified
        | _ -> return Error
    }

let getCacheHeadersImpl (repository: HttpCacheRepository) (url: string) : CacheHeaders option = repository.getCacheHeaders url

let cacheResponseImpl (repository: HttpCacheRepository) (url: string) (content: string) (etag: string option) (lastModified: DateTimeOffset option) =
    repository.save url content etag lastModified

let getCached
    (getCacheEntry: string -> CacheHeaders option)
    (cacheResponse: string -> string -> string option -> DateTimeOffset option -> unit)
    (getCacheEntryContent: Guid -> string)
    (url: string)
    : Task<string> =
    task {
        let cacheHeaders = getCacheEntry url

        match cacheHeaders with
        | Some ch ->
            let pollThrottleWindowExpired =
                DateTimeOffset.UtcNow - ch.LastGet < TimeSpan.FromMinutes 20

            if pollThrottleWindowExpired then
                return getCacheEntryContent ch.Id
            else
                let etag = ch.ETag
                let lastModifiedDate = ch.LastModified

                let! response = get (url, etag, lastModifiedDate)

                match response with
                | Ok r ->
                    cacheResponse url r.Content r.ETag r.LastModified
                    return r.Content
                | NotModified -> return getCacheEntryContent ch.Id
                | Error ->
                    failwith "Error"
                    return ""
        | None ->
            let! response = get (url, None, None)

            match response with
            | Ok r ->
                cacheResponse url r.Content r.ETag r.LastModified
                return r.Content
            | _ ->
                failwith "Error"
                return ""
    }
