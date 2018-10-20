namespace Genbank.DesignTime

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type GenbankProvider(config: TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces(config)
  let ns = "Genbank.Provider"
  let asm = Assembly.GetExecutingAssembly()
  
  do this.AddNamespace(ns, TypeGenerators.createTaxaTypes(asm, ns))

[<assembly:TypeProviderAssembly>]
do ()
