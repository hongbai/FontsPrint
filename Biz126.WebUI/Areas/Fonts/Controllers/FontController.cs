﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace Biz126.WebUI.Areas.Fonts.Controllers
{
    [Route("api/Fonts/[controller]/[action]")]
    [ApiController]
    public class FontController : ControllerBase
    {
        private readonly IOptions<Dictionary<string, string>> _settings;
        private readonly string Fonts_BasePath;
        private readonly Core.Font fontcore;
        public FontController(IOptions<Dictionary<string, string>> settings)
        {
            Fonts_BasePath = "files";
            fontcore = new Core.Font(Fonts_BasePath);
            _settings = settings;
        }

        /// <summary>
        /// 可用字体列表
        /// </summary>
        /// <returns></returns>
        [HttpPost,HttpGet]
        public Dictionary<string, string> List()
        {
            var result = new Dictionary<string, string>();
            var config = fontcore.Config();   //配置
            var fonts = fontcore.List();    //可用字体列表
            fonts.ForEach(font =>
            {
                if (config.ContainsKey(font))
                {
                    result.Add(font, config[font]);
                }
                else
                {
                    result.Add(font, font);
                }
            });

            return result.OrderBy(x => x.Value).ToDictionary(x => x.Key, y => y.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ModelValidationFilterAttribute]
        public Models.ReturnModel<string> Print(Models.FontRequest request)
        {
            var result = new Models.ReturnModel<string>
            {
                Status = false,
                Message = "内容打印失败，请重试",
                Data = string.Empty
            };
            try
            {
                string fontpath = fontcore.GetFontPath(request.font);
                Logger.Log4Net.LogInfo($"[当前字体]:{request.font},路径:{fontpath}");
                Logger.Log4Net.LogInfo($"[查询记录][使用字体]:{request.font},[字号]:{request.fontsize},[生成内容]:{request.body}");
                if (!string.IsNullOrEmpty(fontpath))
                {
                    result.Status = true;
                    result.Message = "生成成功";
                    result.Data = $"data:image/png;base64,{Convert.ToBase64String(Biz126.ImageLib.CreateImages.CreateImage(fontpath, request.body, request.fontsize))}";

                }
            }
            catch (Exception e)
            {
                Logger.Log4Net.ErrorInfo($"[当前字体]:{request.font},异常信息:",e);                
            }

            return result;
        }
    }
}