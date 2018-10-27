namespace Genbank.DesignTime

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection
open System

[<TypeProvider>]
type GenbankProvider(config: TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces(config)
  let ns = "Genbank.Provider"
  let asm = Assembly.GetExecutingAssembly()

  do
    try
      this.AddNamespace(ns, TypeGenerators.createTaxaTypes(asm, ns))
    with
    | x -> failwith(sprintf("you dun goofed %A")(x))

[<assembly:TypeProviderAssembly>]
do ()