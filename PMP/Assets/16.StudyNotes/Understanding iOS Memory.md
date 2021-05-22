# 前言

> Properly managing memory on iOS devices is extremely important, since not doing so will get a game terminated by the OS, resulting in what a user would perceive as a crash, Users usually don't like when games crash and tend to leave one-star reviews.
> To assess a game's memory consumption, it is advised to frequently profile the game on target devices, looking for crashes and memory leaks. For this purpose Unity and Apple provide profiling tools which if used correctly can tell a Unity developer everything she needs to know about the game.
>
> The tools provided are the following:
>
> 	1. Unity Memory Profiler
>  	2. MemoryProfiler(on BitBucket)
>  	3. MemoryProfiler Extension(on Github)
>  	4. Xcode memory gauge in Debug Navigator view
>  	5. VM Tracker Instrument
>  	6. Allocations Instrument



正确的管理IOS设备的内存的内存非常的重要，因为如果不这样做将使游戏被系统所终止，从而导致用户认为这是一个崩溃。用户通常不喜欢游戏崩溃并且会使他倾向于给与差评。

为了评估游戏的内存消耗，建议经常在目标设备上分析游戏，以查找crash和内存泄漏。为此Unity和Apple提供了性能分析工具，使用得当的话，它可以告诉开发者关于游戏中他想要了解的一切。



# Kinds of Memory

> When a developer asks the question “how much memory does my game use?’ usually one of these tools has a good answer. The problem, however, is that the question is ambiguous — the word “memory” can mean several different kinds of memory. So it is critical to understand what kind of memory the question is referring to. The existence of different kinds of memory is what creates a layer of confusion and deems the topic of memory on i0S too complicated to bother.
> This document describes the nature of memory in iOS and goes into details of what data the mentioned tools provide. The information provided is applicable to other platforms, but differences in implementation are not discussed here.

当开发人员问“我的游戏使用了多少内存？”时，通常其中一种工具会提供很好的答案。但是这个问题是模糊不清的，对于**“内存”**可能意味着几种不同类型的内存。因此，了解该问题指的是哪种内存至关重要。各种内存的存在造成了一片混乱，并认为IOS中关于内存的话题过于复杂。

本文档介绍了IOS中内存的性质，并详细介绍了上述工具提供的数据。提供的信息适用于其他平台，但是此处不讨论实现上的差异。



## System Memory

### Physical Memory (RAM)

> Physical Memory is the physical device memory on-chip inside an iPhone or iPad. It has a physical limit (for example 512Mb or 1Gb) and just can't hold more data. Each running application occupies some amount of Physical Memory, but in modern operating systems (like iOS) applications never work directly with memory on-chip. Instead, they deal with so-called Virtual Memory which the OS seamlessly maps into Physical Memory.

物理内存是iPhone或iPad内寄存器上的物理设备内存。它具有物理限制（例如512Mb或1Gb），并且不能容纳更多数据。每个正在运行的应用程序都占用一定大小的物理内存，但是在现代操作系统（如IOS）中，应用程序永远无法直接使用物理内存。取而代之，它们使用所谓的虚拟内存，操作系统将其无缝映射到物理内存。

### Virtual Memory (VM)

> Virtual Memory is what the game sees as its address space, where it can allocate memory and hold pointers to that memory.
> When a process starts, the OS creates a logical address space (or “virtual” address space) for the process. It's called "virtual" because the address space exposed to the process does not necessarily align with the physical address space of the machine, or even the virtual address space of other applications.

虚拟内存是游戏视为其地址空间的东西，它可以分配内存并持有指向该内存的指针。

当一个进程启动时，操作系统为该进程创建一个逻辑地址空间（或 "虚拟 "地址空间）。它被称为 "虚拟"，因为暴露给进程的地址空间不一定与机器的物理地址空间一致，甚至与其他应用程序的虚拟地址空间也不一致。

> The OS divides this address space into uniformly-sized chunks of memory called pages. The processor and its memory management unit (MMU) maintain a page table to map pages in the process's logical address space to hardware addresses in the computer's RAM. When an applications code accesses an address in memory, the MMU uses the page table to translate the specified logical address into the actual hardware memory address. This translation occurs automatically and is transparent to the running application.

操作系统将这个地址空间划分为统一大小的内存块，称为**"页"**。处理器和它的内存管理单元（MMU）维护一个页表，将进程的逻辑地址空间中的页映射到计算机RAM中的硬件地址。当应用程序代码访问内存中的一个地址时，MMU使用页表将指定的逻辑地址转换为实际的硬件内存地址。这种转换是自动发生的，对运行中的应用程序是透明的。

> In earlier versions of iOS, the size of a page is 4 kilobytes. In later versions of iOS, A7- and A8-based systems expose 16-kilobyte pages to the 64-bit userspace backed by 4-kilobyte physical pages, while A9 systems expose 16-kilobyte pages backed by 16-kilobyte physical pages.
> Virtual Memory consists of several regions, including code segments, dynamic libraries, GPU driver memory, malloc heap and others.

