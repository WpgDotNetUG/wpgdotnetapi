#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharpx.Extras/lib/40/FSharpx.Extras.dll"
#load "wpgdotnet.fsx"

open System
open FSharp.Data
open Suave
open Suave.Successful // for OK-result
open Suave.Http 
open FSharpx.Nullable
open wpgdotnet

let inline (|??) a b = match a with 
                        | null -> b
                        | _ -> a

module Slack = 
    let INTERNAL_ERROR ctx = 
        raise (wpgdotnet.InternalException "Internal Error")

    type SlackMessage = JsonProvider<""" { "ok":"false", "error":"not authorized" } """>
    let invite ctx email   =
        async {
            let org = (Environment.GetEnvironmentVariable("SLACK_ORG")) |?? "wpgdotnet" // don't die if environment variable not set
            let token =  Environment.GetEnvironmentVariable("SLACK_TOKEN") 
            let url = sprintf "https://%s.slack.com/api/users.admin.invite?token=%s" org token
            let! apiResponse = Http.AsyncRequestString(url, body = FormValues ["email", email])

            let slackResponse = SlackMessage.Parse(apiResponse)
            if not slackResponse.Ok  then
                INTERNAL_ERROR (slackResponse.Error) ctx
            return! OK (apiResponse) ctx
        }

    let signUp (ctx:HttpContext) = 
        ctx.request.formData "email" 
        |> FSharpx.Choice.choice (invite ctx) INTERNAL_ERROR 
