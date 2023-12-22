import { MessageType, Selector } from './Enums';

declare const vdata: {
    actions: {
        getKeyword: string;
        getTemplate: string;
        saveTemplate: string;
    };
    localizer: {
        saveTemplateMessage: string;
        saveTemplateTitle: string;
    };
    sampleValues: {
        catalogueURL: string;
        defaultCatalogueMessage: string;
        defaultProductMessage: string;
    };
    settings: {
        env: string;
    };
    templateComponents: {
        catalogueCodeValue: string;
        catalogueEndDateValue: string;
        catalogueNameValue: string;
        catalogueStartDateValue: string;
        catalogueUrlValue: string;
        defaultMessageValue: string;
        defaultTemplate_catalogue: string;
        defaultTemplate_product: string;
        messageType_CataloguePost: string;
        messageType_ProductPost: string;
        newLineValue: string;
        productCodeValue: string;
        productDescriptionValue: string;
        productListPriceValue: string;
        productNameValue: string;
        productReplyKeyword: string;
        separator: string;
    };
};

interface TemplateDetails {
    preview: string,
    panel: string
}

interface Selected {
    blockPanel: Selector,
    blockPanelInput_Last: Selector,
    componentSelect: Selector,
    componentAddButton: Selector,
    customTextInput: Selector,
    customTextAddButton: Selector,
    saveTemplateButton: Selector,
    restoreDefaultButton: Selector,
    previewPanel: Selector,
    removeButton_Last: Selector,
    validSelectOptions: Record<string, TemplateDetails>
}

const selectorMapping: Record<MessageType, Selected> = {
    'catalogue': {
        blockPanel: Selector.blockPanel_Catalogue,
        blockPanelInput_Last: Selector.blockPanel_Catalogue_Input_Last,
        componentSelect: Selector.componentSelect_Catalogue,
        componentAddButton: Selector.addComponentButton_Catalogue,
        customTextInput: Selector.customTextInput_Catalogue,
        customTextAddButton: Selector.addCustomTextButton_Catalogue,
        saveTemplateButton: Selector.saveTemplateButton_Catalogue,
        restoreDefaultButton: Selector.restoreDefaultButton_Catalogue,
        previewPanel: Selector.previewPanel_Catalogue,
        removeButton_Last: Selector.blockPanel_Catalogue_RemoveButton_Last,
        validSelectOptions: {},
    },
    'product': {
        blockPanel: Selector.blockPanel_Product,
        blockPanelInput_Last: Selector.blockPanel_Product_Input_Last,
        componentSelect: Selector.componentSelect_Product,
        componentAddButton: Selector.addComponentButton_Product,
        customTextInput: Selector.customTextInput_Product,
        customTextAddButton: Selector.addCustomTextButton_Product,
        saveTemplateButton: Selector.saveTemplateButton_Product,
        restoreDefaultButton: Selector.restoreDefaultButton_Product,
        previewPanel: Selector.previewPanel_Product,
        removeButton_Last: Selector.blockPanel_Product_RemoveButton_Last,
        validSelectOptions: {},
    }
};
let loadIndicatorTop;

function customTextBlock(text: string) {
    return `<span class="rounded border border-primary p-2 mx-1 mb-1 d-inline-block text-wrap"><input class="custom-block" value="${text}"><i class="fa-duotone fa-x ml-2 remove-btn"></i></span>`;
    /*return `<span class="rounded border border-primary p-2 mx-1 mb-1 d-inline-block text-wrap">${text}<i class="fa-duotone fa-x ml-2 remove-btn"></i></span>`;*/
}

function standardComponentBlock(dataset: string, text: string) {
    return `<span data-mapping="${dataset}" class="rounded border border-secondary p-2 mx-1 mb-1 d-inline-block text-wrap"><input disabled value="${text}"><i class="fa-duotone fa-x ml-2 remove-btn"></i></span>`;
    /*return `<span data-mapping="${dataset}" class="rounded border border-secondary p-2 mx-1 mb-1 d-inline-block text-wrap">${text}<i class="fa-duotone fa-x ml-2 remove-btn"></i></span>`;*/
}

function TranslateMessageType(messageType: MessageType): string {
    let messageTypeString: string;
    switch (messageType) {
        case MessageType.CATALOGUE:
            messageTypeString = vdata.templateComponents.messageType_CataloguePost;
            break;
        case MessageType.PRODUCT:
            messageTypeString = vdata.templateComponents.messageType_ProductPost;
            break;
    }
    return messageTypeString;
}

