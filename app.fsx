#if BOOTSTRAP
System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
if not (System.IO.File.Exists "paket.exe") then let url = "https://github.com/fsprojects/Paket/releases/download/2.40.9/paket.exe" in use wc = new System.Net.WebClient() in let tmp = System.IO.Path.GetTempFileName() in wc.DownloadFile(url, tmp); System.IO.File.Move(tmp,System.IO.Path.GetFileName url);;
#r "paket.exe"
Paket.Dependencies.Install (System.IO.File.ReadAllText "paket.dependencies")
#endif

//---------------------------------------------------------------------

#I "packages/Suave/lib/net40"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#load "eventbrite.fsx"

open System
open Suave                 // always open suave
open Suave.Http
open Suave.Filters
open Suave.Successful // for OK-result
open Suave.Web             // for config
open System.Net
open Suave.Operators 
open FSharp.Data
open Eventbrite

printfn "initializing script..."


let config = 
    let port = System.Environment.GetEnvironmentVariable("PORT")
    let ip127  = IPAddress.Parse("127.0.0.1")
    let ipZero = IPAddress.Parse("0.0.0.0")

    { defaultConfig with 
        logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Verbose
        bindings=[ (if port = null then HttpBinding.mk HTTP ip127 (uint16 8080)
                    else HttpBinding.mk HTTP ipZero (uint16 port)) ] }


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

type SponsorsJson = JsonProvider<""" { "sponsors": [{"name":"Great Sponsor", "url": "http://somesite.com", "imgUrl": "http://someurl.com/image1"}] } """, RootName="Root">

let sponsorsText = SponsorsJson.Root(
                        sponsors=[|
                          SponsorsJson.Sponsor(name="Apptius"  , imgUrl="http://images.winnipegdotnet.org/apptius.png", url="http://apptius.com") 
                          SponsorsJson.Sponsor(name="Imaginet" , imgUrl="http://images.winnipegdotnet.org/imaginet.png", url="http://imaginet.com") 
                          SponsorsJson.Sponsor(name="iQmetrix" , imgUrl="http://images.winnipegdotnet.org/iqmetrix.png", url="http://www.iqmetrix.com") 
                          SponsorsJson.Sponsor(name="Microsoft", imgUrl="http://images.winnipegdotnet.org/microsoft.png", url="http://blogs.msdn.com/b/cdndevs/")
                          SponsorsJson.Sponsor(name="Ineta"    , imgUrl="http://images.winnipegdotnet.org/ineta.png", url="http://www.ineta.org/")
                          SponsorsJson.Sponsor(name="JetBrains", imgUrl="http://images.winnipegdotnet.org/jetbrains.png", url="http://www.jetbrains.com") 
                        |]
                   ).JsonValue.ToString()
                   

let boardText = 
    """
{"board": [
  {"name": "Roy Drenker" , "role": "Treasurer"},
  {"name": "David Wesst" , "role": "Master of social media"},
  {"name": "Amir Barylko", "role": "President"}
]}""" 

let jsonMime = Writers.setMimeType "application/json"

let app = 
  choose
    [ GET >=> choose
                [ path "/" >=> OK homePage
                  path "/api/sponsors" >=> jsonMime >=> OK sponsorsText
                  path "/api/board" >=> jsonMime >=> OK boardText 
                  path "/api/events" >=> jsonMime >=> Eventbrite.getEvents
                  path "/goodbye" >=> OK "Good bye GET" ]]
    

startWebServer config app
printfn "exiting server..."


