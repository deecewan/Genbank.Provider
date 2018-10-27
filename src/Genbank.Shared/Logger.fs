module Genbank.Shared.Logger

open System.IO
open System

//let console = new Forest.ConsoleWriter.ConsoleWriter()
//let logFolder =
//  Path.Combine
//    (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs",
//     "Genbank.Provider")
//let file = new Forest.FileWriter.FileWriter(logFolder, true)
// let logger = Forest.create ("Genbank.Provider") [(*console; file *)]
//let createChild = Forest.createChild

type Logger() =
  member __.Log(msg) = printfn(msg)
  member __.Warn(msg) = printfn(msg)
  member __.Error(msg) = printfn(msg)

let create () = Logger()
let createChild logger name = Logger()

let logger = create()
