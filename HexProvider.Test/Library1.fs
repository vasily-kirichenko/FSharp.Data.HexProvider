module HexProvider.Test

open FSharp.Data

let a1 = Hex<"0101">.Value
let md5 = Hex<"08494b448aa5b1de963731c21344f803">.Value
let sha1 = Sha1<"08494b448aa5b1de963731c21344f80301020304">.Value
let wrongLen = Hex<"011">
let wrongChars = Hex<"0p">.Value