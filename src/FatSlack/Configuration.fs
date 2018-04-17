module FatSlack.Configuration
open FatSlack
open Types
open Dsl

let init =
    {
        ApiToken = None
        AppToken = None
        Commands = []
        SlashCommandSpecs = []
        RequestSpecs = []
        Alias = None
    }

let withApiToken value config = { config with ApiToken = Some value }
let withAppToken value config = { config with AppToken = Some value }

let withCommand command config = { config with Commands = command::config.Commands }

let withCommands commands config = { config with Commands = commands @ config.Commands }

let withSlackCommand = SlackCommand >> withCommand

let withSlackCommands = (List.map SlackCommand) >> withCommands

let withSpyCommand = SpyCommand >> withCommand

let withSpyCommands = (List.map SpyCommand) >> withCommands

let withHelpCommand config : FatSlackConfiguration =
    let matcher text _ = text = "help"

    let onlySlackCommands command =
        match command with
        | SlackCommand sc -> Some sc
        | _ -> None

    let join (strs: string list) = System.String.Join("\n", strs)
    let handler slackApi (event: Event) =
        let text =
            config.Commands
            |> List.map onlySlackCommands
            |> List.choose id
            |> List.sortBy (fun sc -> sc.Syntax)
            |> List.map (fun sc -> sprintf "*%s*: %s" sc.Syntax sc.Description)
            |> (fun commands -> (sprintf "*%s*: %s" "help" "List available commands")::commands)
            |> join

        let message =
            ChatMessage.createMessage event.Channel
            |> ChatMessage.withText text
            |> PostMessage

        message
        |> slackApi.Send
        |> Async.RunSynchronously
        |> ignore

    let helpCommand =
        SlackCommand {
            Syntax = "help"
            Description = "List available commands"
            EventHandler = handler
            EventMatcher = matcher
        }

    {
        config
            with Commands = helpCommand :: config.Commands
    }

let withAlias alias config = { config with Alias = Some alias }

let withSlashCommandSpec slashCommandSpecification config = 
    { config with SlashCommandSpecs = slashCommandSpecification :: config.SlashCommandSpecs }

let withRequestCommandSpec requestCommandSpec config =
    { config with RequestSpecs = requestCommandSpec :: config.RequestSpecs }

let liftSpecification spec lift =
    let matcher action =
        action
        |> lift
        |> Option.map spec.Matcher
        |> Option.defaultWith (fun () -> false)
 
    let handler slackApi action =
        action
        |> lift
        |> Option.map (spec.Handler slackApi)
        |> Option.defaultWith (fun () -> Result.Ok None)
    {
        Matcher = matcher
        Handler = handler
    }

let withInteractiveSpecification (spec: Specification<InteractiveMessage>) config =
    let spec' =
        function
        | InteractiveMessage msg -> Some msg
        | _ -> None
        |> liftSpecification spec
    { config with RequestSpecs = spec' :: config.RequestSpecs }

let withDialogSubmissionSpecification (spec: Specification<DialogSubmission>) config =
    let spec' =
        function
        | DialogSubmission msg -> Some msg
        | _ -> None
        |> liftSpecification spec
    { config with RequestSpecs = spec' :: config.RequestSpecs }
