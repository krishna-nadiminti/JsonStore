﻿namespace JStore

open System
open System.Collections.Generic
open PCLStorage
open System.IO
open System.Threading.Tasks

type public IObjectSerializer<'T> =
    abstract member DeserializeAsync: Stream -> Task<'T>
    abstract member SerializeAsync: stream:Stream -> objectGraph:Object -> Task<Unit>

type public IReadOnlyStore<'T> =
    abstract member LoadAsync: Task<'T>

type public IObjectStore<'T> =
    inherit IReadOnlyStore<'T>
        abstract member SaveAsync: 'T -> Task<bool>
    
type public FileObjectStore<'T>(folder: IFolder, fileName: string, serializer: IObjectSerializer<'T>) =
    
    interface IObjectStore<'T> with
        
        override x.LoadAsync =           
            let load() = 
                async {
                    
                    let! file = Async.AwaitTask <| folder.GetFileAsync fileName

                    use! readStream = Async.AwaitTask <| file.OpenAsync(FileAccess.Read)

                    return serializer.DeserializeAsync readStream                        
                }
                
            let loadFromBack() =
                async{

                    let backupFileName = Path.GetFileNameWithoutExtension(fileName) + ".bak"

                    let! file = Async.AwaitTask <| folder.GetFileAsync backupFileName

                    do! file.CopyAsync(PortablePath.Combine(folder.Path, fileName), NameCollisionOption.ReplaceExisting) |> Async.AwaitIAsyncResult |> Async.Ignore
                    
                    return load() |> Async.RunSynchronously
                }
                             
            try
                load() |> Async.RunSynchronously
            with
            | :? System.Xml.XmlException
            | :? System.Runtime.Serialization.SerializationException ->
                loadFromBack() |> Async.RunSynchronously
            | _ -> reraise()

        override x.SaveAsync (objectGraph: 'T) =
            let save (objectGraph) = 
                async {
                    let! file = Async.AwaitTask <| folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName)
                
                    use! stream = Async.AwaitTask <| file.OpenAsync(FileAccess.ReadAndWrite)
                                
                    do! Async.AwaitTask <| serializer.SerializeAsync stream objectGraph

                    if(file.Name <> fileName) then// created a file with different name, so move it
                        let! existingFile = Async.AwaitTask <| folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists)

                        let backupFileName = Path.GetFileNameWithoutExtension(fileName) + ".bak"

                        do! existingFile.MoveAsync(PortablePath.Combine(folder.Path, backupFileName), NameCollisionOption.ReplaceExisting) |> Async.AwaitIAsyncResult |> Async.Ignore
                
                        do! file.MoveAsync(PortablePath.Combine(folder.Path, fileName), NameCollisionOption.ReplaceExisting) |> Async.AwaitIAsyncResult |> Async.Ignore

                    return true
                }
            Task.Factory.StartNew(fun () -> save(objectGraph) |> Async.RunSynchronously)
        
        