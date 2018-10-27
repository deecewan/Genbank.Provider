module Genbank.Shared.Helpers

open System.IO

let loadGenbankFile (location: string) callback =
  Logger.logger.Log ("Location of Genbank File: %s") location
  let file = FTP.downloadFile(location)
  let stream =
    file
    |> fun c ->
      new Compression.GZipStream(c, Compression.CompressionMode.Decompress)
  let s =
    Bio.IO.GenBank.GenBankParser().Parse(stream) |> Seq.cast<Bio.ISequence>
  callback s
