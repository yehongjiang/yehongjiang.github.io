# Datatables   
###### 相关网络知识点  
##
### 
## 如何使用datatables  
引入相关文件  
<pre>
 < link rel="stylesheet" type="text/css" href="http://cdn.datatables.net/1.10.13/css/jquery.dataTables.css"> 
< script type="text/javascript" charset="utf8" src="http://code.jquery.com/jquery-1.10.2.min.js">< /script >
< script type="text/javascript" charset="utf8" src="http://cdn.datatables.net/1.10.13/js/jquery.dataTables.js">< /script>
</pre>
然后用JS初始化table的ID就可以了，很简单
<pre>
< script type="text/javascript">
        $(function () {
            $("#table_id_example").dataTable();
		}
< /script>
</pre>
或者直接  
<pre> 
< script type="text/javascript">
        $(function () {
            $("#table_id_example").dataTable({
                //lengthMenu: [5, 10, 20, 30],//这里也可以设置分页，但是不能设置具体内容，只能是一维或二维数组的方式，所以推荐下面language里面的写法。
                paging: true,//分页
                ordering: false,//是否启用排序
                searching: true,//搜索
                language: {
                    lengthMenu: '<select class="form-control input-xsmall">' + '<option value="1">1</option>' + '<option value="10">10</option>' + '<option value="20">20</option>' + '<option value="30">30</option>' + '<option value="40">40</option>' + '<option value="50">50</option>' + '</select>条记录',//左上角的分页大小显示。
                   // search: '<span class="label label-success">搜索：</span>',//右上角的搜索文本，可以写html标签
                    search: '搜索：',
                    paginate: {//分页的样式内容。
                        previous: "上一页",
                        next: "下一页",
                        first: "首页",
                        last: "尾页"
                    },

                    zeroRecords: "没有内容",//table tbody内容为空时，tbody的内容。
                    //下面三者构成了总体的左下角的内容。
                    info: "总共_PAGES_ 页，显示第_START_ 到第 _END_ ，筛选之后得到 _TOTAL_ 条，初始_MAX_ 条 ",//左下角的信息显示，大写的词为关键字。
                    infoEmpty: "0条记录",//筛选为空时左下角的显示。
                    infoFiltered: ""//筛选之后的左下角筛选提示，
                },
                paging: true,
                pagingType: "full_numbers",//分页样式的类型

            });
            $("#table_local_filter input[type=search]").css({ width: "auto" });//右上角的默认搜索文本框，不写这个就超出去了。
        });

< /script>
</pre>
## 去掉某些默认的功能  
<pre>
$(function(){  
    $('#dyntable2').dataTable({  
        "searching" : false, //去掉搜索框方法一  
        "bFilter": false,   //去掉搜索框方法二
        "bSort": false,  //禁止排序
        "paging": false,   //禁止分页
        "info": false   //去掉底部文字
    });
});
</pre>
## 设置排序规则  
<pre>
$(function(){
    $('#dyntable2').dataTable({
        "aaSorting": [[ 4, "desc" ]]  //以序号为4也就是第5列进行降序排列
        "aoColumnDefs": [ { "bSortable": false, "aTargets": [ 0 ] }]         //初始化datatable，但对序号为0列的列不进行排序，别的列均可进行排序
     });
});
</pre>
## 不同的分页样式  

[仔细阅读官网的内容，会有收获的](https://datatables.net/examples/styling/bootstrap.html)