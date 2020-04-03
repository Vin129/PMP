### 阅前提示
该文章主要记录Lua语言相关的特性，以及作者使用至今的一些笔记内容。
本篇文章属于不断完善型，结构也会随之改变。
适合人群：lua初级学徒，lua使用人群
阅读方式：目录形式阅读，查漏补缺
## 正文
### Lua是什么

> Lua 是一门非常之小，但五脏俱全的动态语言。它由 Roberto Ierusalimschy、Luiz Henrique de Figueiredo 和 Waldemar Celes在1993年创建。Lua 拥有一组精简的强大特性，以及容易使用的 C API ，这使得它易于嵌入与扩展来表达特定领域的概念。Lua在专有软件界声名显赫。例如，在诸多游戏中，比如 Blizzard（暴雪）公司的《魔兽世界》和 Crytek GmbH 公司的《孤岛危机》，还有 Adobe 的 Photoshop Lightroom ，都使用它来作脚本 和 UI 方面的工作。它继承了 Lisp 和 Scheme，或许还有 AWK 的血脉 ； 在设计上类似于 JavaScript、Icon 和 Tcl。
> **你是如何定义 Lua 的？**
LHF：一种可嵌入，轻量，快速，功能强大的脚本语言。
Roberto：不幸的是，越来越多的人们使用“脚本语言”作为“动态语言”的代名词。现在，甚至是 Erlang 或者 Scheme 都被称为脚本语言。这非常糟糕，因为我们无法精确的描述一类特定的动态语言。在最初的含义解释中，Lua 是一种脚本语言，这种语言通常用来控制其它语言编写的其他组件。
**人们在使用Lua设计软件时，应该注意些什么呢？**
Luiz：我想应该是用 Lua 的方式来做事。不建议去模拟出所有你在其它语言中用到的东西。你应该真的去用这个语言提供的特性，我想对于使用任何一门语言都是这样的。就 Lua 来讲，语言的特性主要指用 table 表示所有的东西，用 metamethod 做出优雅的解决方案。还有 coroutine 。

### 函数
lua中函数是一种“**第一类型值**”，它具有和lua中其他传统类型的值相同的权利。
函数可以被存储到变量中 ——这带来了一种是程序可以变得更精致的函数式编程。
这里有一个概念：函数与其他值一样都是匿名的。
但往往我们会将函数所赋值的对象当作函数名

```lua
func1 = function (...)   ...  end
--  它的另一种“语法糖”形式便是 
function func1 (...) ... end 
```

### 元表

作者认为lua中最为重要的就是**table**这个类型，可以说几乎lua的所有都依附于**table**。table的相关介绍这里不多赘述（资料很多很容易了解到）。这里主要介绍一下元表的概念。

lua中每个值都有一套预定义的操作集合（例如+ - ...）即它的行为规则，即使**table**非常强大，但如果仅仅只遵循预先制定的规则的话往往会很局限。比如我们没法相加两个**table**，这是因为在**table**的预定义操作集合中没有定义**“+”**这个操作。**我们可以通过元表来修改一个值的行为，使其在面对一个非预定义的操作时执行一个指定的操作**。

**例：**在lua中，每个值有一个元表，**table**和**userdata**可以有自己独立的元表。lua在创建t**able**时是不会创建元表的。我们可以通过**setmetatable(tab, mtab)** 来给一个**table**设置或修改元表。

在lua中只能设置**table**的元表元方法  ——> 如何实现两个**table**相加 首先会检查两者之一是否有元表（这里不一定两者必须都有，也不需要两者的元表一定要一样）**table a** 和 **b**  如果一方有元表且定义了元方法，则会使用这个元方法，**如果a b 都存在原方法，会取前者的元方法**。



### 元方法

1、算术类的元方法：**add（加）、sub（减）、mul（乘）、div（除）、unm（相反数）、mod（取模）、pow（乘幂）、concat（连接操作符）**  

 2、关系类的元方法：**eq（等于）、lt（小于）、le（小于等于）**   __

3、库定义的元方法：**tostring（print时调用）、metatable（设置后不可修改元表）**

4、table访问的元方法：**index（查询table）、newindex（修改table的字段）、__mode（弱引用table）**

**而这第四点中所提到的元方法将会使我们去构造lua中的面向对象**。

**（提示：所述的元方法都是带双下划线开头的例如__add）**

```lua
-- 改写tostring
local Mytable = {}
function Mytable.tostring (tab)
    local str = "{"
    for k,v in pairs(tab) do
        str = str..k.."="..tostring(v)..","
    end
    str = str.."}"
    return str
end
mtab = {}
tab = {15,a =  10 , b = "hi",100}
print(tab)
mtab.__tostring = Mytable.tostring -- 设置了该mtab的元方法
setmetatable(tab,mtab) -- 将mtab设置为tab的元表
print(tab)
```

