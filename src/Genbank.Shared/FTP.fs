[<RequireQualifiedAccess>]
module Genbank.Shared.FTP

open System.IO
open System

type File =
  | Directory
  | File
  | Symlink

type FileItem =
  { variant: File
    name: string
    location: string }

  member this.childFile child =
    { variant = File
      name = child
      location = this.location + child }

  member this.childSymlink child =
    { variant = Symlink
      name = child
      location = this.location + child + "/" }

  member this.childDirectory child =
    { variant = Directory
      name = child
      location = this.location + child + "/" }

let logger = Logger.createChild (Logger.logger) ("FTP")

let downloadFile(url: string) = Cache.cache.LoadFile(url)

let filenamesFromDirectories (parent: FileItem) (items: string [] list) =
  [ for i in items do
      if i.Length > 1 then
        let fileType: File =
          if i.[0].StartsWith("d") then Directory
          elif i.[0].StartsWith("l") then Symlink
          else File
        yield match fileType with
              // get the symlink name, not it's location
              | Symlink -> parent.childSymlink(Seq.item (Seq.length(i) - 3) (i))
              | Directory -> parent.childDirectory(Seq.last(i))
              | File -> parent.childFile(Seq.last(i)) ]

let loadDirectory(item: FileItem) =
  let stream = Cache.cache.LoadDirectory(item.location)
  let res = (new StreamReader(stream)).ReadToEnd()
  res.Split([| '\r'; '\n' |], StringSplitOptions.RemoveEmptyEntries)
  |> Seq.toList
  |> List.map
       (fun s ->
       s.Split([| ' '; '\t'; '\n' |], StringSplitOptions.RemoveEmptyEntries))
  |> filenamesFromDirectories(item)

let getChildDirectories(item: FileItem) =
  logger.Log ("Loading from URL: %s") item.location
  loadDirectory(item)
  |> List.filter(fun f ->
       match f.variant with
       | Directory -> true
       | _ -> false)
