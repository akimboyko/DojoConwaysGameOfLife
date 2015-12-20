Yet another Conway's Game of Life
=================================

Coderetreat 20130427
--------------------

Implementation details
* Inspired by CodeRetreat session at Kiev 27th of April
* All collections are immutable, implemented by`Microsoft.Bcl.Immutable`
 * `ImmutableHashSet`, `ImmutableQueue` and `ImmutableList`
 * Using of ex-`builder` concept like `ImmutableQueue.Create(collection)`
* All game related algorithms are implemented using `LINQ to Objects`
* Game itself is running from console using ScriptCS script

Coderetreat 20131214
--------------------

Scala based implementation
