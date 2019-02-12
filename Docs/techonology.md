# Technological principles and limitations.

## Asynchronicity.
Page interaction is based on memory-mapped files.
Due to the lack of completion ports I/O there is (almost) no async contracts.
That's it - every operation (except page/record lock waiting) is synchronous, but very fast.

## Thread-safety.
Every operation should be considered thread-safe upon the consistency of atomic physical record operations.
The logical page operations been decorated as physical (autosorting page insertions f.e.) should be thread-safe in that way too.
Of course, overall logical consistency of a data hidden behind records is maintained by the usage logic (by page or mutex locks f.e.).
Thread safety is made in a lock-free way for the record access (the most intensive part). The page creation/deletion has a blocking sync right now.

## Persistence.
There is a strict control flow guarantee to see it's changes after applying (obvious), but there is no strict guarantee to persist them (unless it is explicitly told),
so in case of a unexpected process fail take care of an important records persistence. A good way is an SQL way - you dump fast raw changes into transaction logs (with acknowledgement of storing), then modify a real working page without one.
The benefit is that a group of unrelated changes belonging to the one extent will be persisted later at one piece, optimizing the disk I/O flow.

## Memory management.
Although page allocation and loading from disk into memory is an automatic operation, unloading is not.
The programmer should consider - which pages should be stored in memory for which time (by checking manually which one can be unloaded).
There are a lot of strategies depending on data interaction scenario, probably some of them will be included in future releases.
So beware of a high memory usage and keep track for a pages necessity.

## Exclusive file access.
Due to some header caching operations currently all file access is exclusive for the page manager upon that file.
No other write access from current or other process is available.