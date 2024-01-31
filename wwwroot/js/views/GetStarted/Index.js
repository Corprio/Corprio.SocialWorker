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

/***/ "./Views/GetStarted/Enums.ts":
/*!***********************************!*\
  !*** ./Views/GetStarted/Enums.ts ***!
  \***********************************/
/***/ ((__unused_webpack_module, exports) => {


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
    Selector["previewButtons"] = ".preview-btn";
    Selector["previewPanel_Catalogue"] = "#preview-panel-catalogue";
    Selector["previewPanel_Product"] = "#preview-panel-product";
    Selector["restoreDefaultButton_Catalogue"] = "#restore-default-btn-catalogue";
    Selector["restoreDefaultButton_Product"] = "#restore-default-btn-product";
    Selector["saveSettingButtons"] = ".save-setting-btn";
    Selector["saveTemplateButton_Catalogue"] = "#save-template-btn-catalogue";
    Selector["saveTemplateButton_Product"] = "#save-template-btn-product";
})(Selector || (exports.Selector = Selector = {}));


/***/ }),

/***/ "./Views/Shared/Constants.ts":
/*!***********************************!*\
  !*** ./Views/Shared/Constants.ts ***!
  \***********************************/
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.PERMISSIONS = void 0;
// 'business_management' is required for viewing pages managed by the user
exports.PERMISSIONS = ['email', 'public_profile', 'business_management', 'pages_manage_metadata',
    'pages_messaging', 'pages_manage_posts', 'pages_manage_engagement', 'instagram_basic', 'instagram_content_publish',
    'instagram_manage_comments', 'instagram_manage_messages'];


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
/*!***********************************!*\
  !*** ./Views/GetStarted/Index.ts ***!
  \***********************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
const Enums_1 = __webpack_require__(/*! ./Enums */ "./Views/GetStarted/Enums.ts");
const Constants_1 = __webpack_require__(/*! ../Shared/Constants */ "./Views/Shared/Constants.ts");
// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />
// magic numbers
const DATASET_TRUEVALUE = 'true-value'; // this attribute contains the value of a standard component (e.g., %lineBreak%)
const DATASET_REORDERED = 'reordered'; // this attribute indicates if the block will be moved to another position
const DATASET_SWAPPED = 'swapped'; // this attribute indicates if the block will give up its position to the reordered block
// the following dictionary maps the message type to the relevant selectors
const selectorMapping = {
    'CataloguePost': {
        blockPanel: Enums_1.Selector.blockPanel_Catalogue,
        blockPanelInput_Last: Enums_1.Selector.blockPanel_Catalogue_Input_Last,
        componentSelect: Enums_1.Selector.componentSelect_Catalogue,
        componentAddButton: Enums_1.Selector.addComponentButton_Catalogue,
        customTextInput: Enums_1.Selector.customTextInput_Catalogue,
        customTextAddButton: Enums_1.Selector.addCustomTextButton_Catalogue,
        saveTemplateButton: Enums_1.Selector.saveTemplateButton_Catalogue,
        restoreDefaultButton: Enums_1.Selector.restoreDefaultButton_Catalogue,
        previewPanel: Enums_1.Selector.previewPanel_Catalogue,
        removeButton_Last: Enums_1.Selector.blockPanel_Catalogue_RemoveButton_Last,
        dragBlock_Last: Enums_1.Selector.blockPanel_Catalogue_DragBlock_Last,
        validSelectOptions: {},
    },
    'ProductPost': {
        blockPanel: Enums_1.Selector.blockPanel_Product,
        blockPanelInput_Last: Enums_1.Selector.blockPanel_Product_Input_Last,
        componentSelect: Enums_1.Selector.componentSelect_Product,
        componentAddButton: Enums_1.Selector.addComponentButton_Product,
        customTextInput: Enums_1.Selector.customTextInput_Product,
        customTextAddButton: Enums_1.Selector.addCustomTextButton_Product,
        saveTemplateButton: Enums_1.Selector.saveTemplateButton_Product,
        restoreDefaultButton: Enums_1.Selector.restoreDefaultButton_Product,
        previewPanel: Enums_1.Selector.previewPanel_Product,
        removeButton_Last: Enums_1.Selector.blockPanel_Product_RemoveButton_Last,
        dragBlock_Last: Enums_1.Selector.blockPanel_Product_DragBlock_Last,
        validSelectOptions: {},
    }
};
// contains all the values of a valid select options and maps them to the text that should be rendered in the template and preview panels
let validSelectOptions = {};
// HTML element that will be displayed when 'loading' happens
let loadIndicatorTop;
// this object 'remembers' the value and message type of the block being dragged
const draggedBlock = { value: null, type: null };
/**
 * 'Remember' details of the block being dragged
 * @param obj-The HTML element that triggers this function (we can't simply use 'this' because this function is called inside an anonymous function)
 * @param messageType-Publication of products or catalogues
 */
