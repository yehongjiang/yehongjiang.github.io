# 函数式编程
## 高阶函数
tips：可以把函数赋值给变量，f=abs()，那么f就可以当abs使用  
tips:函数名也是变量，如果把abs = 10了那么它就不是一个函数了  
## map/reduce
map()函数接收两个参数，一个是函数，一个是Iterable可迭代的 ，map将传入的函数依次作用到序列的每个元素，并把结果作为新的Iterator返回。  
<pre>
>>> def f(x):
...     return x * x
...
>>> r = map(f, [1, 2, 3, 4, 5, 6, 7, 8, 9])
>>> list(r)
[1, 4, 9, 16, 25, 36, 49, 64, 81]
</pre>
map()作为高阶函数，事实上它把运算规则抽象了，因此，我们不但可以计算简单的f(x)=x2，还可以计算任意复杂的函数，比如，把这个list所有数字转为字符串：
<pre>
>>> list(map(str, [1, 2, 3, 4, 5, 6, 7, 8, 9]))
['1', '2', '3', '4', '5', '6', '7', '8', '9']</pre>
再看reduce的用法。reduce把一个函数作用在一个序列[x1, x2, x3, ...]上，这个函数必须接收两个参数，reduce把结果继续和序列的下一个元素做累积计算，其效果就是：
<pre>
reduce(f, [x1, x2, x3, x4]) = f(f(f(x1, x2), x3), x4)</pre>
比方说对一个序列求和，就可以用reduce实现：
<pre>
>>> from functools import reduce
>>> def add(x, y):
...     return x + y
...
>>> reduce(add, [1, 3, 5, 7, 9])
25</pre>
## filter
用于过滤序列，也是接受一个函数和一个序列，依次作用于每个元素，然后根据返回值是true还是false决定保留还是丢弃该元素。  
例如，在一个list中，删掉偶数，只保留奇数，可以这么写：
<pre>
def is_odd(n):
    return n % 2 == 1

list(filter(is_odd, [1, 2, 4, 5, 6, 9, 10, 15]))
# 结果: [1, 5, 9, 15]</pre>
tips:用filter求素数   
用Python来实现这个算法，可以先构造一个从3开始的奇数序列：
<pre>
def _odd_iter():
    n = 1
    while True:
        n = n + 2
        yield n</pre>
注意这是一个生成器，并且是一个无限序列。

然后定义一个筛选函数：
<pre>
def _not_divisible(n):
    return lambda x: x % n > 0
最后，定义一个生成器，不断返回下一个素数：

def primes():
    yield 2
    it = _odd_iter() # 初始序列
    while True:
        n = next(it) # 返回序列的第一个数
        yield n
        it = filter(_not_divisible(n), it) # 构造新序列</pre>
这个生成器先返回第一个素数2，然后，利用filter()不断产生筛选后的新的序列。

由于primes()也是一个无限序列，所以调用时需要设置一个退出循环的条件：
<pre>
# 打印1000以内的素数:
for n in primes():
    if n < 1000:
        print(n)
    else:
        break</pre>
注意到Iterator是惰性计算的序列，所以我们可以用Python表示“全体自然数”，“全体素数”这样的序列，而代码非常简洁。
## sorted