#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharpx.Extras/lib/40/FSharpx.Extras.dll"

open System
open FSharp.Data
open Suave
open Suave.ServerErrors
open Suave.Successful 

let inline (|??) a b = match a with  // for pulling values out of non f# code.. that might be null
                        | null -> b
                        | _ -> a

module Slack = 

    type SlackMessage = JsonProvider<""" { "ok":"false", "error":"not authorized" } """>

    let asyncError (ctx:HttpContext) (msg:string) = 
        async{
            let formattedMessage = sprintf """{"errorMessage":"%s"}""" msg
            printfn "error => %s" formattedMessage
            return! INTERNAL_ERROR formattedMessage ctx
        }

    let invite (ctx : HttpContext) email   =
        async {
            let org = (Environment.GetEnvironmentVariable("SLACK_ORG")) |?? "wpgdotnet" // don't die if environment variable not set
            let token =  Environment.GetEnvironmentVariable("SLACK_TOKEN") 
            let url = sprintf "https://%s.slack.com/api/users.admin.invite?t=%O" org (DateTime.Now.ToString("yyyyMMddhhmmss"))
            let! apiResponse = Http.AsyncRequestString(url, body = FormValues ["email", email; "token", token])
            let slackResponse = SlackMessage.Parse(apiResponse)
            match slackResponse.Ok with 
            | false -> return! (asyncError ctx slackResponse.Error)
            | true ->return! OK (apiResponse) ctx
        }

    let signUp (ctx:HttpContext) = 
        ctx.request.formData "email" 
        |> FSharpx.Choice.choice (invite ctx) (fun i -> asyncError ctx "Missing email")
