[<AutoOpen>]
module internal FSharp.Data.Utils

open System.Runtime.Caching
open System
open ProviderImplementation.ProvidedTypes
 
type MemoryCache with  
    member x.GetOrAdd(key, value: Lazy<_>, ?expiration) = 
        let policy = CacheItemPolicy()
        policy.SlidingExpiration <- defaultArg expiration <| TimeSpan.FromHours 24.
        match x.AddOrGetExisting(key, value, policy) with
        | :? Lazy<ProvidedTypeDefinition> as item -> item.Value 
        | x -> 
            assert(x = null)
            value.Value
