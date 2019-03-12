﻿<<<<<<< HEAD
﻿#r "packages/Suave/lib/net40/Suave.dll"
=======
﻿open FSharpx
#r "packages/Suave/lib/net40/Suave.dll"
>>>>>>> master
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/FSharpX.Extras/lib/40/FSharpx.Extras.dll"
#r "packages/FSharpX.Collections/lib/net40/FSharpx.Collections.dll"
#r "System.Net.Http.dll"

#load "common.fsx"

module Meetup =
  open Common
  open System
  open System.Net
  open Suave.Successful
  open FSharp.Data
  open System.Net.Http

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

  type EventsJson = JsonProvider<eventsSample, RootName="root">


  type MeetupJson = JsonProvider<""" {"results":[{"utc_offset":-18000000,"venue":{"zip":"R3B 0S1","country":"ca","localized_country_name":"Canada","city":"Winnipeg","address_1":"171 McDermot Ave","name":"Forth Cafe","lon":-97.13726,"id":26064948,"state":"MB","lat":49.89719,"repinned":true},"headcount":0,"visibility":"public","waitlist_count":0,"created":1548274299000,"maybe_rsvp_count":0,"description":"<p>Have you got a project you want to work on but don't have time at work? Have questions to ask your peers? Some cool code to show off?<br\/>This is your opportunity! Have a beverage of your preference and share some awesome code with your peers.<br\/>No agenda, come and go.<br\/>If people like this meetup format we'll keep doing it. Let us know!<\/p>","how_to_find_us":"look for coffee drinkers with laptops showing code ;)","event_url":"https:\/\/www.meetup.com\/fullstackmb\/events\/258963105\/","yes_rsvp_count":6,"duration":7200000,"announced":false,"name":"Full Stack Code and Coffee","id":"tpqmqqyzfbrb","photo_url":"https:\/\/secure.meetupstatic.com\/photos\/event\/1\/9\/8\/b\/global_479406539.jpeg","time":1552514400000,"updated":1548274299000,"group":{"join_mode":"open","created":1544122305000,"name":"Full Stack Manitoba","group_lon":-97,"id":30661086,"urlname":"fullstackmb","group_lat":49.88999938964844,"who":"Members"},"status":"upcoming"},
   {"utc_offset":-18000000,"venue":{"zip":"R3B 0S1","country":"ca","localized_country_name":"Canada","city":"Winnipeg","address_1":"171 McDermot Ave","name":"Forth Cafe","lon":-97.13726,"id":26064948,"state":"MB","lat":49.89719,"repinned":false},"headcount":0,"visibility":"public","waitlist_count":0,"created":1548274299000,"maybe_rsvp_count":0,"description":"<p>Have you got a project you want to work on but don't have time at work? Have questions to ask your peers? Some cool code to show off?<br\/>This is your opportunity! Have a beverage of your preference and share some awesome code with your peers.<br\/>No agenda, come and go.<br\/>If people like this meetup format we'll keep doing it. Let us know!<\/p>","how_to_find_us":"look for coffee drinkers with laptops showing code ;)","event_url":"https:\/\/www.meetup.com\/fullstackmb\/events\/tpqmqqyzgbnb\/","yes_rsvp_count":0,"duration":7200000,"name":"Full Stack Code and Coffee","id":"tpqmqqyzgbnb","time":1554933600000,"updated":1548274299000,"group":{"join_mode":"open","created":1544122305000,"name":"Full Stack Manitoba","group_lon":-97,"id":30661086,"urlname":"fullstackmb","group_lat":49.88999938964844,"who":"Members"},"status":"upcoming"},
   {"utc_offset":-21600000,"venue":{"zip":"R3B 0S1","country":"ca","localized_country_name":"Canada","city":"Winnipeg","address_1":"171 McDermot Ave","name":"Forth Cafe","lon":-97.13726,"id":26064948,"state":"MB","lat":49.89719,"repinned":false},"headcount":0,"visibility":"public","waitlist_count":0,"created":1548274299000,"maybe_rsvp_count":0,"description":"<p>Have you got a project you want to work on but don't have time at work? Have questions to ask your peers? Some cool code to show off?<br\/>This is your opportunity! Have a beverage of your preference and share some awesome code with your peers.<br\/>No agenda, come and go.<br\/>If people like this meetup format we'll keep doing it. Let us know!<\/p>","how_to_find_us":"look for coffee drinkers with laptops showing code ;)","event_url":"https:\/\/www.meetup.com\/fullstackmb\/events\/tpqmqqyzqbxb\/","yes_rsvp_count":0,"duration":7200000,"name":"Full Stack Code and Coffee","id":"tpqmqqyzqbxb","time":1576710000000,"updated":1548274299000,"group":{"join_mode":"open","created":1544122305000,"name":"Full Stack Manitoba","group_lon":-97,"id":30661086,"urlname":"fullstackmb","group_lat":49.88999938964844,"who":"Members"},"status":"upcoming"}
   ]
   ,"meta":{"next":"https:\/\/api.meetup.com\/2\/events?offset=1&format=json&limited_events=False&group_urlname=fullstackmb&sig=ea3363a7840132362ddb5e1775ad7b3d00db3c8d&photo-host=public&page=20&fields=&sig_id=190044985&order=time&desc=false&status=upcoming","method":"Events","total_count":25,"link":"https:\/\/api.meetup.com\/2\/events","count":20,"description":"Access Meetup events using a group, member, or event id. Events in private groups are available only to authenticated members of those groups. To search events by topic or location, see [Open Events](\/meetup_api\/docs\/2\/open_events).","lon":"","title":"Meetup Events v2","url":"https:\/\/api.meetup.com\/2\/events?offset=0&format=json&limited_events=False&group_urlname=fullstackmb&sig=ea3363a7840132362ddb5e1775ad7b3d00db3c8d&photo-host=public&page=20&fields=&sig_id=190044985&order=time&desc=false&status=upcoming","id":"","updated":1548274299000,"lat":""}} """>

   // (e.Logo |> Option.map (fun l -> l.Url) |> FSharpx.Option.getOrElse ""), 


  let createEvent (e : MeetupJson.Result) = 
    let epoch = DateTimeOffset (1970,1,1, 0,0,0,0,TimeSpan.Zero)
    EventsJson.Event(
      title   = e.Name, 
      status  = e.Status,
      date    = (epoch.AddMilliseconds (float e.Time)).UtcDateTime,
      enddate = (epoch.AddMilliseconds (float (e.Time + (int64 e.Duration)))).UtcDateTime,
      id      = e.Id,
      link    = e.EventUrl,
      logo    = Option.getOrElse "https://meetup.com/mu_static/en-US/group_fallback_large_2.d2eedbb1.png" e.PhotoUrl,
      description = e.Description,
      venue = EventsJson.Venue(name=e.Venue.Name,address=e.Venue.Address1,id=string e.Venue.Id))

  let getEvents ctx =
    let onlyPublished (e: MeetupJson.Result) = e.Status <> "draft"
    let eventDate (e: EventsJson.Event) = e.Date

    let isJustBefore (d1: DateTime) (d2: DateTime) = 
      let target = d2.AddMonths(-1)
      d1.Month = target.Month && d1.Year = target.Year

    async {
      //httpClient.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", "EB_AUTH_TOKEN" |> Env.getVar)
      let key = "MEETUP_KEY"   |> Env.tryGetVar |> FSharpx.Option.getOrElse ""
      let sec = "MEETUP_SIG"   |> Env.tryGetVar |> FSharpx.Option.getOrElse ""
      let url = sprintf "https://api.meetup.com/2/events?offset=0&format=json&limited_events=False&group_urlname=fullstackmb&photo-host=public&page=20&fields=&order=time&desc=false&status=upcoming&sig_id=%s&sig=%s" key sec
      
      let! resp = httpClient.GetAsync url
                    |> Async.AwaitTask
      let! contents = resp.Content.ReadAsStringAsync () |> Async.AwaitTask 
      httpClient.DefaultRequestHeaders.Authorization <- null
      let published = MeetupJson.Parse(contents).Results 
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
