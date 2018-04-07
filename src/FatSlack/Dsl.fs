module FatSlack.Dsl
open Types

module Types =
    type Action =
        | Button of name:string * text:string * value:string

open Types

[<RequireQualifiedAccess>]
module ChatMessage =
    let createMessage channel : ChatMessage =
        { ChatMessage.defaultValue with Channel = channel }
    let withText value msg : ChatMessage = { msg with Text = value }
    let withAttachments value msg = { msg with Attachments = value }
    let withChannel value msg : ChatMessage = { msg with Channel = value }

[<RequireQualifiedAccess>]
module Attachment =
    let createAttachment = Attachment.defaultValue
    let withText value attachment : Attachment = { attachment with Text = value }
    let withActions value attachment : Attachment = { attachment with Actions = value }

[<RequireQualifiedAccess>]
module Action =
    let createAction action =
        match action with
        | Button (name, text, value) ->
            { (Action.defaultValue name "button") with Text = text; Value = value }

[<RequireQualifiedAccess>]
module Dialog =
    let createDialog = Dialog.defaultValue

    let withElement element dialog = { dialog with Elements = element :: dialog.Elements }
    let withElements elements dialog = { dialog with Elements = elements @ dialog.Elements }
    let withSubmitLabel submitLabel dialog = { dialog with SubmitLabel = submitLabel }


[<RequireQualifiedAccess>]
module DialogMessage =
    let createDialogMessage = DialogMessage.defaultValue

[<RequireQualifiedAccess>]
module Element =
    open System
    type Subtype =
        | Email
        | Number
        | Tel
        | Url

    type Type =
        | Text of Subtype option
        | TextArea of Subtype option
        | Select

    let createElement name elemType label =
        let setSubType subtypeOpt elem =
            subtypeOpt
            |> Option.map (fun subtype ->
                match subtype with
                | Email -> "email"
                | Number -> "number"
                | Tel -> "tel"
                | Url -> "url"
                |> (fun subtype -> { elem with Subtype = subtype } : Element))
            |> Option.defaultWith (fun () -> elem)

        match elemType with
        | Text subtype ->
            Element.defaultValue name "text" label
            |> setSubType subtype
        | TextArea subtype ->
            Element.defaultValue name "textarea" label
            |> setSubType subtype
        | Select ->
            Element.defaultValue name "select" label

    let withLabel value element = { element with Label = value }
    let withType elemType (element: Element) : Element =

        let setSubType subtypeOpt elem =
            subtypeOpt
            |> Option.map (fun subtype ->
                match subtype with
                | Email -> "email"
                | Number -> "number"
                | Tel -> "tel"
                | Url -> "url"
                |> (fun subtype -> { elem with Subtype = subtype } : Element))
            |> Option.defaultWith (fun () -> elem)

        match elemType with
        | Text subtype ->
            { element with Type = "text" }
            |> setSubType subtype
        | TextArea subtype ->
            { element with Type = "textarea" }
            |> setSubType subtype
        | Select ->
            { element with Type = "select" }

    let withPlaceholder value element = { element with Placeholder = value }
    let withOptional value element = { element with Optional = value }
    let withMaxLength (value: int) element = { element with MaxLength = Nullable<int>(value) }
    let withMinLength (value: int) element = { element with MinLength = Nullable<int>(value) }
    let withHint value element = { element with Hint = value }
    let withValue value element : Element = { element with Value = value }
    let withOption option element : Element = { element with Options = option :: element.Options }
    let withOptions options element : Element = { element with Options = options @ element.Options }

[<RequireQualifiedAccess>]
module ElementOption =
    let createElementOption label value : ElementOption =
        {
            Label = label
            Value = value
        }