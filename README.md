
# Twitter YouTube 爬虫

在twitter-http下执行  
```
npm i
```  
设置`youtube-dl`到环境变量

---

# 2019年8月23日 10:15:35
+ `twitter-http` 实现简单记录
  1. 调用twitter爬虫，返回json数据
  2. 整理json数据到layui格式
  3. 使用layui栅格化将页面分为上下部分
     + 上部：搜索框
     + 下部：列表
   4. 列名
      + 时间
      + 推人
      + 推文
      + 媒体资源展示按钮
    5. 用弹出层展示媒体资源
       + 一或多张图片
         + 点击图片再弹出一个单独展示的弹出层
       + 一个视频(自动或手动播放)
+ Twitter
  + 根据给出关键字使用高级搜索，抓取推特信息，并简单解析
  + - [x] 爬虫
  + - [X] 导出CSV

# 2019年8月25日 14:01:32
+ - [x] ws协议换http协议

# 2019年9月2日 17:59:15
+ - [x] Twitter 添加 md5(url) 作为视频名称，为视频与用户建立关系
  ```JavaScript
  // Json格式
  {
    keyWord:'', //关键词  //键
    userId: 'element.user_id',  //Twitter Post用户id
    userName: 'userElement.name',   //Twitter Post可读用户名
    userAccount: 'userElement.screen_name', //Twitter Post用户名ID //被@时是这个名字
    quotedId: 'element.quoted_status_id', //被引用推特id
    twitterId: 'element.conversation_id',  
    releaseTime: 'element.created_at',
    text: 'element.full_text',  //Twitter 正文 (纯文字)
    mediaType: 'mediaType', //媒体类型  'video'or'image'
    mediaInfo: ['url','url'...], //媒体下载地址
  }

  //Csv格式
  {
    keyWord:'', //关键词  //键
    userId: 'element.user_id',  //Twitter Post用户id
    userName: 'userElement.name',   //Twitter Post可读用户名
    userAccount: 'userElement.screen_name', //Twitter Post用户名ID //被@时是这个名字
    quotedId: 'element.quoted_status_id', //被引用推特id
    twitterId: 'element.conversation_id',  
    releaseTime: 'element.created_at',
    text: 'element.full_text',  //Twitter 正文 (纯文字)
    mediaType: 'mediaType', //媒体类型  'video'or'image'
    mediaInfo: ['url','url'...], //媒体下载地址  //键
    mediaMd5: md5(mediaInfo)
  }

  ```
+ - [x] YouTube 格式向 Twitter 对齐
  ```JavaScript
  // Json格式
  {
    keyWord:'', //关键词  //键
    userId: '',  //Twitter Post用户id
    userName: '',   //Twitter Post可读用户名
    mediaId: '',  //媒体id
    createTime: '',
    title: '',  //视频标题
    mediaType: 'video', //媒体类型  'video'
    mediaInfo: 'url', //媒体下载地址  //键
  }

  //Csv格式
  {
    keyWord:'', //关键词  //键
    userId: '',  //Twitter Post用户id
    userName: '',   //Twitter Post可读用户名
    mediaId: '',  //媒体id
    createTime: '',
    title: '',  //视频标题
    mediaType: 'video', //媒体类型  'video'
    mediaInfo: 'url', //媒体下载地址  //键
    mediaMd5: md5(mediaInfo)
  }
  ```

# 2019年9月4日 16:57:39
1. Twitter与YouTube频繁被封
   - [x] 添加TorProxy
2. 修改采集顺序
   - [x] Twitter改为同步采集
   - [x] YouTube改为同步采集
3. 统计每日采集量
   - [x] 添加mysql统计

# 2019年9月5日 12:39:28
1. 定时任务
   - [x] 每十分钟执行一次
2. 优化
   - [ ] 数据采集与数据保存解耦合
   - [ ] YouTube添加原始数据保存
   - [x] 抽象现有采集类
   - [x] 流水线执行
   - [x] 报警邮件

# 2019年9月6日 13:50:19
 - [x] Headless 添加日志
 - [x] 解析延时调整
 - [x] YouTube 细节调试
 - [x] Twitter 代理被封判断Bug修改
 
# 2019年9月10日 13:46:11
 - [x] YouTube持续优化
 + 目前采集速度
   +  YouTube 20分钟750 并行数5 每个任务用时3-5分钟不等
   +  Twitter 3分钟50条 流水线工作模式 每个任务用时1分钟

# 2019年9月25日 17:57:39
10-25日记录丢失，按照记忆简单还原，以下记录不分先后
- [x] 下载流程更高级的抽象
- [x] Url格式化Bug修复
- [x] 重复媒体跳过下载处理
- [x] Csv与Db字段整理
- [x] 更高级的Model抽象
- [x] 区分环境
- [x] 配置文件更新
