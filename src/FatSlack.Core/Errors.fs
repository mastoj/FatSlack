module FatSlack.Core.Errors

type Error =
    | SerializationError of string
    | UnsupportedSlackEvent of string
    | JsonError of string