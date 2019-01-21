## 问题背景  
将datatable导出到excel
  
1.首先  
下载NPOI[http://npoi.codeplex.com/releases/](http://npoi.codeplex.com/releases/)  
然后我们打开解压好的文件，打开release文件夹，看到有net20和net40文件夹，这里我们打开net40，可以看到一些dll文件。我们这里只需要引用NPOI.dll和NPOI.OOXML.dll
到VS界面，添加引用，选择刚刚那2个dll，并确定。添加完成后，可以在引用里面看到。
<pre>
using NPOI;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
</pre>
