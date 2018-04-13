module FatSlack.Configuration
open FatSlack
open Types

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

let withAlias alias config = { config with Alias = Some alias }

let withSlashCommandSpec slashCommandSpecification config = 
    { config with SlashCommandSpecs = slashCommandSpecification :: config.SlashCommandSpecs }

let withRequestCommandSpec requestCommandSpec config =
    { config with RequestSpecs = requestCommandSpec :: config.RequestSpecs }

// type Handler<'T> = SlackApi -> 'T -> Result<ChatMessage option, SlackRequestError>
// type Matcher<'T> = 'T -> bool


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
