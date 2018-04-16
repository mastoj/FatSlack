module FatSlack.SlashCommandApp

open Types
open Types.Functions

let authenticateSlashCommand appToken slashCommand =
    if appToken = slashCommand.Token
    then Result.Ok slashCommand
    else Result.Error (SlackRequestError.InvalidAppToken)

let parseSlashCommandMap (slashCommandMap: Map<string, string>) =
    try
        {
            Token = slashCommandMap.["token"]
            TeamId = slashCommandMap.["team_id"]
            TeamDomain = slashCommandMap.["team_domain"]
            ChannelId = slashCommandMap.["channel_id"]
            ChannelName = slashCommandMap.["channel_name"]
            UserId = slashCommandMap.["user_id"]
            UserName = slashCommandMap.["user_name"]
            Command = slashCommandMap.["command"]
            Text = slashCommandMap.["text"]
            ResponseUrl = slashCommandMap.["response_url"]
            TriggerId = slashCommandMap.["trigger_id"]
        }
        |> Result.Ok
    with exn ->
        Result.Error (SlackRequestError.FailedToParseSlashCommand exn)

let createSlashHandler fatSlackConfig =
    match fatSlackConfig.ApiToken, fatSlackConfig.AppToken with
    | Some apiToken, Some appToken ->
        let slackApi = SlackApi.createSlackApi apiToken
        fun slashCommandMap ->
            slashCommandMap
            |> parseSlashCommandMap
            |> Result.bind (authenticateSlashCommand appToken)
            |> Result.bind (createActionApp fatSlackConfig.SlashCommandSpecs slackApi)
    | None, None -> raise (exn "ApiToken and AppToken is required for slash commands")
    | _, None -> raise (exn "AppToken is required for slash commands")
    | None, _ -> raise (exn "ApiToken is required for slash commands")
