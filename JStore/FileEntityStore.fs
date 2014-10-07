namespace JStore

open System
open System.Collections.Generic
open System.Linq
open PCLStorage

[<Struct>]
type public EntityTypeInfo (``type``: Type) =
    member x.``Type``
        with get() = ``type``
    member x.Filename
        with get() = "Money.Model." + x.Type.Name + ".dat"


//type public FileEntityStore =
//    static member CreateFileStore(folder: IFolder, entitytype: EntityTypeInfo) = 
//        let entitySetType = typedefof(EntitySet<_>).MakeGericType(entityType.Type)