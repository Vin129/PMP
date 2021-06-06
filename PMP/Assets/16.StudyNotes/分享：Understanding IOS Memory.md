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









# 内存分析工具介绍











# 演示







## 链接

https://zhuanlan.zhihu.com/p/370467923

https://zhuanlan.zhihu.com/p/87310853

https://docs.google.com/document/d/1J5wbf0Q2KWEoerfFzVcwXk09dV_l1AXJHREIKUjnh3c/edit

https://developer.apple.com/videos/play/wwdc2018/416/