function handleDragStart(obj, messageType) {
    const $dragged = $(obj);
    draggedBlock.value = ($dragged.data(DATASET_TRUEVALUE) in validSelectOptions) ? $dragged.data(DATASET_TRUEVALUE) : String($dragged.find('input').val());
    draggedBlock.type = messageType;
    $dragged.data(DATASET_REORDERED, true);
}
/**
 * Disable default behaviour when a HTML element is dragged
 * @param ev-Event object
 */
function handleDragOver(ev) {
    ev.preventDefault();
}
/**
 * Handle the dropping of a block
 * @param ev-Event object
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function handleDrop(ev, messageType) {
    ev.preventDefault();
    if (messageType !== draggedBlock.type) {
        return;
    }
    let $target = $(ev.target);
    if ($target.is('input') || $target.is('i')) {
        $target = $target.parent();
    }
    let deadend = true; // if true, then the dragged block is moved to the end
    if ($target.is('span')) {
        $target.data(DATASET_SWAPPED, true);
        deadend = false;
    }
    let templateString = '';
    $(selectorMapping[messageType].blockPanel).children('span').each(function () {
        if ($(this).data(DATASET_REORDERED)) {
            return;
        }
        if ($(this).data(DATASET_SWAPPED)) {
            templateString += draggedBlock.value + vdata.templateComponents.separator;
        }
        const key = $(this).data(DATASET_TRUEVALUE);
        if (key in selectorMapping[messageType].validSelectOptions) {
            templateString += key + vdata.templateComponents.separator;
        }
        else {
            templateString += sanitizeInput(String($(this).find('input').val())) + vdata.templateComponents.separator;
        }
    });
    if (deadend) {
        templateString += draggedBlock.value;
    }
    const templateArray = templateString.split(vdata.templateComponents.separator);
    renderTemplate(templateArray, messageType);
    renderPreview(messageType);
}
/**
 * Render a block for custom text
 * @param text-Text to be displayed in the block
 * @returns A HTML element, or a block, that represents the custom text
 */
function customTextBlock(text) {
    return `<span draggable="true" class="rounded border border-primary p-2 mx-1 mb-1 d-inline-block text-wrap drag-block">` +
        `<input class="custom-block" value="${text}">` +
        `<i class="fa-duotone fa-x ml-2 remove-btn"></i>` +
        `</span>`;
}
/**
 * Render a block for standard message component
 * @param dataset-Value to be assigned to the block's data attribute
 * @param text-Text to be displayed in the block
 * @returns A HTML element, or a block, that represents the standard message component
 */
function standardComponentBlock(dataset, text) {
    return `<span draggable="true" data-${DATASET_TRUEVALUE}="${dataset}" class="rounded border border-secondary p-2 mx-1 mb-1 d-inline-block text-wrap drag-block">` +
        `<input disabled value="${text}">` +
        `<i class="fa-duotone fa-x ml-2 remove-btn"></i>` +
        `</span>`;
}
/**
 * Retreive the stored keyword from the backend and render it in the relevant input field
 * @returns
 */
