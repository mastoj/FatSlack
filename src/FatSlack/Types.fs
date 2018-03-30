module FatSlack.Types

open Newtonsoft.Json

type Text = string
type Ts = string
type UserId = string
type ChannelId = string
type Url = string
type TeamId = string
type Domain = string
type TriggerId = string
type CallbackId = string

type User =
    {
        Id: UserId
        Name: string
    }
type Team =
    {
        Id: TeamId
        Domain: Domain
    }

type Channel =
    {
        Id: ChannelId
        Name: string
    }

type Edited =
    {
        User: UserId
        Ts: Ts
    }

type Reaction =
    {
        Name: string
        Count: int
        Users: UserId list
    }

type Field =
    {
        Title: string
        Value: string
        Short: bool
    }

type Confirm =
    {
        Title: string
        Text: string
        [<JsonProperty("ok_text")>]OkText: string
        [<JsonProperty("dismiss_text")>]DismissText: string
    }

type Option =
    {
        Text: string
        Value: string
    }

type Action =
    {
        Name: string
        Text: string
        Style: string
        Type: string
        Value: string
        Confirm: Confirm
        Options: Option list
        [<JsonProperty("selected_options")>]SelectedOptions: Option list
        [<JsonProperty("data_source")>]DataSource: string
    }

type Attachment =
    {
        Fallback: string
        [<JsonProperty("callback_id")>]CallbackId: CallbackId
        Color: string
        Pretext: string
        [<JsonProperty("attachment_type")>]AttachmentType: string
        [<JsonProperty("author_name")>]AuthorName: string
        [<JsonProperty("author_link")>]AuthorLink: Url
        [<JsonProperty("author_icon")>]AuthorIcon: Url
        Title: string
        [<JsonProperty("title_link")>]TitleLink: string
        Text: string
        Fields: Field list
        [<JsonProperty("image_url")>]ImageUrl: Url
        [<JsonProperty("thumb_url")>]ThumbUrl: Url
        Footer: string
        [<JsonProperty("footer_icon")>]FooterIcon: Url
        Ts: Ts
        Actions: Action list
    }

type ChatMessage =
    {
        Type: string
        Subtype: string
        Channel: ChannelId
        User: UserId
        Text: Text
        Ts: Ts
        Edited: Edited
        [<JsonProperty("deleted_ts")>]DeletedTs: Ts
        [<JsonProperty("event_ts")>]EventTs: Ts
        [<JsonProperty("is_starred")>]IsStarred: bool
        [<JsonProperty("pinned_to")>]PinnedTo: ChannelId list
        Reactions: Reaction list
        Attachments: Attachment list
    }

type InteractiveMessage =
    {
        Type: string
        Actions: Action list
        [<JsonProperty("callback_id")>]CallbackId: CallbackId
        Team: Team
        Channel: Channel
        User: User
        [<JsonProperty("action_ts")>]ActionTs: Ts
        [<JsonProperty("message_ts")>]MessageTs: Ts
        [<JsonProperty("attachment_id")>]AttachmentId: string
        Token: string
        [<JsonProperty("original_message")>]OriginalMessage: ChatMessage
        [<JsonProperty("response_url")>]ResponseUrl: Url
        [<JsonProperty("trigger_id")>]TriggerId: TriggerId
    }

type ChatResponseMessage =
    {
        Ok: bool
        Channel: ChannelId
        Ts: Ts
        Message: ChatMessage
    }

type Event = ChatMessage

type PostMessage = ChatMessage -> Async<ChatResponseMessage>
type UpdateMessage = ChatMessage -> Async<ChatResponseMessage>

type EventHandler = Event -> unit

type SlackApi =
    {
        PostMessage: PostMessage
        UpdateMessage: UpdateMessage
    }

//type EventHan

// open FatSlack
// open System.Text.RegularExpressions
// open FatSlack.Domain.Types.Events

// type EventParser = 
//     | SimpleEventParser of string
//     | RegexEventParser of Regex
//     | JsonEventParser of string

// type SimpleEventParseResult = string * string list
// type JsonEventParseResult = string * string
// type RegexEventParseResult = Match
// type EventParseResult = 
//     | SimpleEventParseResult of SimpleEventParseResult
//     | JsonEventParseResult of JsonEventParseResult
//     | RegexEventParseResult of RegexEventParseResult

// type EventHandler = EventParseResult -> Domain.Types.Events.Event -> (Domain.Types.Actions.ActionMessage -> unit) -> unit

// let createRegularHandler h : EventHandler =
//     fun eventParseResult event callback ->
//         match event with
//         | Domain.Types.Events.Message (RegularMessage m) ->
//             h eventParseResult m callback
//         | _ -> raise (exn "Should never get a non RegularMessage here")

// let createSimpleParser parser = SimpleEventParser parser

// type CommandDefinition = 
//     {
//         Syntax: string
//         Description: string
//         EventParser: EventParser
//         EventHandler: EventHandler
//     }
//     with 
//         static member private createCommand parser handler syntax description  = 
//             {
//                 Syntax = syntax
//                 Description = description
//                 EventParser = parser
//                 EventHandler = handler
//             }
//         static member createRegularHandler h : EventHandler =
//             fun eventParseResult event callback ->
//                 match event with
//                 | Domain.Types.Events.Message (RegularMessage m) ->
//                     h eventParseResult m callback
//                 | _ -> raise (exn "Should never get a non RegularMessage here")

//         static member createSimpleCommand handler parser = 
//             CommandDefinition.createCommand (SimpleEventParser parser) (fun (SimpleEventParseResult h) -> handler h)
//         static member createRegexCommand handler parser = 
//             CommandDefinition.createCommand (RegexEventParser parser) (fun (RegexEventParseResult h) -> handler h)
//         static member createJsonCommand handler parser = 
//             CommandDefinition.createCommand (JsonEventParser parser) (fun (JsonEventParseResult h) -> handler h)

// type ListenerDefinition = 
//     {
//         EventParser: EventParser
//         EventHandler: EventHandler
//     }
//     with 
//         static member private createListener parser handler =
//             {
//                 EventParser = parser
//                 EventHandler = handler
//             }
//         static member createRegexListener handler parser = 
//             ListenerDefinition.createListener (RegexEventParser parser) (fun (RegexEventParseResult h) -> handler h)

// type BotConfiguration = 
//     {
//         Token: string
//         Alias: string option
//         Commands: CommandDefinition list
//         Listeners: ListenerDefinition list
//     }

// type Team =
//     {
//         Id: string
//         Name: string
//         Domain: string
//     }

// type SlackUser =
//     {
//         Id: string
//         Name: string
//     }

// type BotInformation = 
//     {
//         Configuration: BotConfiguration
//         Team: Team
//         User: SlackUser
//         WebSocketUrl: string
//     }

// // type ActionRequest = 
// //     {
// //         Actions: Action list
// //         Token: string
// //         Channel: Channel
// //         User: SlackUser
// //         Original_message: Message
// //     }
