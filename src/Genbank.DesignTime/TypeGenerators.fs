module Genbank.DesignTime.TypeGenerators

open ProviderImplementation.ProvidedTypes
open System.Globalization
open System.Reflection
open Genbank.RunTime
open Bio.IO.GenBank

let logger = Logger.createChild (Logger.logger) "TypeGenerators"

//let createGenomeExplorer genome =
//  logger.Log ("exploring %A") genome
//  let latestAssemblies = Helpers.getLatestAssembliesFor(genome)
//  let t = ProvidedTypeDefinition("Assemblies", Some typeof<GenbankAssemblies>, hideObjectMethods = true, nonNullable = true)
//  t.AddMembersDelayed(fun () ->
//    [for assembly in latestAssemblies do
//      let name = assembly.name
//      let file = assembly.file
//      yield ProvidedProperty(
//        name,
//        typeof<GenbankAssembly>,
//        getterCode = (fun (Helpers.Singleton arg) ->
//          <@@ (%%arg : GenbankAssemblies).LoadAssembly(name, file) @@>)
//      )
//    ]
//  )
//  t

let createTestType (assembly: Helpers.AssemblyLocation) () =
  let t = ProvidedTypeDefinition(assembly.name, Some(typeof<obj>), hideObjectMethods = true, nonNullable = true)
  Helpers.loadGenbankFile(assembly.file)(fun items ->
    t.AddMember(ProvidedProperty("Loci", typeof<LociMap>, isStatic = true, getterCode = (fun _ -> <@@ LociMap(items) @@>)))
    t.AddMembers(items |> Seq.map(fun item ->
      let data = item.Metadata.Item("GenBank") :?> GenBankMetadata
      let prop = ProvidedProperty(data.Locus.Name, typeof<GenBankMetadata>, isStatic = true, getterCode = (fun _ -> <@@ data @@>))
      let doc = sprintf "<summary>%s. %s. Keywords: %s</summary>" data.Locus.Name data.Definition data.Keywords
      prop.AddXmlDoc(doc)
      prop
    ) |> Seq.toList)
  )
  t

let createLociType (assembly: Helpers.AssemblyLocation) () =
  let name = assembly.name
  let file = assembly.file
  Helpers.loadGenbankFile(assembly.file)(fun items ->
    ProvidedProperty(
      "Loci",
      typeof<Loci>,
      isStatic = true,
      getterCode = (fun _ -> <@@ Loci(name, file, items) @@>)
    )
  )

let createAssemblyType genome () =
  Helpers.getLatestAssembliesFor(genome)
  |> List.map(fun assembly ->
    let name = assembly.name
    let t = ProvidedTypeDefinition(
              name,
              None,
              hideObjectMethods = true,
              nonNullable = true
            )
    t.AddXmlDoc(sprintf("<summary>The genome for %s. Contains all loci for this genome.</summary>")(genome.name))
    t.AddMemberDelayed(createTestType(assembly))
    t
  )

let createGenomeTypes taxon () =
  logger.Log ("Creating genome types for %A") taxon
  Helpers.loadGenomesForTaxon(taxon)
  |> List.map(fun genome ->
    let prop = ProvidedTypeDefinition(genome.name, None, hideObjectMethods = true, nonNullable = true)
    prop.AddMembersDelayed(createAssemblyType genome)
    prop.AddXmlDoc(sprintf "<summary>A list of assembiles for the %s</summary>" genome.name)
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