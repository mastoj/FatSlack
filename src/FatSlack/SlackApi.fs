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

let createSlackApi token message =
    match message with
    | PostMessage msg -> sendMessage token "https://slack.com/api/chat.postMessage" msg
    | UpdateMessage msg -> sendMessage token "https://slack.com/api/chat.update" msg
    | DialogMessage msg -> sendMessage token "https://slack.com/api/dialog.open" msg
