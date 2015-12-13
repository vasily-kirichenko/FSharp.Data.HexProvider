module HexProvider.Test

open FSharp.Data

type TwoBytes = Hex<2>

let a1 = TwoBytes.Parse<"0101">()
let md5 = Hex<20>.Parse<"08494b448aa5b1de963731c21344f803">()
let sha1 = Hex<20>.Parse<"08494b448aa5b1de963731c21344f80301020304">()
let wrongLen = Hex<2>.Parse<"011">()
let wrongChars = Hex<1>.Parse<"0p">()