### 面向对象

讲面向对象那首先要讲的就是类。类，是面向对象语言极为重要的概念，它是进行面向对象编程的基础。类是定义同一类所有对象的变量和方法的蓝图或原型。当创建类的实例时，就建立了这种类型的一个对象，然后系统为类定义的实例变量分配内存。然后可以调用对象的实例方法实现一些功能。在lua中如何判断一个集合是否是类呢，主要看它的实例之间是否独立，彼此之间操作不会影响各自集合中的属性。

#### **如何在lua中定义一个类**

其实只需要两个步骤：

1**.设置(__index)** 

2.**创建实例是设置元表元方法**：当我们访问table中不存在的字段时，它首先会检查__index 这个元方法，如果有这个元方法，就由元方法来提供结果，如果没有则为nil。

#### 实现继承

我们已经知道了**（__index）**用于当前table如果查询不到某一字段时会去这个元方法中找寻结果。lua中模拟继承也是通过此来完成的。步骤：

设置一个tableA的__index元方法 => 基类       

将tableA设置为tableB的元表 => 继承

```lua
-- 实现类与类继承
function class(classname,super)
	local superType = type(super)
	local cls
	if super then
		cls = {}
		setmetatable(cls, {__index = super})
		cls.super = super
	else
		cls = {ctor = function() end}
	end
	cls.__cname = classname
	cls.__index = cls
	function cls.new(...)
		local instance = setmetatable({}, cls)
		instance.super = cls.super
		instance:ctor(...)
		return instance
	end
	return cls	
end
```

```lua
-- 使用
SuperClassA = {...}
ClassA = class("ClassA",SuperClassA) -- 设置ClassA的元表为SuperClassA,且添加了new方法创建实例
local classAInstance = ClassA.new();
```

#### 多态

简单介绍就是父类引用指向子类对象，并调用该子类重载的父类方法。调用方法时会根据实际的对象类型进行动态绑定。但lua中没有类型这个概念，所以只能遵循基类生成的对象只能使用基类的方法，子类如果重写了基类的方法，子类对象只能使用子类重写的方法。

#### 多重继承

多重继承意味着一个类可以具有多个基类。这里依然需要通过**（__index）**来实现多重继承。

我们之前只是用**table**来作为**（__index）**，如果要实现多重继承的话，关键在于用一个函数作为**（__index）**。若一个table的元表的**（__index）**字段是一个函数，当在table中找不到一个key时，就会调用这个函数，基于这点可以让**（__index）**函数在其他地方查找缺失的key

```lua
local Class = {}
function Class:search(k,plist)
  -- 从父类列表中搜索k值
    for i = 1,#plist do
        local v = plist[i][k]
        if v then
          return v
        end
    end
end
function Class:CreateClass(p1,p2)
  -- 这里简单的只多重继承两个类
    local c = {}
    local parents = {p1,p2}
    setmetatable(c,{__index = function(t,k) return self:search(k,parents)  end})
    c.__index = c
    function c:new(o)
        o = o or {}
        setmetatable(o,c)
        return o
    end
    return c
end
```

#### 封装

lua的作者其实自己有表明过并不是打算让lua去构建需要很多程序员长期投入的大型程序，lua其实定位在于开发中小型程序，这些通常是一个更大的系统的一部分。参与编程的程序员一般只有一名或是几名，甚至还可以是非程序员（这可能是索引启始为1的缘由）。Lua在基础设计中并没有提供私密性机制，但可以通过其他方法来实现。它的作者提供了一种方法。**这种方法基本思想是通过两个table来表示一个对象，一个table来保存对象状态，一个table用来和外界交流。**

```lua
--一个例子
newCustomer = function(money)
    -- self table 来保存对象的状态
    local self = {money = money,costDiscount = 500 -- 消费满500有折扣} 
    -- 一个计算折扣的私有方法
    local checkDiscount = 
    function(cost)
        if cost > self.costDiscount then
          return 0.8
        else
          return 1
        end
    end
    -- 公共方法
    local shopping = 
    function(cost)
        self.money = self.money - cost*checkDiscount(cost)
    end
    local getMoney = function() return self.money end
    -- 返回一个供外界使用的接口
    return  {shopping = shopping,getMoney = getMoney }
end
    
local a = newCustomer(10000)
a.shopping(2000)
print(a.money) -- nil
print(a.getMoney())
```

