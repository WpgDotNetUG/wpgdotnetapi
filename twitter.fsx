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
      "tweets": [ {
          "user": {
              "id": "some_id",
              "name": "User Name",
              "screen_name": "@username",
              "url": "http://some-cool-site.com",
              "profile_image_url": "http://some-cool-site.com/someones-face.jpg"
          },
          "created_at": "Tue Oct 30 13:22:33 +0000 2012",
          "text": "some kind",
          "entity": {
              "hashtags": [ {
                  "text": "poignanttag",
                  "indices": [10, 20] } ],
              "urls": [ {
                  "url": "http://t.co/funUrl",
                  "expanded_url": "http://www.actual-domain.com/?some=query&stuff=123",
                  "display_url": "actual-domain.com/?some=…",
                  "indices": [10, 20] } ],
              "user_mentions": [ {
                  "screen_name": "wpgnetug",
                  "name": "Winnipeg Dotnet",
                  "id_str": "some_id",
                  "indices": [10, 20] } ] }
       } ]
     }
    """

  type TwitterData = JsonProvider<twitterSample>
  type EntityData  = TwitterData.Entity
  type UserData    = TwitterData.User
  type HashtagData = TwitterData.Hashtag
  type UrlData     = TwitterData.Url
  type MentionData = TwitterData.UserMention
  
  type Mention     = TwitterTypes.TimeLine.UserMention
  type Entity      = TwitterTypes.TimeLine.Entities4
  type User        = TwitterTypes.TimeLine.User

  let mapUser (u:User) =
    UserData(
      u.Id.ToString(),
      u.Name,
      u.ScreenName,
      u.Url.Value,
      u.ProfileImageUrl)

  let mapEntity (e:Entity) =
    EntityData(
        e.Hashtags      |> Array.map (fun h -> HashtagData (h.Text, h.Indices)),
        e.Urls          |> Array.map (fun u -> UrlData     (u.Url, u.ExpandedUrl, u.DisplayUrl, u.Indices)),
        e.UserMentions  |> Array.map (fun m -> MentionData (m.ScreenName, m.Name, m.Id.ToString(), m.Indices)) )

  let getTweets ctxt =
    async {
      let twitter = Twitter.AuthenticateAppOnly(key, secret)
      let home = twitter.Timelines.Timeline("wpgnetug", 10)

      let tweets = home |> Array.map (fun t -> TwitterData.Tweet(mapUser t.User, t.CreatedAt, t.Text, mapEntity t.Entities))

      return! OK (TwitterData.Root(tweets).JsonValue.ToString()) ctxt
    }

