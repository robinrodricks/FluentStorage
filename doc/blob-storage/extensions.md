# Blob Storage Extensions

`IBlobStorage` interface is kept really thin to allow creating new storage implementations in shorter time. Storage.Net supports rich functionality on blobs by implementing a set of extension methods which work on top of `IBlobStorage` interface.