在早期版本的iOS中，一个页面的大小是4千字节。在后来的iOS版本中，基于A7和A8的系统向64位用户空间暴露了16千字节的页面，由4千字节的物理页面支持，而A9系统暴露了16千字节的页面，由16千字节的物理页面支持。

虚拟内存由几个区域组成，包括代码段、动态库、GPU驱动内存、malloc堆和其他。

### GPU Driver Memory

> GPU Driver Memory consists of allocations in Virtual Memory used by the driver and essentially being video memory on iOS.
> iOS features so-called unified architecture, where CPU and GPU share the same memory (though on modern hardware GPU has higher bandwidth to this memory). The allocations are done by the driver and mostly consist of texture and mesh data.

GPU驱动内存由驱动使用的虚拟内存中的分配组成，基本上是iOS上的视频内存(video memory)。

iOS具有所谓的统一架构，CPU和GPU共享相同的内存（尽管在现代硬件上GPU有更高的带宽到这个内存）。这些分配是由驱动程序完成的，主要包括纹理和网格数据。

### Malloc Heap

> Malloc Heap is a Virtual Memory region where an application can allocate memory using malloc and calloc functions.
> In other words, this is a chunk of virtual address space available for the application's memory allocations.
> Apple doesn't publish the maximum size of Malloc Heap. In theory, Virtual Memory address space is only limited by pointer size, which is defined by the processor architecture, i.e., roughly 4 gigabytes of logical memory space on 32-bit processors, and 18 exabytes on 64-bit processors. But in reality, the actual limits seem to depend on device and iOS version and are much lower than one might think. A simple app which continuously allocates Virtual Memory gives the following values:

Malloc Heap是一个虚拟内存区域，应用程序可以使用malloc和calloc函数分配内存。

换句话说，这是一块可供应用程序分配内存的虚拟地址空间。

苹果公司并没有公布Malloc Heap的最大尺寸。理论上，虚拟内存地址空间只受指针大小的限制，而指针大小是由处理器架构定义的，即在32位处理器上大约有4千兆字节的逻辑内存空间，在64位处理器上大约有18兆字节。但实际上，实际限制似乎取决于设备和iOS版本，比人们想象的要低得多。一个持续分配虚拟内存的简单应用给出了以下数值。

![](\Textures\UIOSMemory1.png)



> Theoretically, itis possible to exhaust Virtual Memory address space by using too many memory-mapped files.

理论上，通过使用过多的内存映射文件，有可能耗尽虚拟内存地址空间。



### Resident Memory

> Resident Memory is the amount of Physical Memory the game actually uses.
> A process can allocate a block of memory from Virtual Memory, but for the OS to actually reserve a corresponding block of Physical Memory the process has to write to this block. In this case, the allocated block of memory will become a part of the application's Resident Memory.

常驻内存是游戏实际使用的物理内存的大小。

一个进程可以从虚拟内存中分配一个内存块，但是为了让操作系统实际保留一个相应的物理内存块，该进程必须向这个内存块写入。在这种情况下，分配的内存块将成为应用程序常驻内存的一部分。



### Paging

> Paging is a process of moving Physical Memory pages in and out from memory to a backing store.
> When a process tries to allocate and use a block of Virtual Memory, the OS looks for free memory pages in Physical Memory and maps them to the allocated Virtual Memory pages (thus making these pages a part of the application's Resident Memory).

分页是一个将物理内存页从内存中移入和移出到后备存储的过程。

**当一个进程试图分配和使用一个虚拟内存块时，操作系统在物理内存中寻找空闲的内存页，并将它们映射到分配的虚拟内存页（从而使这些页成为应用程序常驻内存的一部分）。**

> If there are no free pages available in Physical Memory, the OS tries to release existing pages to make room for the new pages. How the system releases pages depend on the platform. Usually, some pages deemed rarely used are moved to a backing store like a page file on disk. This process is known as paging out.
> But in case of iOS, there is no backing store, so pages are never paged out to disk. Though, read-only pages can still be removed from memory and reloaded from disk as needed. This process is known as paging in.

如果物理内存中没有可用的页面，操作系统会尝试释放现有的页面，以便为新的页面腾出空间。系统如何释放页面取决于平台。通常，一些被认为很少使用的页面会被移到一个像磁盘上的页面文件一样的后备存储中。这个过程被称为 "paging out"。

但在iOS中，没有后备存储，所以页面永远不会被分页到磁盘。不过，只读的页面仍然可以从内存中删除，并在需要时从磁盘上重新加载。这个过程被称为 "paging in"。

> If an application accesses an address on a memory page that is not currently in Physical Memory, a page fault occurs. When that happens, the Virtual Memory system invokes a special page-fault handler to respond to the fault immediately. The page-fault handler stops the currently executing code, locates a free page of Physical Memory, loads the page containing the needed data from backing store, updates the page table, and then returns control to the program's code.

如果一个应用程序访问一个当前不在物理内存中的内存页上的地址，就会发生一个页故障。当这种情况发生时，虚拟内存系统会调用一个特殊的页面故障处理程序来立即响应这个故障。页面故障处理程序停止当前执行的代码，找到物理内存的一个空闲页面，从备份存储中加载包含所需数据的页面，更新页面表，然后将控制权返回给程序的代码。

