#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharpx.Extras/lib/40/FSharpx.Extras.dll"

#load "common.fsx"

open System
open FSharp.Data
open Suave
open Suave.ServerErrors
open Suave.Successful 

open Common

module Slack = 

    type SlackMessage = JsonProvider<""" { "ok":"false", "error":"not authorized" } """>

    let asyncError (ctx:HttpContext) (msg:string) = 
      async{
        let formattedMessage = sprintf """{"ok": "false", "error":"%s"}""" msg
        printfn "error => %s" formattedMessage
        return! INTERNAL_ERROR formattedMessage ctx
      }

    let org   = "SLACK_ORG"   |> Env.tryGetVar |> FSharpx.Option.getOrElse "wpgdotnet" // don't die if environment variable not set
    let token = "SLACK_TOKEN" |> Env.getVar
    let url   = sprintf "https://%s.slack.com/api/users.admin.invite?t=%O" org 

    let invite (ctx : HttpContext) email =
      async {
        let timeStamp = DateTime.Now.ToString("yyyyMMddhhmmss")
        let! apiResponse = Http.AsyncRequestString(url timeStamp, body = FormValues ["email", email; "token", token])
        let slackResponse = SlackMessage.Parse(apiResponse)
        return! OK apiResponse ctx
      }

    let signUp (ctx:HttpContext) = 
      ctx.request.formData "email" 
      |> FSharpx.Choice.choice (invite ctx) (fun i -> asyncError ctx "Missing email")
