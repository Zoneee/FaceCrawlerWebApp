const path = require('path');
const puppeteer = require('puppeteer');
const CJY = require('../lib/ChaoJiYing');
const Logger = require('../lib/Logger');


class Crawler {
    /**
     * 构造函数
     * @param {String} data 采集必要参数
     * @param {String} token    唯一标识
     */
    constructor(data, token) {
        /**采集必要参数 */
        this.data = data;
        /**采集结果 */
        this.crawlData = '';
        /**解析结果 */
        this.parseData = '';
        /**日志记录者 */
        this.logger = new Logger(token);
        this.cjy = new CJY(5);

        /**Browser参数列表 */
        this.launchOptions = {
            //headless: false,
            ignoreDefaultArgs: ['--enable-automation'],
            //想禁止Chrome的日志，试了--disable-logging没用，
            //--v是日志级别，默认为0，试了一下-1似乎会少记录一些日志
            args: ['--v=-1', '--disable-gpu', '--no-sandbox',
                '--disable-web-security', '--disable-dev-profile'
            ]
        };
    }

    /**
     * 采集入口
     */
    async startCollect() {
        if (this.data) {
            await this.logger.log('接收到数据：' + JSON.stringify(this.data));
        }
        const browser = await this.getBrowser();
        const timerId = setTimeout(async() => {
            await browser.close();
        }, 180 * 1000);
        try {
            const page = await browser.newPage();
            this.crawlData = await this.crawl(page, browser);
            await this.logger.log('成功');
        } catch (err) {
            const pages = await browser.pages();
            for (let i = 0; i < pages.length; i++) {
                const page = pages[i];
                await this.logger.logHtml(await page.content(), 'errorPage' + i);
                await page.screenshot({ path: path.join(this.logger.directory, '/errorPage' + i + '.png'), fullPage: true });
            }
            await this.logger.log('采集异常：' + err.message + err.stack);
            throw err;
        } finally {
            await browser.close();
            clearTimeout(timerId);
        }
    }

    /**
     * 解析入口
     */
    async startParse() {
        try {
            this.parseData = await this.parse(this.crawlData);
        } catch (err) {
            await this.logger.log('解析异常：' + err.message + err.stack);
            throw err;
        }
    }

    /**
     * 创建Browser
     */
    async getBrowser() {
        return await puppeteer.launch(this.launchOptions);
    }

    /**
     * 采集实现函数
     * 由子类实现
     * @param {Page} page 
     * @param {Browser} browser 
     */
    async crawl(page, browser) { return ''; }

    /**
     * 解析实现函数
     * 由子类实现
     * @param {String} source 待解析数据
     */
    async parse(source) { return ''; }

    /**
     * 页面滚动
     * @param {Page} page 页面
     * @param {Number} x X轴
     * @param {Number} y y轴
     */
    async scrollPage(page, x, y) {
        await page.evaluate((x, y) => {
            /* 这里做的是渐进滚动，如果一次性滚动则不会触发获取新数据的监听 */
            window.scrollTo(x, y);
        }, x, y);
    }

    /**
     * 验证码识别
     * @param {number} codetype 
     * @param {string} imgBase64 
     * @param {function} checker  自定义检测规则。示例：(resp)=>bool
     */
    async Recognize(codetype, imgBase64, checker) {
        try {
            return await this.cjy.Recognize(codetype, imgBase64, checker);
        } catch (error) {
            this.logger.log(error);
            throw error;
        }
    }

    /**
     * 验证码报错
     */
    async ReportError() {
        try {
            var request = JSON.parse(await this.cjy.ReportError());
            if (request.err_no !== 0) {
                this.logger.log("反馈失败：" + JSON.stringify(request));
            }
        } catch (error) {
            this.logger.log(error);
        }
    }

    /**
     * 设置验证码最大识别次数
     * @param {Number} nums 
     */
    async setErrorTotal(nums) {
        this.cjy.setErrorTotal(nums);
    }
}

module.exports = Crawler;