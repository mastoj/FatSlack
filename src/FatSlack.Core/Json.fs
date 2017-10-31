[<RequireQualifiedAccess>]
module FatSlack.Core.Json

open Newtonsoft.Json
open Newtonsoft.Json.Linq

let serializerSettings = new JsonSerializerSettings()
serializerSettings.ContractResolver <- new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

let deserialize<'a> str = 
    JsonConvert.DeserializeObject<'a>(str, serializerSettings)

let serialize x = JsonConvert.SerializeObject(x, serializerSettings)

let parse str = 
    try 
        Some (JObject.Parse(str))
    with
    | ex -> 
        printfn "Failed to parse: %s" str    
        None
let getValue key (jObj:JObject) = 
    if jObj.[key] |> isNull then None
    else Some jObj.[key]

let getStringFromToken (token:JToken) = token.ToString()