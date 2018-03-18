namespace FatSlack.Bot

open Newtonsoft.Json
open Newtonsoft.Json.Linq
open FatSlack.Core
open FatSlack.Core.Net
open FatSlack.Core.SlackApi
open FatSlack.Core.Types
open System
open System.Linq
open System.Net.WebSockets
open System.Text.RegularExpressions
open FatSlack.Core.Domain.Types.Events

// let init token = {
//     Token = token
//     Alias = None
//     Commands = []
//     Listeners = []
// }

// let withAlias alias config = { config with Alias = Some alias }
// let withCommand command config = { config with Commands = command :: config.Commands }
// let withCommands commands config = { config with Commands = commands @ config.Commands }

// let withListener listener config = { config with Listeners = listener :: config.Listeners}

// type ConnectResponse = {
//     Ok: bool 
//     Url: string
//     Team: Team
//     Self: SlackUser
// }

// let getBotInfo (config: BotConfiguration) =
//     sprintf "https://slack.com/api/rtm.connect?token=%s" config.Token
//     |> Http.downloadJsonObject<ConnectResponse>
//     |> (fun cr -> 
//         {
//             Configuration = config
//             Team = cr.Team
//             User = cr.Self
//             WebSocketUrl = cr.Url
//         })

// type Agent<'a> = MailboxProcessor<'a>
// type AgentMessage = 
//     | Connected of ClientWebSocket
//     | Reconnected of ClientWebSocket
//     | EventReceived of Domain.Types.Events.Event
//     | SendMessage of (Api.Dto.Actions.Message)

// type BotAgentState = {
//     Socket: ClientWebSocket option
// }

// let handleEvent botInfo callback event = 
//     async {
//         try
//             let parseResult = Parsing.Events.parseEvent botInfo event
//             printfn "Parsed event: %A" parseResult
//             parseResult
//             |> Seq.iter (fun (evt, handler) -> handler evt event callback)
//         with
//         | ex -> 
//             printfn "%A" ex
//             match event with
//             | Domain.Types.Events.Message (RegularMessage msg) ->
//                 let reply = 
//                     Domain.Types.ActionMessage.postMessage 
//                         (msg.Channel)
//                         (Domain.SimpleTypes.Text "Failed to execute action, check log for errors")
//                         (Domain.SimpleTypes.Emoji "")
//                         []
//                 Some reply
//             | _ -> None
//             |> Option.iter callback
//     }

// let agentHandler botInfo (inbox:Agent<AgentMessage>) =
//     let apiClient = { Token = botInfo.Configuration.Token }
//     let rec loop state = 
//         async {
//             let! msg = inbox.Receive()
//             let state = 
//                 match msg with
//                 | Connected socket
//                 | Reconnected socket ->
//                     { Socket = Some socket }
//                 | EventReceived evt ->
//                     handleEvent botInfo (SendMessage >> inbox.Post) evt |> Async.Start
//                     state
//                 | SendMessage msg ->
//                     send apiClient msg
//                     state
//             return! loop state
//         }
//     loop { Socket = None }

// let createBotAgent botInfo = Agent.Start(agentHandler botInfo)

// let deserializeEvent (json:string) = 
//     let jObject = JObject.Parse(json)
//     try
//         if jObject.["type"] |> isNull 
//         then Result.Error (Errors.Error.UnsupportedSlackEvent "null")
//         else
//             match jObject.["type"].ToString() with
//             | "message" ->
//                 (Json.deserialize<Api.Dto.Events.Message>(json))
//                 |> Result.Ok
//             | x ->
//                 Result.Error (Errors.Error.UnsupportedSlackEvent x)
//     with
//     | x -> 
//         printfn "%A" x
//         Result.Error (Errors.Error.JsonError (x.ToString()))

// let startListen botInfo =
//     let apiClient = { Token = botInfo.Configuration.Token }
//     let sendMessage = (Api.Dto.Actions.ActionMessage.toSlackAction botInfo.Configuration.Token) >> (send apiClient)

//     let handle messageString =
//         messageString
//         |> deserializeEvent
//         |> Result.bind (Api.Dto.Events.Message.toDomainType)
//         |> Result.map (Domain.Types.Events.Event.Message)
//         |> Result.map (handleEvent botInfo sendMessage)

//     let handleAsync messageString =
//         async {
//             do handle messageString
//         }
//     WebSocket.connect handleAsync botInfo.WebSocketUrl

    // printfn "Start listening"
    // let receiveBytes = Array.zeroCreate<byte> 4096
    // let receiveBuffer = new ArraySegment<byte>(receiveBytes)
    // let rec listen data (botAgent:Agent<AgentMessage>) (socket:ClientWebSocket) = async {
    //     if socket.State = WebSocketState.Open 
    //     then
    //         printfn "Waiting for message"
    //         let! ct = Async.CancellationToken
    //         let! message = socket.ReceiveAsync(receiveBuffer, ct) |> Async.AwaitTask
    //         if message.MessageType = WebSocketMessageType.Close
    //         then
    //             let socket = WebSocket.connect botInfo.WebSocketUrl
    //             botAgent.Post(Reconnected socket)
    //             return! listen "" botAgent socket
    //         else
    //             let messageBytes = receiveBuffer.Skip(receiveBuffer.Offset).Take(message.Count).ToArray()
    //             let messageString = data + System.Text.Encoding.UTF8.GetString(messageBytes)
    //             if message.EndOfMessage
    //             then
    //                 let slackEvent = messageString |> deserializeEvent
    //                 match slackEvent with
    //                 | Some msg ->
    //                     botAgent.Post(EventReceived msg)
    //                 | None -> ()
    //                 return! listen "" botAgent socket
    //             else
    //                 return! listen messageString botAgent socket
    //     else
    //         let socket = WebSocket.connect botInfo.WebSocketUrl
    //         botAgent.Post(Reconnected socket)
    //         return! listen "" botAgent socket
    // }
    // let socket = WebSocket.connect botInfo.WebSocketUrl
    // let botAgent = createBotAgent botInfo
    // botAgent.Post(Connected socket)
    // listen "" botAgent socket |> Async.Start

// let withHelpCommand config =
//     let join separator (str:string list) = String.Join(separator, str)
//     let messageText = config.Commands |> List.map (fun c -> c.Syntax) |> join "\n"
//     let variableRegex = new Regex("(<.+?>)")
//     let quotedVariables = variableRegex.Replace(messageText, "`$1`")

//     let handler _ (Message (RegularMessage evt)) callback =
//         callback(Domain.Types.ActionMessage.postMessage evt.Channel (Domain.SimpleTypes.Text quotedVariables) (Domain.SimpleTypes.Emoji "") [])

//     config
//         |> withCommand (CommandDefinition.createSimpleCommand 
//             handler "help" "help" "Returns a list of available commands")

//open Bot.Functions
// let start config = 
//     config
//     |> connectBot
//     |> startListen
