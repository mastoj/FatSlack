module FatSlack.Domain.SimpleTypes

type ChannelId = ChannelId of string
    with member this.value = match this with ChannelId v -> v

type UserId = UserId of string
    with member this.value = match this with UserId v -> v

type Text = Text of string
    with member this.value = match this with Text v -> v

type Ts = Ts of string

type UserName = UserName of string

type BotId = BotId of string

type IconId = IconId of string

type Url = Url of string

type Name = Name of string

type Value = Value of string

type Title = Title of string

type Fallback = Fallback of string

type CallbackId = CallbackId of string

type Emoji= Emoji of string

module Text =
    let value (Text v) = v

module ChannelId =
    let value (ChannelId v) = v
