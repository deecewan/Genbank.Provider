module Genbank.RunTime

open Bio.IO.GenBank
open System.Collections

type LociMap (dataURL: string) =
  // let metadata = seq { for item in data -> item.Metadata.Item("GenBank") :?> GenBankMetadata}
  let metadata = ["test"; dataURL] |> List.toSeq
  interface seq<string> with member __.GetEnumerator() = metadata.GetEnumerator()
  interface IEnumerable with member __.GetEnumerator() = metadata.GetEnumerator() :> _

type AssemblyType (name: string, url: string) =
  member __.Name = name;
  member __.Location = url;

  member __.LoadLocus(name: string) =
    name

  member this.LociMap() =
    LociMap(this.Location)