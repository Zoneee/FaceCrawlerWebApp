const path = require('path');

// puppeteer-extra is a drop-in replacement for puppeteer,
// it augments the installed puppeteer with plugin functionality
const puppeteer = require("puppeteer-extra");
// add stealth plugin and use defaults (all evasion techniques)
const pluginStealth = require("puppeteer-extra-plugin-stealth");
puppeteer.use(pluginStealth());


class YouTube extends require('../Crawler') {
    async getBrowser() {
        if (this.data.proxy)
            this.launchOptions.args.push('--proxy-server=' + this.data.proxy);
        this.launchOptions.headless = false;
        return await puppeteer.launch(this.launchOptions);
    }

    async crawl(page, browser) {
        this.logger.log('接收到数据：' + JSON.stringify(this.data));
        return await this.crawlVideoUrlByScrollPage(page);
    }

    async parse(source) {
        return this.sourceFormatByScrollPage(source);
    }

    //crawlSearchTweetByScrollPage
    async crawlVideoUrlByScrollPage(page) {
        var keyWord = this.data.keyWord;
        var token = this.data.token;
        //测试发现，小于60则一定有60条，超过100只会有100+。所以这里默认60
        var count = !this.data.count || this.data.count < 20 ? 20 : this.data.count;
        //页数
        var scrollNum = count / 20 + 1 > 100 ? 100 : count / 20 + 1;

        //https://www.youtube.com/results?search_query=%E5%AE%88%E6%8A%A4%E8%89%AF%E7%9F%A5%E3%80%81%E5%AE%88%E6%8A%A4%E4%B8%8B%E4%B8%80%E4%BB%A3&pbj=1&ctoken=Ep8DEh7lrojmiqToia_nn6XjgIHlrojmiqTkuIvkuIDku6Ma_AJTQlNDQVF0ck4wcHNNa1ZsWlVGTGI0SUJDMTk0ZDFZMllUaDVXVEZyZ2dFTE4wODBkRkZ0TkhwNFIxR0NBUXQxUkRNeVkxVlRTMjFPU1lJQkMycFNaMnBmYlVGdGFIQkZnZ0VMVEY5dlZqa3hjVWcxUlVtQ0FRdGlPVTFSYzBOVk9WWXlPSUlCQzNsVVVrcGFVMWx1YlhOcmdnRUxjM1ZNZVROU2RqVlJZelNDQVF0aGEwRkNNakJqWlU5M1RZSUJDemxZWTJ3dFMweHhjRkpOZ2dFTGExWjZTRGhVZVVSdlJXLUNBUXRzTlMxaWJWaE9VMDVHTUlJQkMwbGxUbGhOVjNaQ2MxQmpnZ0VMWmpac1IxaHRjRWxQYW1PQ0FRdFROSFpHZVdKNFZuaFZiNElCQzFCcmJVWlJlVmd3WWkxcmdnRUxRVkJTUmxRMlZVVjBRa1dDQVF0bFZ6WTBhWGcwYjJSMFVZSUJDMGxwY1hkNlRWTXRNRjh3NmdNQRi83ugY&continuation=Ep8DEh7lrojmiqToia_nn6XjgIHlrojmiqTkuIvkuIDku6Ma_AJTQlNDQVF0ck4wcHNNa1ZsWlVGTGI0SUJDMTk0ZDFZMllUaDVXVEZyZ2dFTE4wODBkRkZ0TkhwNFIxR0NBUXQxUkRNeVkxVlRTMjFPU1lJQkMycFNaMnBmYlVGdGFIQkZnZ0VMVEY5dlZqa3hjVWcxUlVtQ0FRdGlPVTFSYzBOVk9WWXlPSUlCQzNsVVVrcGFVMWx1YlhOcmdnRUxjM1ZNZVROU2RqVlJZelNDQVF0aGEwRkNNakJqWlU5M1RZSUJDemxZWTJ3dFMweHhjRkpOZ2dFTGExWjZTRGhVZVVSdlJXLUNBUXRzTlMxaWJWaE9VMDVHTUlJQkMwbGxUbGhOVjNaQ2MxQmpnZ0VMWmpac1IxaHRjRWxQYW1PQ0FRdFROSFpHZVdKNFZuaFZiNElCQzFCcmJVWlJlVmd3WWkxcmdnRUxRVkJTUmxRMlZVVjBRa1dDQVF0bFZ6WTBhWGcwYjJSMFVZSUJDMGxwY1hkNlRWTXRNRjh3NmdNQRi83ugY&itct=CEoQybcCIhMIqYDkws6n5AIVKM1MAh3rdQIY
        var indexUrl = 'https://www.youtube.com/results?search_query=' + encodeURIComponent(keyWord);
        //同indexUrl，方式为post
        var dataUrl = indexUrl;

        this.logger.log('开始采集：' + token + '。数量：' + count + '页数：' + scrollNum + '。Url：' + indexUrl);

        await page.goto('https://www.youtube.com/');
        var searchInput = await page.waitForSelector('#search');
        await searchInput.type(keyWord);
        var searchBtn = await page.waitForSelector('#search-icon-legacy');
        var indexResp = await Promise.all([
            page.waitForNavigation(),
            searchBtn.click(),
        ]);
        indexResp = await indexResp[0].text();
        var indexSource = indexResp.match(/window\["ytInitialData"\] = (.*?);/)[1];
        indexSource = JSON.parse(indexSource);
        var contents = indexSource.contents.twoColumnSearchResultsRenderer.primaryContents.sectionListRenderer.contents[0].itemSectionRenderer.contents;
        // var indexResp = await page.goto(indexUrl);
        // indexResp = await indexResp.text();

        var sourceArray = [];
        sourceArray.push(contents);
        await this.logger.logHtml(JSON.stringify(sourceResp), token + '_' + 0 + '_source_youtube');

        for (let i = 1;;) {
            try {
                var x = 0,
                    y = 60000;
                if (++i < scrollNum) {
                    //监听数据源
                    var sourceResp = page.waitForResponse(resp => resp.url().startsWith(dataUrl), { timeout: 60 * 1000 });
                    //翻页
                    await this.scrollPage(page, x, y * i);
                    sourceResp = await (await sourceResp).json();
                    //continuationContents
                    var contents = sourceResp[1].response.continuationContents.itemSectionContinuation.contents;
                    if (contents.length < 1) {
                        break;
                    }
                    sourceArray.push(contents);
                    //日志
                    await this.logger.logHtml(JSON.stringify(sourceResp), token + '_' + i + '_source_youtube');
                    await page.screenshot({ path: path.join(this.logger.directory, '/' + token + '_' + i + '_source_youtube.png'), fullPage: false });
                } else {
                    break;
                }
            } catch (error) {
                await this.scrollPage(page, 0, 0);
            }
            this.logger.log('第' + i + '页');
        }
        await page.close();
        return JSON.stringify(sourceArray);
    }