function restoreKeyword() {
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview = vdata.model.keyword;
    $(Enums_1.Selector.keywordInput_Product).val(reverseSanitize(vdata.model.keyword));
}
/**
 * Retrieve the stored template from the backend and render it on the template and preview panels
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function restoreTemplate(messageType) {
    const templateString = messageType === Enums_1.MessageType.CataloguePost ? vdata.model.catalogueTemplate : vdata.model.productTemplate;
    if (!templateString) {
        return;
    }
    const templateArray = templateString.split(vdata.templateComponents.separator);
    renderTemplate(templateArray, messageType);
    renderPreview(messageType);
}
/**
 * Render the template panel
 * @param templateArray-The template as an array of string
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function renderTemplate(templateArray, messageType) {
    if (!templateArray) {
        return;
    }
    $(selectorMapping[messageType].blockPanel).empty();
    for (let i = 0; i < templateArray.length; i++) {
        if (!templateArray[i]) {
            continue;
        }
        const element = (templateArray[i] in selectorMapping[messageType].validSelectOptions)
            ? standardComponentBlock(templateArray[i], selectorMapping[messageType].validSelectOptions[templateArray[i]].panel)
            : customTextBlock(templateArray[i]);
        $(selectorMapping[messageType].blockPanel).append(element);
        $(selectorMapping[messageType].dragBlock_Last).on('dragstart', function () { handleDragStart(this, messageType); });
        $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeBlock(this, messageType); });
        $(selectorMapping[messageType].blockPanelInput_Last).on('keyup', function () { renderPreview(messageType); });
    }
    return;
}
/**
 * Render the template and preview panels with the default template defined on server side
 * @param messageType-Publication of products or catalogues
 */
function restoreDefaultTemplate(messageType) {
    let defaultTemplate;
    switch (messageType) {
        case Enums_1.MessageType.CataloguePost:
            defaultTemplate = vdata.templateComponents.defaultTemplate_catalogue.split(vdata.templateComponents.separator);
            break;
        case Enums_1.MessageType.ProductPost:
            defaultTemplate = vdata.templateComponents.defaultTemplate_product.split(vdata.templateComponents.separator);
            break;
    }
    renderTemplate(defaultTemplate, messageType);
    renderPreview(messageType);
}
/**
 * Validate the keyword supplied by user
 * @param keyword
 * @returns True if the keyword is valid
 */
function validateKeyword(keyword) {
    keyword = keyword.trim();
    if (!keyword || keyword.length > 10) {
        $(Enums_1.Selector.keywordInput_Product).addClass('is-invalid');
        return false;
    }
    $(Enums_1.Selector.keywordInput_Product).removeClass('is-invalid');
    return true;
}
/**
 * Turn the template created in GUI into string
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function stringifyTemplate(messageType) {
    const result = {
        isValid: messageType === Enums_1.MessageType.CataloguePost, // note: there is no validation for catalogue post template
        keyword: '',
        templateString: ''
    };
    if (messageType === Enums_1.MessageType.ProductPost) {
        result.keyword = sanitizeInput(String($(Enums_1.Selector.keywordInput_Product).val()).trim());
        if (!validateKeyword(result.keyword)) {
            return result;
        }
    }
    let containKeyword = false;
    $(selectorMapping[messageType].blockPanel).children('span').each(function () {
        const key = $(this).data(DATASET_TRUEVALUE);
        if (key in selectorMapping[messageType].validSelectOptions) {
            result.templateString += key + vdata.templateComponents.separator;
            if (key === vdata.templateComponents.productReplyKeyword || key === vdata.templateComponents.defaultMessageValue) {
                containKeyword = true;
            }
        }
        else {
            result.templateString += sanitizeInput(String($(this).find('input').val())) + vdata.templateComponents.separator;
        }
    });
    if (messageType === Enums_1.MessageType.ProductPost) {
        if (containKeyword) {
            $(Enums_1.Selector.componentSelect_Product).removeClass('is-invalid');
            result.isValid = true;
        }
        else {
            $(Enums_1.Selector.componentSelect_Product).addClass('is-invalid');
        }
    }
    return result;
}
/**
 * Render the preview panel
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function renderPreview(messageType) {
    $(selectorMapping[messageType].previewPanel).empty();
    let preview = '';
    const space = '&nbsp;';
    $(selectorMapping[messageType].blockPanel).children('span').each(function () {
        const key = $(this).data(DATASET_TRUEVALUE);
        if (key in selectorMapping[messageType].validSelectOptions) {
            if (key === vdata.templateComponents.newLineValue) {
                $(selectorMapping[messageType].previewPanel).append(`<p class="m-0 text-truncate">${preview ? preview : space}</p>`);
                preview = '';
            }
            else {
                preview += (messageType === Enums_1.MessageType.ProductPost && key === vdata.templateComponents.defaultMessageValue)
                    ? selectorMapping[messageType].validSelectOptions[key].preview.replaceAll('{1}', selectorMapping[messageType].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview)
                    : selectorMapping[messageType].validSelectOptions[key].preview;
            }
        }
        else {
            preview += sanitizeInput(String($(this).find('input').val()));
        }
    });
    $(selectorMapping[messageType].previewPanel).append(`<p class="m-0 text-truncate">${preview}</p>`);
    return;
}
/**
 * Reverse the effect of sanitizeInput()
 * @param text-Text that was inputted by user or stored in DB
 * @returns Un-sanitized text
 */
