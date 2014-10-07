namespace JStore

open System
open System.Collections.Generic
open System.Linq
open PCLStorage
open Money.Model

[<Struct>]
type public EntityTypeInfo (``type``: Type) =
    member x.``Type``
        with get() = ``type``
    member x.Filename
        with get() = "Money.Model." + x.Type.Name + ".dat"


type public FileEntityStore =

    let findObjectStore = ()

    member x.SaveAsync<'T when 'T :> EntityBase>(entities: EntitySet<'T>) =
        let fileStore = findObjectStore<'T>()
        Async.AwaitTask <| fileStore.SaveAsync(entities)

    static member private CreateFileStore(folder: IFolder, entityType: EntityTypeInfo) =
     
        let entitySetType = typedefof<EntitySet<_>>.MakeGenericType(entityType.Type)

        let fileObjStoreType = typedefof<FileObjectStore<_>>.MakeGenericType(entitySetType)

        Activator.CreateInstance(fileObjStoreType, folder, entityType.Filename)

    