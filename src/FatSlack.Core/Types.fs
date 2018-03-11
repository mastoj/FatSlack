module FatSlack.Core.Types
open FatSlack.Core
open System.Text.RegularExpressions

type EventParser = 
    | SimpleEventParser of string
    | RegexEventParser of Regex
    | JsonEventParser of string

type SimpleEventParseResult = string * string list
type JsonEventParseResult = string * string
type RegexEventParseResult = Match
type EventParseResult = 
    | SimpleEventParseResult of SimpleEventParseResult
    | JsonEventParseResult of JsonEventParseResult
    | RegexEventParseResult of RegexEventParseResult

type EventHandler = EventParseResult -> Domain.Types.Events.Event -> (Domain.Types.Actions.ActionMessage -> unit) -> unit

type CommandDefinition = 
    {
        Syntax: string
        Description: string
        EventParser: EventParser
        EventHandler: EventHandler
    }
    with 
        static member private createCommand parser handler syntax description  = 
            {
                Syntax = syntax
                Description = description
                EventParser = parser
                EventHandler = handler
            }

        static member createSimpleCommand handler parser = 
            CommandDefinition.createCommand (SimpleEventParser parser) (fun (SimpleEventParseResult h) -> handler h)
        static member createRegexCommand handler parser = 
            CommandDefinition.createCommand (RegexEventParser parser) (fun (RegexEventParseResult h) -> handler h)
        static member createJsonCommand handler parser = 
            CommandDefinition.createCommand (JsonEventParser parser) (fun (JsonEventParseResult h) -> handler h)

type ListenerDefinition = 
    {
        EventParser: EventParser
        EventHandler: EventHandler
    }
    with 
        static member private createListener parser handler =
            {
                EventParser = parser
                EventHandler = handler
            }
        static member createRegexListener handler parser = 
            ListenerDefinition.createListener (RegexEventParser parser) (fun (RegexEventParseResult h) -> handler h)

type BotConfiguration = 
    {
        Token: string
        Alias: string option
        Commands: CommandDefinition list
        Listeners: ListenerDefinition list
    }

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

type BotInformation = 
    {
        Configuration: BotConfiguration
        Team: Team
        User: SlackUser
        WebSocketUrl: string
    }

// type ActionRequest = 
//     {
//         Actions: Action list
//         Token: string
//         Channel: Channel
//         User: SlackUser
//         Original_message: Message
//     }