function reverseSanitize(text) {
    text = text.replaceAll('&amp;', '&').replaceAll('&lt;', '<').replaceAll('&gt;', '>').replaceAll('&quot;', '"').replaceAll('&#x27;', "'").replaceAll('&nbsp;', ' ');
    return text;
}
/**
 * Escape potentially problematic characters
 * @param text-Text that was inputted by user or stored in DB
 * @returns Sanitized text
 */
function sanitizeInput(text) {
    // space also needs to be turned into HTML entity so that it can be rendered in the preview panel
    text = text.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#x27;').replaceAll(' ', '&nbsp;');
    return text;
}
/**
 * Remove a block, which can be a standard message component or a custom text, from the template
 * @param obj-The HTML element that triggers this function (we can't simply use 'this' because this function is called inside an anonymous function)
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function removeBlock(obj, messageType) {
    $(obj.parentNode).remove();
    renderPreview(messageType);
    return;
}
/**
 * Add a custom text block to the template
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function addText(messageType) {
    let text = $(selectorMapping[messageType].customTextInput).val();
    if (text === '') {
        return;
    }
    text = sanitizeInput(String(text));
    $(selectorMapping[messageType].customTextInput).val('');
    $(selectorMapping[messageType].blockPanel).append(customTextBlock(text));
    $(selectorMapping[messageType].dragBlock_Last).on('dragstart', function () { handleDragStart(this, messageType); });
    $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeBlock(this, messageType); });
    $(selectorMapping[messageType].blockPanelInput_Last).on('keyup', function () { renderPreview(messageType); });
    renderPreview(messageType);
    return;
}
/**
 * Add a standard message component to the template
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function addComponent(messageType) {
    let value = String($(selectorMapping[messageType].componentSelect).val());
    if (!(value in selectorMapping[messageType].validSelectOptions)) {
        return;
    }
    $(selectorMapping[messageType].blockPanel).append(standardComponentBlock(value, selectorMapping[messageType].validSelectOptions[value].panel));
    $(selectorMapping[messageType].dragBlock_Last).on('dragstart', function () { handleDragStart(this, messageType); });
    $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeBlock(this, messageType); });
    renderPreview(messageType);
    return;
}
/**
 * Assign values to global variables
 * @returns
 */
