module FatSlack.BotApp

open Types
open Net
open Suave
open System.Text.RegularExpressions

[<AutoOpen>]
module Helpers =
    type MessageType =
        | NotAddressedToBot
        | AddressedToBot of commandText:string

    let startsWith first (str: string) = str.StartsWith(first)

    let isDirectMessage (message: ChatMessage) =
        message.Channel |> startsWith "D"

    let isDirectedToBot alias botId (message: ChatMessage) =
        let checkPattern text pattern = 
            let regexPattern = "^" + pattern
            let regex = Regex(regexPattern)
            text |> isNull |> not && regex.IsMatch(text)

        let idPattern = "<@" + botId
        let patternsToLookFor = 
            alias
            |> Option.fold (fun patterns alias -> alias :: patterns) [idPattern]
        let isMatch =
            patternsToLookFor
            |> List.exists (checkPattern message.Text)
        if isMatch then AddressedToBot (message.Text.Substring(message.Text.IndexOf(" ") + 1))
        else NotAddressedToBot

    let isAdressedBot alias botId (message: ChatMessage) =
        if message.Text |> isNull then NotAddressedToBot
        else if isDirectMessage message then AddressedToBot message.Text
        else isDirectedToBot alias botId message

    let isMatchingCommand alias botId (event: Event) command =
        match command with
        | SpyCommand sc -> sc.EventMatcher event.Text event
        | SlackCommand sc ->
            match isAdressedBot alias botId event with
            | NotAddressedToBot -> 
                printfn "==> NotAddresedBot: %A" event
                false
            | AddressedToBot commandText ->
                printfn "==> AddressedBot: %A" commandText
                sc.EventMatcher commandText event

    let handleCommand slackApi event command =
        match command with
        | SpyCommand sc -> sc.EventHandler slackApi event
        | SlackCommand sc -> sc.EventHandler slackApi event

let getBotInfo token =
    let connectUrl = sprintf "https://slack.com/api/rtm.connect?token=%s" token
    printfn "Connecting to: %A" connectUrl
    connectUrl
    |> Http.downloadJsonObject<ConnectResponse>
    |> (fun x -> printfn "Connected: %A" x; x)

let startListen commands alias connectResponse slackApi =
    let deserializeEvent = Json.deserialize<Event>
    let execute (event: Event) (command: BotCommand) =
        async {
            printfn "==> Checking match: %A" event
            if command |> isMatchingCommand alias (connectResponse.Self.Id) event
            then
                printfn "==> Handling: %A" event
                command |> handleCommand slackApi event
        }

    let handleEvent commands (event: Event) =
        commands
        |> List.map (execute event)

    let handleSocketMessage messageString =
        messageString
        |> deserializeEvent
        |> handleEvent commands
        |> Async.Parallel
        |> Async.Ignore

    WebSocket.connect handleSocketMessage connectResponse.Url
    ()

let start config =
    match config.ApiToken with
    | Some apiToken ->
        let connectResponse = getBotInfo apiToken
        let slackApi = SlackApi.createSlackApi apiToken
        startListen config.Commands config.Alias connectResponse slackApi
    | None -> raise (exn "ApiToken is required for bot")
