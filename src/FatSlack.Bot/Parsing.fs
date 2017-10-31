module FatSlack.Parsing

open System
open System.Text.RegularExpressions
open FatSlack.Core.Types
open FatSlack.Core.Types.Events

module Events =
    type MessageType = 
        | Directed
        | DirectMessage
        | GroupMessage
        | NotAddressedToBot
        | BotMessage

    let isAdressedBot (botInfo: BotInformation) (message:MessageEvent) = 
        let checkPattern text pattern = 
            let regexPattern = "^" + pattern
            let regex = Regex(regexPattern)
            regex.IsMatch(text)

        let idPattern = "<@" + botInfo.User.Id
        let patternsToLookFor = 
            botInfo.Configuration.Alias
            |> Option.fold (fun patterns alias -> alias :: patterns) [idPattern]
        patternsToLookFor
        |> List.exists (checkPattern message.Text)

    let getMessageType (botInfo: BotInformation) (message:MessageEvent) =
        if message.User = botInfo.User.Id
        then BotMessage
        else if String.IsNullOrWhiteSpace(message.Text) 
        then NotAddressedToBot
        else if isAdressedBot botInfo message 
        then Directed
        else if message.Channel.StartsWith("D") 
        then DirectMessage 
        else if message.Channel.StartsWith("G")
        then GroupMessage
        else NotAddressedToBot

    let splitToFirstWordAndRest (str: string) = 
        let first = str.Split([|' '|]).[0]
        first, str.Substring(first.Length).Trim()

    let parseSimple (str:string) (command:string) argConvert resultType = 
        if str.StartsWith(command)
        then Some (resultType (str, (str.Substring(command.Length).Trim() |> argConvert)))
        else None

    let parseRegex str (regex:Regex) =
        let regexMatch = regex.Match(str)
        if regexMatch.Success 
        then Some (RegexEventParseResult regexMatch)
        else None

    let doParse commandStr (parser, handler) = 
        match parser with
        | SimpleEventParser str ->
            parseSimple commandStr str (fun (str:string) -> str.Split([|' '|]) |> List.ofArray) SimpleEventParseResult
        | RegexEventParser regex ->
            parseRegex commandStr regex
        | JsonEventParser str ->
            parseSimple commandStr str id JsonEventParseResult
        |> Option.bind (fun x -> Some (x, handler))

    let findMatchingCommand (commands: CommandDefinition list) commandStr = 
        commands
        |> Seq.map (fun c -> c.EventParser, c.EventHandler)
        |> Seq.map (doParse commandStr)
        |> Seq.choose id

    let findMatchingListeners (listeners: ListenerDefinition list) text = 
        listeners
        |> Seq.map (fun c -> c.EventParser, c.EventHandler)
        |> Seq.map (doParse text)
        |> Seq.choose id

    let parseEvent (botInfo: BotInformation) (message:MessageEvent) =
        let messageType = getMessageType botInfo message
        let matchingCommands = 
            match messageType with
            | BotMessage
            | NotAddressedToBot -> None
            | Directed ->
                let (_, commandStr) = splitToFirstWordAndRest message.Text
                if commandStr |> isNull then None else Some commandStr
            | DirectMessage ->
                if message.Text |> isNull then None else Some message.Text
            | GroupMessage -> None
            |> Option.fold (fun _ v -> findMatchingCommand botInfo.Configuration.Commands v) Seq.empty

        if matchingCommands |> Seq.isEmpty 
        then
            match messageType with
            | BotMessage -> Seq.empty
            | _ -> findMatchingListeners (botInfo.Configuration.Listeners) message.Text
        else
            matchingCommands
