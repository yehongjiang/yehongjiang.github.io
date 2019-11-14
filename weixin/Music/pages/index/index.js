//index.js
//获取应用实例
const app = getApp()

Page({
  data: {
    item:0,
    tab:0
  },
  //事件处理函数

  onLoad: function () {
    
  },

  changeItem:function(e){
    this.setData({
      item:e.target.dataset.item
    })
  },
  changeTab:function(e){
    console.log(e)
    this.setData({
      tab:e.detail.current
    })
  }
})
