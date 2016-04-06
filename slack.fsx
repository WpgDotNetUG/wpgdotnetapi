#r "System.Configuration"
open System.Configuration
open System.Net
open System.IO

module Slack = 
    let AppSettings = 
        let path = __SOURCE_DIRECTORY__ + "/web.config"
        let fileMap = ExeConfigurationFileMap(ExeConfigFilename=path)
        let config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None)
        config.AppSettings.Settings

    let invite email =
        let org = AppSettings.["SlackOrg"].Value
        let token =  AppSettings.["SlackToken"].Value
        let url = sprintf "https://%s.slack.com/api/users.admin.invite?token=%s" org token

        // Create & configure HTTP web request
        let req = HttpWebRequest.Create(url) :?> HttpWebRequest 
        req.ProtocolVersion <- HttpVersion.Version10
        req.Method <- "POST"

        // Encode body with POST data as array of bytes
        let postBytes = System.Text.Encoding.ASCII.GetBytes(sprintf "email=%s" email)
        req.ContentType <- "application/x-www-form-urlencoded"
        req.ContentLength <- int64 postBytes.Length  

        // Write data to the request
        let reqStream = req.GetRequestStream() 
        reqStream.Write(postBytes, 0, postBytes.Length);
        reqStream.Close()

        // Obtain and return response - could process here but easier to just pass along the slack response
        let resp = req.GetResponse() 
        let stream = resp.GetResponseStream() 
        let reader = new StreamReader(stream) 
        let html = reader.ReadToEnd()
        html
 
    let signUp emailArg = 
        match emailArg with
            | Choice1Of2 email -> 
                invite email
            | Choice2Of2 email-> 
                raise  (System.ArgumentException("email"))
