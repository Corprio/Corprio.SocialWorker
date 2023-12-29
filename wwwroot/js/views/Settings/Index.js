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

/***/ "./Views/Settings/Enums.ts":
/*!*********************************!*\
  !*** ./Views/Settings/Enums.ts ***!
  \*********************************/
/***/ ((__unused_webpack_module, exports) => {


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


/***/ }),

/***/ "./Views/Settings/Index.ts":
/*!*********************************!*\
  !*** ./Views/Settings/Index.ts ***!
  \*********************************/
/***/ (function(__unused_webpack_module, exports, __webpack_require__) {


var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", ({ value: true }));
const Enums_1 = __webpack_require__(/*! ./Enums */ "./Views/Settings/Enums.ts");
// magic numbers
const DataSet_TrueValue = 'true-value'; // this attribute contains the value of a standard component (e.g., %lineBreak%)
const DataSet_Reordered = 'reordered'; // this attribute indicates if the block will be moved to another position
const DataSet_Swapped = 'swapped'; // this attribute indicates if the block will give up its position to the reordered block
// the following dictionary maps the message type to the relevant selectors
const selectorMapping = {
    'catalogue': {
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
    'product': {
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
 * @param obj
 * @param messageType-Publication of products or catalogues
 */
function handleDragStart(obj, messageType) {
    const $dragged = $(obj);
    draggedBlock.value = ($dragged.data(DataSet_TrueValue) in validSelectOptions) ? $dragged.data(DataSet_TrueValue) : String($dragged.find('input').val());
    draggedBlock.type = messageType;
    $dragged.data(DataSet_Reordered, true);
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
        $target.data(DataSet_Swapped, true);
        deadend = false;
    }
    let templateString = '';
    $(selectorMapping[messageType].blockPanel).children('span').each(function () {
        if ($(this).data(DataSet_Reordered)) {
            return;
        }
        if ($(this).data(DataSet_Swapped)) {
            templateString += draggedBlock.value + vdata.templateComponents.separator;
        }
        const key = $(this).data(DataSet_TrueValue);
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
    return `<span draggable="true" data-${DataSet_TrueValue}="${dataset}" class="rounded border border-secondary p-2 mx-1 mb-1 d-inline-block text-wrap drag-block">` +
        `<input disabled value="${text}">` +
        `<i class="fa-duotone fa-x ml-2 remove-btn"></i>` +
        `</span>`;
}
/**
 * Convert the MessageType enum - which, at the risk of stating the obvious, is on the client-side - into the name of a similar enum on the server side
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function TranslateMessageType(messageType) {
    let messageTypeString;
    switch (messageType) {
        case Enums_1.MessageType.CATALOGUE:
            messageTypeString = vdata.templateComponents.messageType_CataloguePost;
            break;
        case Enums_1.MessageType.PRODUCT:
            messageTypeString = vdata.templateComponents.messageType_ProductPost;
            break;
    }
    return messageTypeString;
}
/**
 * Retreive the stored keyword from the backend and render it in the relevant input field
 * @returns
 */
function restoreKeyword() {
    return $.post({
        url: vdata.actions.getKeyword,
    }).done((keyword) => {
        selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview = keyword;
        $(Enums_1.Selector.keywordInput_Product).val(reverseSanitize(keyword));
    }).fail(corprio.formatError);
}
/**
 * Retrieve the stored template from the backend and render it on the template and preview panels
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function restoreTemplate(messageType) {
    return $.post({
        url: vdata.actions.getTemplate,
        data: {
            messageType: TranslateMessageType(messageType)
        }
    }).done((templateString) => {
        if (templateString) {
            const templateArray = templateString.split(vdata.templateComponents.separator);
            renderTemplate(templateArray, messageType);
            renderPreview(messageType);
        }
    }).fail(corprio.formatError);
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
        case Enums_1.MessageType.CATALOGUE:
            defaultTemplate = vdata.templateComponents.defaultTemplate_catalogue.split(vdata.templateComponents.separator);
            break;
        case Enums_1.MessageType.PRODUCT:
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
 * Trigger the backend to save the template (and keyword)
 * @param messageType-Publication of products or catalogues
 * @returns
 */
function saveTemplate(messageType) {
    loadIndicatorTop.option('visible', true);
    let keyword = null;
    if (messageType === Enums_1.MessageType.PRODUCT) {
        keyword = sanitizeInput(String($(Enums_1.Selector.keywordInput_Product).val()).trim());
        if (!validateKeyword(keyword)) {
            loadIndicatorTop.option('visible', false);
            return;
        }
    }
    let templateString = '';
    let containKeyword = false;
    $(selectorMapping[messageType].blockPanel).children('span').each(function () {
        const key = $(this).data(DataSet_TrueValue);
        if (key in selectorMapping[messageType].validSelectOptions) {
            templateString += key + vdata.templateComponents.separator;
            if (key === vdata.templateComponents.productReplyKeyword || key === vdata.templateComponents.defaultMessageValue) {
                containKeyword = true;
            }
        }
        else {
            templateString += sanitizeInput(String($(this).find('input').val())) + vdata.templateComponents.separator;
        }
    });
    if (messageType === Enums_1.MessageType.PRODUCT) {
        if (containKeyword) {
            $(Enums_1.Selector.componentSelect_Product).removeClass('is-invalid');
        }
        else {
            $(Enums_1.Selector.componentSelect_Product).addClass('is-invalid');
            loadIndicatorTop.option('visible', false);
            return;
        }
    }
    if (!templateString) {
        loadIndicatorTop.option('visible', false);
        return;
    }
    return $.post({
        url: vdata.actions.saveTemplate,
        data: {
            templateString: templateString,
            messageType: TranslateMessageType(messageType),
            keyWord: keyword
        }
    }).done(function () {
        var message = DevExpress.ui.dialog.custom({
            title: vdata.localizer.saveTemplateTitle,
            messageHtml: vdata.localizer.saveTemplateMessage
        });
        message.show();
    }).fail(corprio.formatError)
        .always(() => { loadIndicatorTop.option('visible', false); });
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
        const key = $(this).data(DataSet_TrueValue);
        if (key in selectorMapping[messageType].validSelectOptions) {
            if (key === vdata.templateComponents.newLineValue) {
                $(selectorMapping[messageType].previewPanel).append(`<p class="m-0 text-truncate">${preview ? preview : space}</p>`);
                preview = '';
            }
            else {
                preview += (messageType === Enums_1.MessageType.PRODUCT && key === vdata.templateComponents.defaultMessageValue)
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
 * @param obj-The button that was clicked to trigger this function
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
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.newLineValue] = { panel: '&#9166;', preview: '\n' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.defaultMessageValue] = { panel: '{' + vdata.localizer.defaultMessage + '}', preview: vdata.sampleValues.defaultCatalogueMessage };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueNameValue] = { panel: '{' + vdata.localizer.catalogueName + '}', preview: 'Example Catalogue' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueCodeValue] = { panel: '{' + vdata.localizer.catalogueEndDate + '}', preview: 'EXAMPLE' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueStartDateValue] = { panel: '{' + vdata.localizer.catalogueStartDate + '}', preview: '01/12/2023' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueEndDateValue] = { panel: '{' + vdata.localizer.catalogueEndDate + '}', preview: '31/12/2023' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueUrlValue] = { panel: '{' + vdata.localizer.catalogueUrl + '}', preview: vdata.sampleValues.catalogueURL };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.newLineValue] = { panel: '&#9166;', preview: '\n' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.defaultMessageValue] = { panel: '{' + vdata.localizer.defaultMessage + '}', preview: vdata.sampleValues.defaultProductMessage };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productNameValue] = { panel: '{' + vdata.localizer.productName + '}', preview: 'Example Product' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productCodeValue] = { panel: '{' + vdata.localizer.productCode + '}', preview: 'EXAMPLE' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productDescriptionValue] = { panel: '{' + vdata.localizer.productDescription + '}', preview: 'G.O.A.T.' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productListPriceValue] = { panel: '{' + vdata.localizer.productListPrice + '}', preview: 'HKD888.88' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productReplyKeyword] = { panel: '{' + vdata.localizer.productReplyKeyword + '}', preview: 'BUY' };
    validSelectOptions = Object.assign(Object.assign({}, selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions), selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions);
    return;
}
/**
 * Render the template and preview panels and assign event listeners to relevant DOM elements
 * @param messageType-Publication of products or catalogues
 */
function preparePanels(messageType) {
    return __awaiter(this, void 0, void 0, function* () {
        // currently only product posts have a keyword that may trigger the chatbot
        if (messageType === Enums_1.MessageType.PRODUCT) {
            yield restoreKeyword();
            $(Enums_1.Selector.keywordInput_Product).on('keyup', function () {
                const keyword = sanitizeInput(String($(this).val()).trim());
                selectorMapping[messageType].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview = keyword;
                validateKeyword(keyword);
                renderPreview(messageType);
            });
        }
        yield restoreTemplate(messageType);
        $(selectorMapping[messageType].customTextAddButton).on('click', function () { addText(messageType); });
        $(selectorMapping[messageType].customTextInput).on('keydown', function (event) {
            if (event.key === 'Enter') {
                addText(messageType);
            }
        });
        $(selectorMapping[messageType].componentAddButton).on('click', function () { addComponent(messageType); });
        $(selectorMapping[messageType].restoreDefaultButton).on('click', function () { restoreDefaultTemplate(messageType); });
        $(selectorMapping[messageType].saveTemplateButton).on('click', function () { saveTemplate(messageType); });
        $(selectorMapping[messageType].blockPanel).on('drop', function (event) { handleDrop(event, messageType); });
        $(selectorMapping[messageType].blockPanel).on('dragover', function (event) { handleDragOver(event); });
    });
}
function submitSetting() {
    // TODO
}
// initialize global variables and restore the saved templates
$(function () {
    return __awaiter(this, void 0, void 0, function* () {
        $('.save-setting-btn').on('click', submitSetting);
        if (vdata.settings.env === "PRD") {
            $(Enums_1.Selector.catalogueSetting).hide();
        }
        else {
            $(Enums_1.Selector.catalogueSetting).show();
        }
        initializeGlobalVariables();
        loadIndicatorTop.option('visible', true);
        yield preparePanels(Enums_1.MessageType.CATALOGUE);
        yield preparePanels(Enums_1.MessageType.PRODUCT);
        loadIndicatorTop.option('visible', false);
    });
});


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
/******/ 		__webpack_modules__[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module is referenced by other modules so it can't be inlined
/******/ 	var __webpack_exports__ = __webpack_require__("./Views/Settings/Index.ts");
/******/ 	
/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Index.js.map