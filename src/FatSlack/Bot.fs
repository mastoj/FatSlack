module FatSlack.Bot

open Types
open Net
open Suave
open System.Text.RegularExpressions

[<AutoOpen>]
module Helpers =
    let startsWith first (str: string) = str.StartsWith(first)

    type MessageType =
        | NotAddressedToBot
        | AddressedToBot of commandText:string
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

[<AutoOpen>]
module Types =
    type ConnectResponse = 
        {
            Ok: bool 
            Url: Url
            Team: Team
            Self: User
        }

    type SpyCommand =
        {
            Description: string
            EventHandler: EventHandler
            EventMatcher: EventMatcher
        }

    type SlackCommand =
        {
            Syntax: string
            Description: string
            EventHandler: EventHandler
            EventMatcher: EventMatcher
        }

    type Command =
        | SpyCommand of SpyCommand
        | SlackCommand of SlackCommand
        with 
            member this.isMatch alias botId event =
                match this with
                | SpyCommand sc -> sc.EventMatcher event.Text event
                | SlackCommand sc ->
                    match isAdressedBot alias botId event with
                    | NotAddressedToBot -> 
                        printfn "==> NotAddresedBot: %A" event
                        false
                    | AddressedToBot commandText ->
                        printfn "==> AddressedBot: %A" commandText
                        sc.EventMatcher commandText event
            member this.handle event =
                match this with
                | SpyCommand sc -> sc.EventHandler event
                | SlackCommand sc -> sc.EventHandler event

    type BotConfiguration =
        {
            Alias: string option
            Token: string
            Commands: Command list
        }

let init token =
    {
        Token = token
        Commands = []
        Alias = None
    }


let withCommand command config = { config with Commands = command::config.Commands }

let withCommands commands config = { config with Commands = commands @ config.Commands }

let withSlackCommand = SlackCommand >> withCommand

let withSlackCommands = (List.map SlackCommand) >> withCommands

let withSpyCommand = SpyCommand >> withCommand

let withSpyCommands = (List.map SpyCommand) >> withCommands

let withAlias alias config = { config with Alias = Some alias }

let getBotInfo token =
    let connectUrl = sprintf "https://slack.com/api/rtm.connect?token=%s" token
    printfn "Connecting to: %A" connectUrl
    connectUrl
    |> Http.downloadJsonObject<ConnectResponse>
    |> (fun x -> printfn "Connected: %A" x; x)

let startListen commands alias connectResponse slackApi =
    let deserializeEvent = Json.deserialize<Event>
    let execute event (command: Command) =
        async {
            printfn "==> Checking match: %A" event
            if command.isMatch alias (connectResponse.Self.Id) event
            then
                printfn "==> Handling: %A" event
                command.handle slackApi event
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
    let connectResponse = getBotInfo config.Token
    let slackApi = SlackApi.createSlackApi config.Token
    startListen config.Commands config.Alias connectResponse slackApi
