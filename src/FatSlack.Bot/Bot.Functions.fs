module FatSlack.Bot.Functions
open System
open System.Text.RegularExpressions
open Types
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open FatSlack.Core
open FatSlack.Core.Domain.SimpleTypes
open FatSlack.Core.Domain.Types
open FatSlack.Core.Domain.Types.Events
open FatSlack.Core.Domain.Types.Actions
open FatSlack.Core.Api.Dto.Actions
open FatSlack.Core.Net
open FatSlack.Core.SlackApi

let isAdressedBot alias (UserId botId) (message: RegularMessage) = 
    let checkPattern text pattern = 
        let regexPattern = "^" + pattern
        let regex = Regex(regexPattern)
        regex.IsMatch(text)

    let idPattern = "<@" + botId
    let patternsToLookFor = 
        alias
        |> Option.fold (fun patterns alias -> alias :: patterns) [idPattern]
    patternsToLookFor
    |> List.exists (checkPattern message.Text.value)

let getMessageType alias (UserId botId) (message: Events.Message) =
    match message with
    | BotMessage _ -> MessageType.BotMessage
    | RegularMessage m ->
        let (Text text) = m.Text
        let (ChannelId channelId) = m.Channel
        let startsWith first (str: string) = str.StartsWith(first)
        if String.IsNullOrWhiteSpace(text)
        then NotAddressedToBot
        else if isAdressedBot alias (UserId botId) m 
        then Directed
        else if channelId |> startsWith "D"
        then DirectMessage 
        else if channelId |> startsWith "G"
        then GroupMessage
        else NotAddressedToBot

module Async =
    let joinAsyncSeq<'T> (state: Async<'T seq>) (item: Async<'T seq>) : Async<'T seq>=
        async {
            let! stateSeq = state
            let! itemSeq = item
            let res = itemSeq |> Seq.append stateSeq
            printfn "This is the sequences I'm returning: %A" res
            return res
        }

    let emptyAsyncSeq<'T> : Async<'T seq> =
        async {
            return Seq.empty
        }

let eventHandler (commands: Command list): EventHandler =
    fun event ->
        commands 
            |> List.map (fun c -> c.MatchEvent event)
            |> List.choose id
            |> Seq.map (fun (handler, event) -> printfn "Hello, here I am"; handler event)
            |> Seq.fold Async.joinAsyncSeq Async.emptyAsyncSeq

let executor commands : Executor =
    fun sendMessage event ->
        let actionMessages =
            event
            |> eventHandler commands
        let onOk x =
            printfn "I deal with this: %A" x
            x
            |> Seq.iter (fun m -> m |> sendMessage |> Async.Start)
            |> ignore
        let onFail = printfn "Send message failed: %A"
        let onCancel = printfn "Send message cancelled: %A"
        Async.StartWithContinuations (actionMessages, onOk, onFail, onCancel)

let connectBot: ConnectBot =
    fun botConfig ->
        sprintf "https://slack.com/api/rtm.connect?token=%s" botConfig.Token
        |> Http.downloadJsonObject<ConnectResponse>
        |> (fun cr -> 
            {
                Token = botConfig.Token
                Alias = botConfig.Alias
                Executor = executor botConfig.Commands
                Team = cr.Team
                User = cr.Self
                WebSocketUrl = cr.Url
            })

let deserializeEvent (json:string) = 
    let jObject = JObject.Parse(json)
    try
        if jObject.["type"] |> isNull 
        then Result.Error (Errors.Error.UnsupportedSlackEvent "null")
        else
            match jObject.["type"].ToString() with
            | "message" ->
                (Json.deserialize<Api.Dto.Events.Message>(json))
                |> Result.Ok
            | x ->
                Result.Error (Errors.Error.UnsupportedSlackEvent x)
    with
    | x -> 
        printfn "%A" x
        Result.Error (Errors.Error.JsonError (x.ToString()))


let startListen: StartListen =
    fun botSpec ->
        let sendMessage = (fun m -> printfn "Sending: %A" m; m) >> (ActionMessage.toSlackAction botSpec.Token) >> send

        let handle messageString =
            messageString
            |> deserializeEvent
            |> Result.bind (Api.Dto.Events.Message.toDomainType)
            |> Result.map (Domain.Types.Events.Event.Message)
            |> Result.map (botSpec.Executor sendMessage)

        let handleAsync messageString =
            async {
                do handle messageString
            }
        WebSocket.connect handleAsync botSpec.WebSocketUrl

let init token = 
    {
        Token = token
        Alias = None
        Commands = []
    }

let withAlias alias botConfig : BotConfig = { botConfig with Alias = Some alias }

let withCommand command botConfig = { botConfig with Commands = command :: botConfig.Commands}

let start config = 
    config
    |> connectBot
    |> startListen