    async sourceFormatByScrollPage(contents) {
        contents = JSON.parse(contents);
        var sourceArray = [];
        for (let i = 0; i < contents.length; i++) {
            var element = contents[i];
            for (let i = 0; i < element.length; i++) {
                var elementItem = element[i];
                if (!elementItem.videoRenderer || !elementItem.videoRenderer.videoId) {
                    continue;
                }
                var formatItem = {
                    videoId: elementItem.videoRenderer.videoId,
                    mediaType: 'video',
                    mediaInfo: 'https://www.youtube.com/watch?v=' + elementItem.videoRenderer.videoId,
                    keyWord: this.data.token,
                };
                sourceArray.push(formatItem);
            }
        }
        await this.logger.logHtml(JSON.stringify(sourceArray), this.data.token + '_format_youtube');
        return JSON.stringify(sourceArray);
    }

    randomNum(minNum, maxNum) {
        switch (arguments.length) {
            case 1:
                return parseInt(Math.random() * minNum + 1, 10);
                break;
            case 2:
                return parseInt(Math.random() * (maxNum - minNum + 1) + minNum, 10);
                break;
            default:
                return 0;
                break;
        }
    }

    guid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random() * 16 | 0,
                v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}


YouTube.id = "youtube";
module.exports = YouTube;