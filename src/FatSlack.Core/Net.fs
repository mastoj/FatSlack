module FatSlack.Core.Net

[<RequireQualifiedAccess>]
module Http =
    open System
    open System.Collections.Specialized

    type FormValues = (string * string) list
    type Body = 
        | FormValues of FormValues
        | JsonData of string
        with member this.toData =
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
        printfn "==> REQUEST STARTED"
        request.Headers
        |> List.iter (printfn "%A")

        printfn "Url: %s" request.Url

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
        use webClient = new System.Net.WebClient()
        let contentType = ContentType.getContentType body
        let data = body.toData
        let uri = Uri(url)
        ("Content-Type", contentType) :: headers
        |> List.iter (fun (k,v) -> webClient.Headers.Add(k,v))
        webClient.UploadStringAsync(uri, data :?> string)

    let post (url:string) body = 
        use webClient = new System.Net.WebClient()
        let contentType = ContentType.getContentType body
        let data = body.toData
        let uri = Uri(url)
        webClient.UploadValuesAsync(uri, data :?> NameValueCollection)

[<RequireQualifiedAccess>]
module WebSocket =
    open System
    open System.Net.WebSockets

    let connect url = 
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
                    return socket
                | WebSocketState.Connecting ->
                    printfn "Websocket connecting"
                    do! Async.Sleep 10000
                    return! waitForConnection socket
                | WebSocketState.Aborted
                | WebSocketState.Closed
                | WebSocketState.CloseReceived
                | WebSocketState.CloseSent
                | WebSocketState.None -> 
                    return! innerConnect()
            }
        innerConnect() |> Async.RunSynchronously
