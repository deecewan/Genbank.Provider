module Genbank.DesignTime.Logger

open System.IO
open System

let console = new Forest.ConsoleWriter.ConsoleWriter()
let logFolder =
  Path.Combine
    (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Logs", 
     "Genbank.Provider")
let file = new Forest.FileWriter.FileWriter(logFolder, true)
let logger = Forest.create ("Genbank.Provider") [ console; file ]
let createChild = Forest.createChild