function restoreKeyword() {
    return $.post({
        url: vdata.actions.getKeyword,
    }).done((keyword: string) => {
        selectorMapping[MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview = keyword;
        $(Selector.keywordInput_Product).val(keyword);
    }).fail(corprio.formatError);
}

function restoreTemplate(messageType: MessageType) {    
    return $.post({
        url: vdata.actions.getTemplate,
        data: {            
            messageType: TranslateMessageType(messageType)
        }
    }).done((templateString: string) => {        
        if (templateString) {
            const templateArray = templateString.split(vdata.templateComponents.separator);
            renderTemplate(templateArray, messageType);
            renderPreview(messageType);
        }        
    }).fail(corprio.formatError);
}

function renderTemplate(templateArray: string[], messageType: MessageType) {
    if (!templateArray) { return; }

    $(selectorMapping[messageType].blockPanel).empty();
    for (let i = 0; i < templateArray.length; i++) {
        if (!templateArray[i]) { continue; }        
        const element = (templateArray[i] in selectorMapping[messageType].validSelectOptions)
            ? standardComponentBlock(templateArray[i], selectorMapping[messageType].validSelectOptions[templateArray[i]].panel)
            : customTextBlock(templateArray[i]);        
        
        $(selectorMapping[messageType].blockPanel).append(element);        
        $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeText(this, messageType) });
        $(selectorMapping[messageType].blockPanelInput_Last).on('keyup', function () { renderPreview(messageType) });
    }
    return;
}

function restoreDefaultTemplate(messageType: MessageType) {    
    let defaultTemplate: string[];
    switch (messageType) {
        case MessageType.CATALOGUE:
            defaultTemplate = vdata.templateComponents.defaultTemplate_catalogue.split(vdata.templateComponents.separator);
            break;
        case MessageType.PRODUCT:
            defaultTemplate = vdata.templateComponents.defaultTemplate_product.split(vdata.templateComponents.separator);
            break;
    }
    renderTemplate(defaultTemplate, messageType);
    renderPreview(messageType);
}

function validateKeyword(keyword: string): boolean {    
    keyword = keyword.trim();

    if (!keyword || keyword.length > 10) {
        $(Selector.keywordInput_Product).addClass('is-invalid');
        return false;
    }

    $(Selector.keywordInput_Product).removeClass('is-invalid');
    return true;
}

function saveTemplate(messageType: MessageType) {    
    loadIndicatorTop.option('visible', true);
    let keyword:string = null;

    if (messageType === MessageType.PRODUCT) {
        keyword = sanitizeInput(String($(Selector.keywordInput_Product).val()).trim());
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
            if (key === vdata.templateComponents.productReplyKeyword || key === vdata.templateComponents.defaultMessageValue) { containKeyword = true; }
        } else {
            templateString += sanitizeInput(String($(this).find('input').val())) + vdata.templateComponents.separator;            
            /*templateString += sanitizeInput(this.innerText) + vdata.templateComponents.separator;*/
        }
    });

    if (messageType === MessageType.PRODUCT) {

        if (containKeyword) {
            $(Selector.componentSelect_Product).removeClass('is-invalid');
        } else {
            $(Selector.componentSelect_Product).addClass('is-invalid');
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

function renderPreview(messageType: MessageType) {
    $(selectorMapping[messageType].previewPanel).empty();
    let preview = '';
    const space = '&nbsp;';
    $(selectorMapping[messageType].blockPanel).children('span').each(function () {        
        const key = this.dataset.mapping;
        if (key in selectorMapping[messageType].validSelectOptions) {
            if (key === vdata.templateComponents.newLineValue) {
                $(selectorMapping[messageType].previewPanel).append(`<p class="m-0 text-truncate">${preview ? preview : space}</p>`);
                preview = '';
            } else {
                preview += (messageType === MessageType.PRODUCT && key === vdata.templateComponents.defaultMessageValue)
                    ? selectorMapping[messageType].validSelectOptions[key].preview.replaceAll('{1}', selectorMapping[messageType].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview)
                    : selectorMapping[messageType].validSelectOptions[key].preview;
            }            
        } else {
            preview += sanitizeInput(String($(this).find('input').val()));
            /*preview += sanitizeInput(this.innerText);*/
        }
    })
    $(selectorMapping[messageType].previewPanel).append(`<p class="m-0 text-truncate">${preview}</p>`);
    return;
}

function sanitizeInput(text: string) {
    // space also needs to be turned into HTML entity so that it can be rendered in the preview panel
    text = text.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#x27;').replaceAll(' ', '&nbsp;');
    return text;
}

function removeText(obj, messageType: MessageType) {    
    obj.parentNode.remove();
    renderPreview(messageType);
    return;
}

function addText(messageType: MessageType) {    
    let text = $(selectorMapping[messageType].customTextInput).val();
    if (text === '') { return; }
    text = sanitizeInput(String(text));
    $(selectorMapping[messageType].customTextInput).val('');    
    $(selectorMapping[messageType].blockPanel).append(customTextBlock(text));
    $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeText(this, messageType) });
    $(selectorMapping[messageType].blockPanelInput_Last).on('keyup', function () { renderPreview(messageType) });
    renderPreview(messageType);
    return;
}

