namespace JStore

open System

[<Struct>]
type EntityTypeInfo (``type``: Type) =
    member x.``Type``
        with get() = ``type``
    member x.Filename
        with get() = "Money.Model." + x.Type.Name + ".dat"


type FileEntityStore() = 
    member this.X = "F#"
