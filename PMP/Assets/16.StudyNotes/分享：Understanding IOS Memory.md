# 内存基本介绍

## Process Address Space

当我们使用IOS打开我们的游戏时，内存上究竟发生了什么？

首先，每个进程都会有一个地址空间（Process Address Space），地址空间中会分为很多个区域（Regions）,区域内会细分很多个页（Page 4kb/ A7 16kb）。当应用访问内存中的一个地址时，内存管理单元（MMU ：Memory Management Unit ）会使用页将指定的逻辑地址转换为实际的硬件地址。

![](.\Textures\sUIOSM2.jpg)



通过虚拟内存来实现从地址空间到实际物理内存的映射。

![](.\Textures\sUIOSM1.jpg)

- Process Address Space
  - ​	Regions
    - ​		Pages  (4kb/16kb) ——>   Physical Memory  



## Virtual Memory

物理内存非常好理解，物理硬件所支持的内存大小都是显而易见的。

相对与物理内存而言，虚拟内存就显得那么的不好理解了。

![](.\Textures\sUIOSM3.jpg)

**虚拟内存到物理内存的映射发生在对内存的第一次使用(e.g读写)**  

也就是说，虚拟内存中，有一部分已经与物理内存相关联了，也存在还未与物理内存产生映射的部分。

### Resident Memory

与物理内存产生映射关系的部分：Resident Memory 常驻内存

![](.\Textures\sUIOSM4.jpg)

常驻内存又分为 Clean/Dirty Memory

Clean Memory  最大的特点时只读，系统可以安全地移除或重新加载这部分数据。

通常为：

- ​	应用程序的二进制文件
- ​	内存映射文
- ​	System frameworks*

Dirty Memory  则区别与Clean Memory，这是无法被系统所清除的内存。

这部分也是我们后续内存分析中重点关注的部分。

这有个例子，分配了20000个integers组成的数组，此时会有page被创建，如果将第一个元素和最后一个元素赋值，则第一个page和最后一个page会变成dirty，而中间部分则是clean。

![](.\Textures\sUIOSM6.jpg)



### Malloc Heap

![](.\Textures\sUIOSM7.jpg)

应用申请内存的地方。Unity的内存申请行为都会发生在这里，通过malloc 和calloc 函数进行内存申请

**Maximum Size unknown,seems to be 2x Physical Memory**

![](.\Textures\UIOSMemory1.png)

### Compressed Memory

它属于赃内存。当应用内存不足的时候，OS会将脏内存中使用频次较少的内存进行压缩存放。等需要使用的时候再重新解压出来。

![](.\Textures\sUIOSM8.jpg)

![](.\Textures\sUIOSM9.jpg)



压缩内存   《OS X Mavericks Core Technology Overview》

压缩内存可以在你最需要的时候释放内存，从而使你的 Mac 保持快速和响应。

在你最需要的时候释放内存。当你的系统内存开始耗尽时，压缩内存会

自动压缩内存中最近使用最少的项目，将它们压缩到

约为其原始大小的一半。当再次需要这些项目时，它们可以立即被

解压缩。

压缩内存提高了系统的总带宽和响应速度，使你的 Mac 能够处理大量数据。

你的 Mac 能更有效地处理大量的数据。通过使用字典

的WKdm算法（dictionary-based WKdm algorithm），压缩和解压的速度比读取和写入磁盘的速度快。

写入磁盘。如果你的Mac需要在磁盘上交换文件，压缩后的对象会被存储在全尺寸的片段中。

在全尺寸段中存储，这提高了读/写效率，减少了固态硬盘和闪存驱动器的磨损。

对固态硬盘和闪存驱动器的磨损。压缩内存的优点包括以下几点。

\+ Shrinks memory usage.  缩减内存用量。压缩内存将内存中最近没有使用过的项目的大小减少了50%以上，为你目前正在使用的应用程序释放内存。

\+ Improves power efficiency. 提高能源效率。压缩内存减少了在磁盘上读写虚拟内存交换文件的需要，提高了 Mac 的电源效率。

\+ Minimizes CPU usage.  最大限度地减少CPU的使用。压缩内存的速度非常快，压缩或解压一页内存只需几百万分之一秒的时间。

\+ Is multicore aware.  具有多核意识。与传统的虚拟内存不同，压缩内存可以在多个 CPU 内核上并行运行，在回收未使用的内存和访问内存中很少使用的对象方面，都能实现快如闪电的性能。



**内存压缩使得释放内存变得复杂：**

假如我们有一个Dictionary 对象占用了3个page的内存空间，内存压缩可以将它压缩到一个page大小，这是可以多出两个可以的内存空间。

但如果我们因为收到了内存警告，决定亲自去移除这个dictionary中的一些数据。

首先被压缩的Ditionary会被解压为3个page，然后我们操作移除，使得它的大小变为1个page。实际上是对内存没任何减小的。

![](.\Textures\sUIOSM8.jpg)

![](.\Textures\sUIOSM9.jpg)





### Native (Unity) Memory

Native Memory是游戏Malloc Heap的一部分，用于Unity所需的内存分配，包括Mono Heap。

所有的资产数据也在Native Memory中，以一直轻量的封装(lightweight wrappers)暴露给C#

![](.\Textures\sUIOSM10.jpg)

### Mono Heap

Mono Heap是Native Memory的一部分，为.NET虚拟机的需要而分配。它包含所有被管理的C#的内存分配，并由垃圾收集器维护。

Mono Heap 在 Native Memory 上是不连续的

它以块（block）的形式去申请内存来存储相似大小的对象。

块（block）也存在使用中的块和未被使用的块（unused block）

当一个块经过8次GC后任然未被使用，则它的物理内存会被释放。

![](.\Textures\sUIOSM11.jpg)



### Native Plugins

原生的插件则会在Malloc Heap自行申请内存

而它的二进制文件会在Clean Memory 中

### Outline

- Virtual Memory
  - ​	Process Address Space
    - Page  (4kb/16kb) ——>   Physical Memory  
  - ​	Resident Memory   ==  Physical Memory  
    - ​	Clean Memory  :   read-only ,can remove/loaded by OS
      - native plugins (code binaries)
    - ​	Dirty Memory   ：can't be removed by OS
      - Malloc Heap ：app can allocate memory (malloc & calloc)
        - Native (Unity)  Memory： Unity`s allocations  & All asset data (lightweight wrappers)
          - Mono Heap： discontinuity region , allocated in blocks, physical memory(unused blocks) decommitted after 8 GC 
        - Native Plugins (own allocations)
      - Swapped Memory ：Compresses stale pages & Decompresses pages upon access



## **GPU Driver Memory**

![](.\Textures\sUIOSM5.jpg)



## Memory Footprint













# 内存分析工具介绍











# 演示







## 链接

https://zhuanlan.zhihu.com/p/370467923

https://zhuanlan.zhihu.com/p/87310853

https://docs.google.com/document/d/1J5wbf0Q2KWEoerfFzVcwXk09dV_l1AXJHREIKUjnh3c/edit

https://developer.apple.com/videos/play/wwdc2018/416/

https://blog.csdn.net/cdy15626036029/article/details/81014959

https://v.youku.com/v_show/id_XMzIwNjQ5MjkyOA==.html

