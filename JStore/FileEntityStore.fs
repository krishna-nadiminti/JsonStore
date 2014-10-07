namespace JStore

open System
open System.Collections.Generic
open PCLStorage

[<Struct>]
type public EntityTypeInfo (``type``: Type) =
    member x.``Type``
        with get() = ``type``
    member x.Filename
        with get() = "Money.Model." + x.Type.Name + ".dat"


type public FileEntityStore(folder: IFolder, entityType: IEnumerable<EntityTypeInfo>) = 
    member this.X = "F#"
