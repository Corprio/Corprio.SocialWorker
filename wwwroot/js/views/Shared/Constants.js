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
/*!***********************************!*\
  !*** ./Views/Shared/Constants.ts ***!
  \***********************************/

//export const PERMISSIONS: string[] = ['email', 'public_profile', 'business_management', 'pages_manage_metadata',
//    'pages_messaging', 'pages_manage_posts', 'pages_manage_engagement', 'instagram_basic', 'instagram_content_publish',
//    'instagram_manage_comments', 'instagram_manage_messages'];
Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.PERMISSIONS = void 0;
/**
 * updates on 5 Feb 2024:
 * - tested that 'pages_manage_engagement' was uncessary for reading replies to posts on FB page
 * - advanced access for 'instagram_manage_comments' is NOT required for receiving webhook triggered by no-role users
 * - advanced access for 'pages_messaging' is NOT required for sending messages to no-role users
 * - advanced access for 'instagram_manage_messages' is required for sending messages to no-role users
 * - 'business_management' is required for viewing pages managed by the user
 * - 'pages_read_engagement' is required for creating posts on FB pages
 * - 'pages_manage_metadata' is required for managing webhooks
 * - 'pages_read_user_content' is required for reading comments on posts on FB pages
 */
exports.PERMISSIONS = ['email', 'public_profile', 'business_management', 'pages_show_list', 'pages_manage_metadata',
    'pages_messaging', 'pages_manage_posts', 'instagram_basic', 'instagram_content_publish',
    'instagram_manage_comments', 'instagram_manage_messages', 'pages_read_engagement', 'pages_read_user_content'];

})();

/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Constants.js.map