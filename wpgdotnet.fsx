namespace wpgdotnet
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/Suave/lib/net40/Suave.dll"
open Suave
open Suave.Web
open System
open System.Text
open FSharp.Data

module wpgdotnet =
    exception InternalException of string

    let [<Literal>] exceptionSample = """ { "message":"friendly error" } """
    type exceptionJson = JsonProvider<exceptionSample, RootName="Root">

    let FiveHundred(msg: string) (ctx : HttpContext) = 
       Response.response HTTP_500 (Encoding.UTF8.GetBytes (msg)) ctx

    let INTERNAL_ERROR (ex: Exception) (ctx : HttpContext)= 
//        if ctx.isLocal then
//            let msg = sprintf "%s: %s %A" (ex.GetType().ToString()) ex.Message ex
//            FiveHundred (exceptionJson.Root(message=msg).JsonValue.ToString()) ctx
//        else
        match ex with 
        | :? InternalException as ie -> 
            FiveHundred (exceptionJson.Root(message=ie.Message).JsonValue.ToString()) ctx
        |_ -> 
            let internalErrText = exceptionJson.Root(message=HTTP_500.message).JsonValue.ToString()
            FiveHundred internalErrText ctx
