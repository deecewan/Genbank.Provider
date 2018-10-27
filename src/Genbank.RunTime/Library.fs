namespace Genbank.RunTime

open Bio.IO.GenBank
open System.Collections
open Genbank.Shared

type LociMap (dataURL: string) =
  // let metadata = seq { for item in data -> item.Metadata.Item("GenBank") :?> GenBankMetadata}
  let metadata = Helpers.loadGenbankFile(dataURL)(fun items ->
     items |> Seq.map(fun item -> item.Metadata.Item("GenBank") :?> GenBankMetadata)
  )
  interface seq<GenBankMetadata> with member __.GetEnumerator() = metadata.GetEnumerator()
  interface IEnumerable with member __.GetEnumerator() = metadata.GetEnumerator() :> _

type AssemblyType (url: string) =
  member __.Location = url;

  member __.LoadLocus(name: string) =
    Helpers.loadGenbankFile(url)(fun items ->
      items
      |> Seq.map(fun item -> item.Metadata.Item("GenBank") :?> GenBankMetadata)
      |> Seq.find(fun item -> item.Locus.Name = name)
    )

type Taxon (name, location) =
  member __.Name = name
  member __.Location = location

type Taxa () =
  member __.Test = 1
