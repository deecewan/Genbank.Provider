module Genbank.DesignTime.TypeGenerators

open Bio.IO.GenBank
open Genbank.DesignTime.Helpers
open Microsoft.FSharp.Quotations
open ProviderImplementation
open ProviderImplementation.ProvidedTypes
open FSharp.Core.CompilerServices
open FSharp.Quotations
open System.Globalization
open System.IO
open System.Reflection
open Genbank.RunTime

type PropertyType =
  { name: string
    value: string }

let logger = Logger.createChild (Logger.logger) "TypeGenerators"

let ftpUrlType(location: string) =
  let t =
    ProvidedProperty
      ("FTPUrl", typeof<string>, isStatic = true,
       getterCode = (fun _ -> Expr.Value(location)))
  t.AddXmlDoc("The URL where this item was retrieved from.")
  t

let provideSimpleValue(name: string, value: string) =
  ProvidedProperty
    (name, typeof<string>, isStatic = true,
     getterCode = fun _ -> Expr.Value(value))

let loadGenbankFile (location: string) callback =
  logger.Log ("Location of Genbank File: %s") location
  use file = Helpers.downloadFileFromFTP(location)
  use stream =
    file
    |> fun c ->
      new Compression.GZipStream(c, Compression.CompressionMode.Decompress)
  let s =
    Bio.IO.GenBank.GenBankParser().Parse(stream) |> Seq.cast<Bio.ISequence>
  callback s

let createTypeForAssembly(file: Helpers.FTPFileItem) =
  logger.Log ("Creating types for assembly: %A") file
  loadGenbankFile (file.location) (fun genbankFile ->
    genbankFile
    |> Seq.map(fun item ->
         let gb = item.Metadata.Item("GenBank") :?> GenBankMetadata
         let t = ProvidedTypeDefinition(gb.Locus.Name, Some typeof<obj>)
         t.AddMembers([ provideSimpleValue("LocusName", gb.Locus.Name)

                        provideSimpleValue
                          ("AccessionPrimary", gb.Accession.Primary)
                        provideSimpleValue("BaseCount", gb.BaseCount)
                        provideSimpleValue("Origin", gb.Origin)

                        provideSimpleValue
                          ("SourceCommonName", gb.Source.CommonName)
                        provideSimpleValue("Version", gb.Version.Version)
                        provideSimpleValue("Contig", gb.Contig)
                        provideSimpleValue("DbSource", gb.DbSource) ])
         t)
    |> Seq.toList)

let createGenomeExplorer(genome: FTPFileItem) =
  logger.Log ("exploring %A") genome
  let latestAssemblies = Helpers.getLatestAssembliesFor(genome)
  let t = ProvidedTypeDefinition("Assemblies", Some typeof<GenbankAssemblies>, hideObjectMethods = true, nonNullable = true)
  t.AddMembersDelayed(fun () ->
    [for assembly in latestAssemblies do
      let name = assembly.name
      let file = assembly.file
      yield ProvidedProperty(
        name,
        typeof<Assembly>,
        getterCode = (fun (Helpers.Singleton arg) -> <@@ (%%arg : GenbankAssemblies).LoadAssembly(name, file) @@>)
      )
    ]
  )
  t

let createGenomesTypes(variant: FTPFileItem) =
  logger.Log ("Creating genome types for %A") variant
  // load the files for this variant
  Helpers.loadGenomesForVariant(variant)
  |> List.map(fun genome ->
       logger.Log ("Loaded for genome %A") genome
       let genomeType = ProvidedTypeDefinition(genome.name, Some typeof<obj>)
       genomeType.AddMember(ftpUrlType(genome.location))
       genomeType.AddMemberDelayed(fun _ -> createGenomeExplorer(genome))
       genomeType)

let textInfo = CultureInfo("en-US").TextInfo

let createGenomeVariantType (asm: Assembly, ns: string) (root: FTPFileItem) =
  logger.Log ("Creating variant %A") root
  let name = textInfo.ToTitleCase(root.name.Replace("_", ", "))
  let t = ProvidedTypeDefinition(asm, ns, name, Some typeof<obj>)
  let sub = ProvidedTypeDefinition("Genomes", Some typeof<obj>)
  sub.AddMembersDelayed(fun _ -> createGenomesTypes(root))
  t.AddMember(sub)
  t.AddMember(ftpUrlType(root.location))
  logger.Log ("Finished with variant %A") root
  t