module FatSlack.Bot.Functions
open System
open System.Text.RegularExpressions
open Types
open FatSlack.Core.Domain.SimpleTypes
open FatSlack.Core.Domain.Types.Events
open FatSlack.Core.Domain.Types.Actions

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

let getMessageType alias (UserId botId) (message: Message) =
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
            return (itemSeq |> Seq.append stateSeq)
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
        |> Seq.collect (fun (handler, event) -> handler event)

let executor : Executor =
    fun eventHandler sendMessage event ->
        event
        |> eventHandler
        |> Seq.map sendMessage
        |> Seq.iter Async.Start
