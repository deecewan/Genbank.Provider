module Genbank.RunTime

open Bio.IO.GenBank

type GenbankAssembly (name: string, file: string) =
  member __.Name = name
  member __.FileLocation = file

type GenbankAssemblies () =
  member __.LoadAssembly(name: string, file: string) = GenbankAssembly(name, file)

type Genomes (file: string) =
  member __.Load() = "load"

type Genome (seqs: seq<Bio.ISequence>) =
  //let parsed = data.Metadata.Item("GenBank") :?> GenBankMetadata
  member __.x = seqs