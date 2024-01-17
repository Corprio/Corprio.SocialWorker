import { Selector, MessageType } from './Enums';

export interface TemplateDetails {
    preview: string,
    panel: string
}

export interface DraggedBlock {
    value: string,
    type: MessageType
}

export interface Selected {
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
    dragBlock_Last: Selector,
    validSelectOptions: Record<string, TemplateDetails>
}

export interface StringifiedTemplate {
    isValid: boolean,
    keyword: string,
    templateString: string
}