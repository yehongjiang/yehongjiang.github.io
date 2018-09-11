# Python3基础
## 输入和输出
1）输出  
<pre>>>>print('hello, world')</pre>


还可以接受多个字符串，逗号隔开，运行时逗号变成空格,并且逗号可以分隔变量类型  
<pre>>>>print('The quick brown fox', 'jumps over', 'the lazy dog')
The quick brown fox jumps over the lazy dog</pre>
2）输入  
<pre>>>>name = input()
Michael
#在input()中加入字符串可以用作输入提示语
>>>name = input('请输入你的名字：')</pre>
tips:当语句以冒号:结尾时，缩进的语句视为代码块。  
tips:Python程序是大小写敏感的  
## 数据类型
1）整数  
tips:计算机由于使用二进制，所以，有时候用十六进制表示整数比较方便，十六进制用0x前缀和0-9，a-f表示，例如：0xff00，0xa5b4c3d2   
tips:另外大小没有限制   
2）浮点数  
tips:但是对于很大或很小的浮点数，就必须用科学计数法表示，把10用e替代，1.23x109就是1.23e9，或者12.3e8，0.000012可以写成1.2e-5  
tips:浮点数也没有大小限制，但是超出一定范围就直接表示为inf（无限大）。  
3）字符串  
tips:字符串是以单引号'或双引号"括起来的任意文本，比如'abc'，"xyz"等等。请注意，''或""本身只是一种表示方式，不是字符串的一部分，因此，字符串'abc'只有a，b，c这3个字符。如果'本身也是一个字符，那就可以用""括起来，比如"I'm OK"包含的字符是I，'，m，空格，O，K这6个字符。  
4）转义字符还是和c一样用‘\’  
tips:如果字符串里面有很多字符都需要转义，就需要加很多\，为了简化，Python还允许用r''表示''内部的字符串默认不转义，可以自己试试：  
<pre>>>> print('\\\t\\')
\       \
>>> print(r'\\\t\\')
\\\t\\</pre>  
tips:如果字符串内部有很多换行，用\n写在一行里不好阅读，为了简化，Python允许用'''...'''的格式表示多行内容  
<pre>>>> print('''line1
... line2
... line3''')
line1
line2
line3</pre>  
5）布尔值
#  
- True 和 False 首字母要大写
- or 运算符相当于C的||
- not 运算符是非运算符

6）空值  
有个None，不能理解为0，具体用法没详说  
7）变量  
python的变量属于动态变量，意味着可以给同一个变量不断赋不同类型的值，与之对应的静态变量需要先定义类型  
<pre>a = 'ABC'
b = a
a = 'XYZ'
print(b)
#输出的是ABC，这说明此处的变量就是我传统意义上的变量用法，
后面的列表复制类似于指针，结合上C++的知识这一块还是很好理解的。

</pre>
8）常量  
Python中约定用大写变量名表示常量，但本质还是变量  
9）除法与求余  
# 
- 第一种是 / 这种除法结果为浮点数
- 第二种是 // 称为地板除，两个整数的除法仍然是整数（向下取整）
- 求余还是%

## 字符串和编码
1）Python的字符串  
最新的py3版本是Unicode编码  
<pre>
>>> ord('A') #转换为编码
65
>>> ord('中')
20013
>>> chr(66) #转换为对应字符
'B'
>>> chr(25991)
'文'
</pre>
tips:由于Python的字符串类型是str，在内存中以Unicode表示，一个字符对应若干个字节。如果要在网络上传输，或者保存到磁盘上，就需要把str变为以字节为单位的bytes。  
Python对bytes类型的数据用带b前缀的单引号或双引号表示：
<pre>
x = b'ABC
</pre>
tips:len()函数可以计算str包含多少个字符，就算中文也是按个计算，如果换成bytes则计算字节数  
tips:由于Python源代码也是一个文本文件，所以，当你的源代码中包含中文的时候，在保存源代码时，就需要务必指定保存为UTF-8编码。当Python解释器读取源代码时，为了让它按UTF-8编码读取，我们通常在文件开头写上这两行：
<pre>
#!/usr/bin/env python3
# -*- coding: utf-8 -*-
</pre>
在Python中，采用的格式化方式和C语言是一致的，用%实现,并且具有替换作用（就算不是该格式的），举例如下：
<pre>
>>> 'Hello, %s' % 'world'
'Hello, world'
>>> 'Hi, %s, you have $%d.' % ('Michael', 1000000)
'Hi, Michael, you have $1000000.'
</pre>
你可能猜到了，%运算符就是用来格式化字符串的。在字符串内部，%s表示用字符串替换，%d表示用整数替换，有几个%?占位符，后面就跟几个变量或者值，顺序要对应好。如果只有一个%?，括号可以省略。 

