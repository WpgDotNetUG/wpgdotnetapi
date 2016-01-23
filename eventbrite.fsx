﻿#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

module Eventbrite = 

    open System
    open System.Net
    open Suave.Successful
    open FSharp.Data

    let [<Literal>] eventsSample = """ { "events" : [{"title":"talk title", "date":"2011-01-01T17:00:00", 
                                                  "id" : "use as string",  
                                                  "description" : "string",
                                                  "enddate" : "2011-01-01T17:30:00",
                                                  "status" : "string",
                                                  "venue" : {"name":"string", "address":"123 mean st.", "id":"use as string"}}]} """

    type EventsJson = JsonProvider<eventsSample, RootName="Root">

    type EbEventsJson = JsonProvider<""" {"pagination": {"object_count": 32, "page_number": 1, "page_size": 50, "page_count": 1}, 
                                                    "events": [{"name": {"text": "T vs B", "html": "T vs B"}, 
                                                                "description": {"text": "description in text format", "html": "description in html"}, 
                                                                "id": "could be a string",
                                                                "status": "draft",
                                                                "url": "http://www.eventbrite.ca/e/blah", 
                                                                "start": {"timezone": "America/Winnipeg", "local": "2016-04-21T17:30:00", "utc": "2016-04-21T22:30:00Z"}, 
                                                                "end": {"timezone": "America/Winnipeg", "local": "2016-04-21T20:30:00", "utc": "2016-04-22T01:30:00Z"}, 
                                                                "venue": {"address": {"address_1": "251 Donald Street"}, "id": "stringify", "name": "Millenium Library   Buchwald Room, 2nd floor"}}]} """>

    let getEvents = (fun x -> 
        let createEbReq (url : string) = 
            let auth = Environment.GetEnvironmentVariable("EB_AUTH_TOKEN") |> sprintf "Bearer %s"
            let wr = HttpWebRequest.Create(url)
            wr.Headers.Add("Authorization", auth)
            wr

        let createEvent (e : EbEventsJson.Event) = 
            EventsJson.Event(title=e.Name.Text, 
                            status = e.Status,
                            date=e.Start.Local,
                            enddate=e.End.Local,
                            id=e.Id,
                            description=e.Description.Text,
                            venue = EventsJson.Venue(name=e.Venue.Name,address=e.Venue.Address.Address1,id=e.Venue.Id))

        async {
            let wr = createEbReq "https://www.eventbriteapi.com/v3/users/me/owned_events/?order_by=start_desc&expand=venue"
            let! resp = wr.AsyncGetResponse ()
            use s = resp.GetResponseStream ()
            let result = EbEventsJson.Load s
            let events = result.Events |> Array.filter (fun e -> e.Status <> "draft" ) 
                         |> Array.map createEvent
        
            return! OK (EventsJson.Root(events = events).JsonValue.ToString()) x
        })