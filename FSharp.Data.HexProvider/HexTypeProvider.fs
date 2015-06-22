namespace FSharp.Data

open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Reflection
open System.Runtime.Caching

module internal HexParser =
    open System.Text.RegularExpressions

    let private getHexVal (hex: char) =
        let v = int hex
        //For uppercase A-F letters:
        v - (if v < 58 then 48 else 55)

    let parseString (hashLength: int option) (hex: string) =
        if hex.Length % 2 = 1 then
            failwith "Hex string cannot have an odd number of digits"

        match hashLength with
        | Some x when hex.Length / 2 <> x ->
            failwithf "Hex string must be of length %d." (x * 2)
        | _ -> ()

        let hex = hex.ToUpper()
        let m = Regex.Match (hex, @"^(0[xX])?(?<hex>[0-9A-F]+)$")
        if not m.Success then
            failwith "Hex string contains invalid chars"
        let hex = m.Groups.["hex"].Value
        let arr = Array.zeroCreate (hex.Length >>> 1)
        for i = 0 to arr.Length - 1 do
            arr.[i] <- byte ((getHexVal (hex.[i <<< 1]) <<< 4) + (getHexVal (hex.[(i <<< 1) + 1])))
        arr

[<TypeProvider>]
type HexProvider (_config: TypeProviderConfig) as self =
    inherit TypeProviderForNamespaces()

    let ns = "FSharp.Data"
    let asm = Assembly.GetExecutingAssembly()
    let hexType = ProvidedTypeDefinition(asm, ns, "Hex", Some typeof<obj>, HideObjectMethods = true)
    let parameters = [ProvidedStaticParameter("HexString", typeof<string>)]
    let cache = new MemoryCache("HexProvider")

    do hexType.DefineStaticParameters(parameters, fun typeName args ->
        let value = 
            lazy
                let root = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, HideObjectMethods = true)
                let hexString = args.[0] :?> string
                let arr = HexParser.parseString None hexString
                let valueProp = ProvidedProperty("Value", typeof<byte[]>, IsStatic = true, GetterCode = (fun _ -> <@@ arr @@>))
                root.AddMember(valueProp)
                root
        cache.GetOrAdd (typeName, value))

    do self.AddNamespace(ns, [hexType])
       self.Disposing.Add <| fun _ -> cache.Dispose()

[<TypeProvider>]
type Sha1Provider (_config: TypeProviderConfig) as self =
    inherit TypeProviderForNamespaces()

    let ns = "FSharp.Data"
    let asm = Assembly.GetExecutingAssembly()
    let hexType = ProvidedTypeDefinition(asm, ns, "Sha1", Some typeof<obj>, HideObjectMethods = true)
    let parameters = [ProvidedStaticParameter("HexString", typeof<string>)]
    let cache = new MemoryCache("Sha1Provider")

    do hexType.DefineStaticParameters(parameters, fun typeName args ->
        let value = 
            lazy
                let root = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, HideObjectMethods = true)
                let hexString = args.[0] :?> string
                let arr = HexParser.parseString (Some 20) hexString
                let valueProp = ProvidedProperty("Value", typeof<byte[]>, IsStatic = true, GetterCode = (fun _ -> <@@ arr @@>))
                root.AddMember(valueProp)
                root
        cache.GetOrAdd (typeName, value))

    do self.AddNamespace(ns, [hexType])
       self.Disposing.Add <| fun _ -> cache.Dispose()

[<assembly:TypeProviderAssembly>]
do ()
