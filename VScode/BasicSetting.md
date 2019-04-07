# 基本设置
### 1.下载
---  
在visual studio code的[官网](https://code.visualstudio.com/)就可以下载安装最新的软件 。 
### 2.配置中文格式
--- 
在主界面中按下shift+Ctrl+P即可弹出所谓的命令调色板，然后输入Configure Display Language安装中文插件，再次输入此命令选择ch就会自动重启软件，之后可以使用中文界面了。
### 3.配置C#编程环境
---  
按Ctrl+Shift+X调出商店（或者说是扩展程序)搜索C#，安装后，利用cd 和 md 进入或创建项目的文件夹，用dotnet new console创建项目，这里可能会出现无法使用dotnet的情况，原因是：  
###
1. 环境变量没有设置，这个一般在安装VScode的时候会自动安装  
2. 是因为没有下载[.net core sdk](https://dotnet.microsoft.com/download) 的原因  
  
之后用dotnet run就可以运行程序了  
dotnet -h 可以弹出帮助菜单  
可以选择添加Extensions还有snippets来扩展还有列入auto-using for C#这个是在编写代码的时候自动添加相应的引用