function initializeGlobalVariables() {
    loadIndicatorTop = $(Enums_1.Selector.loadIndicator_Top).dxLoadIndicator({ visible: false }).dxLoadIndicator('instance');
    selectorMapping[Enums_1.MessageType.CataloguePost].validSelectOptions[vdata.templateComponents.newLineValue] = { panel: '&#9166;', preview: '\n' };
    selectorMapping[Enums_1.MessageType.CataloguePost].validSelectOptions[vdata.templateComponents.defaultMessageValue] = { panel: '{' + vdata.localizer.defaultMessage + '}', preview: vdata.sampleValues.defaultCatalogueMessage };
    selectorMapping[Enums_1.MessageType.CataloguePost].validSelectOptions[vdata.templateComponents.catalogueNameValue] = { panel: '{' + vdata.localizer.catalogueName + '}', preview: 'Example Catalogue' };
    selectorMapping[Enums_1.MessageType.CataloguePost].validSelectOptions[vdata.templateComponents.catalogueCodeValue] = { panel: '{' + vdata.localizer.catalogueEndDate + '}', preview: 'EXAMPLE' };
    selectorMapping[Enums_1.MessageType.CataloguePost].validSelectOptions[vdata.templateComponents.catalogueStartDateValue] = { panel: '{' + vdata.localizer.catalogueStartDate + '}', preview: '01/12/2023' };
    selectorMapping[Enums_1.MessageType.CataloguePost].validSelectOptions[vdata.templateComponents.catalogueEndDateValue] = { panel: '{' + vdata.localizer.catalogueEndDate + '}', preview: '31/12/2023' };
    selectorMapping[Enums_1.MessageType.CataloguePost].validSelectOptions[vdata.templateComponents.catalogueUrlValue] = { panel: '{' + vdata.localizer.catalogueUrl + '}', preview: vdata.sampleValues.catalogueURL };
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.newLineValue] = { panel: '&#9166;', preview: '\n' };
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.defaultMessageValue] = { panel: '{' + vdata.localizer.defaultMessage + '}', preview: vdata.sampleValues.defaultProductMessage };
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.productNameValue] = { panel: '{' + vdata.localizer.productName + '}', preview: 'Example Product' };
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.productCodeValue] = { panel: '{' + vdata.localizer.productCode + '}', preview: 'EXAMPLE' };
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.productDescriptionValue] = { panel: '{' + vdata.localizer.productDescription + '}', preview: 'G.O.A.T.' };
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.productListPriceValue] = { panel: '{' + vdata.localizer.productListPrice + '}', preview: 'HKD888' };
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.productPublicPriceValue] = { panel: '{' + vdata.localizer.productPublicPrice + '}', preview: 'HKD888.88' };
    selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions[vdata.templateComponents.productReplyKeyword] = { panel: '{' + vdata.localizer.productReplyKeyword + '}', preview: 'BUY' };
    validSelectOptions = Object.assign(Object.assign({}, selectorMapping[Enums_1.MessageType.CataloguePost].validSelectOptions), selectorMapping[Enums_1.MessageType.ProductPost].validSelectOptions);
    return;
}
/**
 * Assign event listeners to DOM elements related to social media templates
 * @param messageType-Publication of products or catalogues
 */
function AssignEventListenersForTemplates(messageType) {
    // currently only product posts have a keyword that may trigger the chatbot
    if (messageType === Enums_1.MessageType.ProductPost) {
        $(Enums_1.Selector.keywordInput_Product).on('keyup', function () {
            const keyword = sanitizeInput(String($(this).val()).trim());
            selectorMapping[messageType].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview = keyword;
            validateKeyword(keyword);
            renderPreview(messageType);
        });
    }
    $(selectorMapping[messageType].customTextAddButton).on('click', function () { addText(messageType); });
    $(selectorMapping[messageType].customTextInput).on('keydown', function (event) {
        if (event.key === 'Enter') {
            addText(messageType);
        }
    });
    $(selectorMapping[messageType].componentAddButton).on('click', function () { addComponent(messageType); });
    $(selectorMapping[messageType].restoreDefaultButton).on('click', function () { restoreDefaultTemplate(messageType); });
    $(selectorMapping[messageType].blockPanel).on('drop', function (event) { handleDrop(event, messageType); });
    $(selectorMapping[messageType].blockPanel).on('dragover', function (event) { handleDragOver(event); });
}
/**
 * React to change in the user's Facebook login status
 * @param response-Facebook's response to getLoginStatus()
 */
