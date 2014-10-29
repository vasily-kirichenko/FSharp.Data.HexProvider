namespace FSharp.Data

open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Reflection

module internal HexParser =
    open System.Text.RegularExpressions

    let private getHexVal (hex: char) =
        let v = int hex
        //For uppercase A-F letters:
        v - (if v < 58 then 48 else 55)

    let parseString (hex: string) =
        if hex.Length % 2 = 1 then
            failwith "The binary key cannot have an odd number of digits"

        let hex = hex.ToUpper()
        
        if not (Regex.IsMatch (hex, @"^[0-9A-F]+$")) then
            failwith "Hex string contains invalid chars"

        let arr = Array.zeroCreate (hex.Length >>> 1)
        for i = 0 to arr.Length - 1 do
            arr.[i] <- byte ((getHexVal (hex.[i <<< 1]) <<< 4) + (getHexVal (hex.[(i <<< 1) + 1])))
        arr

[<TypeProvider>]
type HexProvider (_config : TypeProviderConfig) as self =
    inherit TypeProviderForNamespaces()

    let ns = "FSharp.Data"
    let asm = Assembly.GetExecutingAssembly()
    let hexType = ProvidedTypeDefinition(asm, ns, "Hex", Some typeof<obj>, HideObjectMethods = true)
    let parameters = [ProvidedStaticParameter("HexString", typeof<string>)]

    do hexType.DefineStaticParameters(parameters, fun typeName args ->
        let root = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, HideObjectMethods = true)
        let hexString = args.[0] :?> string
        let arr = HexParser.parseString hexString
        let myProp = ProvidedProperty("Value", typeof<byte[]>, IsStatic = true,
                                       GetterCode = (fun _ -> <@@ arr @@>))
        root.AddMember(myProp)
        root
    )

    do self.AddNamespace(ns, [hexType])

[<assembly:TypeProviderAssembly>]
do ()
