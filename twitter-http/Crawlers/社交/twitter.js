const path = require('path');

// puppeteer-extra is a drop-in replacement for puppeteer,
// it augments the installed puppeteer with plugin functionality
const puppeteer = require("puppeteer-extra");
// add stealth plugin and use defaults (all evasion techniques)
const pluginStealth = require("puppeteer-extra-plugin-stealth");
puppeteer.use(pluginStealth());


class Twitter extends require('../Crawler') {
    async getBrowser() {
        if (this.data.proxy)
            this.launchOptions.args.push('--proxy-server=' + this.data.proxy);
        // this.launchOptions.headless = false;
        return await puppeteer.launch(this.launchOptions);
    }
    async crawl(page, browser) {
        // 简单实现阐述：
        // 必要参数：某人推特ID(如：特朗普TwitterID：realDonaldTrump)
        // 流程：
        // 1. 访问 https://mobile.twitter.com/ID 如： https://mobile.twitter.com/realDonaldTrump
        // 2. 监听 https://api.twitter.com/2/timeline/media/ 为推特地址，改count字段为2000(任意数)
        //     返回结果json， tweets 字段中 每项 key 为 推文ID ，full_text 为正文
        // 3. 访问 https://mobile.twitter.com/推特ID/status/推文ID 如： https://mobile.twitter.com/realDonaldTrump/status/1163603361423351808
        // 4. 监听 https://api.twitter.com/2/timeline/conversation 为评论地址，改count字段为2000(任意数)
        //     返回结果json， tweets 字段中 每项 key 为 评论ID，full_text 为正文
        //var search = { keyWord: '特朗普', sinceTime: '2019-01-01', untilTime: '2019-12-31' };
        return await this.crawlSearchTweetByScrollPage(page, this.data);
        // return await this.crawlSearchTweet(page, this.data);
    }

    async parse(source) {
        return this.searchTweetFormatByScrollPage(source);
        // return this.searchTweetFormat(source);
    }

    /**
     * 翻页采集
     * 每60条翻一页
     */
    async crawlSearchTweetByScrollPage(page, searchMessage) {
        //参数整理
        var token = searchMessage.token; //token是额外定义的，用于保存文件便于区分
        var keyWord = searchMessage.keyWord ? searchMessage.keyWord : '';
        var phraseWord = searchMessage.phraseWord ? ' "' + searchMessage.phraseWord + '"' : '';
        var anyWord = searchMessage.anyWord ? ' (' + searchMessage.anyWord + ')' : '';
        var excludeWord = searchMessage.excludeWord ? ' -' + searchMessage.excludeWord : '';
        var hasTitle = searchMessage.hasTitle ? ' #(' + searchMessage.hasTitle + ')' : '';
        var fromAccount = searchMessage.fromAccount ? ' (from:' + searchMessage.fromAccount + ')' : '';
        var toAccount = searchMessage.toAccount ? ' (to:' + searchMessage.toAccount + ')' : '';
        var mentionAccount = searchMessage.mentionAccount ? ' (@' + searchMessage.mentionAccount + ')' : '';
        var sinceTime = searchMessage.sinceTime ? ' since:' + searchMessage.sinceTime : '';
        var untilTime = searchMessage.untilTime ? ' until:' + searchMessage.untilTime : '';
        //测试发现，小于60则一定有60条，超过100只会有100+。所以这里默认60
        var count = !searchMessage.count || searchMessage.count < 60 ? 60 : searchMessage.count;
        //页数
        var scrollNum = count / 60 + 1 > 100 ? 100 : count / 60 + 1;

        //链接整理
        var search = keyWord + phraseWord + anyWord + excludeWord + hasTitle + fromAccount + toAccount + mentionAccount + untilTime + sinceTime;
        var index = 'https://mobile.twitter.com/search?q=' + encodeURIComponent(search) + '&src=typed_query';
        var checkUrl = 'https://api.twitter.com/2/search/adaptive.json';

        this.logger.log('开始采集：' + token + '。数量：' + count + '页数：' + scrollNum + '。Url：' + index);
        //修改必要的请求
        await page.setRequestInterception(true);
        page.on('request', async req => {
            if (req.url().startsWith(checkUrl)) {
                var url = req.url().replace('count=20', 'count=60');
                await req.continue({ url: url });
            } else {
                await req.continue();
            }
        });

        //https://api.twitter.com/2/search/adaptive.json?include_profile_interstitial_type=1&include_blocking=1&include_blocked_by=1&include_followed_by=1&include_want_retweets=1&include_mute_edge=1&include_can_dm=1&include_can_media_tag=1&skip_status=1&cards_platform=Web-12&include_cards=1&include_composer_source=true&include_ext_alt_text=true&include_reply_count=1&tweet_mode=extended&include_entities=true&include_user_entities=true&include_ext_media_color=true&include_ext_media_availability=true&send_error_codes=true&q=%E5%8D%8E%E5%86%9C%E5%85%84%E5%BC%9F%20until%3A2019-12-31%20since%3A2019-01-01&count=20&query_source=typed_query&pc=1&spelling_corrections=1&ext=mediaStats%2ChighlightedLabel%2CcameraMoment
        //监听数据源
        var sourceArray = [];
        var tweetsSource1 = page.waitForResponse(resp => resp.url().startsWith(checkUrl), 2 * 60 * 1000);
        await page.goto(index);

        for (let i = 1;;) {
            try {
                //等消息
                ++i;
                tweetsSource1 = await (await tweetsSource1).text();
                //{"errors":[{"message":"Rate limit exceeded","code":88}]}
                if (tweetsSource1.includes('Rate limit exceeded')) {
                    throw new Error('被封了：' + tweetsSource1);
                }
                sourceArray.push(tweetsSource1);
                //日志
                await this.logger.logHtml(tweetsSource1, token + '_' + i + '_source_tweers');
                await page.screenshot({ path: path.join(this.logger.directory, '/' + token + '_' + i + '_source_tweers.png'), fullPage: false });
                //监听数据源
                if (i < scrollNum) {
                    var source = JSON.parse(tweetsSource1);
                    var tweets = Object.keys(source.globalObjects.tweets).map(function(t) {
                        return source.globalObjects.tweets[t];
                    });
                    if (tweets.length < 1) {
                        break;
                    }
                    tweetsSource1 = page.waitForResponse(resp => resp.url().startsWith(checkUrl), 2 * 60 * 1000);
                } else {
                    break;
                }
            } catch (error) {
                tweetsSource1 = page.waitForResponse(resp => resp.url().startsWith(checkUrl), 2 * 60 * 1000);
                await this.scrollPage(page, 0, 0);
            }
            var x = 0,
                y = 60000;
            await this.scrollPage(page, x, y * i);
            this.logger.log('第' + i + '页');
            await page.waitFor(Math.random() * 1000);
        }
        await page.close();
        return JSON.stringify(sourceArray);
    }

