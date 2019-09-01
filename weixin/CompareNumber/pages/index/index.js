//index.js
//Page()函数用来注册一个页面，该函数的参数是一个对象，通过该对象可以指定页面的初始数据/生命周期函数/事件处理函数等。
Page({
  //页面的初始数据
  data: {
    result:''
  },
  //生命周期函数——监听页面加载
  onLoad:function(options){

  },
  onReady:function(){
    
    wx.navigateTo({
      //url: '/pages/test/test?name1=value1&name2=value2',
    })
  },
  onPullDownRefresh:function(){
    console.log("此时用户下拉触底")
  },
  str: '两数相等',
  compare:function(e){
    var data = {};
    data['num1'] = 'ddd';
    data['num2'] = 'sss';
    console.log(data);

    if(this.num1>this.num2){
      this.str = '第一个数字大';

    }else if(this.num1<this.num2){
      this.str = '第二个数字大';
    }
    this.setData({result:this.str});
  },
  num1:0,
  num2:0,
  num1change:function(e){
    this.num1 = Number(e.detail.value)
  },
  num2change: function (e) {
    this.num2 = Number(e.detail.value)
  }
})
