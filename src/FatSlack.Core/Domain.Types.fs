module FatSlack.Core.Domain.Types
open SimpleTypes

module Events =

    type RegularMessage = {
        Channel: ChannelId
        User: UserId
        Text: Text
        Ts: Ts
    }

    type BotMessage = {
        Channel: ChannelId
        User: UserId
        Text: Text
        Ts: Ts
        BotId: BotId
        UserName: UserName
        Icons: Map<IconId, Url>
    }

    type Message =
        | RegularMessage of RegularMessage
        | BotMessage of BotMessage

    type Event =
        | Message of Message

module Actions =
    type Color =
        | Good
        | Warning
        | Danger
        | Custom of string

    type Style =
        | Default
        | Primary
        | Danger

    type Confirm = {
        Title: Title
        Text: Text
        OkText: Text
        DismissText: Text
    }

    type ValueButton = {
        Name: Name
        Text: Text
        Value: Value
        Style: Style
        Confirm: Confirm option
    }

    type LinkButton = {
        Name: Name
        Text: Text
        Url: Url
        Style: Style
        Confirm: Confirm option
    }

    type Option = {
        Text: Text
        Value: Value
    }

    type SelectedDataSource =
        | Users
        | Channel
        | Conversations
        | External

    type DataSourceSelect = {
        Name: Name
        Text: Text
        DataSource: SelectedDataSource
    }

    type OptionsSelect = {
        Name: Name
        Text: Text
        Options: Option list
        SelectedOptions: Option list
    }

    type Action =
        | ValueButton of ValueButton
        | LinkButton of LinkButton
        | DataSourceSelect of DataSourceSelect
        | OptionsSelect of OptionsSelect

    type FieldData = {
        Title: Title
        Value: Value
    }

    type Field =
        | ShortField of FieldData
        | LongField of FieldData

    type Attachment = {
        Title: Title
        Fallback: Fallback
        CallbackId: CallbackId
        Color: Color           // good, warning, danger, #439FE0
        ImageUrl: Url
        ThumbUrl: Url
        Actions: Action list
        Fields: Field list
    }

    type PostMessage = {
        Channel: ChannelId
        Text: Text
        IconEmoji: Emoji
        Attachment: Attachment
    }

    type UpdateMessage = {
        Channel: ChannelId
        Text: Text
        IconEmoji: Emoji
        Attachment: Attachment
        Ts: Ts
    }

    type ActionMessage =
        | PostMessage of PostMessage
        | UpdateMessage of UpdateMessage

module Workflow =
    type Handler = Events.Event -> Actions.ActionMessage