function handleFbLoginStatusChange(response) {
    var _a, _b;
    /*console.log(response);*/
    if ((response === null || response === void 0 ? void 0 : response.status) === 'connected') {
        $(Enums_1.Selector.loginButton).hide();
        $(Enums_1.Selector.logoutButton).show();
        FB.api('/me', { fields: 'name' }, function (response) {
            const alert = `<div class="alert alert-success my-3">` +
                `<i class="fa-regular fa-circle-check"></i>` +
                `&nbsp;${vdata.localizer.fbConnected.replaceAll('{0}', response.name).replaceAll('{1}', vdata.settings.shortName)}` +
                `</div>`;
            $(Enums_1.Selector.fbDialogue).empty().append(alert);
            $(Enums_1.Selector.fbDialogue2).empty();
        });
        /*The following functions run in a non-blocking manner because there is no interdependence*/
        getPages();
        refreshAccessToken((_a = response.authResponse) === null || _a === void 0 ? void 0 : _a.userID, (_b = response.authResponse) === null || _b === void 0 ? void 0 : _b.accessToken);
    }
    else {
        $(Enums_1.Selector.loginButton).show();
        $(Enums_1.Selector.logoutButton).hide();
        const alert = `<div class="alert alert-warning">` +
            `<i class="fa-regular fa-circle-exclamation"></i>` +
            `&nbsp;${vdata.localizer.fbNotConnected.replaceAll('{0}', vdata.settings.shortName)}` +
            `</div>`;
        $(Enums_1.Selector.fbDialogue).empty().append(alert);
        $(Enums_1.Selector.saveSettingButtons).attr('disabled', 'disabled');
        $(selectorMapping[Enums_1.MessageType.ProductPost].blockPanel).empty();
        $(selectorMapping[Enums_1.MessageType.CataloguePost].blockPanel).empty();
        $(selectorMapping[Enums_1.MessageType.ProductPost].previewPanel).empty();
        $(selectorMapping[Enums_1.MessageType.CataloguePost].previewPanel).empty();
    }
}
/**
 * Validate and submit the setting to backend for saving
 */
