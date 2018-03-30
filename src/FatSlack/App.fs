module FatSlack.App

open Suave
open Operators
open Filters
open FatSlack
open FatSlack.Types

module Choice = 
    let handle<'a, 'b> okF failF choice = 
        match choice with
        | Choice1Of2 v -> okF v
        | Choice2Of2 v -> failF v

let getString rawForm =
    System.Text.Encoding.UTF8.GetString(rawForm)

let validateToken verificationToken (actionReq: ActionRequest) = 
    match actionReq.Token with
    | x when x = verificationToken -> Choice1Of2 actionReq
    | _ -> Choice2Of2 "Invalid token"

type ActionHandler = ActionRequest -> WebPart

let handleAction actionHandler verificationToken = 
    request (fun req -> 
        match req.formData "payload" with
        | Choice1Of2 actionRequestStr -> 
            actionRequestStr
            |> Json.deserialize<ActionRequest>
            |> validateToken verificationToken
            |> Choice.handle actionHandler (fun v -> Suave.RequestErrors.UNAUTHORIZED "Invalid token")
        | Choice2Of2 s -> 
            printfn "Missing payload: %s" s
            Successful.OK ""
    )

let create endpoint actionHandler verificationToken : WebPart =
    path endpoint >=> handleAction actionHandler verificationToken
