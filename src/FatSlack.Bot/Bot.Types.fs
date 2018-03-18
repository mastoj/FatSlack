module FatSlack.Bot.Types

open FatSlack.Core.Domain.Types.Events
open FatSlack.Core.Domain.Types.Actions
open FatSlack.Core.Domain.SimpleTypes
open System

type MessageType = 
    | Directed
    | DirectMessage
    | GroupMessage
    | NotAddressedToBot
    | BotMessage

type EventMatcher = Event -> Event option
type EventHandler = Event -> Async<ActionMessage seq>
type ActiveCommand = {
    Syntax: string
    Description: string
    EventMatcher: EventMatcher
    EventHandler: EventHandler
}
type PassiveCommand = {
    Description: string
    EventHandler: EventHandler
}
type Command =
    | ActiveCommand of ActiveCommand
    | PassiveCommand of PassiveCommand
    with 
        member this.MatchEvent event =
            match this with
            | ActiveCommand c -> 
                event |> c.EventMatcher |> Option.map (fun e -> c.EventHandler, e)
            | PassiveCommand c -> Some (c.EventHandler, event)

type Alias = string
type Token = string

type Team =
    {
        Id: string
        Name: string
        Domain: string
    }

type SlackUser =
    {
        Id: string
        Name: string
    }

type ConnectResponse = {
    Ok: bool 
    Url: string
    Team: Team
    Self: SlackUser
}

type SendMessage = ActionMessage -> Async<unit>
type Executor = SendMessage -> Event -> unit
type BotSpecification = {
    Token: Token
    Alias: Alias option
    Executor: Executor
    Team: Team
    User: SlackUser
    WebSocketUrl: string
}

type BotConfig =
    {
        Token: Token
        Alias: Alias option
        Commands: Command list
    }

type ConnectBot = BotConfig -> BotSpecification
type StartListen = BotSpecification -> unit
type GetMessageType = Alias option -> UserId -> Message -> MessageType
