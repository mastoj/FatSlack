namespace FatSlack.Core.Api.Dto
open System
open System.Collections.Generic
open FatSlack.Core.Domain
open FatSlack.Core.Errors
open FatSlack.Core.Domain.Types
open FatSlack.Core.Domain.Types.Actions

module Events =
    
    type ChannelId = string
    type UserId = string
    type BotId = string
    type Username = string

    type Edited = {
        User: string
        Ts: string
    }

    type Reaction = {
        Name: string
        Count: int
        Users: UserId list
    }

    type Message = {
        Type: string
        Channel: ChannelId
        User: UserId
        Text: string
        Ts: string

        Subtype: string
        Hidden: Nullable<bool>
        Edited: Edited
        Is_starred: Nullable<bool>
        Deleted_ts: string
        Event_ts: string
        Pinned_to: ChannelId[]
        Reactions: Reaction[]
        Icons: Dictionary<string, string>
        Bot_id: BotId
        Username: Username
    }

    module Ts =
        let toDomainType ts =
            ts |> SimpleTypes.Ts

    module ChannelId =
        let toDomainType channelId =
            channelId |> SimpleTypes.ChannelId

    module UserId =
        let toDomainType userId =
            userId |> SimpleTypes.UserId

    module Text =
        let toDomainType text =
            text |> SimpleTypes.Text

    module BotId =
        let toDomainType text =
            text |> SimpleTypes.BotId

    module UserName =
        let toDomainType text =
            text |> SimpleTypes.UserName

    module IconId =
        let toDomainType text =
            text |> SimpleTypes.IconId

    module Url =
        let toDomainType text =
            text |> SimpleTypes.Url

    module Message =
        let toBotMessage messageDto =
            let toIconMap (iconDict: Dictionary<string, string>) =
                if iconDict |> isNull then Map.empty
                else
                    iconDict
                    |> Seq.map (fun kv -> kv.Key |> IconId.toDomainType, kv.Value |> Url.toDomainType)
                    |> Map.ofSeq

            Types.Events.BotMessage {
                Channel = (messageDto.Channel |> ChannelId.toDomainType)
                User = (messageDto.User |> UserId.toDomainType)
                Text = (messageDto.Text |> Text.toDomainType)
                Ts = (messageDto.Ts |> Ts.toDomainType)
                BotId = (messageDto.Bot_id |> BotId.toDomainType)
                UserName = (messageDto.Username |> UserName.toDomainType)
                Icons = toIconMap messageDto.Icons
            }

        let toRegularMessage messageDto =
            Types.Events.RegularMessage {
                Channel = (messageDto.Channel |> ChannelId.toDomainType)
                User = (messageDto.User |> UserId.toDomainType)
                Text = (messageDto.Text |> Text.toDomainType)
                Ts = (messageDto.Ts |> Ts.toDomainType)
            }

        let toDomainType messageDto =
            match messageDto.Subtype with
            | "bot_message" -> toBotMessage messageDto |> Result.Ok
            | "" | null -> toRegularMessage messageDto |> Result.Ok
            | _ -> Result.Error (SerializationError (sprintf "Unkown message: %A" messageDto))

