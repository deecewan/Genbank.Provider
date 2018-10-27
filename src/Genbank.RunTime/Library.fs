module Genbank.RunTime

open Bio.IO.GenBank
open System.Collections

type Loci (name: string, file: string, data: seq<Bio.ISequence>) =
  let metadata = seq { for item in data -> item.Metadata.Item("GenBank") :?> GenBankMetadata}
  member __.Name = name
  member __.FileLocation = file
  member __.Metadata = data

  interface seq<GenBankMetadata> with member __.GetEnumerator() = metadata.GetEnumerator()
  //interface System.Collections.Generic.IEnumerable<GenBankMetadata> with member __.GetEnumerator() = metadata.GetEnumerator()
  interface IEnumerable with member __.GetEnumerator() = metadata.GetEnumerator() :> _

type LociMap (data: seq<Bio.ISequence>) =
  let metadata = seq { for item in data -> item.Metadata.Item("GenBank") :?> GenBankMetadata}
  interface seq<GenBankMetadata> with member __.GetEnumerator() = metadata.GetEnumerator()
  interface IEnumerable with member __.GetEnumerator() = metadata.GetEnumerator() :> _

type TestType () = class end