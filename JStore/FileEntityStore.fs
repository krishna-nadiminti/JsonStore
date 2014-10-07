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


type public FileEntityStore = class end
//    static member CreateFileStore(folder: IFolder, entitytype: EntityTypeInfo) = 
//        store = Activator.CreateInstance()