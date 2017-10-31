module FatSlack.Core.SlackApi

open Net
open Types

type ApiClient = {
    Token: string
}

let createApiClient token = {
    Token = token
}
let postMessage client (msg:Message) =
    let formValues = 
        [
            "token", client.Token 
            "channel", msg.Channel
            "text", msg.Text
            "as_user", "true"
            "attachments", (Json.serialize msg.Attachments)
        ]
    printfn "Posting to slack: %A" formValues
    Http.post "https://slack.com/api/chat.postMessage" (Http.FormValues formValues)
