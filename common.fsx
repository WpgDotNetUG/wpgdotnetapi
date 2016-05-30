#I @"packages/FSharp.Data/lib/net40"

#r "packages/FSharpX.Extras/lib/40/FSharpx.Extras.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "FSharp.Data.dll"

open System

[<AutoOpen>]
module Common =
  open FSharp.Data

  let flip f a b = f b a
  let jsonStr (t: Runtime.BaseTypes.IJsonDocument) = t.JsonValue.ToString()
  
module Option =
  let ofNull = function null -> None | var -> Some var
  let getOrElse = FSharpx.Option.getOrElse

module Env =
  let tryGetVar = Environment.GetEnvironmentVariable >> Option.ofNull
  let getVar = tryGetVar >> Option.get

module Season =
  let isSummer (d: DateTime) = d.Month = 7
  let isWinter (d:DateTime)  = d.Month = 12
  let isBeforeSummer (d:DateTime) = d.Month = 6 && d.Year = DateTime.Today.Year
  let isBeforeWinter (d:DateTime) = d.Month = 11 && d.Year = DateTime.Today.Year


