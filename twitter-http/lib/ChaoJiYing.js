const rp = require('request-promise');

class ChaoJiYing {
    /**
     * 
     * @param {number} errorTotal 
     */
    constructor(errorTotal) {
        this.setErrorTotal(errorTotal);

        this.errorTotal = 5;
        // 配置信息
        this.user = "juxinli";
        // this.pass2 = "bab765be667aaad0f45758d03f2dcc09"; //旧密码
        this.pass2 = "7e52dca23962d04c06d388e797011f56"; //新密码
        this.softid = 891136;
    }

    setErrorTotal(errorTotal) {
        if (errorTotal > 20) {
            throw new Error("设置识别错误数值过大");
        }
        this.errorTotal = errorTotal;
    }

    /**
     * 
     * @param {number} codetype 
     * @param {string} imgBase64 
     * @param {function} checker 传入识别结果(JsonObj)返回Boolean
     */
    async Recognize(codetype, imgBase64, checker) {
        // 发送的json示例：{"user":"userabc","pass":"passpass","softid":96001,"codetype":1902,"file_base64":"base64字符串"}
        var params = {
            "user": this.user,
            "pass2": this.pass2,
            "softid": this.softid,
            "codetype": codetype,
            "file_base64": imgBase64
        };
        var options = {
            method: 'POST',
            uri: "http://upload.chaojiying.net/Upload/Processing.php",
            formData: params,
            headers: {
                'content-type': 'application/json' // Is set automatically
            }
        };
        for (var i = 0;; i++) {
            if (i > this.errorTotal) {
                throw new Error("识别错误次数过多：" + this.errorTotal);
            }
            var request = JSON.parse(await rp(options));
            // 返回json字符串示例:{"err_no":0,"err_str":"OK","pic_id":"1662228516102","pic_str":"8vka","md5":"35d5c7f6f53223fbdc5b72783db0c2c0"}
            if (request.err_no === 0) {
                await reportError;
                this.imgId = request.pic_id;
                if (typeof checker === 'function') {
                    // 进入检测
                    if (!checker(request)) {
                        var reportError = this.ReportError();
                        continue;
                    }
                }
                return request;
            }
        }
    }

    async ReportError() {
        var params = {
            "user": this.user,
            "pass2": this.pass2,
            "softid": this.softid,
            "id": this.imgId,
        };
        var options = {
            method: 'POST',
            uri: "http://upload.chaojiying.net/Upload/ReportError.php",
            formData: params,
            headers: {
                'content-type': 'application/json' // Is set automatically
            }
        };
        return await rp(options);
    }
}

module.exports = ChaoJiYing;