#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#load "common.fsx"

module Youtube = 
  open Common
  open System
  open System.Net
  open Suave.Successful
  open FSharp.Data

  let [<Literal>] videosSample = """
    { "videos" : [
      { "title":"talk title", 
        "date":"2011-01-01T17:00:00",
        "thumbnail" : "url string",
        "description" : "some video about lots of stuff",
        "link" : "http://youtube.some.video"
      }
    ]}
  """

  type VideoData = JsonProvider<videosSample, RootName="Root">

  let [<Literal>] youTubeSample = """
    {
      "items": [
        { "kind": "some kind",
          "id": {"videoId": "vvkkppll", "kind": "youtube#video"},
          "snippet":
            { "publishedAt": "2016-02-24T22:39:21.000Z",
              "channelId": "UC6OzdI6-htXE_97zamJRaaA",
              "title": "Stealing Time with the .NET ThreadPool",
              "description": "Scalable applications something",
              "thumbnails":
                { "default": { "url": "https://dV_4/default.jpg", "width": 120, "height": 90 },
                  "medium" : { "url": "https://mqdefault.jpg", "width": 320, "height": 180 },
                  "high"   : { "url": "https://qdefault.jpg", "width": 480, "height": 360 }
                },
              "channelTitle": "",
              "liveBroadcastContent": "none"
            }
        }]}
    """

  type YouTubeData = JsonProvider<youTubeSample>

  let getVideos x = 
    let query () =
      let api_token = "YOUTUBE_API_TOKEN" |> Env.tryGetVar |> Option.get
      let channel = "UC6OzdI6-htXE_97zamJRaaA"
      let url = sprintf "https://www.googleapis.com/youtube/v3/search?key=%s&channelId=%s&part=snippet,id&order=date&maxResults=10" api_token channel 
      YouTubeData.Load(url)

    let createVideo (i : YouTubeData.Item) = 
      VideoData.Video(
        title=i.Snippet.Title,
        date=i.Snippet.PublishedAt,
        thumbnail=i.Snippet.Thumbnails.Medium.Url,
        description=i.Snippet.Description,
        link="https://www.youtube.com/watch?v=" + i.Id.VideoId
      )

    async {
      let onlyVideos (v: YouTubeData.Item) = v.Id.Kind = "youtube#video"
      let ytResult = query()
      let videos = ytResult.Items |> Array.filter onlyVideos |> Array.map createVideo
      return! OK (VideoData.Root(videos = videos).JsonValue.ToString()) x
    }