module Actions =

    let mapListToArray map list =
        list |> List.map map |> List.toArray

    type Method =
        | PostMessage
        | UpdateMessage

    type FormValues = (string * string) list
    type SlackActionData<'T> =
        | FormValues of FormValues
        | Dto of 'T

    type SlackAction<'T> = {
        Method: Method
        Data: SlackActionData<'T>
    }

    type Confirm = {
        Title: string
        Text: string
        Ok_text: string
        Dismiss_text: string
    }

    type Option = {
        Text: string
        Value: string
    }

    type Action = {
        Type: string // button, select
        Name: string
        Text: string
        Url: string  // ValueButton
        Value: string // LinkButton
        Confirm: Confirm
        Style: string // default, primary, danger
        Options: Option[]
        Selected_options: Option[]
        Data_source: string // users, channel, conversations, external
    } with static member defaultAction = {
            Type = null // button, select
            Name = null
            Text = null
            Url = null  // ValueButton
            Value = null // LinkButton
            Confirm = Unchecked.defaultof<Confirm>
            Style = null // default, primary, danger
            Options = [||]
            Selected_options = [||]
            Data_source = null // users, channel, conversations, external
        }

    type Field = {
        Title: string
        Value: string
        Short: bool
    }

    type Attachment = {
        Title: string
        Fallback: string
        Callback_id: string
        Color: string           // good, warning, danger, #439FE0
        Image_url: string
        Thumb_url: string
        Actions: Action[]
        Fields: Field[]
    }

    type Message = {
        Token: string
        Channel: string
        Text: string
        Icon_emoji: string
        Attachment: Attachment
        Ts: string
    }

    module ChannelId =
        let toDto (SimpleTypes.ChannelId value) = value

    module Text =
        let toDto (SimpleTypes.Text value) = value

    module IconEmoji =
        let toDto (SimpleTypes.Emoji value) = value

    module Title =
        let toDto (SimpleTypes.Title value) = value

    module Fallback =
        let toDto (SimpleTypes.Fallback value) = value

    module CallbackId =
        let toDto (SimpleTypes.CallbackId value) = value

    module Color =
        let toDto (color: Actions.Color) =
            match color with
            | Good -> "good"
            | Warning -> "warning"
            | Color.Danger -> "danger"
            | Custom hexString -> hexString

    module Url =
        let toDto (SimpleTypes.Url value) = value

    module Name =
        let toDto (SimpleTypes.Name value) = value

    module Value =
        let toDto (SimpleTypes.Value value) = value

    module Confirm =
        let toDto (confirm: Types.Actions.Confirm) =
            {
                Title = (confirm.Text |> Text.toDto)
                Text = (confirm.Title |> Title.toDto)
                Ok_text = (confirm.OkText |> Text.toDto)
                Dismiss_text = (confirm.DismissText |> Text.toDto)
            }
    module Style =
        let toDto (style: Style) =
            match style with
            | Default -> "default"
            | Primary -> "primary"
            | Danger -> "danger"

    module ValueButton =
        let toDto (valueButton: ValueButton) =
            let confirm = 
                valueButton.Confirm 
                |> Option.map Confirm.toDto 
                |> Option.defaultValue (Unchecked.defaultof<Confirm>)
            {
                Action.defaultAction with
                    Type = "button"
                    Name = (valueButton.Name |> Name.toDto)
                    Text = (valueButton.Text |> Text.toDto)
                    Value = (valueButton.Value |> Value.toDto)
                    Confirm = confirm
                    Style = (valueButton.Style |> Style.toDto)
            }

    module LinkButton =
        let toDto (linkButton: LinkButton) =
            let confirm = 
                linkButton.Confirm 
                |> Option.map Confirm.toDto 
                |> Option.defaultValue (Unchecked.defaultof<Confirm>)
            {
                Action.defaultAction with
                    Type = "button"
                    Name = (linkButton.Name |> Name.toDto)
                    Text = (linkButton.Text |> Text.toDto)
                    Url = (linkButton.Url |> Url.toDto)
                    Confirm = confirm
                    Style = (linkButton.Style |> Style.toDto)
            }

    module DataSource =
        let toDto (ds: SelectedDataSource) =
            match ds with
            | Users -> "users"
            | Channel -> "channel"
            | Conversations -> "conversations"
            | External -> "external"

    module DataSourceSelect =
        let toDto (dsSelect: DataSourceSelect) =
            {
                Action.defaultAction with
                    Type = "select"
                    Name = (dsSelect.Name |> Name.toDto)
                    Text = (dsSelect.Text |> Text.toDto)
                    Data_source = (dsSelect.DataSource |> DataSource.toDto)
            }

    module Option =
        let toDto (option: Types.Actions.Option) =
            {
                Text = (option.Text |> Text.toDto)
                Value = (option.Value |> Value.toDto)
            }

    module OptionsSelect =
        let toDto (dsSelect: OptionsSelect) =
            {
                Action.defaultAction with
                    Type = "select"
                    Name = (dsSelect.Name |> Name.toDto)
                    Text = (dsSelect.Text |> Text.toDto)
                    Options = (dsSelect.Options |> mapListToArray Option.toDto)
                    Selected_options = (dsSelect.SelectedOptions |> mapListToArray Option.toDto)
            }

    module Action =
        let toDto (action: Actions.Action) =
            match action with
            | ValueButton valueButton -> ValueButton.toDto valueButton
            | LinkButton linkButton -> LinkButton.toDto linkButton
            | DataSourceSelect dsSelect -> DataSourceSelect.toDto dsSelect
            | OptionsSelect optSelect -> OptionsSelect.toDto optSelect

    module Field =
        let toDto (field: Types.Actions.Field) =
            match field with
            | ShortField f ->
                {
                    Title = (f.Title |> Title.toDto)
                    Value = (f.Value |> Value.toDto)
                    Short = true
                }
            | LongField f ->
                {
                    Title = (f.Title |> Title.toDto)
                    Value = (f.Value |> Value.toDto)
                    Short = false
                }

    module Attachment =
        let toDto (attachment: Actions.Attachment) =
            {
                Title = (attachment.Title |> Title.toDto)
                Fallback = (attachment.Fallback |> Fallback.toDto)
                Callback_id = (attachment.CallbackId |> CallbackId.toDto)
                Color = (attachment.Color |> Color.toDto)
                Image_url = (attachment.ImageUrl |> Url.toDto)
                Thumb_url = (attachment.ThumbUrl |> Url.toDto)
                Actions = (attachment.Actions |> mapListToArray Action.toDto)
                Fields = (attachment.Fields |> mapListToArray Field.toDto)
            }

    module PostMessage =
        let toDto token (postMessage: Actions.PostMessage) =
            {
                Token = token
                Channel = (postMessage.Channel |> ChannelId.toDto)
                Text = (postMessage.Text |> Text.toDto)
                Icon_emoji = (postMessage.IconEmoji |> IconEmoji.toDto)
                Attachment = (postMessage.Attachment |> Attachment.toDto)
                Ts = null
            }

    module Ts =
        let toDto (SimpleTypes.Ts ts) = ts

    module UpdateMessage =
        let toDto token (message: Actions.UpdateMessage) =
            {
                Token = token
                Channel = (message.Channel |> ChannelId.toDto)
                Text = (message.Text |> Text.toDto)
                Icon_emoji = (message.IconEmoji |> IconEmoji.toDto)
                Attachment = (message.Attachment |> Attachment.toDto)
                Ts = (message.Ts |> Ts.toDto)
            }

    module ActionMessage =

        let toSlackAction token (action: Actions.ActionMessage) =
            match action with
            | Actions.PostMessage msg -> 
                let method = Method.PostMessage
                let data = Dto (PostMessage.toDto token msg)
                {
                    Method = method
                    Data = data
                }
            | Actions.UpdateMessage msg ->
                let method = Method.UpdateMessage
                let data = Dto (UpdateMessage.toDto token msg)
                {
                    Method = method
                    Data = data
                }
