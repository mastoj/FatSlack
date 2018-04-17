# FatSlack - The F# wrapper of the Slack API

The intention of FatSlack is to be a handy wrapper of the Slack API to create bots, slash commands and apps that deal with request from interactive slack messages.

There is a [sample app](/sample/FatSlack.Sample/Program.fs) that cover some of features of the framework.

I tried to minimize the number of external dependencies, but I did settle on `Newtonsoft.Json` to make life a little bit easier. This means that you are free to use asp.net, giraffe, suave or something else to host your app. If it is just a bot you are hosting you can settle with a console app.

## Getting started

I assume that you know how to set up an app in Slack and want cover that topic since it is covered by: https://api.slack.com/. The small getting started shows you more than just prints a text back, that is why it is a little bit longer.

1.  Install it through nuget: Install https://www.nuget.org/packages/FatSlack

2.  Create your first bot:

```
open FatSlack
open FatSlack.Types
open FatSlack.Dsl
open FatSlack.Dsl.Types
open FatSlack.Configuration

let leetHandler : EventHandler =
    fun slackApi event ->
        async {
            let pongMessage = { event with Text = "Pong 1337" }
            let! response = slackApi.Send (PostMessage pongMessage)
            do! Async.Sleep 3000
            let updateMessage = { response.Message with Text = "Updated Pong 1337"; Channel = response.Channel}
            do! slackApi.Send (UpdateMessage updateMessage) |> Async.Ignore
        } |> Async.RunSynchronously

let leetMatcher : EventMatcher =
    fun commandText event ->
        let isMatch = commandText = "1337"
        isMatch

let leetCommand =
    {
        Syntax = "1337"
        Description = "Return a pong and then updates it"
        EventHandler = leetHandler
        EventMatcher = leetMatcher
    }

let config apiToken =
    init
    |> withApiToken apiToken
    |> withAlias "jarvis"
    |> withSlackCommand leetCommand
    |> withHelpCommand

config (getApiToken()) |> BotApp.start
```

There are a couple of things that goes on here:

* The `leetHandler` is the handler for the actual event that triggered the bot. This part might change since the `event` is just an alias over the `ChatMessage` type. The reason for this is simplicity. An handler will only be run if the corresponding `EventMatcher` returns true for the same `commandText` and `event` combination.
* The `leetMatcher` implements the matching logic for the `leetCommand`. If this function returns true for an event the `EventHandler` will be run. the `commandText` argument is there for convenience and the purpose is that it should be text after the name or alias of the bot or it will be the full text if you are in a private conversation with the bot. Example: "yourbotname hello bot" will yield "hello bot" as `commandText`.
* The `leetCommand` is how you define a command and there you can provide information on how to use it and what it does together with the `EventHandler` and `EventMatcher`.
* Every `config` should start with `init` and then you use the helper functions to create what you need.
  * `withApiToken` - sets the api token for the bot
  * `withAlias` - is optional, but it might be nice to have so you can use a shorter name for your bot
  * `withSlackCommand` - adds a command specifictation to the bot configuration, you can do this multiple times.
  * `withHelpCommand` - if you want an automatic help command you can just call this helper function and it will add it for you. Remember to do this as the last step since it will only create help for the commands already defined.
* To start the bot you run `BotApp.start`.

3.  More complex messages.

There are a small dsl on top of the api types to make it easier to get them right, and they should also make it much harder to create an invalid message. A small example of the dsl in use is this:

```
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
```

You should be able to read the code and quite quickly understand what is going on here. First an `action` list is created, this list of actions are then added to the list of `attachment`, which in turn is added to the `message`. The `PostMessage` part is used to differentiate between an message post and an update of a message.
