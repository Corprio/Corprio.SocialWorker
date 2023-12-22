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
        validSelectOptions: {},
    }
};
let loadIndicatorTop;
function customTextBlock(text) {
    return `<span class="rounded border border-primary p-2 mx-1 mb-1 d-inline-block text-wrap"><input class="custom-block" value="${text}"><i class="fa-duotone fa-x ml-2 remove-btn"></i></span>`;
    /*return `<span class="rounded border border-primary p-2 mx-1 mb-1 d-inline-block text-wrap">${text}<i class="fa-duotone fa-x ml-2 remove-btn"></i></span>`;*/
}
function standardComponentBlock(dataset, text) {
    return `<span data-mapping="${dataset}" class="rounded border border-secondary p-2 mx-1 mb-1 d-inline-block text-wrap"><input disabled value="${text}"><i class="fa-duotone fa-x ml-2 remove-btn"></i></span>`;
    /*return `<span data-mapping="${dataset}" class="rounded border border-secondary p-2 mx-1 mb-1 d-inline-block text-wrap">${text}<i class="fa-duotone fa-x ml-2 remove-btn"></i></span>`;*/
}
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
function restoreKeyword() {
    return $.post({
        url: vdata.actions.getKeyword,
    }).done((keyword) => {
        selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview = keyword;
        $(Enums_1.Selector.keywordInput_Product).val(keyword);
    }).fail(corprio.formatError);
}
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
        $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeText(this, messageType); });
        $(selectorMapping[messageType].blockPanelInput_Last).on('keyup', function () { renderPreview(messageType); });
    }
    return;
}
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
function validateKeyword(keyword) {
    keyword = keyword.trim();
    if (!keyword || keyword.length > 10) {
        $(Enums_1.Selector.keywordInput_Product).addClass('is-invalid');
        return false;
    }
    $(Enums_1.Selector.keywordInput_Product).removeClass('is-invalid');
    return true;
}
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
        const key = this.dataset.mapping;
        if (key in selectorMapping[messageType].validSelectOptions) {
            templateString += key + vdata.templateComponents.separator;
            if (key === vdata.templateComponents.productReplyKeyword || key === vdata.templateComponents.defaultMessageValue) {
                containKeyword = true;
            }
        }
        else {
            templateString += sanitizeInput(String($(this).find('input').val())) + vdata.templateComponents.separator;
            /*templateString += sanitizeInput(this.innerText) + vdata.templateComponents.separator;*/
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
            messageHtml: vdata.localizer.saveTemplateTitle
        });
        message.show();
    }).fail(corprio.formatError)
        .always(() => { loadIndicatorTop.option('visible', false); });
}
function renderPreview(messageType) {
    $(selectorMapping[messageType].previewPanel).empty();
    let preview = '';
    const space = '&nbsp;';
    $(selectorMapping[messageType].blockPanel).children('span').each(function () {
        const key = this.dataset.mapping;
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
            /*preview += sanitizeInput(this.innerText);*/
        }
    });
    $(selectorMapping[messageType].previewPanel).append(`<p class="m-0 text-truncate">${preview}</p>`);
    return;
}
function sanitizeInput(text) {
    // space also needs to be turned into HTML entity so that it can be rendered in the preview panel
    text = text.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#x27;').replaceAll(' ', '&nbsp;');
    return text;
}
function removeText(obj, messageType) {
    obj.parentNode.remove();
    renderPreview(messageType);
    return;
}
function addText(messageType) {
    let text = $(selectorMapping[messageType].customTextInput).val();
    if (text === '') {
        return;
    }
    text = sanitizeInput(String(text));
    $(selectorMapping[messageType].customTextInput).val('');
    $(selectorMapping[messageType].blockPanel).append(customTextBlock(text));
    $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeText(this, messageType); });
    $(selectorMapping[messageType].blockPanelInput_Last).on('keyup', function () { renderPreview(messageType); });
    renderPreview(messageType);
    return;
}
function addComponent(messageType) {
    let value = String($(selectorMapping[messageType].componentSelect).val());
    if (!(value in selectorMapping[messageType].validSelectOptions)) {
        return;
    }
    $(selectorMapping[messageType].blockPanel).append(standardComponentBlock(value, selectorMapping[messageType].validSelectOptions[value].panel));
    $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeText(this, messageType); });
    renderPreview(messageType);
    return;
}
function initializeGlobalVariables() {
    loadIndicatorTop = $(Enums_1.Selector.loadIndicator_Top).dxLoadIndicator({ visible: false }).dxLoadIndicator('instance');
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.newLineValue] = { panel: '&#9166;', preview: '\n' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.defaultMessageValue] = { panel: '{Default Message}', preview: vdata.sampleValues.defaultCatalogueMessage };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueNameValue] = { panel: '{Catalogue Name}', preview: 'Example Catalogue' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueCodeValue] = { panel: '{Catalogue Code}', preview: 'EXAMPLE' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueStartDateValue] = { panel: '{Catalogue Start Date}', preview: '01/12/2023' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueEndDateValue] = { panel: '{Catalogue End Date}', preview: '31/12/2023' };
    selectorMapping[Enums_1.MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueUrlValue] = { panel: '{Catalogue URL}', preview: vdata.sampleValues.catalogueURL };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.newLineValue] = { panel: '&#9166;', preview: '\n' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.defaultMessageValue] = { panel: '{Default Message}', preview: vdata.sampleValues.defaultProductMessage };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productNameValue] = { panel: '{Product Name}', preview: 'Example Product' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productCodeValue] = { panel: '{Product Code}', preview: 'EXAMPLE' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productDescriptionValue] = { panel: '{Product Description}', preview: 'G.O.A.T.' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productListPriceValue] = { panel: '{Product Price}', preview: 'HKD888.88' };
    selectorMapping[Enums_1.MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productReplyKeyword] = { panel: '{Keyword}', preview: 'BUY' };
    return;
}
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
        $(selectorMapping[messageType].customTextAddButton).on('click', () => addText(messageType));
        $(selectorMapping[messageType].customTextInput).on('keydown', function (event) {
            if (event.key === 'Enter') {
                addText(messageType);
            }
        });
        $(selectorMapping[messageType].componentAddButton).on('click', () => addComponent(messageType));
        $(selectorMapping[messageType].restoreDefaultButton).on('click', () => restoreDefaultTemplate(messageType));
        $(selectorMapping[messageType].saveTemplateButton).on('click', () => saveTemplate(messageType));
    });
}
$(function () {
    return __awaiter(this, void 0, void 0, function* () {
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