module FatSlack.Core.SlackApi

open Net
open Types
open System

type ApiClient = {
    Token: string
}

let createApiClient token = {
    Token = token
}

let createBaseForm client (messageBaseData: MessageBaseData) =
        [
            "token", client.Token 
            "channel", messageBaseData.Channel
            "text", messageBaseData.Text
            "as_user", "true"
            "attachments", (Json.serialize messageBaseData.Attachments)
        ]

let sendMessage client (msg: Message) =
    match msg with
    | PostMessage pm ->
        let fv =
            pm.MessageBaseData 
            |> createBaseForm client
        fv, "https://slack.com/api/chat.postMessage"
    | UpdateMessage um ->
        let fv =
            um.MessageBaseData 
            |> createBaseForm client 
            |> (fun fv -> ("ts", (um.Timestamp)) :: fv)
        fv, "https://slack.com/api/chat.update"
    |> (fun (fv, url) ->
        printfn "Posting to slack: %A" fv
        Http.post url (Http.FormValues fv))
