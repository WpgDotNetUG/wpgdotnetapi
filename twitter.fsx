#I @"packages/FSharp.Data.Toolbox.Twitter/lib/net40"
#I @"packages/FSharp.Data/lib/net40"

#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "FSharp.Data.Toolbox.Twitter.dll"
#r "FSharp.Data.dll"

module Twitter = 

  open System
  open System.Net
  open Suave.Successful
  open FSharp.Data.Toolbox.Twitter
  open FSharp.Data

  let key = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY")
  let secret = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET")

  let [<Literal>] twitterSample = """
    {
      "tweets": [
        { 
          "text": "some kind",
          "entity" :  {
            "hashtags" : [],
            "urls" : [],
            "user_mentions": []
          }
        }
      ]
    }
    """

  type TwitterData = JsonProvider<twitterSample>
  type EntityData = TwitterData.Entity

  let mapEntity e = TwitterData.Entity([||], [||], [||])

  let getTweets ctxt =
    async {
      let twitter = Twitter.AuthenticateAppOnly(key, secret)
      let home = twitter.Timelines.Timeline("wpgnetug", 10)

      let tweets = home |> Array.map (fun t -> TwitterData.Tweet(t.Text, mapEntity t.Entities))


      return! OK (TwitterData.Root(tweets).JsonValue.ToString()) ctxt
    }

