module FatSlack.Types

open Newtonsoft.Json
open System

type Text = string
type Ts = string
type UserId = string
type ChannelId = string
type Url = string
type TeamId = string
type Domain = string
type TriggerId = string
type CallbackId = string
type Title = string
type Name = string
type Label = string
type Value = string

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
    with
        static member defaultValue name actionType =
            {
                Name = name
                Type = actionType
                Text = null
                Style = null
                Value = null
                Confirm = Unchecked.defaultof<Confirm>
                Options = []
                SelectedOptions = []
                DataSource = null
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
    with
        static member defaultValue callBackId = 
            {
                Fallback = null
                CallbackId = callBackId
                Color = null
                Pretext = null
                AttachmentType = null
                AuthorName = null
                AuthorLink = null
                AuthorIcon = null
                Title = null
                TitleLink = null
                Text = null
                Fields = []
                ImageUrl = null
                ThumbUrl = null
                Footer = null
                FooterIcon = null
                Ts = null
                Actions = []
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
    with 
        static member defaultValue =
            {
                Type = null
                Subtype = null
                Channel = null
                User = null
                Text = null
                Ts = null
                Edited = Unchecked.defaultof<Edited>
                DeletedTs = null
                EventTs = null
                IsStarred = false
                PinnedTo = []
                Reactions = []
                Attachments = []
            }

type ElementOption =
    {
        Label: Label
        Value: Value
    }
type Element =
    {
        Label: string
        Name: Name
        Type: string
        Subtype: string
        Placeholder: string
        Optional: bool
        [<JsonProperty("max_length")>]MaxLength: Nullable<int>
        [<JsonProperty("min_length")>]MinLength: Nullable<int>
        Hint: string
        Value: string
        Options: ElementOption list
    }
    with
        static member defaultValue name elemType label =
            {
                Type = elemType
                Name = name
                Label = label
                Subtype = null
                Placeholder = null
                Optional = false
                MaxLength = Nullable<int>()
                MinLength = Nullable<int>()
                Hint = null
                Value = null
                Options = []
            }

type Dialog =
    {
        [<JsonProperty("callback_id")>]CallbackId: CallbackId
        Title: Title
        [<JsonProperty("submit_label")>]SubmitLabel: string
        Elements: Element list
    }
    with
        static member defaultValue callbackId title =
            {
                CallbackId = callbackId
                Title = title
                SubmitLabel = null
                Elements = []
            }

type DialogMessage =
    {
        [<JsonProperty("trigger_id")>]TriggerId: TriggerId
        Dialog: Dialog
    }
    with
        static member defaultValue triggerId dialog =
            {
                TriggerId = triggerId
                Dialog = dialog
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

type DialogSubmission =
    {
        Type: string
        Submission: Map<string, string>
        [<JsonProperty("callback_id")>]CallbackId: CallbackId
        Team: Team
        User: User
        Channel: Channel
        [<JsonProperty("action_ts")>]ActionTs: Ts
        Token: string
        [<JsonProperty("response_url")>]ResponseUrl: Url
    }

type Message =
    | PostMessage of ChatMessage
    | UpdateMessage of ChatMessage
    | DialogMessage of DialogMessage

type ChatResponseMessage =
    {
        Ok: bool
        Channel: ChannelId
        Ts: Ts
        Message: ChatMessage
    }

type Payload = string

type SlackRequest =
    | InteractiveMessage of InteractiveMessage
    | DialogSubmission of DialogSubmission

type SlackRequestError =
    | InvalidToken
    | MissingToken
    | FailedToParsePayload
    | UnknownSlackRequest
    | FailedToHandleRequest of SlackRequest

type Event = ChatMessage
type CommandText = Text

type SlackApi = Message -> Async<ChatResponseMessage>
type EventHandler = SlackApi -> Event -> unit
type EventMatcher = CommandText -> Event -> bool
type RequestHandler = SlackApi -> Payload -> Result<string, SlackRequestError>
type AppHandler = SlackApi -> SlackRequest -> Result<string, SlackRequestError>
