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
