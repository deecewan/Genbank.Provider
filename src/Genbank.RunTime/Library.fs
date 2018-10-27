module Genbank.RunTime

open Bio.IO.GenBank
open System.Collections

type LociMap (dataURL: string) =
  // let metadata = seq { for item in data -> item.Metadata.Item("GenBank") :?> GenBankMetadata}
  let metadata = ["test"; dataURL] |> List.toSeq
  interface seq<string> with member __.GetEnumerator() = metadata.GetEnumerator()
  interface IEnumerable with member __.GetEnumerator() = metadata.GetEnumerator() :> _

type TestType () = class end
