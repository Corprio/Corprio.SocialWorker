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
  !*** ./Views/GetStarted/Enums.ts ***!
  \***********************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.Selector = exports.MessageType = void 0;
var MessageType;
(function (MessageType) {
    MessageType["CataloguePost"] = "CataloguePost";
    MessageType["ProductPost"] = "ProductPost";
})(MessageType || (exports.MessageType = MessageType = {}));
var Selector;
(function (Selector) {
    Selector["addComponentButton_Catalogue"] = "#component-btn-catalogue";
    Selector["addCustomTextButton_Catalogue"] = "#custom-btn-catalogue";
    Selector["addComponentButton_Product"] = "#component-btn-product";
    Selector["addCustomTextButton_Product"] = "#custom-btn-product";
    Selector["blockPanel_Catalogue"] = "#block-panel-catalogue";
    Selector["blockPanel_Catalogue_RemoveButton_Last"] = "#block-panel-catalogue .remove-btn:last";
    Selector["blockPanel_Catalogue_Input_Last"] = "#block-panel-catalogue .custom-block:last";
    Selector["blockPanel_Catalogue_DragBlock_Last"] = "#block-panel-catalogue .drag-block:last";
    Selector["blockPanel_Product"] = "#block-panel-product";
    Selector["blockPanel_Product_RemoveButton_Last"] = "#block-panel-product .remove-btn:last";
    Selector["blockPanel_Product_Input_Last"] = "#block-panel-product .custom-block:last";
    Selector["blockPanel_Product_DragBlock_Last"] = "#block-panel-product .drag-block:last";
    Selector["catalogueSetting"] = "#catalogue-setting";
    Selector["componentSelect_Catalogue"] = "#component-select-catalogue";
    Selector["componentSelect_Product"] = "#component-select-product";
    Selector["customTextInput_Catalogue"] = "#custom-input-catalogue";
    Selector["customTextInput_Product"] = "#custom-input-product";
    Selector["fbDialogue"] = "#fb-dialogue";
    Selector["fbDialogue2"] = "#fb-dialogue2";
    Selector["keywordInput_Product"] = "#keyword-input-product";
    Selector["loadIndicator_Top"] = "#load-indicator-top";
    Selector["loginButton"] = "#loginBtn";
    Selector["logoutButton"] = "#logoutBtn";
    Selector["previewPanel_Catalogue"] = "#preview-panel-catalogue";
    Selector["previewPanel_Product"] = "#preview-panel-product";
    Selector["restoreDefaultButton_Catalogue"] = "#restore-default-btn-catalogue";
    Selector["restoreDefaultButton_Product"] = "#restore-default-btn-product";
    Selector["saveSettingButtons"] = ".save-setting-btn";
    Selector["saveTemplateButton_Catalogue"] = "#save-template-btn-catalogue";
    Selector["saveTemplateButton_Product"] = "#save-template-btn-product";
})(Selector || (exports.Selector = Selector = {}));

})();

/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Enums.js.map