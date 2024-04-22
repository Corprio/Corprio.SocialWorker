using Corprio.SocialWorker.Models;
using DevExpress.CodeParser;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Dictionaries
{
    public class BabelFish
    {
        public const string KillCode = "!c";
        public const string KillCode_CN = "！c"; // note: the exclamation mark in Chinese is different from English
        public const string SOS = "!h";
        public const string SOS_CN = "！h";
        public const string BotSummon = "!a";
        public const string BotSummon_CN = "！a";
        public const string ProductEpName = "MetaPostId";
        public const string CustomerEpName = "MetaSenderId";
        public const string CustomDataKeyForMetaUser = "MetaUser";
        public const string RobotEmoji = "🤖";        

        public static readonly Dictionary<string, Dictionary<BotLanguage, string>> Vocab = new()
        {
            ["AcknowlegeMention"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "{0}, thanks for mentioning us in your story. ",
                [BotLanguage.TraditionalChinese] = "{0}，感謝您在故事中提到我們。",
                [BotLanguage.SimplifiedChinese] = "{0}，感谢您在故事中提到我们。",
            },
            ["AskCheckout"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Do you want to checkout now? Please answer in YES or NO. ",
                [BotLanguage.TraditionalChinese] = "您希望現在結帳嗎？請以「是」或「否」回答。",
                [BotLanguage.SimplifiedChinese] = "您希望现在结帐吗？请以「是」或「否」回答。",
            },
            ["AskClearCart"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Do you want to empty your cart? Please answer in YES or NO. ",
                [BotLanguage.TraditionalChinese] = "您想清空您的購物車嗎？請以「是」或「否」回答。",
                [BotLanguage.SimplifiedChinese] = "您想清空您的购物车吗？请以「是」或「否」回答。",
            },
            ["AskEmail"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "To proceed with your transaction, please provide your email address. ",
                [BotLanguage.TraditionalChinese] = "請提供電子郵件地址以確認您的身份。",
                [BotLanguage.SimplifiedChinese] = "请提供电子邮件地址以确认您的身份。",
            },
            ["AskMultiProducts"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Are you interested in the following products? If yes, input the number next to the product name.\n",
                [BotLanguage.TraditionalChinese] = "您對以下商品有興趣嗎？如果是，請輸入商品名稱旁邊的數字。\n",
                [BotLanguage.SimplifiedChinese] = "您对以下商品有兴趣吗？如果是，请输入商品名称旁边的数字。\n",
            },
            ["AskNewCustomer"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Hello. Is it your first time shopping at {0}? Please answer in YES or NO. ",
                [BotLanguage.TraditionalChinese] = "您好。這是您第一次在{0}購物嗎？請以「是」或「否」回答。",
                [BotLanguage.SimplifiedChinese] = "您好。这是您第一次在{0}购物吗？请以「是」或「否」回答。",
            },
            ["AskOneProduct"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Do you want to buy {0}? Please answer in YES or NO. ",
                [BotLanguage.TraditionalChinese] = "您想購買{0}嗎？請以「是」或「否」回答。",
                [BotLanguage.SimplifiedChinese] = "您想购买{0}吗？请以「是」或「否」回答。",
            },
            ["AskProduct"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "If you want to purchase a product, please input its name or product code, or visit the relevant catalogue. ",
                [BotLanguage.TraditionalChinese] = "如果您想購買商品，請輸入商品名稱或商品代碼，或造訪相關的商品目錄。",
                [BotLanguage.SimplifiedChinese] = "如果您想购买商品，请输入商品名称或商品代码，或造访相关的商品目录。",
            },                        
            ["AskProductVariation"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Which \'{0}\'? Please input the number next to each choice.\n{1}",
                [BotLanguage.TraditionalChinese] = "哪個「{0}」？請輸入選項旁邊的數字。\n{1}",
                [BotLanguage.SimplifiedChinese] = "哪个「{0}」？请输入选项旁边的数字。\n{1}",
            },
            ["AskQty"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "{0} was added to your shopping cart. How many units do you want to buy? ",
                [BotLanguage.TraditionalChinese] = "{0}已加入到您的購物車，您想購買多少件？",
                [BotLanguage.SimplifiedChinese] = "{0}已加入到您的购物车，您想购买多少件？",
            },
            ["CannotFindProduct"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "No product matches your input: \'{0}\'.\nIf you want to purchase a product, please input its name or product code, or visit the relevant catalogue. ",
                [BotLanguage.TraditionalChinese] = "沒有商品符合您輸入的關鍵字：「{0}」。\n如果您想購買商品，請輸入商品名稱或商品代碼，或造訪相關的商品目錄。",
                [BotLanguage.SimplifiedChinese] = "没有商品符合您输入的关键字：「{0}」。\n如果您想购买商品，请输入商品名称或商品代码，或造访相关的商品目录。",
            },
            ["CartUpdated"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "{0} units of {1} was added to your shopping cart. ",
                [BotLanguage.TraditionalChinese] = "{0}件{1}已加入到您的購物車。",
                [BotLanguage.SimplifiedChinese] = "{0}件{1}已加入到您的购物车。",
            },            
            ["ChatbotSays"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "\n(by 🤖 assistant)",
                [BotLanguage.TraditionalChinese] = "\n(🤖如此說)",
                [BotLanguage.SimplifiedChinese] = "\n(🤖如此说)",
            },
            ["CodeSent"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = $"A {{0}}-digit confirmation code has been sent to email {{1}}. Please input the code in {{2}} minutes. If you wish to use another email address for receiving the confirmation code, please input '{KillCode}'.",
                [BotLanguage.TraditionalChinese] = $"一個{{0}}位數字的確認碼已傳送至電子郵件地址 {{1}}。請在{{2}}分鐘內輸入該確認碼。如果您想使用其他電子郵件地址接收確認碼，請輸入「{KillCode_CN}」。",
                [BotLanguage.SimplifiedChinese] = $"一个{{0}}位数字的确认码已传送至电子邮件地址 {{1}}。请在{{2}}分钟内输入该确认码。如果您想使用其他电子邮件地址接收确认码，请输入「{KillCode_CN}」。",
            },
            ["DefaultKeyWordForShoppingIntention"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "BUY",
                [BotLanguage.TraditionalChinese] = "買",
                [BotLanguage.SimplifiedChinese] = "买",
            },
            ["Email_body"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "${OTP} is your confirmation code. ",
                [BotLanguage.TraditionalChinese] = "${OTP}是您的確認碼。",
                [BotLanguage.SimplifiedChinese] = "${OTP}是您的确认码。",
            },
            ["Email_subject"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Thank you for shopping at {0}. ",
                [BotLanguage.TraditionalChinese] = "感謝您在{0}購物。",
                [BotLanguage.SimplifiedChinese] = "感谢您在{0}购物。",
            },
            ["EmptyCart"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Your cart is empty now. ",
                [BotLanguage.TraditionalChinese] = "您的購物車沒有任何商品。",
                [BotLanguage.SimplifiedChinese] = "您的购物车没有任何商品。",
            },
            ["Err_DefaultMsg"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = $"Error in the chatbot. Please visit the relevant catalogue if you wish to make a purchase. Sorry for any inconvenience caused. If you wish to chat with a human, please input '{SOS}'. Our team will reach out to you. ",
                [BotLanguage.TraditionalChinese] = $"機器人發生錯誤。如果您想購買商品，請造訪相關的商品目錄。給您帶來不便，敬請原諒。如果您希望與真人而非聊天機器人交談，請輸入「{SOS_CN}」，我們的團隊將與您聯繫。",
                [BotLanguage.SimplifiedChinese] = $"机器人发生错误。如果您想购买商品，请造访相关的商品目录。给您带来不便，敬请原谅。如果您希望与真人而非聊天机器人交谈，请输入「{SOS_CN}」，我们的团队将与您联系。",
            },
            ["Err_InvalidEmail"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "The email address is invalid. Please input again. ",
                [BotLanguage.TraditionalChinese] = "電子郵件地址無效。請再次輸入。",
                [BotLanguage.SimplifiedChinese] = "电子邮件地址无效。请再次输入。",
            },
            ["Err_OTP_Expired"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "The confirmation code has expired. ",
                [BotLanguage.TraditionalChinese] = "確認碼已過期。",
                [BotLanguage.SimplifiedChinese] = "确认码已过期。",
            },
            ["Err_OTP_Invalid"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "The confirmation code is invalid. ",
                [BotLanguage.TraditionalChinese] = "確認碼無效。",
                [BotLanguage.SimplifiedChinese] = "确认码无效。",
            },
            ["Err_OTP_MaxFailedAttempts"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "You have reached the maximum number of attempts; the confirmation code is now invalid. ",
                [BotLanguage.TraditionalChinese] = "您已達到嘗試次數的上限；確認碼現已無效。",
                [BotLanguage.SimplifiedChinese] = "您已达到尝试次数的上限；确认码现已无效。",
            },
            ["Err_NonPositiveInput"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Please input a positive number. ",
                [BotLanguage.TraditionalChinese] = "請輸入一個正數。",
                [BotLanguage.SimplifiedChinese] = "请输入一个正数。",
            },
            ["Err_NonPositiveInteger"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Please input a positive integer. ",
                [BotLanguage.TraditionalChinese] = "請輸入一個正整數。",
                [BotLanguage.SimplifiedChinese] = "请输入一个正整数。",
            },
            ["Err_NotUnderstand"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "I don't understand the input. ",
                [BotLanguage.TraditionalChinese] = "我不明白輸入的內容。",
                [BotLanguage.SimplifiedChinese] = "我不明白输入的内容。",
            },
            ["Err_OutOfStock"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Sorry, {0} is out of stock. ",
                [BotLanguage.TraditionalChinese] = "抱歉，{0} 缺貨。",
                [BotLanguage.SimplifiedChinese] = "抱歉，{0} 缺货。",
            },
            ["Err_ProductNotFound"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Sorry, that product no longer exists. ",
                [BotLanguage.TraditionalChinese] = "抱歉，該商品已不存在。",
                [BotLanguage.SimplifiedChinese] = "抱歉，该商品已不存在。",
            },
            ["Err_UndeliveredOTP"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "We tried to send a confirmation code to your email address but failed. Please try again with a valid email address. ",
                [BotLanguage.TraditionalChinese] = "我們的確認碼無法傳送到您提供的電子郵件地址。請使用一個有效的電子郵件地址重試。",
                [BotLanguage.SimplifiedChinese] = "我们的确认码无法传送到您提供的电子邮件地址。请使用一个有效的电子邮件地址重试。",
            },
            ["Escalated"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = $"Your request to speak with a human agent is received; our staff will contact you shortly. If you wish to speak with a chatbot again, please input '{BotSummon}'. ",
                [BotLanguage.TraditionalChinese] = $"已收到您希望與真人對話的請求，我們的工作人員將盡快與您聯繫。如果您想再次與聊天機器人對話，請輸入「{BotSummon_CN}」。",
                [BotLanguage.SimplifiedChinese] = $"已收到您希望与真人对话的请求，我们的工作人员会尽快与您联系。如果您想再次与聊天机器人对话，请输入「{BotSummon_CN}」。",
            },
            ["Hint_Cancel"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "(Note: You can input '{0}' to cancel the current operation.) ",
                [BotLanguage.TraditionalChinese] = "（提示：輸入「{0}」可以取消目前操作。）",
                [BotLanguage.SimplifiedChinese] = "（提示：输入「{0}」可以取消目前操作。）",
            },
            ["Hint_CancelAndEscalate"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = $"(Note: You can input '{KillCode}' to cancel the current operation. If you wish to chat with a human rather than a bot, input '{SOS}' and our team will reach out to you.) ",
                [BotLanguage.TraditionalChinese] = $"（提示：輸入「{KillCode_CN}」可以取消目前操作。如果您希望與真人而非聊天機器人交談，請輸入「{SOS_CN}」，我們的團隊將與您聯繫。）",
                [BotLanguage.SimplifiedChinese] = $"（提示：输入「{KillCode_CN}」可以取消目前操作。如果您希望与真人而非聊天机器人交谈，请输入「{SOS_CN}」，我们的团队将与您联系。）",
            },
            ["Hint_Escalate"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = $"(Note: If you wish to chat with a human rather than a bot, input '{SOS}' and our team will reach out to you.) ",
                [BotLanguage.TraditionalChinese] = $"（提示：如果您希望與真人而非聊天機器人交談，請輸入「{SOS_CN}」，我們的團隊將與您聯繫。）",
                [BotLanguage.SimplifiedChinese] = $"（提示：如果您希望与真人而非聊天机器人交谈，请输入「{SOS_CN}」，我们的团队将与您联系。）",
            },
            ["Hint_GenByBot"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "(Note: This message is generated by a bot.) ",
                [BotLanguage.TraditionalChinese] = "（注意：此訊息是由機器人自動生成。）",
                [BotLanguage.SimplifiedChinese] = "（注意：此讯息是由机器人自动生成。）",
            },
            ["HowToBuy"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Send \"{1}\" in a direct message to us if you like to buy \"{0}\".",
                [BotLanguage.TraditionalChinese] = "如果您想購買「{0}」，請發送私訊「{1}」給我們。",
                [BotLanguage.SimplifiedChinese] = "如果您想购买「{0}」，请发送私信「{1}」给我们。",
            },
            ["ListCart"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "You have the following items in your cart now.\n{0}",
                [BotLanguage.TraditionalChinese] = "您的購物車中有以下商品。\n{0}",
                [BotLanguage.SimplifiedChinese] = "您的购物车中有以下商品。\n{0}",
            },
            ["NoneOfTheAbove"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "0 - None of the above ",
                [BotLanguage.TraditionalChinese] = "0 - 以上都不是",
                [BotLanguage.SimplifiedChinese] = "0 - 以上都不是",
            },
            ["ThankYou"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Thank you for your order. Itemized below is your shopping cart.\n{0}Please proceed to {1} to complete the transaction.",
                [BotLanguage.TraditionalChinese] = "謝謝您的訂單。下面列出了您購物車中的商品。\n{0}請造訪 {1} 以完成支付。",
                [BotLanguage.SimplifiedChinese] = "谢谢您的订单。下面列出了您购物车中的商品。\n{0}请造访 {1} 以完成支付。",
            },
            ["TooManyResults"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = $"(Note: More than {{0}} results were found and some are not included above. You may want to try a different input. If you wish to chat with a human rather than a bot, input '{SOS}' and our team will reach out to you.) ",
                [BotLanguage.TraditionalChinese] = $"（注意：找到了超過{{0}}個結果，有些結果未包含在上面。您可以考慮嘗試不同的關鍵字。如果您希望與真人而非聊天機器人交談，請輸入「{SOS_CN}」，我們的團隊將與您聯繫。）",
                [BotLanguage.SimplifiedChinese] = $"（注意：找到了超过{{0}}个结果，有些结果未包含在上面。您可以考虑尝试不同的关键字。如果您希望与真人而非聊天机器人交谈，请输入「{SOS_CN}」，我们的团队将与您联系。）",
            },
            ["VisitCatalogue"] = new Dictionary<BotLanguage, string>()
            {
                [BotLanguage.English] = "Check out our new product catalogue at: ",
                [BotLanguage.TraditionalChinese] = "造訪我們新的商品目錄：",
                [BotLanguage.SimplifiedChinese] = "造访我们新的商品目录：",
            },
        };        

        public static readonly Dictionary<string, int> YesNo = new()
        {
            ["是"] = 1,
            ["對"] = 1,
            ["对"] = 1,
            ["yes"] = 1,
            ["y"] = 1,
            ["yup"] = 1,
            ["yea"] = 1,
            ["yeah"] = 1,
            ["sure"] = 1,            
            ["oui"] = 1,
            ["不是"] = 2,
            ["不"] = 2,
            ["不對"] = 2,
            ["不对"] = 2,
            ["否"] = 2,
            ["no"] = 2,
            ["n"] = 2,
            ["nope"] = 2,
            ["non"] = 2,
        };        
    }
}