    /**
     * crawlSearchTweetByScrollPage 结果格式化
     * @param {String} source 源数据
     */
    async searchTweetFormatByScrollPage(source) {
        //正文在 full_text 节点

        //图片信息
        //图片在 extended_entities>media 数组中，每一项是一张图片

        //视频信息
        //视频封面图在 extended_entities>media 数组中
        //视频在 extended_entities>media>video_info>variants 数组中，取 bitrate 最大项的 url
        //注：结构和图片信息一样，不排除有两个视频的可能

        //数据保存
        //源数据存一份
        //整理后的数据存一份
        //整理后的数据格式：用户id、用户名、用户帐号、推特id、发送时间、正文、媒体类型、媒体信息
        var source = JSON.parse(source);

        var formatTweers = [];
        for (let s = 0; s < source.length; s++) {
            var sourceElement = JSON.parse(source[s]);
            var tweets = Object.keys(sourceElement.globalObjects.tweets).map(function(t) {
                return sourceElement.globalObjects.tweets[t];
            });
            var users = Object.keys(sourceElement.globalObjects.users).map(function(t) {
                return sourceElement.globalObjects.users[t];
            });

            for (let i = 0; i < tweets.length; i++) {
                var element = tweets[i];
                var formatItem = await this.formatSingle(element, users);
                formatTweers.push(formatItem);
            }
        }

        await this.logger.logHtml(JSON.stringify(formatTweers), this.data.token + '_format_tweers');
        return JSON.stringify(formatTweers);
    }

    /**
     * 格式化单个数据
     */
    async formatSingle(element, users) {
        try {
            var mediaArray = element.extended_entities.media;
        } catch (error) {
            //获取不到则为纯文字推特
            var mediaType = 'text';
        }

        //有媒体信息推特
        if (mediaArray) {
            //区分媒体类型
            if (mediaArray.length === 1) {
                var mediaInfo = mediaArray[0];
                var mediaType = mediaInfo.video_info ? 'video' : 'image';
            } else {
                var mediaType = 'image';
            }

            //处理媒体信息
            switch (mediaType) {
                case 'image':
                    var mediaInfo = mediaArray.map(function(m) {
                        return m.media_url + '?name=large';
                    });
                    break;
                case 'video':
                    var mediaInfo = mediaArray.map(function(m) {
                        var variants = m.video_info.variants.sort(function(a, b) {
                            return b.bitrate - a.bitrate;
                        });
                        return variants[0].url;
                    });
                    break;
            }
        }

        var userElement = users.find(u => u.id == element.user_id);
        var formatItem = {
            userId: element.user_id,
            userName: userElement.name,
            userAccount: userElement.screen_name,
            twitterId: element.conversation_id,
            releaseTime: new Date(element.created_at).toISOString(),
            text: element.full_text,
            mediaType: mediaType,
            mediaInfo: mediaInfo,
            keyWord: this.data.token,
            quotedId: element.quoted_status_id, //被引用推特id
        };
        return formatItem;
    }

