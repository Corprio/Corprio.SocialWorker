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


// 'business_management' is required for viewing pages managed by the user
//export const PERMISSIONS: string[] = ['email', 'public_profile', 'business_management', 'pages_manage_metadata',
//    'pages_messaging', 'pages_manage_posts', 'pages_manage_engagement', 'instagram_basic', 'instagram_content_publish',
//    'instagram_manage_comments', 'instagram_manage_messages'];
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.PERMISSIONS = void 0;
// updates on 5 Feb 2024:
// tested that 'pages_manage_engagement' was uncessary
// 'business_management' is required for viewing pages managed by the user
// 'pages_read_engagement' is required for creating posts on FB pages
// 'pages_manage_metadata' is required for managing webhooks
// 'pages_read_user_content' is required for reading comments on posts on FB pages
exports.PERMISSIONS = ['public_profile', 'business_management', 'pages_show_list', 'pages_manage_metadata',
    'pages_messaging', 'pages_manage_posts', 'instagram_basic', 'instagram_content_publish',
    'instagram_manage_comments', 'instagram_manage_messages', 'pages_read_engagement', 'pages_read_user_content'];


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
/*!******************************************!*\
  !*** ./Views/ReconnectFacebook/Index.ts ***!
  \******************************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
const Constants_1 = __webpack_require__(/*! ../Shared/Constants */ "./Views/Shared/Constants.ts");
// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />
// The order of the following three code blocks is extremely important. It has to be:
// (1) fbAsyncInit, (2) IIFE, then (3) whatever needs to run upon DOM being loaded
// For reference, see: https://www.devils-heaven.com/facebook-javascript-sdk-login
window.fbAsyncInit = function () {
    FB.init({
        appId: vdata.settings.metaApiID,
        cookie: true,
        xfbml: true,
        version: vdata.settings.metaApiVersion
    });
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response); // Returns the login status.
    });
};
/**
 * IIFE to make a reference to the SDK, if it does not already exist
 */
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
/**
 * Entry point
 */
$(function () {
    $('#loginBtn').on('click', function () {
        FB.login((response) => {
            if (response.authResponse) {
                checkLoginState();
            }
        }, {
            scope: Constants_1.PERMISSIONS.toString(),
            return_scopes: true
        });
    });
    $('#logoutBtn').on('click', function () {
        FB.logout(checkLoginState);
    });
});
/**
 * Check the user's FB login status
 */
function checkLoginState() {
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response);
    });
}
/**
 * React to change in the user's Facebook login status
 * @param response-Facebook's response to getLoginStatus()
 */
function statusChangeCallback(response) {
    var _a, _b;
    if ((response === null || response === void 0 ? void 0 : response.status) === 'connected') {
        $('#loginBtn').hide();
        $('#logoutBtn').show();
        reassignMetaProfile((_a = response.authResponse) === null || _a === void 0 ? void 0 : _a.userID, (_b = response.authResponse) === null || _b === void 0 ? void 0 : _b.accessToken, false);
    }
    else {
        // Not logged into Facebook or we are unable to tell.
        $('#loginBtn').show();
        $('#logoutBtn').hide();
        $('#reconnect-warning').show();
        const alert = `<div class="alert alert-warning">` +
            `<i class="fa-regular fa-circle-exclamation"></i>` +
            `&nbsp;${vdata.localizer.fbNotConnected.replaceAll('{0}', vdata.settings.shortName)}` +
            `</div>`;
        $('#reconnect-dialogue').empty().append(alert);
    }
}
/**
 * Pass the short-lived user access token to server, which will check if another active Meta profile exists for the same Meta ID
 * @param metaId-Facebook user ID
 * @param accessToken-Short-lived user access token
 * @param reAssignMetaProfile-True if the Facebook account can be reassigned from one organization to another
 * @returns
 */
function reassignMetaProfile(metaId, accessToken, reAssignMetaProfile) {
    return $.post({
        url: vdata.actions.refreshAccessToken,
        data: {
            metaId: metaId,
            token: accessToken,
            reAssignMetaProfile: reAssignMetaProfile,
        }
    }).done(function () {
        $('#reconnect-warning').hide();
        FB.api('/me', { fields: 'name' }, function (response) {
            const alert = `<div class="alert alert-success my-3">` +
                `<i class="fa-regular fa-circle-check"></i>` +
                `&nbsp;${vdata.localizer.fbConnected.replaceAll('{0}', response.name).replaceAll('{1}', vdata.settings.shortName)}` +
                `</div>`;
            $('#reconnect-dialogue').empty().append(alert);
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        // note: 409 means that another organization is connected with the Facebook account
        if (jqXHR.status !== 409) {
            FB.logout(checkLoginState);
            return corprio.formatError(jqXHR, textStatus, errorThrown);
        }
        var prompt = DevExpress.ui.dialog.custom({
            title: vdata.localizer.confirmation,
            messageHtml: `<p>${vdata.localizer.finalWarning}</p>`,
            buttons: [{
                    text: vdata.localizer.confirm,
                    onClick: function () { reassignMetaProfile(metaId, accessToken, true); },
                    type: "danger",
                }, {
                    text: vdata.localizer.cancel,
                    onClick: function () { FB.logout(checkLoginState); },
                    type: "normal",
                }]
        });
        prompt.show();
    });
}

})();

/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Index.js.map