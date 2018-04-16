module FatSlack.RequestApp
open FatSlack
open Newtonsoft.Json.Linq
open Types
open Types.Functions

let validateRequest token (jObj:JObject) =
    match jObj |> Json.getValue "token" |> Option.map (Json.getStringFromToken) with
    | Some requestToken -> 
        if token = requestToken then Result.Ok jObj
        else Result.Error InvalidAppToken
    | None -> Result.Error MissingToken

let (|InteractiveMessageType|_|) (jObj:JObject) =
    match jObj |> Json.getValue "type" |> Option.map (Json.getStringFromToken) with
    | Some "interactive_message" -> Some(InteractiveMessageType)
    | _ -> None

let (|DialogSubmisstionType|_|) (jObj:JObject) =
    match jObj |> Json.getValue "type" |> Option.map (Json.getStringFromToken) with
    | Some "dialog_submission" -> Some(DialogSubmisstionType)
    | _ -> None

let parsePayload jObj =
    match jObj with
    | InteractiveMessageType ->
        Json.deserialize<InteractiveMessage> (jObj.ToString()) |> InteractiveMessage |> Result.Ok
    | DialogSubmisstionType ->
        Json.deserialize<DialogSubmission> (jObj.ToString()) |> DialogSubmission |> Result.Ok
    | _ -> Result.Error (UnknownSlackRequest)

let handleRequest (slackApi: SlackApi) payload =
    Result.Ok (payload |> Json.serialize)

let createRequestHandler appHandler appToken : RequestHandler =
    fun slackApi payload ->
        payload
        |> Json.parse
        |> Option.map (Result.Ok)
        |> Option.defaultValue (Result.Error UnknownSlackRequest)
        |> Result.bind (validateRequest appToken)
        |> Result.bind parsePayload
        |> Result.bind (appHandler slackApi)

let createRequestApp fatSlackConfig : RequestApp =
    match fatSlackConfig.ApiToken, fatSlackConfig.AppToken with
    | Some apiToken, Some appToken ->
        let slackApi = apiToken |> FatSlack.SlackApi.createSlackApi
        fun payload ->
            payload
            |> Json.parse
            |> Option.map (Result.Ok)
            |> Option.defaultValue (Result.Error UnknownSlackRequest)
            |> Result.bind (validateRequest appToken)
            |> Result.bind parsePayload
            |> Result.bind (createActionApp fatSlackConfig.RequestSpecs slackApi)
    | None, None -> raise (exn "API and App token is required for request app")
    | _, None -> raise (exn "App token is required for request app")
    | None, _ -> raise (exn "API token is required for request app")
