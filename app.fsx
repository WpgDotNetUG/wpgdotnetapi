//---------------------------------------------------------------------

#I "packages/Suave/lib/net40"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

#load "eventbrite.fsx"
#load "youtube.fsx"

open System
open Suave                 // always open suave
open Suave.Http
open Suave.Filters
open Suave.Successful // for OK-result
open Suave.Web             // for config
open System.Net
open Suave.Operators 
open Suave.Writers 
open FSharp.Data
open Eventbrite
open Youtube

let angularHeader = """<head>
<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.2.0/css/bootstrap.min.css">
<script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.2.26/angular.min.js"></script>
</head>"""

let homePage = 
    [ yield """<html>"""
      yield angularHeader 
      yield """ <body>"""
      yield """ <h1>Welcome to the WPG .NET UG API</h1>"""
      yield """  <table class="table table-striped">"""
      yield """   <thead><tr><th>Page</th><th>Link</th></tr></thead>"""
      yield """   <tbody>"""
      yield """      <tr><td>Board members</td><td><a href="/api/board">Link to board members json</a></td></tr>""" 
      yield """      <tr><td>Goodbye</td><td><a href="/goodbye">Link</a></td></tr>"""
      yield """   </tbody>"""
      yield """  </table>"""
      yield """ </body>""" 
      yield """</html>""" ]
    |> String.concat "\n"

printfn "starting web server..."

let [<Literal>] sponsorSample = """ { "sponsors": [{"name":"Great Sponsor", "url": "http://somesite.com", "imgUrl": "http://someurl.com/image1"}] } """
type SponsorsJson = JsonProvider<sponsorSample, RootName="Root">

let sponsorsText = 
  let imgPath = (+) "http://winnipegdotnet.org/Images/"
  let mkSponsor (n, l, u) = SponsorsJson.Sponsor(name=n, imgUrl=imgPath l, url=u)
  let sponsors =
    [
      ("Apptius"  , "Apptius-Logo.png", "http://apptius.com") 
      ("Imaginet" , "imaginet.png"    , "http://imaginet.com") 
      ("iQmetrix" , "iqmetrix-logo.png", "http://www.iqmetrix.com") 
      ("Vision Critical", "vc_logo.png", "http://www.visioncritical.com/")
      ("Microsoft", "MSFT.png", "http://blogs.msdn.com/b/cdndevs/")
      ("JetBrains", "jetbrains_logo.png", "http://www.jetbrains.com")
    ]
    |> List.map mkSponsor
    |> Array.ofList

  SponsorsJson.Root( sponsors=sponsors).JsonValue.ToString()

let [<Literal>] boardSample = """ { "board": [{"name":"John Doe", "role":"Important Role", "imgUrl":"http://someimage.com", "contact": "Inquiries"}] } """
type BoardJson = JsonProvider<boardSample, RootName="Root">

let boardText =
  let imgPath = (+) "http://winnipegdotnet.org/content/contactus/"
  let mkMember (n, r, i, c) = BoardJson.Board(name=n, role=r, imgUrl=imgPath i, contact=c)
  let members = 
    [|
      ("Amir Barylko", "President"     , "amir.jpg" , "General Inquiries")
      ("Roy Drenker" , "Treasurer"     , "roy.jpg"  , "Sponsorship")
      ("David Wesst" , "Event Planning", "david.jpg", "Events")
    |]
    |> Array.map mkMember

  BoardJson.Root(board=members).JsonValue.ToString()

let jsonMime = setMimeType "application/json" >=> setHeader  "Access-Control-Allow-Origin" "*"

let app = 
  choose
    [ GET >=> choose
                [ path "/" >=> OK homePage
                  path "/api/sponsors"        >=> jsonMime >=> OK sponsorsText
                  path "/api/sponsors/sample" >=> jsonMime >=> OK sponsorSample
                  path "/api/board"           >=> jsonMime >=> OK boardText 
                  path "/api/board/sample"    >=> jsonMime >=> OK boardSample
                  path "/api/events"          >=> jsonMime >=> Eventbrite.getEvents
                  path "/api/events/sample"   >=> jsonMime >=> OK Eventbrite.eventsSample
                  path "/api/videos"          >=> jsonMime >=> Youtube.getVideos
                  path "/goodbye" >=> OK "Good bye GET" 
                  RequestErrors.NOT_FOUND "Resource not found." ]]
