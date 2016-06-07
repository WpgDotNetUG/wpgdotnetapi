module Env

let tryGetVar = System.Environment.GetEnvironmentVariable >> function
                | null -> None
                | v    -> Some v 