    /**
     * 根据关键词采集推特
     * @param {JSON} searchMessage 必要参数
     * @param {string} keyWord 关键字
     * @param {string} phraseWord 精准短语
     * @param {string} anyWord 任何一词
     * @param {string} excludeWord 排除词语
     * @param {string} hasTitle 话题
     * @param {string} language 语言
     * @param {string} fromAccount 来自帐号
     * @param {string} toAccount 发送账号
     * @param {string} mentionAccount 提及账号
     * @param {string} sinceTime 开始时间
     * @param {string} untilTime 结束时间
     */
    async crawlSearchTweet(page, searchMessage) {
        //参数整理
        var token = searchMessage.token; //token是额外定义的，用于保存文件便于区分
        var keyWord = searchMessage.keyWord ? searchMessage.keyWord : '';
        var phraseWord = searchMessage.phraseWord ? ' "' + searchMessage.phraseWord + '"' : '';
        var anyWord = searchMessage.anyWord ? ' (' + searchMessage.anyWord + ')' : '';
        var excludeWord = searchMessage.excludeWord ? ' -' + searchMessage.excludeWord : '';
        var hasTitle = searchMessage.hasTitle ? ' #(' + searchMessage.hasTitle + ')' : '';
        var fromAccount = searchMessage.fromAccount ? ' (from:' + searchMessage.fromAccount + ')' : '';
        var toAccount = searchMessage.toAccount ? ' (to:' + searchMessage.toAccount + ')' : '';
        var mentionAccount = searchMessage.mentionAccount ? ' (@' + searchMessage.mentionAccount + ')' : '';
        var sinceTime = searchMessage.sinceTime ? ' since:' + searchMessage.sinceTime : '';
        var untilTime = searchMessage.untilTime ? ' until:' + searchMessage.untilTime : '';
        //测试发现，小于60则一定有60条，超过100只会有100+。所以这里默认60
        var count = !searchMessage.count || searchMessage.count < 60 ? 60 : searchMessage.count;

        //链接整理
        var search = keyWord + phraseWord + anyWord + excludeWord + hasTitle + fromAccount + toAccount + mentionAccount + untilTime + sinceTime;
        var index = 'https://mobile.twitter.com/search?q=' + encodeURIComponent(search) + '&src=typed_query';
        var checkUrl = 'https://api.twitter.com/2/search/adaptive.json';

        this.logger.log('开始采集：' + token + '。数量：' + count + '。Url：' + index);
        //修改必要的请求
        await page.setRequestInterception(true);
        page.on('request', async req => {
            if (req.url().startsWith(checkUrl)) {
                var url = req.url().replace('count=20', 'count=' + count);
                await req.continue({ url: url });
            } else {
                await req.continue();
            }
        });

        //https://api.twitter.com/2/search/adaptive.json?include_profile_interstitial_type=1&include_blocking=1&include_blocked_by=1&include_followed_by=1&include_want_retweets=1&include_mute_edge=1&include_can_dm=1&include_can_media_tag=1&skip_status=1&cards_platform=Web-12&include_cards=1&include_composer_source=true&include_ext_alt_text=true&include_reply_count=1&tweet_mode=extended&include_entities=true&include_user_entities=true&include_ext_media_color=true&include_ext_media_availability=true&send_error_codes=true&q=%E5%8D%8E%E5%86%9C%E5%85%84%E5%BC%9F%20until%3A2019-12-31%20since%3A2019-01-01&count=20&query_source=typed_query&pc=1&spelling_corrections=1&ext=mediaStats%2ChighlightedLabel%2CcameraMoment
        //监听数据源
        var tweetsSource1 = page.waitForResponse(resp => resp.url().startsWith(checkUrl), 2 * 60 * 1000);
        await page.goto(index);
        //等消息
        tweetsSource1 = await (await tweetsSource1).text();
        //日志
        await this.logger.logHtml(tweetsSource1, token + '_source_tweers');
        await page.screenshot({ path: path.join(this.logger.directory, '/' + token + '_source_tweers.png'), fullPage: false });
        await page.close();
        return tweetsSource1;
    }

    /**
     * crawlSearchTweet 结果格式化
     * @param {String} source 源数据
     */
    async searchTweetFormat(source) {
        //正文在 full_text 节点

        //图片信息
        //图片在 extended_entities>media 数组中，每一项是一张图片

        //视频信息
        //视频封面图在 extended_entities>media 数组中
        //视频在 extended_entities>media>video_info>variants 数组中，取 bitrate 最大项的 url
        //注：结构和图片信息一样，不排除有两个视频的可能

        //数据保存
        //源数据存一份
        //整理后的数据存一份
        //整理后的数据格式：用户id、用户名、用户帐号、推特id、发送时间、正文、媒体类型、媒体信息
        var source = JSON.parse(source);

        var tweets = Object.keys(source.globalObjects.tweets).map(function(t) {
            return source.globalObjects.tweets[t];
        });

        var formatTweers = [];
        for (let i = 0; i < tweets.length; i++) {
            const element = tweets[i];
            var formatItem = await this.formatSingle(element);
            formatTweers.push(formatItem);
        }

        await this.logger.logHtml(JSON.stringify(formatTweers), this.data.token + '_format_tweers');
        return JSON.stringify(formatTweers);
    }
}
Twitter.id = "twitter";
module.exports = Twitter;