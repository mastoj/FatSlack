module FatSlack.SlackApi
open Types
open Net

let sendMessage url token (data: obj) =
    let payload = Json.serialize data
    let authorizationHeader = sprintf "Bearer %s" token
    async {
        let jsonPayload = (Http.JsonData payload)
        printfn "==> Posting: %A" jsonPayload
        let! response = Http.postJson [("Authorization", authorizationHeader)] url jsonPayload
        return 
            response
            |> (fun x -> printfn "==> Raw response: %A" x; x)
            |> Json.deserialize<ChatResponseMessage>
            |> (fun x -> printfn "==> Deserialized: %A" x; x)
    }

let createSlackApi token =
    let send message =
        match message with
        | PostMessage msg -> sendMessage "https://slack.com/api/chat.postMessage" token msg
        | UpdateMessage msg -> sendMessage "https://slack.com/api/chat.update" token msg
        | DialogMessage msg -> sendMessage "https://slack.com/api/dialog.open" token msg

    let respond url message =
        match message with
        | PostMessage msg -> msg :> obj
        | UpdateMessage msg -> msg :> obj
        | DialogMessage msg -> msg :> obj
        |> sendMessage url token
    {
        Send = send
        Respond = respond
    }