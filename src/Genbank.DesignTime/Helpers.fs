module Genbank.DesignTime.Helpers

open System.IO
open Genbank.Shared

let logger = Logger.createChild (Logger.logger) ("Helpers")

let BaseFile: FTP.FileItem =
  { name = ""
    variant = FTP.Directory
    location = "ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/" }

type AssemblyLocation =
  { name: string
    file: string
  }

let (|Singleton|) = function [l] -> l | _ -> failwith "Parameter mismatch"

let loadAssembliesForGenome(genome: FTP.FileItem) =
  let latestItem = genome.childDirectory("latest_assembly_versions")
  // in the latest assembly location, there should only ever be one item
  let items = latestItem |> FTP.loadDirectory
  let length = List.length(items)
  if length = 0 then
    let message =
      sprintf "Couldn't get latest assembly for %A at %s" genome
        latestItem.location
    logger.Error ("%s") message
    message |> failwith
  items |> List.map(fun item -> { name = item.name; file = item.childFile(item.name + "_genomic.gbff.gz").location })

let loadGenomesForTaxon(variant: FTP.FileItem) = variant |> FTP.getChildDirectories
let loadTaxa() = FTP.getChildDirectories(BaseFile)
