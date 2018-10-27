module Genbank.DesignTime.TypeGenerators

open ProviderImplementation.ProvidedTypes
open System.Globalization
open System.Reflection
open Genbank.RunTime
open Bio.IO.GenBank
open Genbank.Shared

let logger = Logger.createChild (Logger.logger) "TypeGenerators"

// used to capitalize the names of the items correctly
let textInfo = CultureInfo("en-US").TextInfo

let generate (asm: Assembly, ns: string) =
  let top = ProvidedTypeDefinition(
              asm,
              ns,
              "Provider",
              None,
              nonNullable = true
            )

  let loci (assembly: Helpers.AssemblyLocation) () =
    let file = assembly.file
    Helpers.loadGenbankFile(file)(fun items ->
      items
      |> Seq.map(fun item -> item.Metadata.Item("GenBank") :?> GenBankMetadata)
      |> Seq.map(fun item ->
        let locus = item.Locus.Name
        ProvidedProperty(locus, typeof<GenBankMetadata>, isStatic = true, getterCode = (fun _ -> <@@ AssemblyType(file).LoadLocus(locus) @@>))
      )
      |> Seq.toList
    )

  let assemblies genome () =
    Helpers.loadAssembliesForGenome(genome)
    |> List.map(fun assembly ->
      logger.Log("Assembly Name: %s") assembly.name
      let t = ProvidedTypeDefinition(assembly.name, Some typeof<obj>, nonNullable = true)
      let url = assembly.file
      t.AddMember(ProvidedProperty(
                    "Loci",
                    typeof<LociMap>,
                    isStatic = true,
                    getterCode = (fun _ -> <@@ LociMap(url) @@>)
                 ))
      t.AddMembersDelayed(loci(assembly))
      t
    )

  let genomes taxon () =
    Helpers.loadGenomesForTaxon(taxon)
    |> List.map(fun genome ->
      let pretty = textInfo.ToTitleCase(genome.name.Replace("_", " - "))
      let t = ProvidedTypeDefinition(pretty, Some typeof<obj>, nonNullable = true)
      t.AddMembersDelayed(assemblies(genome))
      t
    )

  let taxa () =
    Helpers.loadTaxa()
      |> List.map(fun taxon ->
        let pretty = textInfo.ToTitleCase(taxon.name.Replace("_", " - "))
        let t = ProvidedTypeDefinition(pretty, Some typeof<obj>, nonNullable = true)
        t.AddMembersDelayed(genomes(taxon))
        t
      )

  top.AddMembersDelayed(taxa)

  top
