module FatSlack.Core.Net

[<RequireQualifiedAccess>]
module Http =
    open System
    open System.Collections.Specialized

    type FormValues = (string * string) list
    type Body = 
        | FormValues of FormValues
        | JsonData of string
        with 
            member this.toData =
                match this with 
                | FormValues values ->
                    let nameValueCollection = NameValueCollection()
                    values
                    |> List.fold (fun (nameValueCollection:NameValueCollection) (key, value) ->
                            nameValueCollection.Add(key, value)
                            nameValueCollection
                        ) nameValueCollection
                    :> obj
                | JsonData json -> json :> obj

    module ContentType = 
        let [<Literal>] Form = "application/x-www-form-urlencoded"
        let [<Literal>] Json = "application/json"
        let getContentType = function
            | FormValues _ -> Form
            | JsonData _ -> Json

    module MethodType = 
        let [<Literal>] Post = "POST"
        let [<Literal>] Get = "GET"

    module Request = 
        type Headers = (string * string) list
        type GetRequest = {
            Headers: Headers
            Url: string
        }

    let getString (request:Request.GetRequest) =
        use webClient = new System.Net.WebClient()

        request.Headers
        |> List.iter (fun (k,v) -> webClient.Headers.Add(k, v))
        webClient.DownloadString(Uri(request.Url))

    let downloadString (url:string) = 
        getString {Url = url; Headers = []}

    let downloadJsonObject<'a> (url:string) = 
        downloadString url
        |> Json.deserialize<'a>

    let postWithHeaders (headers:Request.Headers) (url:string) body =
        use webClient = new System.Net.WebClient()
        let contentType = ContentType.getContentType body
        let data = body.toData
        let uri = Uri(url)
        headers
        |> List.iter (fun (k,v) -> webClient.Headers.Add(k,v))
        webClient.UploadValuesAsync(uri, data :?> NameValueCollection)

    let postJson (headers:Request.Headers) (url:string) body =
        async {
            use webClient = new System.Net.WebClient()
            let contentType = ContentType.getContentType body
            let data = body.toData
            let uri = Uri(url)
            printfn "The data to be sent: %A" data
            printfn "Content-Type: %A" contentType
            printfn "Uri: %A" uri
            let! result =
                ("Content-Type", contentType) :: headers
                |> List.iter (fun (k,v) -> webClient.Headers.Add(k,v))
                webClient.UploadStringTaskAsync(uri, data :?> string)
                |> Async.AwaitTask
            return result
        }

    let post (url:string) body =
        async {
            use webClient = new System.Net.WebClient()
            let contentType = ContentType.getContentType body
            let data = body.toData
            let uri = Uri(url)
            printfn "The data to be sent: %A" data
            printfn "Content-Type: %A" contentType
            printfn "Uri: %A" uri
            [("Content-Type", contentType)]
            |> List.iter (fun (k,v) -> webClient.Headers.Add(k,v))
            webClient.UploadStringAsync(uri, data :?> string)
        }

[<RequireQualifiedAccess>]
module WebSocket =
    open System
    open System.Net.WebSockets
    open System.Linq

    type private WebSocketClient =
        | Open of ClientWebSocket
        | Connecting
        | Aborted
        | Closed
        | CloseReceived
        | CloseSent
        | None

    type HandleWebsocketMessage = string -> Async<unit>

    let private listen (messageHandler: HandleWebsocketMessage) (socket: ClientWebSocket) =
        let receiveBytes = Array.zeroCreate<byte> 4096
        let receiveBuffer = new ArraySegment<byte>(receiveBytes)
        let rec innerListen data =
            async {
                let! ct = Async.CancellationToken
                let! message = socket.ReceiveAsync(receiveBuffer, ct) |> Async.AwaitTask

                let messageBytes = receiveBuffer.Skip(receiveBuffer.Offset).Take(message.Count).ToArray()
                let messageString = data + System.Text.Encoding.UTF8.GetString(messageBytes)
                if message.EndOfMessage
                then
                    printfn "End of message :%A" messageString
                    messageString
                    |> messageHandler
                    |> Async.Start
                    return! innerListen ""
                else
                    return! innerListen messageString
            }
        innerListen "" |> Async.Start

    let connect<'T> (messageHandler: HandleWebsocketMessage) url = 
        let rec innerConnect() =
            async {
                printfn "Websocket connect"
                let socket = new System.Net.WebSockets.ClientWebSocket()
                let uri = Uri(url)
                let! ct = Async.CancellationToken
                let connectTask = socket.ConnectAsync(uri, ct)
                do! Async.AwaitTask connectTask
                return! waitForConnection socket
            }
        and waitForConnection socket = 
            async {
                match socket.State with
                | WebSocketState.Open -> 
                    printfn "Websocket connection opened"
                    return listen messageHandler socket
                | WebSocketState.Connecting ->
                    printfn "Websocket connecting"
                    do! Async.Sleep 10000
                    return! waitForConnection socket
                | WebSocketState.Aborted
                | WebSocketState.Closed
                | WebSocketState.CloseReceived
                | WebSocketState.CloseSent
                | WebSocketState.None
                | _ ->
                    return! innerConnect()
            }
        innerConnect() |> Async.RunSynchronously
