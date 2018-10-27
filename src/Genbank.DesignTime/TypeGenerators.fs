module Genbank.DesignTime.TypeGenerators

open ProviderImplementation.ProvidedTypes
open System.Globalization
open System.Reflection
open Genbank.RunTime
open Bio.IO.GenBank

let logger = Logger.createChild (Logger.logger) "TypeGenerators"

let createLociType (assembly: Helpers.AssemblyLocation) () =
  let assemblyName = assembly.name
  let file = assembly.file
  let lociType = ProvidedProperty(
                   "Loci",
                   typeof<LociMap>,
                   isStatic = true,
                   getterCode = (fun _ -> <@@ LociMap(file) @@>)
                 )
  lociType :: Helpers.loadGenbankFile(assembly.file)(fun items ->
    items
    |> Seq.map(fun item ->
      let data = item.Metadata.Item("GenBank") :?> GenBankMetadata
      let name = data.Locus.Name
      logger.Log("%s")(name)
      let prop = ProvidedProperty(
                  data.Locus.Name,
                  typeof<string>,
                  isStatic = true,
                  getterCode = (fun _ -> <@@ AssemblyType(assemblyName, file).LoadLocus(name) @@>)
                 )
      let doc = sprintf "<summary>%s. %s. Keywords: %s</summary>" data.Locus.Name data.Definition data.Keywords
      prop.AddXmlDoc(doc)
      prop
    )
    |> Seq.toList
  )

let createAssemblyType genome () =
  Helpers.getLatestAssembliesFor(genome)
  |> List.map(fun assembly ->
    let name = assembly.name
    let t = ProvidedTypeDefinition(
              name,
              Some(typeof<AssemblyType>),
              hideObjectMethods = true,
              nonNullable = true
            )
    t.AddXmlDoc(sprintf("<summary>The genome for %s. Contains all loci for this genome.</summary>")(genome.name))
    t.AddMembersDelayed(createLociType(assembly))

    t
  )

let createGenomeTypes taxon () =
  logger.Log ("Creating genome types for %A") taxon
  Helpers.loadGenomesForTaxon(taxon)
  |> List.map(fun genome ->
    let prop = ProvidedTypeDefinition(genome.name, None, hideObjectMethods = true, nonNullable = true)
    prop.AddMembersDelayed(createAssemblyType genome)
    prop.AddXmlDoc(sprintf "<summary>A list of assemblies for the %s</summary>" genome.name)
    prop
  )

let textInfo = CultureInfo("en-US").TextInfo

let createTaxaTypes (asm: Assembly, ns: string) =
  Helpers.loadTaxa()
  |> List.map(fun taxon ->
    logger.Log ("Creating variant %A") taxon
    let name = textInfo.ToTitleCase(taxon.name.Replace("_", ", "))
    let t = ProvidedTypeDefinition(asm, ns, name, None, hideObjectMethods = true, nonNullable = true)
    t.AddMembersDelayed(createGenomeTypes(taxon))
    logger.Log ("Finished with variant %A") taxon
    t
  )