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
/*!*********************************!*\
  !*** ./Views/Settings/Enums.ts ***!
  \*********************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.Selector = exports.MessageType = void 0;
var MessageType;
(function (MessageType) {
    MessageType["CATALOGUE"] = "catalogue";
    MessageType["PRODUCT"] = "product";
})(MessageType || (exports.MessageType = MessageType = {}));
var Selector;
(function (Selector) {
    Selector["catalogueSetting"] = "#catalogue-setting";
    Selector["blockPanel_Catalogue"] = "#block-panel-catalogue";
    Selector["blockPanel_Catalogue_RemoveButton_Last"] = "#block-panel-catalogue .remove-btn:last";
    Selector["blockPanel_Catalogue_Input_Last"] = "#block-panel-catalogue .custom-block:last";
    Selector["blockPanel_Catalogue_DragBlock_Last"] = "#block-panel-catalogue .drag-block:last";
    Selector["componentSelect_Catalogue"] = "#component-select-catalogue";
    Selector["addComponentButton_Catalogue"] = "#component-btn-catalogue";
    Selector["customTextInput_Catalogue"] = "#custom-input-catalogue";
    Selector["addCustomTextButton_Catalogue"] = "#custom-btn-catalogue";
    Selector["restoreDefaultButton_Catalogue"] = "#restore-default-btn-catalogue";
    Selector["saveTemplateButton_Catalogue"] = "#save-template-btn-catalogue";
    Selector["previewPanel_Catalogue"] = "#preview-panel-catalogue";
    Selector["blockPanel_Product"] = "#block-panel-product";
    Selector["blockPanel_Product_RemoveButton_Last"] = "#block-panel-product .remove-btn:last";
    Selector["blockPanel_Product_Input_Last"] = "#block-panel-product .custom-block:last";
    Selector["blockPanel_Product_DragBlock_Last"] = "#block-panel-product .drag-block:last";
    Selector["componentSelect_Product"] = "#component-select-product";
    Selector["addComponentButton_Product"] = "#component-btn-product";
    Selector["customTextInput_Product"] = "#custom-input-product";
    Selector["addCustomTextButton_Product"] = "#custom-btn-product";
    Selector["restoreDefaultButton_Product"] = "#restore-default-btn-product";
    Selector["saveTemplateButton_Product"] = "#save-template-btn-product";
    Selector["previewPanel_Product"] = "#preview-panel-product";
    Selector["loadIndicator_Top"] = "#load-indicator-top";
    Selector["keywordInput_Product"] = "#keyword-input-product";
})(Selector || (exports.Selector = Selector = {}));

})();

/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Enums.js.map