//index.js
//Page()函数用来注册一个页面，该函数的参数是一个对象，通过该对象可以指定页面的初始数据/生命周期函数/事件处理函数等。
Page({
  //页面的初始数据
  /**
   * 页面的初始数据
   */
  audioCtx:"",
  data: {
    item:0,
    tab:0
  },
  changeItem:function(e){
    this.setData({item:e.target.dataset.item})
  },
  changeTab:function(e){
    this.setData({tab:e.detail.current})
    if(e.detail.current == 0){
      this.audioCtx.play();
    }
    else{
      this.audioCtx.stop();
    }

  },
  //页面初次渲染完成
  onReady:function(){
    this.audioCtx = wx.createInnerAudioContext()
    //设置音频资源地址
    this.audioCtx.src = '/pages/music/李老板 - 嘉 禾 天 橙 国 际 大 影 院.mp3'
    //开始播放时输出调试信息
    this.audioCtx.onPlay(function(){
      console.log('开始播放');
    })
    //发送错误时，输出调试信息
    this.audioCtx.onError(function(res){
      console.log(res.errMsg) //错误信息
      console.log(res.errCode) //错误码
    })
    //开始播放
   // this.audioCtx.play();
  }
})
