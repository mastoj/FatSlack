module FatSlack.Core.Types
open System.Text.RegularExpressions

type Team = 
    {
        Id: string
        Name: string
        Domain: string
    }


type Channel = 
    {
        Id: string
        Name: string
    }


type SlackUser = 
    {
        Id: string
        Name: string
    }

type MessageEvent = 
    {
        Type: string
        Channel: string
        User: string
        Text: string
        Ts: string
        Source_team: string
        Team: string
    }

type SlackEvent = 
    | MessageEvent of MessageEvent

type Action = 
    {
        Name: string
        Text: string
        Type: string
        Value: string
    }
    with 
        static member button name text value = 
            {
                Name = name
                Text = text
                Value = value
                Type = "button"
            }

type Field = 
    {
        Title: string
        Value: string
        Short: bool
    }
    with 
        static member createField short title value = 
            {
                Title = title
                Value = value
                Short = short
            }
        static member createLongField = Field.createField false
        static member createShortField = Field.createField true

type Attachment = 
    {
        Title: string
        Fallback: string
        Callback_id: string
        Color: string
        Actions: Action list
        Attachment_type: string
        Fields: Field list
    }
    with
        static member create callbackId fallback = 
            { 
                Title = ""
                Fallback = fallback
                Callback_id = callbackId
                Color = ""
                Actions = []
                Fields = []
                Attachment_type = "default"
            }
        static member withAction action this = { this with Actions = action :: this.Actions }
        static member withField field this = { this with Fields = field :: this.Fields }
        static member withFields fields this = { this with Fields = fields @ this.Fields }
        static member withColor color this = { this with Color = color }
        static member withTitle title this = { this with Title = title }

type MessageBaseData =
    {
        Type: string
        Channel: string
        Text: string
        Attachments: Attachment list
    }

type PostMessage =
    {
        MessageBaseData: MessageBaseData
    }

type UpdateMessage =
    {
        MessageBaseData: MessageBaseData
        Timestamp: string
    }

type Message =
    | PostMessage of PostMessage
    | UpdateMessage of UpdateMessage
    with
        static member createMessageBaseData channelName text =
            {
                Type = "message"
                Channel = channelName
                Text = text
                Attachments = []
            }
        static member createPostMessage channelName text =
            Message.createMessageBaseData channelName text 
            |> (fun mbd -> PostMessage { MessageBaseData = mbd }) 
        static member createUpdateMessage channelName text ts =
            Message.createMessageBaseData channelName text
            |> (fun mbd -> UpdateMessage { MessageBaseData = mbd; Timestamp = ts })

        static member withAttachment attachment this =
            let updateBaseData baseData =
                { baseData with Attachments = attachment :: baseData.Attachments }
            match this with
            | PostMessage pm -> 
                let baseData = updateBaseData pm.MessageBaseData
                PostMessage { pm with  MessageBaseData = baseData }
            | UpdateMessage um ->
                let baseData = updateBaseData um.MessageBaseData
                UpdateMessage { um with  MessageBaseData = baseData }

type MessageSender = Message -> unit

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

type EventHandler = EventParseResult -> MessageEvent -> MessageSender -> unit

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

type BotInformation = 
    {
        Configuration: BotConfiguration
        Team: Team
        User: SlackUser
        WebSocketUrl: string
    }

type ActionRequest = 
    {
        Actions: Action list
        Token: string
        Channel: Channel
        User: SlackUser
        Original_message: Message
    }