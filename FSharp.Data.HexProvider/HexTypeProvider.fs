﻿namespace FSharp.Data

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
            failwith "Hex string cannot have an odd number of digits"

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
        let valueProp = ProvidedProperty("Value", typeof<byte[]>, IsStatic = true, GetterCode = (fun _ -> <@@ arr @@>))
        root.AddMember(valueProp)
        root
    )

    do self.AddNamespace(ns, [hexType])

[<assembly:TypeProviderAssembly>]
do ()
