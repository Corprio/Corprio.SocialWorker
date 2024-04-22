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
    'messages', 'mention',
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
/*!*************************************!*\
  !*** ./Views/StoryMention/Index.ts ***!
  \*************************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
const Constants_1 = __webpack_require__(/*! ../Shared/Constants */ "./Views/Shared/Constants.ts");
// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />
/**
 * React to change in the user's Facebook login status
 * @param response-Facebook's response to getLoginStatus()
 */
function handleFbLoginStatusChange(response) {
    /*console.log(response);*/
    if ((response === null || response === void 0 ? void 0 : response.status) === 'connected') {
        $('#loginBtn').hide();
        $('#logoutBtn').show();
        FB.api('/me', { fields: 'name' }, function (response) {
            const alert = `<div class="alert alert-success my-3">` +
                `<i class="fa-regular fa-circle-check"></i>` +
                `&nbsp;${vdata.localizer.fbConnected.replaceAll('{0}', response.name).replaceAll('{1}', vdata.settings.shortName)}` +
                `</div>`;
            $('#fb-dialogue').empty().append(alert);
            $('#mentions').show();
            //$.ajax({
            //    url: vdata.url.getPage,
            //    method: 'GET',
            //    data: { metaUserId: response.id },
            //    success: function (data) {
            //        console.log('mentions:');
            //        console.log(data);
            //    }
            //});
            $('#mentions').dxDataGrid({
                dataSource: {
                    store: {
                        type: 'odata',
                        version: 2,
                        url: vdata.url.getPage,
                        beforeSend(request) {
                            request.params.metaUserId = response.id;
                        },
                    },
                },
                paging: { pageSize: 10 },
                pager: {
                    showPageSizeSelector: true,
                    allowedPageSizes: [10, 25, 50, 100],
                },
                remoteOperations: false,
                searchPanel: {
                    visible: true,
                    highlightCaseSensitive: true,
                },
                /*groupPanel: { visible: true },*/
                grouping: { autoExpandAll: false },
                allowColumnReordering: true,
                rowAlternationEnabled: true,
                showBorders: true,
                width: '100%',
                columns: [
                    {
                        dataField: 'CreateDate',
                        caption: vdata.localizer.storyMentionDate,
                        dataType: 'datetime',
                        sortOrder: 'desc'
                    },
                    {
                        dataField: 'Mentioned',
                        caption: vdata.localizer.storyMentionMentioned,
                        dataType: 'string'
                    },
                    {
                        dataField: 'CreatorName',
                        caption: vdata.localizer.storyMentionCreator,
                        dataType: 'string'
                    },
                    {
                        type: 'buttons',
                        buttons: [{
                                name: 'Story',
                                icon: 'find',
                                hint: vdata.localizer.storyMentionHint,
                                onClick: function (e) {
                                    window.open(e.row.data.CDNUrl, '_blank');
                                }
                            }]
                    }
                ],
            });
        });
        /*The following functions run in a non-blocking manner because there is no interdependence*/
        getPages();
    }
    else {
        $('#loginBtn').show();
        $('#logoutBtn').hide();
        const alert = `<div class="alert alert-warning">` +
            `<i class="fa-regular fa-circle-exclamation"></i>` +
            `&nbsp;${vdata.localizer.fbNotConnected.replaceAll('{0}', vdata.settings.shortName)}` +
            `</div>`;
        $('#fb-dialogue').empty().append(alert);
        $('#mentions').hide();
    }
}
/**
 * Check the user's FB login status
 */
function checkLoginState() {
    FB.getLoginStatus(function (response) {
        handleFbLoginStatusChange(response);
    });
}
/**
 * Turn on Meta's Built-in NLP to help detect locale (and meaning)
 * (note: we run this function on the client side to reduce workload on the server side)
 * https://developers.facebook.com/docs/graph-api/reference/page/nlp_configs/
 * https://developers.facebook.com/docs/messenger-platform/built-in-nlp/
 * @param page_id-ID of Facebook page
 * @param page_access_token-Page access token
 * @returns
 */
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
/**
 * Add webhooks to page subscriptions (IMPORTANT: subscribe to the fields as those subscribed on App level)
 * (note: we run this function on the client side to reduce workload on the server side)
 * https://developers.facebook.com/docs/messenger-platform/webhooks/#connect-your-app
 * @param page_id-ID of Facebook page
 * @param page_access_token-Page access token
 * @returns
 */
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
/**
 * Query the pages on which the user has a role and trigger webhook subscription for each of them
 */
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
        handleFbLoginStatusChange(response); // Returns the login status.
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
    /*corprio.page.initTour({ defaultTour: 'getstarted.index', autoStart: true });*/
    // facebook-related stuff
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

})();

/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Index.js.map