|占位符|替换内容|  
|:--|:--|   
|%d|整数|  
|%f|浮点数|  
|%s|字符串|  
|%x|十六进制整数|

**format()**
另一种格式化字符串的方法是使用字符串的format()方法，它会用传入的参数依次替换字符串内的占位符{0}、{1}……，不过这种方式写起来比%要麻烦得多：<pre>>>>'Hello, {0}, 成绩提升了 {1:.1f}%'.format('小明', 17.125)
'Hello, 小明, 成绩提升了 17.1%'
</pre>

## 使用list和tuple
1）list列表
<pre>>>> classmates = ['Michael', 'Bob', 'Tracy']
>>> classmates
['Michael', 'Bob', 'Tracy']</pre>
classmates.insert(1, 'Jack')把元素插入到指定的位置，比如索引号为1的位置  
classmates.pop()  删除list末尾的元素，用pop()方法  
要删除指定位置的元素，用pop(i)方法，其中i是索引位置  
tips:list里面的元素的数据类型也可以不同  
<pre>L = ['Apple', 123, True]</pre>

2）tuple元祖  
不能修改的列表  
<pre> classmates = ('Michael', 'Bob', 'Tracy')</pre>
tips:tuple的陷阱：当你定义一个tuple时，在定义的时候，tuple的元素就必须被确定下来
<pre>>>> t = (1, 2)
>>> t
(1, 2)
>>> t = ()
>>> t
()</pre>
但是，要定义一个只有1个元素的tuple，如果你这么定义：
<pre>>>> t = (1)
>>> t
1</pre>
定义的不是tuple，是1这个数！这是因为括号()既可以表示tuple，又可以表示数学公式中的小括号，这就产生了歧义，因此，Python规定，这种情况下，按小括号进行计算，计算结果自然是1。

所以，只有1个元素的tuple定义时必须加一个逗号,，来消除歧义：
<pre>
>>> t = (1,)
>>> t
(1,)</pre>
## 条件判断  
tips:elif 相当于 C++的else if  
<pre>if x:
    print('True')</pre>
只要x是非零数值、非空字符串、非空list等，就判断为True，否则为False。  
## 循环
<pre>>>> list(range(5))
[0, 1, 2, 3, 4]</pre>
## 使用dict和set
1）dict就是字典，c++中的map  
<pre>>>> d = {'Michael': 95, 'Bob': 75, 'Tracy': 85}
>>> d['Michael']
95</pre>
tips:如果key不存在，dict就会报错  
tips:要避免key不存在的错误，有两种办法，一是通过in判断key是否存在  
tips:二是通过dict提供的get()方法，如果key不存在，可以返回None，或者自己指定的value：
<pre>>>> d.get('Thomas')
>>> d.get('Thomas', -1)
-1</pre>
要删除一个key，用pop(key)方法，对应的value也会从dict中删除：
<pre>
>>> d.pop('Bob')
75
>>> d
{'Michael': 95, 'Tracy': 85}</pre>  
tips:请务必注意，dict内部存放的顺序和key放入的顺序是没有关系的。  
tips:要保证hash的正确性，作为key的对象就不能变。在Python中，字符串、整数等都是不可变的，因此，可以放心地作为key。而list是可变的，就不能作为key  
2）set就是C++里的set意思
要创建一个set，需要提供一个list作为输入集合：
<pre>
>>> s = set([1, 1, 2, 3])
>>> s
{1, 2, 3}</pre>
通过add(key)方法可以添加元素到set中  
通过remove(key)方法可以删除元素  
和C++不同的是python的set不会自动排序，但是可以通过sort()方法
<pre>
>>> a = 'abc'
>>> b = a.replace('a', 'A')
>>> b
'Abc'
>>> a
'abc'
>>> a.replace('ab','C')
>>> a
'C'
>>> a.replace('ac','C')
>>> a
'abc'
</pre>
