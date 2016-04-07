#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharpx.Extras/lib/40/FSharpx.Extras.dll"

open System
open FSharp.Data
open Suave
open Suave.Successful // for OK-result
open Suave.Http 

module Slack = 
    let invite ctx email   =
        async {
            let org =   Environment.GetEnvironmentVariable("SlackOrg")
            let token =  Environment.GetEnvironmentVariable("SlackToken")
            let url = sprintf "https://%s.slack.com/api/users.admin.invite?token=%s" org token
            let! html = Http.AsyncRequestString(url, body = FormValues ["email", email])
            return! OK (html) ctx
        }

    let signUp (ctx:HttpContext) = 
        ctx.request.formData "email" 
        |> FSharpx.Choice.get 
        |> invite ctx 
