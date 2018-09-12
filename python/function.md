# 函数 
## 常用函数
- 绝对值函数abs()
- 最大值函数max()可以有多个参数
- 数值类型转换函数如int()、float()、str()、bool()
- 函数名其实就是指向一个函数对象的引用，完全可以把函数名赋给一个变量，相当于给这个函数起了一个“别名”：
<pre>
>>> a = abs # 变量a指向abs函数
>>> a(-1) # 所以也可以通过a调用abs函数
1</pre>
- 十六进制转换函数hex()  

## 定义函数
<pre>def my_abs(x):
    if x >= 0:
        return x
    else:
        return -x</pre>
tips:return None可以简写为return  
tips:如果你已经把my_abs()的函数定义保存为abstest.py文件了，那么，可以在该文件的当前目录下启动Python解释器，用from abstest import my_abs来导入my_abs()函数，注意abstest是文件名（不含.py扩展名）  
##空函数
如果想定义一个什么事也不做的空函数，可以用pass语句：
<pre>
def nop():
    pass</pre>
缺少了pass，代码运行就会有语法错误。所以可以用做占位符  
让我们修改一下my_abs的定义，对参数类型做检查，只允许整数和浮点数类型的参数。数据类型检查可以用内置函数isinstance()实现：
<pre>
def my_abs(x):
    if not isinstance(x, (int, float)):
        raise TypeError('bad operand type')
    if x >= 0:
        return x
    else:
        return -x</pre>
添加了参数检查后，如果传入错误的参数类型，函数就可以抛出一个错误：
<pre>
>>> my_abs('A')
Traceback (most recent call last):
  File "<stdin>", line 1, in <module>
  File "<stdin>", line 3, in my_abs
TypeError: bad operand type</pre>

## 返回多个值
函数可以返回多个值吗？答案是肯定的。

比如在游戏中经常需要从一个点移动到另一个点，给出坐标、位移和角度，就可以计算出新的新的坐标：
<pre>
import math

def move(x, y, step, angle=0):
    nx = x + step * math.cos(angle)
    ny = y - step * math.sin(angle)
    return nx, ny</pre>
import math语句表示导入math包，并允许后续代码引用math包里的sin、cos等函数。

然后，我们就可以同时获得返回值：
<pre>
>>> x, y = move(100, 100, 60, math.pi / 6)
>>> print(x, y)
151.96152422706632 70.0</pre>
但其实这只是一种假象，Python函数返回的仍然是单一值：
<pre>
>>> r = move(100, 100, 60, math.pi / 6)
>>> print(r)
(151.96152422706632, 70.0)</pre>
原来返回值是一个tuple！但是，在语法上，返回一个tuple可以省略括号，而多个变量可以同时接收一个tuple，按位置赋给对应的值，所以，Python的函数返回多值其实就是返回一个tuple，但写起来更方便。  
## 函数的参数
1）默认参数  
由于我们经常计算x2，所以，完全可以把第二个参数n的默认值设定为2：
<pre>
def power(x, n=2):
    s = 1
    while n > 0:
        n = n - 1
        s = s * x
    return s</pre>
这样，当我们调用power(5)时，相当于调用power(5, 2)：
<pre>
>>> power(5)
25
>>> power(5, 2)
25</pre>
tips:一是必选参数在前，默认参数在后  
2）可变参数  
<pre>
def calc(numbers):
    sum = 0
    for n in numbers:
        sum = sum + n * n
    return sum</pre>
但是调用的时候，需要先组装出一个list或tuple：
<pre>
>>> calc([1, 2, 3])
14
>>> calc((1, 3, 5, 7))
84</pre>
如果利用可变参数，调用函数的方式可以简化成这样：
<pre>
>>> calc(1, 2, 3)
14
>>> calc(1, 3, 5, 7)
84</pre>
所以，我们把函数的参数改为可变参数：
<pre>
def calc(*numbers):
    sum = 0
    for n in numbers:
        sum = sum + n * n
    return sum</pre>