function addComponent(messageType: MessageType) {
    let value = String($(selectorMapping[messageType].componentSelect).val());    
    if (!(value in selectorMapping[messageType].validSelectOptions)) { return; }    
    $(selectorMapping[messageType].blockPanel).append(standardComponentBlock(value, selectorMapping[messageType].validSelectOptions[value].panel));
    $(selectorMapping[messageType].removeButton_Last).on('click', function () { removeText(this, messageType) });    
    renderPreview(messageType);
    return;
}

function initializeGlobalVariables() {
    loadIndicatorTop = $(Selector.loadIndicator_Top).dxLoadIndicator({ visible: false }).dxLoadIndicator('instance');
    
    selectorMapping[MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.newLineValue] = { panel: '&#9166;', preview: '\n' };
    selectorMapping[MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.defaultMessageValue] = { panel: '{Default Message}', preview: vdata.sampleValues.defaultCatalogueMessage };
    selectorMapping[MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueNameValue] = { panel: '{Catalogue Name}', preview: 'Example Catalogue' };
    selectorMapping[MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueCodeValue] = { panel: '{Catalogue Code}', preview: 'EXAMPLE' };
    selectorMapping[MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueStartDateValue] = { panel: '{Catalogue Start Date}', preview: '01/12/2023' };
    selectorMapping[MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueEndDateValue] = { panel: '{Catalogue End Date}', preview: '31/12/2023' };
    selectorMapping[MessageType.CATALOGUE].validSelectOptions[vdata.templateComponents.catalogueUrlValue] = { panel: '{Catalogue URL}', preview: vdata.sampleValues.catalogueURL };
    
    selectorMapping[MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.newLineValue] = { panel: '&#9166;', preview: '\n' };
    selectorMapping[MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.defaultMessageValue] = { panel: '{Default Message}', preview: vdata.sampleValues.defaultProductMessage };
    selectorMapping[MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productNameValue] = { panel: '{Product Name}', preview: 'Example Product' };
    selectorMapping[MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productCodeValue] = { panel: '{Product Code}', preview: 'EXAMPLE' };
    selectorMapping[MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productDescriptionValue] = { panel: '{Product Description}', preview: 'G.O.A.T.' };
    selectorMapping[MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productListPriceValue] = { panel: '{Product Price}', preview: 'HKD888.88' };
    selectorMapping[MessageType.PRODUCT].validSelectOptions[vdata.templateComponents.productReplyKeyword] = { panel: '{Keyword}', preview: 'BUY' };

    return;
}

async function preparePanels(messageType: MessageType) {    
    // currently only product posts have a keyword that may trigger the chatbot
    if (messageType === MessageType.PRODUCT) {
        await restoreKeyword();
        $(Selector.keywordInput_Product).on('keyup', function () {
            const keyword = sanitizeInput(String($(this).val()).trim());
            selectorMapping[messageType].validSelectOptions[vdata.templateComponents.productReplyKeyword].preview = keyword;
            validateKeyword(keyword);
            renderPreview(messageType);
        });
    }

    await restoreTemplate(messageType);
    $(selectorMapping[messageType].customTextAddButton).on('click', () => addText(messageType));
    $(selectorMapping[messageType].customTextInput).on('keydown', function (event) {        
        if (event.key === 'Enter') {
            addText(messageType);
        }
    });
    $(selectorMapping[messageType].componentAddButton).on('click', () => addComponent(messageType));
    $(selectorMapping[messageType].restoreDefaultButton).on('click', () => restoreDefaultTemplate(messageType));
    $(selectorMapping[messageType].saveTemplateButton).on('click', () => saveTemplate(messageType));
}

$(async function () {
    if (vdata.settings.env === "PRD") {
        $(Selector.catalogueSetting).hide();
    } else {
        $(Selector.catalogueSetting).show();
    }
    initializeGlobalVariables();
    loadIndicatorTop.option('visible', true);
    await preparePanels(MessageType.CATALOGUE);    
    await preparePanels(MessageType.PRODUCT);    
    loadIndicatorTop.option('visible', false);    
});

    