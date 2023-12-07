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
var __webpack_exports__ = {};
// This entry need to be wrapped in an IIFE because it uses a non-standard name for the exports (exports).
(() => {
var exports = __webpack_exports__;
/*!********************************!*\
  !*** ./Views/MetaApi/Index.ts ***!
  \********************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />
window.fbAsyncInit = function () {
    console.log('fbAsyncInit is doing its things...');
    FB.init({
        appId: '312852768233605',
        cookie: true,
        xfbml: true,
        version: 'v18.0' // Use this Graph API version for this call.
    });
    document.addEventListener("DOMContentLoaded", (event) => {
        console.log("DOM fully loaded and parsed");
        $('fb:login-button').on('click', checkLoginState);
    });
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response); // Returns the login status.
    });
};
function statusChangeCallback(response) {
    var _a, _b;
    console.log('statusChangeCallback');
    console.log(response); // The current login status of the person.
    if ((response === null || response === void 0 ? void 0 : response.status) === 'connected') { // Logged into your webpage and Facebook.                                        
        getPages();
        refreshAccessToken((_a = response.authResponse) === null || _a === void 0 ? void 0 : _a.userID, (_b = response.authResponse) === null || _b === void 0 ? void 0 : _b.accessToken);
    }
    else { // Not logged into your webpage or we are unable to tell.
        $('#status').text('Please log into this webpage.');
    }
}
function checkLoginState() {
    // Called when a person is finished with the Login Button.    
    FB.getLoginStatus(function (response) {
        // See the onlogin handler
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
// add webhooks to page subscriptions (IMPORTANT: subscribe to the fields as those subscribed on App level)
function addPageSubscriptions(page_id, page_access_token) {
    console.log(`Subscribing to page ${page_id}...`);
    return FB.api(`/${page_id}/subscribed_apps`, 'post', {
        subscribed_fields: [
            'feed',
            // webhook for pages: https://developers.facebook.com/docs/graph-api/webhooks/getting-started/webhooks-for-pages/
            'messages',
            // any other webhook event: https://developers.facebook.com/docs/messenger-platform/webhook/#events
        ],
        access_token: page_access_token,
    }, function (response) {
        if (response && !response.error) {
            console.log({ response });
        }
        else {
            console.error(response === null || response === void 0 ? void 0 : response.error);
        }
    });
}
function getPages() {
    FB.api('/me/accounts', function (response) {
        if (response && !response.error) {
            console.log('response from getPages()...');
            console.log({ response });
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