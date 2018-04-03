module FatSlack.App
open FatSlack
open Newtonsoft.Json.Linq
open System
open Types
open FatSlack.Types

type Payload = string
type InteractiveMessage = unit
type Submission = unit

type SlackRequest =
    | InteractiveMessage of InteractiveMessage
    | Submission of Submission

type SlackRequestError =
    | InvalidToken
    | MissingToken
    | FailedToParsePayload
    | UnknownSlackRequest
    | FailedToHandleRequest of SlackRequest

type RequestHandler = SlackApi -> Payload -> Result<string, SlackRequestError>

let validateRequest token (jObj:JObject) =
    Result.Ok jObj
    // match jObj |> Json.getValue "token" |> Option.map (Json.getStringFromToken) with
    // | Some requestToken -> 
    //     if token = requestToken then Result.Ok jObj
    //     else Result.Error InvalidToken
    // | None -> Result.Error MissingToken

let (|InteractiveMessageType|_|) (jObj:JObject) =
    match jObj |> Json.getValue "type" |> Option.map (Json.getStringFromToken) with
    | Some "interactive_message" -> Some(InteractiveMessageType)
    | _ -> None

let (|SubmisstionType|_|) (jObj:JObject) =
    match jObj |> Json.getValue "type" |> Option.map (Json.getStringFromToken) with
    | Some "dialog_submission" -> Some(SubmisstionType)
    | _ -> None

let parsePayload jObj =
    match jObj with
    | InteractiveMessageType ->
        Json.deserialize<InteractiveMessage> (jObj.ToString()) |> InteractiveMessage |> Result.Ok
    | SubmisstionType ->
        Json.deserialize<Submission> (jObj.ToString()) |> Submission |> Result.Ok
    | _ -> Result.Error (UnknownSlackRequest)

let handleRequest (slackApi: SlackApi) payload =
    Result.Ok (payload |> Json.serialize)

let createRequestHandler token : RequestHandler =
    fun slackApi payload ->
        payload
        |> Json.parse
        |> Option.map (Result.Ok)
        |> Option.defaultValue (Result.Error UnknownSlackRequest)
        |> Result.bind (validateRequest token)
        |> Result.bind parsePayload
        |> Result.bind (handleRequest slackApi)
