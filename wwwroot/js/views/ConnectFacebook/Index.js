(function webpackUniversalModuleDefinition(root, factory) {
	if(typeof exports === 'object' && typeof module === 'object')
		module.exports = factory();
	else if(typeof define === 'function' && define.amd)
		define([], factory);
	else if(typeof exports === 'object')
		exports["vjs"] = factory();
	else
		root["vjs"] = factory();
})(self, () => {
return /******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ "./Views/Shared/Constants.ts":
/*!***********************************!*\
  !*** ./Views/Shared/Constants.ts ***!
  \***********************************/
/***/ ((__unused_webpack_module, exports) => {


//export const PERMISSIONS: string[] = ['email', 'public_profile', 'business_management', 'pages_manage_metadata',
//    'pages_messaging', 'pages_manage_posts', 'pages_manage_engagement', 'instagram_basic', 'instagram_content_publish',
//    'instagram_manage_comments', 'instagram_manage_messages'];
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.WEBHOOKS = exports.PERMISSIONS = void 0;
/**
 * updates on 8 Feb 2024:
 * - advanced access for 'instagram_manage_comments' is NOT required for receiving webhook triggered by no-role users
 * - advanced access for 'pages_messaging' is NOT required for sending messages to no-role users
 * - advanced access for 'instagram_manage_messages' is required for sending messages to no-role users
 * - 'business_management' is required for viewing pages managed by the user
 * - 'pages_read_engagement' is required for creating posts on FB pages
 * - 'pages_manage_metadata' is required for managing webhooks
 * - 'pages_read_user_content' is required for reading comments on posts on FB pages
 * - 'pages_manage_engagement' is retained for making comments on other comments on FB pages
 */
//export const PERMISSIONS: string[] = ['email', 'public_profile', 'business_management', 'pages_show_list', 'pages_manage_metadata',
//    'pages_messaging', 'pages_manage_posts', 'instagram_basic', 'instagram_content_publish', 'pages_manage_engagement',
//    'instagram_manage_comments', 'instagram_manage_messages', 'pages_read_engagement', 'pages_read_user_content'];
exports.PERMISSIONS = ['email', 'public_profile', 'business_management', 'pages_show_list', 'pages_manage_metadata',
    'pages_messaging', 'pages_manage_posts', 'instagram_basic', 'instagram_content_publish', 'instagram_manage_messages'];
exports.WEBHOOKS = [
    /*'feed',*/
    // webhook for pages: https://developers.facebook.com/docs/graph-api/webhooks/getting-started/webhooks-for-pages/
    'messages',
    // any other webhook event: https://developers.facebook.com/docs/messenger-platform/webhook/#events
];


/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId](module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
var __webpack_exports__ = {};
// This entry need to be wrapped in an IIFE because it need to be isolated against other modules in the chunk.
(() => {
var exports = __webpack_exports__;
/*!****************************************!*\
  !*** ./Views/ConnectFacebook/Index.ts ***!
  \****************************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
const Constants_1 = __webpack_require__(/*! ../Shared/Constants */ "./Views/Shared/Constants.ts");
// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />
// For the order of the following three code blocks, see: https://www.devils-heaven.com/facebook-javascript-sdk-login
window.fbAsyncInit = function () {
    FB.init({
        appId: '312852768233605',
        cookie: true, // Enable cookies to allow the server to access the session.
        xfbml: true, // Parse social plugins on this webpage.
        version: 'v18.0' // Use this Graph API version for this call.
    });
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response); // Returns the login status.
    });
};
(function (element, tagName, selector) {
    var js, fjs = element.getElementsByTagName(tagName)[0];
    if (element.getElementById(selector)) {
        return;
    }
    js = element.createElement(tagName);
    js.id = selector;
    js.src = "https://connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));
document.addEventListener("DOMContentLoaded", (event) => {
    console.log("DOM fully loaded and parsed");
    $('#loginBtn').on('click', function () {
        console.log('Logging into Facebook...');
        FB.login((response) => {
            if (response.authResponse) {
                //user just authorized your app                
                checkLoginState();
            }
        }, {
            scope: Constants_1.PERMISSIONS.toString(),
            return_scopes: true
        });
    });
    $('#logoutBtn').on('click', function () {
        console.log('Logging out from Facebook...');
        FB.logout(checkLoginState);
    });
});
function statusChangeCallback(response) {
    var _a, _b;
    console.log('statusChangeCallback');
    /*console.log(response);*/
    if ((response === null || response === void 0 ? void 0 : response.status) === 'connected') {
        // Logged into Facebook.
        $('#loginBtn').hide();
        $('#logoutBtn').show();
        FB.api('/me', { fields: 'name' }, function (response) {
            $('#page-headline').text('Good to have you back, ' + response.name + '!');
        });
        $('#page-var-text').text('Corprio Social Worker can now do the following: ');
        getPages();
        refreshAccessToken((_a = response.authResponse) === null || _a === void 0 ? void 0 : _a.userID, (_b = response.authResponse) === null || _b === void 0 ? void 0 : _b.accessToken);
    }
    else {
        // Not logged into Facebook or we are unable to tell.
        $('#loginBtn').show();
        $('#logoutBtn').hide();
        $('#page-headline').text('Welcome to Corprio Social Worker!');
        $('#page-var-text').text('Click the login button below to connect your Facebook page(s) and Instagram accounts with Corprio. ' +
            'Once it is done, Corprio Social Worker can do the following magic for you: ');
    }
}
// Called when the user is finished with the Login/Logout Button.
function checkLoginState() {
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response);
    });
}
function refreshAccessToken(metaId, accessToken) {
    return $.post({
        url: vdata.actions.refreshAccessToken,
        data: {
            metaId: metaId,
            token: accessToken,
        }
    }).done(function () {
        console.log(`Token for ${metaId} is fed to backend successfully.`);
    }).fail(function () {
        console.log(`Failed to pass the token for ${metaId} to backend.`);
    });
}
// turn on Meta's Built-in NLP to help detect locale (and meaning)
// (note: we run this function on the client side to reduce workload on the server side)
// https://developers.facebook.com/docs/graph-api/reference/page/nlp_configs/
// https://developers.facebook.com/docs/messenger-platform/built-in-nlp/
function turnOrNLP(page_id, page_access_token) {
    console.log(`Turning on NLP for page ${page_id}`);
    return FB.api(`/${page_id}/nlp_configs`, 'post', {
        nlp_enabled: true,
        model: 'CHINESE',
        access_token: page_access_token
    }, function (response) {
        console.log('Response from nlp_configs:');
        console.log(response);
    });
}
// add webhooks to page subscriptions (IMPORTANT: subscribe to the fields as those subscribed on App level)
// (note: we run this function on the client side to reduce workload on the server side)
// https://developers.facebook.com/docs/messenger-platform/webhooks/#connect-your-app
function addPageSubscriptions(page_id, page_access_token) {
    console.log(`Subscribing to page ${page_id}`);
    return FB.api(`/${page_id}/subscribed_apps`, 'post', {
        subscribed_fields: Constants_1.WEBHOOKS,
        access_token: page_access_token,
    }, function (response) {
        console.log('Response from subscribed_apps:');
        console.log(response);
        if (response && !response.error) {
            return turnOrNLP(page_id, page_access_token);
        }
    });
}
function getPages() {
    FB.api('/me/accounts', function (response) {
        if (response && !response.error) {
            /*console.log('response from getPages()...');*/
            /*console.log({ response });*/
            for (let i = 0; i < response.data.length; i++) {
                const page_id = response.data[i].id;
                const page_access_token = response.data[i].access_token;
                addPageSubscriptions(page_id, page_access_token);
            }
        }
        else {
            console.error(response.error);
        }
    });
}

})();

/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Index.js.map