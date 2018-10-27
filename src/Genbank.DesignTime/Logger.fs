module Genbank.DesignTime.Logger

open System.IO
open System

let logger = Genbank.Shared.Logger.create()
let createChild = Genbank.Shared.Logger.createChild
