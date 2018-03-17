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
type Handler = Event -> ActionMessage seq
type ActiveCommand = {
    Syntax: string
    Description: string
    EventMatcher: EventMatcher
    Handler: Handler
}
type PassiveCommand = {
    Description: string
    Handler: Handler
}
type Command =
    | ActiveCommand of ActiveCommand
    | PassiveCommand of PassiveCommand
    with 
        member this.MatchEvent event =
            match this with
            | ActiveCommand c -> 
                event |> c.EventMatcher |> Option.map (fun e -> c.Handler, e)
            | PassiveCommand c -> Some (c.Handler, event)

type Alias = string
// type BotSpecification = {
//     Token: string
//     Alias: string option
//     Commands: Command list
// }

type GetMessageType = Alias option -> UserId -> Message -> MessageType
type EventHandler = Event -> ActionMessage seq
type SendMessage = ActionMessage -> Async<unit>
type Executor = EventHandler -> SendMessage -> Event -> unit
