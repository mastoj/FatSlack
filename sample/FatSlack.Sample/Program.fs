// Learn more about F# at http://fsharp.org

open System
open FatSlack
open FatSlack.Types
open FatSlack.Dsl
open FatSlack.Dsl.Types
open FatSlack.Configuration
open Suave
open Suave.Filters
open Suave.Operators
open System.Threading

let leetHandler : EventHandler =
    fun slackApi event ->
        async {
            let pongMessage = { event with Text = "Pong 1337" }
            printfn "==> Ponging: %A" pongMessage
            let! response = slackApi.Send (PostMessage pongMessage)
            do! Async.Sleep 3000
            match response with
            | OkResponse (Some response) ->
                let updateMessage = { response.Message with Text = "Updated Pong 1337"; Channel = response.Channel}
                printfn "==> Updating: %A" updateMessage
                do! slackApi.Send (UpdateMessage updateMessage) |> Async.Ignore
            | x -> printfn "Unexpected response: %A" x
        } |> Async.RunSynchronously

let leetMatcher : EventMatcher =
    fun commandText event -> 
        let isMatch = commandText = "1337"
        printfn "==> Is it a match? %A" isMatch
        isMatch

let buttonHandler : EventHandler =
    fun slackApi event ->
        let actions = [
            Action.createAction (Button ("mybutton","Click me", "clicked"))
        ]
        let attachments = [
            (
                Attachment.createAttachment "callbackid"
                |> Attachment.withText "Some attachment"
                |> Attachment.withActions actions
            )
        ]
        let message =
            ChatMessage.createMessage event.Channel
            |> ChatMessage.withText "Show a button"
            |> ChatMessage.withAttachments attachments
            |> PostMessage
        slackApi.Send message |> Async.RunSynchronously |> ignore

let buttonMatcher : EventMatcher =
    fun commandText _ -> 
        let isMatch = commandText = "button"
        printfn "==> Is it a match? %A" isMatch
        isMatch

let gcloudPubsub =
    let reply sc = { ChatMessage.defaultValue with Text = (sprintf "Hello slash command: %A" sc) }
    {
        Matcher = (fun sc -> sc.Command = "/gcloud" && sc.Text.StartsWith("pubsub"))
        Handler = (fun slackApi sc -> printfn "%A" sc; Result.Ok (sc |> reply |> Some))
    }

let interactiveSpecificationSample =
    let handler slackApi (msg: InteractiveMessage) =
        let elements = [
            Element.createElement "text" (Element.Text (Some Element.Email)) "Text label"
            Element.createElement "textarea" (Element.TextArea None) "Textarea label"
            Element.createElement "select" Element.Select "Select label" |> Element.withOption (ElementOption.createElementOption "option1" "value1")
        ]
        let dialog =
            Dialog.createDialog "MyCallbackId" "This is a dialog"
            |> Dialog.withElements elements
        let dialogMessage = 
            DialogMessage.createDialogMessage msg.TriggerId dialog

        async {
            do! Async.Sleep 5000
            let message = 
                ChatMessage.createMessage msg.Channel.Id
                |> ChatMessage.withText "Responding to you"
                |> PostMessage
                |> slackApi.Respond msg.ResponseUrl
            return! message
        } |> Async.Ignore |> Async.Start

        dialogMessage 
        |> DialogMessage 
        |> slackApi.Send
        |> Async.RunSynchronously 
        |> printfn "Pushed dialog: %A"
        Result.Ok None
    {
        Matcher = (fun _ -> true)
        Handler = handler
    }

let submissionSpecificationSample =
    let handler slackApi msg =
        printfn "Submission: %A" msg
        Result.Ok None

    {
        Matcher = (fun _ -> true)
        Handler = handler
    }

let leetCommand =
    {
            Syntax = "1337"
            Description = "Return a pong and then updates it"
            EventHandler = leetHandler
            EventMatcher = leetMatcher
    }

let buttonCommand =
    {
        Syntax = "button"
        Description = "Return a button"
        EventHandler = buttonHandler
        EventMatcher = buttonMatcher
    }

let createFatSlackConfiguration appToken apiToken =
    init
    |> withApiToken apiToken
    |> withAppToken appToken
    |> withAlias "jarvis"
    |> withSlackCommand leetCommand
    |> withSlackCommand buttonCommand
    |> withSlashCommandSpec gcloudPubsub
    |> withHelpCommand
    |> withInteractiveSpecification interactiveSpecificationSample
    |> withDialogSubmissionSpecification submissionSpecificationSample

[<EntryPoint>]
let main argv =
    printfn "Argv: %A" argv
    let apiToken = argv.[0]
    let appToken = argv.[1]

    let cfg =
            { defaultConfig with
                  bindings =
                    [ HttpBinding.createSimple HTTP "0.0.0.0" 8080 ] }

    let logRequest : WebPart =
        fun (ctx: HttpContext) ->
            async {
                printfn "Request: %A" ctx.request
                return (Some ctx)
            }

    let healthCheck =
        choose
            [
                path "/health/ready" >=> Successful.OK "Ready"
                path "/health/health" >=> Successful.OK "Healthy"
            ]

    let requestRoute (requestApp: RequestApp) =
        let slackApi = FatSlack.SlackApi.createSlackApi apiToken
        request(fun req ->
            match req.formData "payload" with
            | Choice1Of2 payloadStr ->
                payloadStr
                |> requestApp
                |> function
                    | Result.Ok message ->
                        message
                        |> Option.map FatSlack.Json.serialize
                        |> Option.defaultWith (fun () -> "")
                        |> Successful.OK
                        >=> Writers.setHeader "Content-type" "application/json; charset=UTF-8"
                    | Result.Error message -> message |> sprintf "%A" |> RequestErrors.BAD_REQUEST
            | Choice2Of2 s ->
                printfn "Missing payload: %s" s
                Successful.OK ""
        )

    let slashCommandRoute slashCommandHandler =
        request(fun req ->
            req.form 
            |> List.map (fun (x, y) -> x, (y |> Option.get))
            |> Map.ofList
            |> slashCommandHandler
            |> function
                | Ok (Some msg) -> 
                    msg 
                    |> FatSlack.Json.serialize 
                    |> Successful.OK
                    >=> Writers.setHeader "Content-type" "application/json; charset=UTF-8"
                | Ok None -> "" |> Successful.OK
                | Error e ->
                    ChatMessage.defaultValue
                    |> ChatMessage.withResponseType Ephemeral
                    |> ChatMessage.withText (sprintf "Failed to handle command : %A" e)
                    |> FatSlack.Json.serialize |> RequestErrors.BAD_REQUEST
        )

    let fatSlackConfig = createFatSlackConfiguration appToken apiToken
    fatSlackConfig |> BotApp.start
    let slashApp = fatSlackConfig |> FatSlack.SlashCommandApp.createSlashHandler
    let requestApp = fatSlackConfig |> FatSlack.RequestApp.createRequestApp

    let app =
        choose [
            healthCheck
            logRequest >=>
            choose
                [
                    path "/action" >=> requestRoute requestApp
                    path "/slash" >=> slashCommandRoute slashApp
                ]
        ]

    printfn "Starting web server"
    let cts = new CancellationTokenSource()
    let _, server = startWebServerAsync cfg app
    Async.Start(server, cts.Token)
    printfn "Hello World from F#! %s" argv.[0]
    System.Threading.Thread.Sleep(Int32.MaxValue)
    0 // return an integer exit code
