#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/FSharpX.Extras/lib/40/FSharpx.Extras.dll"
#r "packages/FSharpX.Collections/lib/net40/FSharpx.Collections.dll"
#r "packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll"
#r "System.Net.Http.dll"

#load "common.fsx"

module Meetup =
  open Common
  open System
  open System.Net
  open Suave.Successful
  open FSharp.Data
  open System.Net.Http
  open Newtonsoft.Json

  let httpClient = new HttpClient() 
  System.Net.ServicePointManager.SecurityProtocol <- SecurityProtocolType.Tls12

  let [<Literal>] eventsSample = """
    { 
      "config" : {
        "isWinter": false, 
        "isSummer": false
      },
      "events" : [
      { "title":"talk title", 
        "date":"2011-01-01T17:00:00",
        "id" : "use as string",
        "description" : "string",
        "enddate" : "2011-01-01T17:30:00",
        "status" : "string",
        "logo"   : "url string",
        "link"   : "http://site/link_to_this_event",
        "venue"  : {"name":"string", "address":"123 mean st.", "id":"use as string"}
      }
    ]}
  """
  type FeaturedPhoto () =
    let mutable link = ""
    member x.Photo_Link 
      with get() = link
      and set(v) = link <- v

  type Venue () = 
    let mutable _id = ""
    let mutable name = ""
    let mutable address1 = ""
    member x.Id
      with get() = _id
      and set(value) = _id <- value
    member x.Name
      with get() = name
      and set(value) = name <- value
    member x.Address1
      with get() = address1
      and set(value) = address1 <- value

  type Meetup () =
    let mutable venue = Venue ()
    let mutable time = 0L
    let mutable name = ""
    let mutable status = ""
    let mutable _id = ""
    let mutable link = ""
    let mutable photo = FeaturedPhoto ()
    let mutable description = ""
    let mutable duration = 0

    member x.Duration
      with get() = duration
      and set(v) = duration <- v
    member x.Venue
      with get() = venue
      and set(v) = venue <- v
    member x.Time 
      with get() = time
      and set(v) = time <- v
    member x.Name
      with get() = name
      and set(v) = name <- v
    member x.Status
      with get() = status
      and set(v) = status <- v
    member x.Id
      with get() = _id
      and set(v) = _id <- v
    member x.Link 
      with get() = link
      and set(v) = link <- v
    member x.Featured_Photo
      with get() = photo
      and set(v) = photo <- v
    member x.Plain_Text_Description
      with get() = description
      and set(v) = description <- v

  type EventsJson = JsonProvider<eventsSample, RootName="root">

  let createEvent (e : Meetup) = 
    let epoch = DateTimeOffset (1970,1,1, 0,0,0,0,TimeSpan.Zero)
    let logo = if e.Featured_Photo.Photo_Link = "" then "https://meetup.com/mu_static/en-US/group_fallback_large_2.d2eedbb1.png" 
               else e.Featured_Photo.Photo_Link
    EventsJson.Event(
      title   = e.Name, 
      status  = e.Status,
      date    = (epoch.AddMilliseconds (float e.Time)).UtcDateTime,
      enddate = (epoch.AddMilliseconds (float (e.Time + (int64 e.Duration)))).UtcDateTime,
      id      = e.Id,
      link    = e.Link,
      logo    = logo,
      description = e.Plain_Text_Description,
      venue = EventsJson.Venue(name=e.Venue.Name,address=e.Venue.Address1,id=string e.Venue.Id))

  let getEvents ctx =
    let onlyPublished (e: Meetup) = e.Status <> "draft"
    let eventDate (e: EventsJson.Event) = e.Date

    let isJustBefore (d1: DateTime) (d2: DateTime) = 
      let target = d2.AddMonths(-1)
      d1.Month = target.Month && d1.Year = target.Year

    async {
      //let url = sprintf "https://api.meetup.com/2/events?offset=0&format=json&limited_events=False&group_urlname=fullstackmb&photo-host=public&page=20&fields=&order=time&desc=false&status=upcoming&sig_id=%s&sig=%s" key sec
      let url = "https://api.meetup.com/fullstackmb/events?&sign=true&photo-host=public&page=5&fields=plain_text_description,featured_photo"
      let! resp = httpClient.GetAsync url
                    |> Async.AwaitTask
      let! contents = resp.Content.ReadAsStringAsync () |> Async.AwaitTask 
      httpClient.DefaultRequestHeaders.Authorization <- null
      let mevents = JsonConvert.DeserializeObject<Meetup[]>(contents)
      
      let published = mevents
                      |> Array.filter onlyPublished 
                      |> Array.map createEvent
      let lastEvent = published |> Array.head |> eventDate
      let now = DateTime.UtcNow 
      let alreadyHappened  = lastEvent < now 

      let isSummer = Season.isSummer now || (alreadyHappened && Season.isBeforeSummer lastEvent)

      let isWinter = Season.isWinter now && alreadyHappened

      let config = EventsJson.Config(isSummer = isSummer, isWinter = isWinter)

      return! EventsJson.Root(config, published) |> jsonStr |> flip OK ctx
    }
