#load ".paket/load/main.group.fsx"
#r "src/Genbank.RunTime/bin/Debug/netstandard2.0/Genbank.RunTime.dll"
#r "src/Genbank.DesignTime/bin/Debug/netstandard2.0/Genbank.DesignTime.dll"

Genbank.Provider.Archaea.``Acidianus - Brierleyi``.``GCA_003201835.1_ASM320183v1``.Loci |> Seq.iter(fun x -> printfn("Value: %A") x.Locus.Name)
Genbank.Provider.Archaea.``Acidianus - Brierleyi``.``GCA_003201835.1_ASM320183v1``.CP029289 |> printfn "Metadata: %A"