function saveSettings(previewCheckout, previewThankyou) {
    let validationResult = DevExpress.validationEngine.validateGroup();
    if (!validationResult.isValid) {
        return;
    }
    if ($("#ShipToCustomer").dxCheckBox("option", "value") && $("#DeliveryCharge").dxNumberBox("option", "value") > 0) {
        const $deliveryChargeProduct = $("#DeliveryChargeProductID").dxSelectBox("instance");
        if (!$deliveryChargeProduct.option("value")) {
            $deliveryChargeProduct.option("validationStatus", "invalid");
            DevExpress.ui.dialog.alert(vdata.localizer.msgMissingDeliveryChargeProductError, vdata.localizer.error);
            return;
        }
    }
    const productTemplate = stringifyTemplate(Enums_1.MessageType.ProductPost);
    if (!productTemplate.isValid) {
        return;
    }
    // fill in the hidden text boxes so that the template will be captured in the form data
    $('#ProductPostTemplate').val(productTemplate.templateString);
    $('#KeywordForShoppingIntention').val(productTemplate.keyword);
    const catalogueTemplate = vdata.settings.env === "PRD"
        ? { isValid: true, keyword: '', templateString: '' }
        : stringifyTemplate(Enums_1.MessageType.CataloguePost);
    if (!catalogueTemplate.isValid) {
        return;
    }
    // ditto
    $('#CataloguePostTemplate').val(catalogueTemplate.templateString);
    const savedData = new FormData($("#settings-form")[0]);
    if (!savedData) {
        console.log('Failed to find a form with selector "#settings-form"');
        return;
    }
    $.ajax({
        type: 'POST',
        url: `/${vdata.settings.organizationID}/GetStarted/Save`,
        data: savedData,
        cache: false,
        contentType: false,
        processData: false,
        success: function () {
            if (previewCheckout) {
                window.open(`/${vdata.settings.organizationID}/GetStarted/PreviewCheckout`, '_blank');
                return;
            }
            if (previewThankyou) {
                window.open(`/${vdata.settings.organizationID}/GetStarted/PreviewThankYou`, '_blank');
                return;
            }
            DevExpress.ui.notify(vdata.localizer.msgSettingSaved, 'success');
        },
        error: corprio.formatError
    });
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
 * Pass the short-lived user access token to server
 * @param metaId-Facebook user ID
 * @param accessToken-Short-lived user access token
 * @returns
 */
function refreshAccessToken(metaId, accessToken) {
    return $.ajax({
        type: 'POST',
        url: vdata.actions.refreshAccessToken,
        data: {
            metaId: metaId,
            token: accessToken,
        },
        success: function () {
            console.log(`Token for ${metaId} is fed to backend successfully.`);
            initializeGlobalVariables(); // initialize global variables again because theoritically fbAsyncInit and its callbacks can all run before DOM is loaded            
            restoreKeyword();
            restoreTemplate(Enums_1.MessageType.ProductPost);
            restoreTemplate(Enums_1.MessageType.CataloguePost);
            $(Enums_1.Selector.saveSettingButtons).removeAttr('disabled');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $(Enums_1.Selector.saveSettingButtons).attr('disabled', 'disabled');
            if (jqXHR.status === 500) {
                return FB.logout(checkLoginState);
            }
            // note: 409 means that another organization is connected with the Facebook account
            if (jqXHR.status !== 409) {
                $(Enums_1.Selector.fbDialogue2).empty();
                return corprio.formatError(jqXHR, textStatus, errorThrown);
            }
            // query Facebook to obtain the Facebook user name
            FB.api('/me', { fields: 'name' }, function (response) {
                const alert = `<div class="alert alert-danger">` +
                    `<i class="fa-regular fa-circle-exclamation"></i>` +
                    `&nbsp;${vdata.localizer.fbAlreadyConnected.replaceAll('{0}', response.name).replaceAll('{1}', vdata.settings.shortName)}&nbsp;` +
                    `<u class="text-primary"><a href="${vdata.settings.appBaseUrl}/${vdata.settings.organizationID}/ReconnectFacebook">${vdata.localizer.reconnectFacebook}</a></u>` +
                    `${vdata.localizer.period}` +
                    `</div>`;
                $(Enums_1.Selector.fbDialogue2).empty().append(alert);
                return FB.logout(checkLoginState);
            });
        }
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
        subscribed_fields: [
            'feed',
            // webhook for pages: https://developers.facebook.com/docs/graph-api/webhooks/getting-started/webhooks-for-pages/
            'messages',
            // any other webhook event: https://developers.facebook.com/docs/messenger-platform/webhook/#events
        ],
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
        cookie: true, // Enable cookies to allow the server to access the session.
        xfbml: true, // Parse social plugins on this webpage.
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
    // prevent 'Enter' from triggering form submission
    $(window).on('keydown', function (event) {
        if (event.key == 'Enter') {
            event.preventDefault();
            return;
        }
    });
    // facebook-related stuff
    $(Enums_1.Selector.loginButton).on('click', function () {
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
    $(Enums_1.Selector.logoutButton).on('click', function () {
        console.log('Logging out from Facebook...');
        FB.logout(checkLoginState);
    });
    // UX-related stuff    
    $("#smtp-dialogue").toggle(vdata.settings.sendConfirmationEmail.toLowerCase() === 'true');
    $(".page-title button").on('click', function () {
        $(".sidebar-wrapper").toggleClass("hidden", $(".sidebar").hasClass("show"));
    });
    const sections = $($('section').get().reverse()); //sections from bottom to top for optimization
    const navPillLinks = $('#settings-nav > li > a');
    $('#main').on('scroll', function () {
        let scrollPosition = $('#main').scrollTop();
        sections.each(function () {
            const currentSection = $(this);
            const sectionTop = currentSection[0].offsetTop;
            if (scrollPosition + 50 >= sectionTop) {
                const id = currentSection.attr('id');
                const navPillLink = $(`#settings-nav > li > a[href='#${id}']`);
                if (!navPillLink.hasClass('active')) {
                    navPillLinks.removeClass('active');
                    navPillLink.addClass('active');
                }
                return false;
            }
        });
        if (scrollPosition + 1 >= $('#main')[0].scrollHeight - $('#main').height()) {
            navPillLinks.removeClass('active');
            navPillLinks.last().addClass('active');
        }
    });
    navPillLinks.on('click', function (e) {
        $(".sidebar-wrapper").addClass("hidden");
        $(".sidebar").removeClass("show");
    });
    // template-related stuff    
    $(Enums_1.Selector.catalogueSetting).toggle(vdata.settings.env !== "PRD");
    initializeGlobalVariables();
    AssignEventListenersForTemplates(Enums_1.MessageType.CataloguePost);
    AssignEventListenersForTemplates(Enums_1.MessageType.ProductPost);
    $(Enums_1.Selector.saveSettingButtons).on('click', function () { saveSettings(false, false); });
    // miscellaneous
    $('#preview-checkout').on('click', function () { saveSettings(true, false); });
    $('#preview-thank-you').on('click', function () { saveSettings(false, true); });
    corprio.page.initTour({ defaultTour: 'getstarted.index', autoStart: true, driverCssLoaded: true }); // must set driverCssLoaded to true    
});

})();

/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Index.js.map