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

open FatSlack.Core.Api.Dto.Actions
let send (client: ApiClient) (msg: Api.Dto.Actions.SlackAction) =
    printfn "send> Sending: %A" msg
    let toUrl = function
        | PostMessage -> "https://slack.com/api/chat.postMessage"
        | UpdateMessage -> "https://slack.com/api/chat.update"
    match msg.Data with
    | Dto data ->
        let payload = Json.serialize data
        let authorizationHeader = sprintf "Bearer %s" client.Token
        Http.postJson [("Authorization", authorizationHeader)] (msg.Method |> toUrl) (Http.JsonData payload)
