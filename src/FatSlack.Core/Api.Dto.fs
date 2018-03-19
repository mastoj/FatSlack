namespace FatSlack.Core.Api.Dto
open System
open System.Collections.Generic
open FatSlack.Core.Domain
open FatSlack.Core.Errors
open FatSlack.Core.Domain.Types
open FatSlack.Core.Domain.Types.Actions
open FatSlack.Core.Domain.SimpleTypes

[<AutoOpen>]
module Helpers =
    let emptyStringIfNull (str: string) =
        if str |> isNull then "" else str

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

    module Emoji =
        let toDomainType emoji = emoji |> SimpleTypes.Emoji


    module Message =
        let toBotMessage (messageDto: Message) =
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

        let toRegularMessage (messageDto: Message) =
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

    let mapArrayToList map array =
        if array |> isNull then []
        else array |> List.ofArray |> List.map map

    type Method =
        | Post
        | Update

    type FormValues = (string * string) list
    type SlackActionData =
        | FormValues of FormValues
        | Dto of obj

    type SlackAction= {
        Method: Method
        Data: SlackActionData
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
        Channel: string
        Text: string
        Icon_emoji: string
        Attachments: Attachment[]
        Ts: string
    }

    module ChannelId =
        let toDto (SimpleTypes.ChannelId value) = value

    module Text =
        let toDto (SimpleTypes.Text value) = value
        let toDomainType = emptyStringIfNull >> SimpleTypes.Text

    module IconEmoji =
        let toDto (SimpleTypes.Emoji value) = value

    module Title =
        let toDto (SimpleTypes.Title value) = value
        let toDomainType = SimpleTypes.Title

    module Fallback =
        let toDto (SimpleTypes.Fallback value) = value
        let toDomainType = emptyStringIfNull >> SimpleTypes.Fallback

    module CallbackId =
        let toDto (SimpleTypes.CallbackId value) = value
        let toDomainType = emptyStringIfNull >> SimpleTypes.CallbackId

    module Color =
        let toDto (color: Actions.Color) =
            match color with
            | Good -> "good"
            | Warning -> "warning"
            | Color.Danger -> "danger"
            | Custom hexString -> hexString

        let toDomainType (color: string) =
            if color |> isNull then Custom ""
            else
                match color with
                | "good" -> Good
                | "warning" -> Warning
                | "danger" -> Color.Danger
                | hexString -> Custom hexString

    module Url =
        let toDto (SimpleTypes.Url value) = value
        let toDomainType = emptyStringIfNull >> SimpleTypes.Url

    module Name =
        let toDto (SimpleTypes.Name value) = value
        let toDomainType = emptyStringIfNull >> SimpleTypes.Name

    module Value =
        let toDto (SimpleTypes.Value value) = value
        let toDomainType = emptyStringIfNull >> SimpleTypes.Value

    module Confirm =
        let toDto (confirm: Actions.Confirm) =
            {
                Title = (confirm.Title |> Title.toDto)
                Text = (confirm.Text |> Text.toDto)
                Ok_text = (confirm.OkText |> Text.toDto)
                Dismiss_text = (confirm.DismissText |> Text.toDto)
            }

        let toDomainType (confirm: Confirm) =
            {
                Text = (confirm.Text |> Text.toDomainType)
                Title = (confirm.Title |> Title.toDomainType)
                OkText = (confirm.Ok_text |> Text.toDomainType)
                DismissText = (confirm.Dismiss_text |> Text.toDomainType)
            } : Actions.Confirm

    module Style =
        let toDto (style: Style) =
            match style with
            | Default -> "default"
            | Primary -> "primary"
            | Danger -> "danger"

        let toDomainType (str: string) =
            match str with
            | "default" -> Default
            | "primary" -> Primary
            | "danger" -> Danger
            | _ -> raise (exn "invalid style")

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

        let toDomainType (action: Action) =
            let confirm =
                if action.Confirm |> box |> isNull then None
                else action.Confirm |> Confirm.toDomainType |> Some

            let nullMap map o =
                if o |> isNull then None
                else o |> map |> Some
            ValueButton {
                Name = (action.Name |> Name.toDomainType)
                Text = (action.Text |> Text.toDomainType)
                Style = (action.Style |> Style.toDomainType)
                Value = (action.Value |> Value.toDomainType)
                Confirm = confirm
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

        let toDomainType (action: Action) =
            let confirm =
                if action.Confirm |> box |> isNull then None
                else action.Confirm |> Confirm.toDomainType |> Some

            let nullMap map o =
                if o |> isNull then None
                else o |> map |> Some
            LinkButton {
                Name = (action.Name |> Name.toDomainType)
                Text = (action.Text |> Text.toDomainType)
                Url = (action.Url |> Url.toDomainType)
                Style = (action.Style |> Style.toDomainType)
                Confirm = confirm
            }

    module DataSource =
        let toDto (ds: SelectedDataSource) =
            match ds with
            | Users -> "users"
            | Channel -> "channel"
            | Conversations -> "conversations"
            | External -> "external"

        let toDomainType (selectedDataSourceStr: string) =
            match selectedDataSourceStr with
            | "users" -> Users
            | "channel" -> Channel
            | "conversations" -> Conversations
            | "external" -> External
            | _ -> raise (exn "Unknown datasource")

    module DataSourceSelect =
        let toDto (dsSelect: DataSourceSelect) =
            {
                Action.defaultAction with
                    Type = "select"
                    Name = (dsSelect.Name |> Name.toDto)
                    Text = (dsSelect.Text |> Text.toDto)
                    Data_source = (dsSelect.DataSource |> DataSource.toDto)
            }

        let toDomainType (action: Action) =
            DataSourceSelect {
                Name = (action.Name |> Name.toDomainType)
                Text = (action.Text |> Text.toDomainType)
                DataSource = (action.Data_source |> DataSource.toDomainType)
            }


    module Option =
        let toDto (option: Types.Actions.Option) =
            {
                Text = (option.Text |> Text.toDto)
                Value = (option.Value |> Value.toDto)
            }

        let toDomainType (option: Option) =
            {
                Text = (option.Text |> Text.toDomainType)
                Value = (option.Value |> Value.toDomainType)
            } : Actions.Option

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

        let toDomainType (action: Action) =
            OptionsSelect {
                Name = (action.Name |> Name.toDomainType)
                Text = (action.Text |> Text.toDomainType)
                Options = (action.Options |> mapArrayToList Option.toDomainType)
                SelectedOptions = (action.Selected_options |> mapArrayToList Option.toDomainType)
            }

    module Action =
        let toDto (action: Actions.Action) =
            match action with
            | ValueButton valueButton -> ValueButton.toDto valueButton
            | LinkButton linkButton -> LinkButton.toDto linkButton
            | DataSourceSelect dsSelect -> DataSourceSelect.toDto dsSelect
            | OptionsSelect optSelect -> OptionsSelect.toDto optSelect

        let toDomainType (action: Action) =
            let mapper =
                match action.Type, action.Url, action.Data_source with
                | "select", _, ds when ds |> isNull |> not && ds <> "" ->
                    DataSourceSelect.toDomainType
                | "select", _, _ ->
                    OptionsSelect.toDomainType
                | "button", url, _ when url |> isNull |> not && url <> "" ->
                    LinkButton.toDomainType
                | "button", _, _ ->
                    ValueButton.toDomainType

            action |> mapper

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

        let toDomainType (field: Field) =
            let fieldData = 
                {
                    Title = (field.Title |> Title.toDomainType)
                    Value = (field.Value |> Value.toDomainType)
                } : FieldData
            let fieldType =
                match field.Short with
                | true -> ShortField
                | false -> LongField
            fieldType fieldData

    module Attachment =
        let toDto (attachment: Actions.Attachment) =
            {
                Title = (attachment.Title |> Title.toDto)
                Fallback = (attachment.Fallback |> Fallback.toDto)
                Callback_id = (attachment.CallbackId |> CallbackId.toDto)
                Color = (attachment.Color |> Color.toDto)
                Image_url = (attachment.ImageUrl |> Option.map Url.toDto |> Option.defaultValue "")
                Thumb_url = (attachment.ThumbUrl |> Option.map Url.toDto |> Option.defaultValue "")
                Actions = (attachment.Actions |> mapListToArray Action.toDto)
                Fields = (attachment.Fields |> mapListToArray Field.toDto)
            }

        let toDomainType (attachment: Attachment) =
            let getNullUrl url =
                if url |> isNull then None
                else url |> Url.toDomainType |> Some
            {
                Title = (attachment.Title |> Title.toDomainType)
                Fallback = (attachment.Fallback |> Fallback.toDomainType)
                CallbackId = (attachment.Callback_id |> CallbackId.toDomainType)
                Color = (attachment.Color |> Color.toDomainType)
                ImageUrl = (attachment.Image_url |> getNullUrl)
                ThumbUrl = (attachment.Thumb_url |> getNullUrl)
                Actions = (attachment.Actions |> mapArrayToList Action.toDomainType)
                Fields = (attachment.Fields |> mapArrayToList Field.toDomainType)
            } : Actions.Attachment


    module PostMessage =
        let toDto token (postMessage: Actions.PostMessage) =
            {
                Channel = (postMessage.Channel |> ChannelId.toDto)
                Text = (postMessage.Text |> Text.toDto)
                Icon_emoji = (postMessage.IconEmoji |> IconEmoji.toDto)
                Attachments = (postMessage.Attachments |> List.map Attachment.toDto |> List.toArray)
                Ts = null
            }

    module Ts =
        let toDto (SimpleTypes.Ts ts) = ts

    module UpdateMessage =
        let toDto token (message: Actions.UpdateMessage) =
            {
                Channel = (message.Channel |> ChannelId.toDto)
                Text = (message.Text |> Text.toDto)
                Icon_emoji = (message.IconEmoji |> IconEmoji.toDto)
                Attachments = (message.Attachments |> List.map Attachment.toDto |> List.toArray)
                Ts = (message.Ts |> Ts.toDto)
            }

        let toDomainType (dto: Message) =
            let attachments = 
                if dto.Attachments |> isNull then [] 
                else dto.Attachments |> List.ofArray |> List.map Attachment.toDomainType
            {
                Channel = (Events.ChannelId.toDomainType dto.Channel)
                Text = (Events.Text.toDomainType dto.Text)
                IconEmoji = (Events.Emoji.toDomainType dto.Icon_emoji)
                Attachments = attachments
                Ts = (Events.Ts.toDomainType dto.Ts)
            } : Actions.UpdateMessage

    module ActionMessage =

        let toSlackAction token (action: Actions.ActionMessage) =
            match action with
            | Actions.PostMessage msg -> 
                let method = Method.Post
                let data = Dto (PostMessage.toDto token msg)
                {
                    Method = method
                    Data = data
                }
            | Actions.UpdateMessage msg ->
                let method = Method.Update
                let data = Dto (UpdateMessage.toDto token msg)
                {
                    Method = method
                    Data = data
                }

module Response =
    type ResponseMessage = {
        Ok: bool
        Channel: Events.ChannelId
        Ts: string
        Message: Actions.Message
        Warning: string
    }


    module ResponseMessage =
        let toDomainType responseMessage =
            let message = Actions.UpdateMessage.toDomainType responseMessage.Message
            {
                Ok = responseMessage.Ok
                Channel = (Events.ChannelId.toDomainType responseMessage.Channel)
                Ts = (Events.Ts.toDomainType responseMessage.Ts)
                Message = message
                Warning = responseMessage.Warning
            } : Types.Actions.ResponseMessage
