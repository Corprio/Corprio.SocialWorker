import { DeliveryOption } from './Enums';
import { CheckoutDataModel, CheckoutOrderLine, ProductVariationInfo } from './Interfaces';

declare const vdata: {
    actions: {
        deleteSalesOrderLine: string;
        editSalesOrderLine: string;
        finalizeSalesOrder: string;
        getProductVariations: string;
        voidAndRecall: string;
        voidOrder: string;
    };
    localizer: {
        addNewProduct: string;
        billTo: string;
        cancel: string;
        cancelOrder: string;
        cannotEdit_PaymentProcessing: string;
        cannotEdit_VoidedOrPaid: string;
        colour: string;
        confirm: string;
        confirmAddProduct: string;
        confirmVoidOrder: string;
        confirmation: string;
        delete: string;
        deliveryCharge: string;
        deliveryMethod: string;
        editQuantity: string;
        editVariant: string;
        errMsgChatbotNotFound: string;
        errMsgUnselectedVariations: string;
        familyName: string;
        freeShippingHint1: string;
        freeShippingHint2: string;
        freeShippingQualified: string;
        givenName: string;
        invalidFeedback_AddressLine1: string;
        invalidFeedback_CountryCode: string;
        invalidFeedback_FamilyName: string;
        invalidFeedback_GivenName: string;
        invalidFeedback_InvalidPhoneNumber: string;
        noDeliveryMethod: string;
        noPickupInstruction: string;
        proceedToPayment: string;
        processing: string;
        quantity: string;
        receipient: string;
        remainingStockMessage: string;
        remainingStockTitle: string;
        save: string;
        selfPickup: string;
        shipToCustomer: string;
        size: string;
        titleEditVariant: string;
        warningDeleteProduct: string;
    };
    model: {
        allowSelfPickup: string;
        billedPersonFamilyName: string;
        billedPersonGivenName: string;
        billedPhoneCountryCallingCode: string;
        billedPhoneNationalDestinationCode: string;
        billedPhoneSubscriberNumber: string;
        chosenDeliveryMethod: string;
        currencyCode: string;
        defaultCountryCode: string;
        deliveryAddress_City: string;
        deliveryAddress_CountryAlphaCode: string;
        deliveryAddress_Line1: string;
        deliveryAddress_Line2: string;
        deliveryAddress_PostalCode: string;
        deliveryAddress_State: string;
        deliveryChargeAmount: string;
        deliveryContact_FamilyName: string;
        deliveryContact_GivenName: string;
        deliveryPhoneCountryCallingCode: string;
        deliveryPhoneNationalDestinationCode: string;
        deliveryPhoneSubscriberNumber: string;
        freeShippingAmount: string;
        hasFreeShipping: string;
        isOrderVoidOrPaid: string;
        isPaymentClicked: string;
        orderLineJsonString: string;
        organizationEmailAddress: string;
        organizationID: string;
        organizationShortName: string;
        provideShipping: string;
        salesOrderID: string;
        selfPickUpInstruction: string;
    };
    settings: {
        appUrl: string;
        paymentPortalUrl: string;
    };
};

// magic numbers
const VALIDATION_GROUP = 'checkout';
const DATASET_ATTRIBUTE = 'attribute';
const DATASET_ORDERLINE = 'orderline';

// global variables (state)
let chosenDeliveryMethod: DeliveryOption = DeliveryOption.NoOption;
let allowSelfPickup: boolean = false;
let provideShipping: boolean = false;
let hasFreeShippingPolicy: boolean = false;
let freeShippingAmount: number = 0;
let isPaymentClicked: boolean = false;
let isOrderVoidOrPaid: boolean = false;
let totalQty: number = 0;
let totalAmount: number = 0;
let actualDeliveryCharge: number = 0;
const availableDeliveryMethods: DeliveryOption[] = [];
let orderLines: CheckoutOrderLine[] = [];


function deleteRow() {
    if (isPaymentClicked) { return; }

    // IMPORTANT: since we are using 'this', the following two lines must NOT be nested in another function
    const salesOrderLineID = $(this).data(DATASET_ORDERLINE);
    const $row = $(this).closest('.orderline-row');

    DevExpress.ui.dialog
        .confirm(vdata.localizer.warningDeleteProduct, vdata.localizer.confirmation)
        .done(function (dialogResult) {            
            if (dialogResult) {
                $.post({
                    url: vdata.actions.deleteSalesOrderLine,
                    data: {
                        salesOrderID: vdata.model.salesOrderID,
                        salesOrderLineID: salesOrderLineID
                    }
                }).done(function () {                    
                    orderLines = orderLines.filter(x => x.SalesOrderLineID !== salesOrderLineID);                    
                    $row.remove();
                    recalculateTotals();
                    adjustDeliveryCharge();
                    renderTotals();
                    renderShippingChoice($('#shipping-choice'));
                }).fail(corprio.formatError);
            }
        });
}

function editQtyTemplate(line: CheckoutOrderLine) {
    const $content = $(`<div id="popup-${line.SalesOrderLineID}">`);
    $('<label class="flex-even">').html(vdata.localizer.quantity).appendTo($content);
    const $qtyDiv = $('<div class="flex-even mb-4">').appendTo($content);
    const $numBox = $qtyDiv.dxNumberBox({
        width: String(Math.trunc(line.Quantity)).length * 8 + 92,  // note: the quantity is orginally a decimal
        value: Math.trunc(line.Quantity),
        max: line.DisallowOutOfStock ? Math.max(0, line.ProductStockLevel) : undefined,
        min: 0,
        buttons: [
            {
                location: 'before',
                name: 'minus',
                options: {
                    icon: 'minus',
                    onClick: function () {                        
                        $numBox.option('value', Math.max(0, $numBox.option('value') - 1));
                    },
                },
            },
            {
                location: 'after',
                name: 'add',
                options: {
                    icon: 'add',
                    onClick: function () {                        
                        $numBox.option('value', $numBox.option('value') + 1);
                    },
                },
            }
        ],
        readOnly: isPaymentClicked,
        onValueChanged: function (e) {
            let changedValue: number = e.value;
            if (changedValue < 0) {
                $numBox.option('value', 0);  // WARNING: this function will run again
                return;
            }
            if (line.DisallowOutOfStock && changedValue > line.ProductStockLevel) {
                DevExpress.ui.dialog.alert(vdata.localizer.remainingStockMessage.replaceAll('{0}', line.ProductName).replaceAll('{1}', String(line.ProductStockLevel)), vdata.localizer.remainingStockTitle);
                $numBox.option('value', Math.max(0, line.ProductStockLevel));  // WARNING: this function will run again
                return;
            }
            if (changedValue % 1) {
                $numBox.option('value', Math.trunc(changedValue));  // WARNING: this function will run again
                return;
            }
            line.Quantity = changedValue;
            $qtyDiv.css('width', String(changedValue).length * 8 + 92);  // re-adjust the width if necessary            
        },
        onInput: function (e) {
            const value = +$(e.event.target).val();
            if (line.DisallowOutOfStock && value > line.ProductStockLevel) {
                DevExpress.ui.dialog.alert(vdata.localizer.remainingStockMessage.replaceAll('{0}', line.ProductName).replaceAll('{1}', String(line.ProductStockLevel)), vdata.localizer.remainingStockTitle);                
                return;
            }
            
        }
    }).dxNumberBox('instance');  // note: we have to manually initialize a widget instance and attach it to the container

    $('<button class="btn btn-info m-2">').html(vdata.localizer.save).on('click', function () { saveRow(line) }).appendTo($content);
    $('<button class="btn btn-secondary m-2">').html(vdata.localizer.cancel).on('click', hidePopup).appendTo($content);

    return $content;
}

function hidePopup() {
    const $popup = $("#edit-popup").dxPopup("instance");
    $popup.hide();
}

function determineProduct(line: CheckoutOrderLine) {
    if (!(line.ChildProductInfo?.length > 1)) { return hidePopup(); }

    const $selectBoxes = $('.productAttr');
    const selectedVariations: ProductVariationInfo[] = [];
    const missingAttributes: string[] = [];
    let numberOfAttributes = 0;  // count the number of unique attributes
    $selectBoxes.each(function (_, obj: HTMLElement) {
        numberOfAttributes++;
        const selected = $(obj).dxSelectBox().dxSelectBox('instance').option("value");
        if (selected) {
            const productVariation: ProductVariationInfo = { Attribute: $(obj).data(DATASET_ATTRIBUTE), Code: selected, Name: selected };
            selectedVariations.push(productVariation);
        } else {
            let attributeName = $(obj).data(DATASET_ATTRIBUTE);
            if (attributeName === 'size') {
                attributeName = vdata.localizer.size;
            } else if (attributeName === 'colour') {
                attributeName = vdata.localizer.colour;
            }
            missingAttributes.push(attributeName);
        }
    });

    if (selectedVariations.length !== numberOfAttributes) {
        DevExpress.ui.dialog.alert(vdata.localizer.errMsgUnselectedVariations.replaceAll('{0}', missingAttributes.join(', ')), vdata.localizer.titleEditVariant);
        return;
    }

    let selectedProductID: string;    
    for (const childProduct of line.ChildProductInfo) {
        let count = 0;
        for (const productVariation of childProduct.ChildProductAttributes) {            
            count++;
            if (!selectedVariations.find(x => x.Attribute === productVariation.Attribute && x.Code === productVariation.Code)) {
                break;
            }
            if (count === numberOfAttributes) {
                selectedProductID = childProduct.ID;
                break;
            }
        }
        if (selectedProductID) { break; }
    }

    if (!selectedProductID) {
        const combinations: string[] = [];
        for (let i = 0; i < selectedVariations.length; i++) {
            combinations.push(`${selectedVariations[i].Attribute}:${selectedVariations[i].Code}`);
        }
        console.log(`${combinations.join(',')} is not available.`);
        return hidePopup();
    }

    line.ProductID = selectedProductID;
    return saveRow(line);
}

// note: a template function CANNOT be async
function editVariantTemplate(line: CheckoutOrderLine) {    
    const $content = $(`<div id="popup-${line.SalesOrderLineID}">`);    

    if (line.ChildProductInfo.length) {        
        const combinations: Record<string, ProductVariationInfo[]> = {};        
        for (const childProduct of line.ChildProductInfo) {
            for (const productVariation of childProduct.ChildProductAttributes) {
                if (combinations[productVariation.Attribute]?.length) {
                    if (!combinations[productVariation.Attribute].find(x => x.Code === productVariation.Code)) {
                        combinations[productVariation.Attribute].push(productVariation);
                    }
                } else {
                    combinations[productVariation.Attribute] = [productVariation];
                }
            }
        }

        for (const attribute in combinations) {
            let attributeName;
            switch (attribute) {
                case 'size':
                    attributeName = vdata.localizer.size;
                    break;
                case 'colour':
                    attributeName = vdata.localizer.colour;
                    break;
                default:
                    attributeName = attribute;
            }

            $('<div class="productAttr mb-4">').appendTo($content).data(DATASET_ATTRIBUTE, attribute).dxSelectBox({
                dataSource: combinations[attribute],                                
                label: attributeName,
                labelMode: 'floating',
                displayExpr: 'Name',
                valueExpr: 'Code',
                onValueChanged: function (e) {
                    console.log(e.value);
                },
            });

        }
    }
    
    $('<button class="btn btn-info m-2">').html(vdata.localizer.save).on('click', function () { determineProduct(line) }).appendTo($content);
    $('<button class="btn btn-secondary m-2">').html(vdata.localizer.cancel).on('click', hidePopup).appendTo($content);

    return $content;
}

function saveRow(line: CheckoutOrderLine) {
    return $.post({
        url: vdata.actions.editSalesOrderLine,
        data: {
            salesOrderID: vdata.model.salesOrderID,
            salesOrderLineID: line.SalesOrderLineID,
            productID: line.ProductID,
            quantity: line.Quantity
        }
    }).done((newLine: CheckoutOrderLine) => {
        for (const line of orderLines) {
            if (line.SalesOrderLineID === newLine.SalesOrderLineID) {
                line.DisallowOutOfStock = newLine.DisallowOutOfStock;
                line.NetUnitPrice = newLine.NetUnitPrice;
                line.ProductDesc = newLine.ProductDesc;
                line.ProductID = newLine.ProductID;
                line.ProductName = newLine.ProductName;
                line.ProductStockLevel = newLine.ProductStockLevel;
                line.Quantity = newLine.Quantity;                
                line.UOMCode = newLine.UOMCode;
                line.URL = newLine.URL;
                break;
            }
        }
        prepareCartTable();
        hidePopup();
        recalculateTotals();
        adjustDeliveryCharge();
        renderTotals();
        renderShippingChoice($('#shipping-choice'));
    }).fail(corprio.formatError);
}

function recalculateTotals() {
    totalQty = orderLines.reduce(function (result, item) {
        result += item.Quantity;
        return result;
    }, 0);

    totalAmount = orderLines.reduce(function (result, item) {
        result += item.Quantity * item.NetUnitPrice;
        return result;
    }, 0);
}

function renderTotals() {
    $('#order-subtotal').text(StaticData.FormatCurrency(totalAmount, vdata.model.currencyCode));
    if (actualDeliveryCharge > 0) {
        $('#delivery-charge-div').show();
        $('#delivery-charge-amt').text(StaticData.FormatCurrency(actualDeliveryCharge, vdata.model.currencyCode));
    } else {
        $('#delivery-charge-div').hide();
    }
    $('#order-total').text(StaticData.FormatCurrency(totalAmount + actualDeliveryCharge, vdata.model.currencyCode));    
}

function prepareBillToFields(defCallingCode: string) {
    const $billToFieldset = $('<fieldset id="bill-to-fieldset">').appendTo($('#customer-info-form'));
    $('<h5>').addClass('mt-3').text(vdata.localizer.billTo).appendTo($billToFieldset);

    const $givenNameWidget = $('<div id="bill-person-given-name">')
        .dxTextBox({
            value: vdata.model.billedPersonGivenName,
            disabled: isPaymentClicked,
            maxLength: 100,
            label: vdata.localizer.givenName,
            labelMode: 'floating',
            inputAttr: { autocomplete: 'given-name', name: 'bill-person-given-name-input', id: 'bill-person-given-name-input' },
            onValueChanged: function (e) {
                const deliveryName = $('#delivery-contact-given-name');
                if (deliveryName.length) deliveryName.dxTextBox('option', 'value', e.value);
            },
        })
        .dxValidator({
            validationGroup: VALIDATION_GROUP,
            validationRules: [{ type: 'required', message: vdata.localizer.invalidFeedback_GivenName }]
        });

    const $familyNameWidget = $('<div id="bill-person-family-name">')
        .dxTextBox({
            value: vdata.model.billedPersonFamilyName,
            disabled: isPaymentClicked,
            maxLength: 100,
            label: vdata.localizer.familyName,
            labelMode: 'floating',
            inputAttr: { autocomplete: 'family-name', name: 'bill-person-family-name-input', id: 'bill-person-family-name-input' },
            onValueChanged: function (e) {
                const deliveryName = $('#delivery-contact-family-name');
                if (deliveryName.length) deliveryName.dxTextBox('option', 'value', e.value)
            }
        })
        .dxValidator({
            validationGroup: VALIDATION_GROUP,
            validationRules: [{ type: 'required', message: vdata.localizer.invalidFeedback_FamilyName }]
        });
    
    $('<div>').addClass('d-flex mb-2').appendTo($billToFieldset).append($givenNameWidget, $familyNameWidget);
    
    const $phoneWidget = $('<div id="bill-contact-phone">').appendTo($billToFieldset);
    corprio.geography.addPhoneNumberTo(
        $phoneWidget,
        vdata.model.billedPhoneSubscriberNumber
            ? { NumberType: 3, CountryCallingCode: vdata.model.billedPhoneCountryCallingCode, NationalDestinationCode: vdata.model.billedPhoneNationalDestinationCode, SubscriberNumber: vdata.model.billedPhoneSubscriberNumber }
            : { NumberType: 3, CountryCallingCode: defCallingCode },
        'contact-phone',
        null,
        function (phoneNumber) {
            if (isPaymentClicked) { return; }

            const $deliveryPhone = $('#delivery-phone-number');            
            if ($deliveryPhone.length) {
                corprio.geography.setPhoneNumber($deliveryPhone, phoneNumber);
            }
        },
        true,
        VALIDATION_GROUP
    );    
    $phoneWidget.find('.phone').dxValidator({
        validationGroup: VALIDATION_GROUP,
        validationRules: [{
            type: 'custom',
            reevaluate: true,
            message: vdata.localizer.invalidFeedback_InvalidPhoneNumber,
            validationCallback: function (input) {
                if (input.value == null || input.value == "" || input.value == undefined) return false;
                return true;
            }
        }]
    });
    if (isPaymentClicked) {
        $phoneWidget.find('input').attr('disabled', 'disabled');
        $phoneWidget.find('div[role="button"]').remove();
    }
}

// stopped at here; this function worked. you can move on to do the email template/bot/settings
function renderShippingChoice($shipToCustomerDiv: JQuery<HTMLElement>) {
    $shipToCustomerDiv.empty();
    $shipToCustomerDiv.append(
        $(`<span class="font-weight-bold">${translateDeliveryOption(DeliveryOption.Shipping)}</span>`)
    );

    // note: free shipping status is not shown if the customer has proceeded to payment, because the merchant may have adjusted the free shipping policy since then
    if (hasFreeShippingPolicy && !isPaymentClicked) {
        let $freeShippingStatus = $('<span>');
        if (freeShippingAmount > totalAmount) {
            $freeShippingStatus.append('<span class="text-warning mr-1"><i class="fa-solid fa-circle-exclamation"></i></span>', vdata.localizer.freeShippingHint2.replaceAll('{0}', StaticData.FormatCurrency(freeShippingAmount - totalAmount, vdata.model.currencyCode)));
        } else {
            $freeShippingStatus.append('<i class="fa-solid fa-circle-check text-success mr-1"></i>', vdata.localizer.freeShippingQualified);
        }
        $shipToCustomerDiv.append(
            $('<span class="ml-2">').append($('<span>').text('('), $freeShippingStatus, $('<span>').text(')'))
        );
    }
    return $shipToCustomerDiv;
}

function prepareDeliveryMethodFields(defCallingCode: string) {
    const $deliveryMethodFieldset = $('<fieldset id="delivery-method-fieldset">').appendTo($('#customer-info-form'));

    if (!availableDeliveryMethods.length) {
        $deliveryMethodFieldset.append(
            `<div class="font-italic">${vdata.localizer.noDeliveryMethod.replaceAll('{0}', vdata.model.organizationShortName).replaceAll('{1}', vdata.model.organizationEmailAddress)}</div>`);
        return;
    }

    // the following scenario could happen if the number of delivery methods went from zero to positive 
    // after the user 'clicked' or the order was voided / paid
    if (chosenDeliveryMethod === DeliveryOption.NoOption) { return; }

    $deliveryMethodFieldset.append(
        $('<h5 class="mt-3">').text(`${vdata.localizer.deliveryMethod}`),
        $('<div id="delivery-method">').dxRadioGroup({
            // dependencies: the following two variables are assigned with adjustGlobalVariables()
            dataSource: availableDeliveryMethods,
            disabled: isPaymentClicked,
            value: chosenDeliveryMethod,
            itemTemplate: function (itemData, _, itemElement) {
                if (itemData === DeliveryOption.SelfPickup) {
                    itemElement.append(
                        $('<div class="font-weight-bold">').text(translateDeliveryOption(DeliveryOption.SelfPickup)),
                        $('<div class="delivery-method-info my-2">').text(vdata.model.selfPickUpInstruction
                            ? vdata.model.selfPickUpInstruction
                            : vdata.localizer.noPickupInstruction.replaceAll('{0}', vdata.model.organizationShortName).replaceAll('{1}', vdata.model.organizationEmailAddress))
                    );
                } else if (itemData === DeliveryOption.Shipping) {
                    const $shipToCustomerDiv = renderShippingChoice($('<div id="shipping-choice">'));                    
                    let $deliveryChargeContent = $('<div>').html(`${vdata.localizer.deliveryCharge}: ${StaticData.FormatCurrency(parseFloat(vdata.model.deliveryChargeAmount), vdata.model.currencyCode)}`);
                    let $freeShippingHint = $('<div class="font-weight-bold">').html(vdata.localizer.freeShippingHint1.replaceAll('{0}', StaticData.FormatCurrency(freeShippingAmount, vdata.model.currencyCode)));
                    itemElement.append(
                        $shipToCustomerDiv,
                        // note: free shipping hint is not shown if the customer has proceeded to payment, because the merchant may have adjusted the free shipping policy since then
                        $('<div class="delivery-method-info my-2">').append(
                            (hasFreeShippingPolicy && !isPaymentClicked) ? [$deliveryChargeContent, $freeShippingHint] : $deliveryChargeContent
                        )
                    );                    
                }
            },
            onValueChanged: function (e) {
                if (isPaymentClicked) return;

                chosenDeliveryMethod = e.value;
                $('#deliver-to-section').toggle(chosenDeliveryMethod === DeliveryOption.Shipping);
                $('#recipient-section').toggle(chosenDeliveryMethod === DeliveryOption.Shipping);

                adjustDeliveryCharge();
                $('#delivery-charge-amt').text(StaticData.FormatCurrency(actualDeliveryCharge, vdata.model.currencyCode));
                $('#delivery-charge-div').toggle(chosenDeliveryMethod === DeliveryOption.Shipping);
                $('#order-total').text(StaticData.FormatCurrency(actualDeliveryCharge + totalAmount, vdata.model.currencyCode));
            }
        })
    );

    if (!provideShipping) return;

    // a validation callback that is used below (ideally we should tailor-make one for each input)
    function inputValidationForShipping(input) {
        if (chosenDeliveryMethod === DeliveryOption.SelfPickup) return true;
        if (input.value == null || input.value == "" || input.value == undefined) return false;
        return true;
    }

    const $deliverToDiv = $('<div id="deliver-to-section">').appendTo($deliveryMethodFieldset);
    $deliverToDiv.toggle(chosenDeliveryMethod === DeliveryOption.Shipping);
    $deliverToDiv.append(
        $('<h5>').addClass('mt-3').text('Deliver To'),
        $('<div id="delivery-address-line1" class="mb-2">').dxTextBox({
            value: vdata.model.deliveryAddress_Line1,
            disabled: isPaymentClicked,
            maxLength: 250,
            label: corprio.globalization.getMessage('line1'),
            labelMode: 'floating',
            inputAttr: { autocomplete: 'address-line1', name: 'delivery-address-line1-input', id: 'delivery-address-line1-input' }
        }).dxValidator({
            validationGroup: VALIDATION_GROUP, validationRules: [{
                type: 'custom',
                reevaluate: true,
                message: vdata.localizer.invalidFeedback_AddressLine1,
                validationCallback: inputValidationForShipping
            }]
        }),
        $('<div id="delivery-address-line2" class="mb-2">').dxTextBox({
            value: vdata.model.deliveryAddress_Line2,
            disabled: isPaymentClicked,
            maxLength: 250,
            label: corprio.globalization.getMessage('line2'),
            labelMode: 'floating',
            inputAttr: { autocomplete: 'address-line2', name: 'delivery-address-line2-input', id: 'delivery-address-line2-input' }
        }),
        $('<div id="delivery-address-city" class="mb-2">').dxTextBox({
            value: vdata.model.deliveryAddress_City,
            disabled: isPaymentClicked,
            maxLength: 250,
            label: corprio.globalization.getMessage('City'),
            labelMode: 'floating',
            inputAttr: { autocomplete: 'address-level2', name: 'delivery-address-city-input', id: 'delivery-address-city-input' }
        }),
        $('<div id="delivery-address-state" class="mb-2">').dxTextBox({
            value: vdata.model.deliveryAddress_State,
            disabled: isPaymentClicked,
            maxLength: 250,
            label: corprio.globalization.getMessage('State'),
            labelMode: 'floating',
            inputAttr: { autocomplete: 'address-level1', name: 'delivery-address-state-input', id: 'delivery-address-state-input' }
        }),
        $('<div id="delivery-address-postal" class="mb-2">').dxTextBox({
            value: vdata.model.deliveryAddress_PostalCode,
            disabled: isPaymentClicked,
            maxLength: 20,
            label: corprio.globalization.getMessage('PostalCode'),
            labelMode: 'floating',
            inputAttr: { autocomplete: 'postal-code', name: 'delivery-address-postal-input', id: 'delivery-address-postal-input' }
        }),
        $('<div id="delivery-address-country-code" class="mb-2">').dxSelectBox({
            value: vdata.model.defaultCountryCode,
            disabled: isPaymentClicked,
            dataSource: StaticData.CountryList,
            displayExpr: 'Value.GlobalizedName',
            valueExpr: 'Key',
            label: corprio.globalization.getMessage('Country'),
            labelMode: 'floating',            
            searchEnabled: true,
            inputAttr: { autocomplete: 'country-code', name: 'delivery-address-country-code-input', id: 'delivery-address-country-code-input' }
        }).dxValidator({
            validationGroup: VALIDATION_GROUP,
            validationRules: [{
                type: 'custom',
                reevaluate: true,
                message: vdata.localizer.invalidFeedback_CountryCode,
                validationCallback: inputValidationForShipping
            }]
        })
    );

    const recipientDiv = $('<div id="recipient-section">').appendTo($deliveryMethodFieldset);
    recipientDiv.toggle(chosenDeliveryMethod === DeliveryOption.Shipping);
    $('<h5>').addClass('mt-3').text(vdata.localizer.receipient).appendTo(recipientDiv);
    $('<div>').addClass('d-flex').appendTo(recipientDiv).append(
        $('<div id="delivery-contact-given-name">').dxTextBox({
            value: (isPaymentClicked) ? vdata.model.deliveryContact_GivenName : String($('#bill-person-given-name').find('input').val()),
            disabled: isPaymentClicked,
            maxLength: 100,
            label: 'Given Name',
            labelMode: 'floating',
            inputAttr: { autocomplete: 'given-name', name: 'delivery-contact-given-name-input', id: 'delivery-contact-given-name-input' },
        }).dxValidator({
            validationGroup: VALIDATION_GROUP,
            validationRules: [{
                type: 'custom',
                reevaluate: true,
                message: vdata.localizer.invalidFeedback_GivenName,
                validationCallback: inputValidationForShipping
            }]
        }),
        $('<div id="delivery-contact-family-name">').dxTextBox({
            value: (isPaymentClicked) ? vdata.model.deliveryContact_FamilyName : String($('#bill-person-family-name').find('input').val()),
            disabled: isPaymentClicked,
            maxLength: 100,
            label: 'Family Name',
            labelMode: 'floating',
            inputAttr: { autocomplete: 'family-name', name: 'delivery-contact-family-name-input', id: 'delivery-contact-family-name-input' },
        }).dxValidator({
            validationGroup: VALIDATION_GROUP,
            validationRules: [{
                type: 'custom',
                reevaluate: true,
                message: vdata.localizer.invalidFeedback_FamilyName,
                validationCallback: inputValidationForShipping
            }]
        })
    );
    
    const $deliveryPhone = $('<div id="delivery-phone-number">').appendTo(recipientDiv);
    corprio.geography.addPhoneNumberTo(
        $deliveryPhone,
        vdata.model.deliveryPhoneSubscriberNumber
            ? { NumberType: 3, CountryCallingCode: vdata.model.deliveryPhoneCountryCallingCode, NationalDestinationCode: vdata.model.deliveryPhoneNationalDestinationCode, SubscriberNumber: vdata.model.deliveryPhoneSubscriberNumber }
            : { NumberType: 3, CountryCallingCode: defCallingCode },
        'delivery-phone-number',
        null,
        null,
        false,
        VALIDATION_GROUP);
    $deliveryPhone.find('.phone').dxValidator({
        validationGroup: VALIDATION_GROUP,
        validationRules: [{
            type: 'custom',
            reevaluate: true,
            message: vdata.localizer.invalidFeedback_InvalidPhoneNumber,
            validationCallback: inputValidationForShipping
        }]
    });
    if (isPaymentClicked) {
        $deliveryPhone.find('input').attr('disabled', 'disabled');
        $deliveryPhone.find('div[role="button"]').remove();
    }
}

function handleAddProduct() {
    DevExpress.ui.dialog
        .confirm(vdata.localizer.confirmAddProduct, vdata.localizer.confirmation)
        .done(function (dialogResult) {
            if (dialogResult) {
                $.post({
                    url: vdata.actions.voidAndRecall,
                    data: { salesOrderID: vdata.model.salesOrderID }
                }
                ).done(() => { location.reload() }
                ).fail(function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status != 410) {
                        return corprio.formatError(jqXHR, textStatus, errorThrown);
                    }

                    var prompt = DevExpress.ui.dialog.custom({
                        title: vdata.localizer.confirmation,
                        messageHtml: `<p>${vdata.localizer.errMsgChatbotNotFound}</p>`,
                        buttons: [{
                            text: vdata.localizer.confirm,
                            onClick: function () {
                                return $.post({
                                    url: vdata.actions.voidOrder,
                                    data: { salesOrderID: vdata.model.salesOrderID }
                                }
                                ).done(() => { location.reload() }
                                ).fail(corprio.formatError);
                            },
                            type: "danger",
                        }, {
                            text: vdata.localizer.cancel,                            
                            type: "normal",
                        }]
                    });
                    prompt.show();

                });
            }
        });
}

function handleVoidOrder() {
    DevExpress.ui.dialog
        .confirm(vdata.localizer.confirmVoidOrder, vdata.localizer.confirmation)
        .done(function (dialogResult) {
            if (dialogResult) {
                $.post({
                    url: vdata.actions.voidOrder,
                    data: { salesOrderID: vdata.model.salesOrderID }
                }
                ).done(() => { location.reload() }
                ).fail(corprio.formatError);
            }
        });
}

function prepareCustomerInfoForm() {        
    const $form = $('#customer-info-form').addClass('customer-info my-4');    
    const defCountryInfo = StaticData.CountryList.find(c => c.Key === vdata.model.defaultCountryCode);
    const defCallingCode = defCountryInfo && defCountryInfo.Value.CountryCallingCode;    
    prepareBillToFields(defCallingCode);
    prepareDeliveryMethodFields(defCallingCode);
            
    $('<div id="validation-summary-purchase">').appendTo($form).dxValidationSummary({ validationGroup: VALIDATION_GROUP });
    const buttons = $('<div>').appendTo($('<div>').addClass('d-flex justify-content-center mt-2 mb-2').appendTo($form));
    if (!isPaymentClicked) {
        buttons.append($('<button type="submit">').addClass('btn btn-primary m-2').html(`<i class="fas fa-check mr-1"></i>${vdata.localizer.proceedToPayment}`));
    }
    if (!isOrderVoidOrPaid) {
        buttons.append($('<button type="button">').addClass('btn btn-danger m-2').html(`<i class="fas fa-times mr-1"></i>${vdata.localizer.cancelOrder}`).on('click', handleVoidOrder));
    }
    
    const loadPanel = $('#load-panel').dxLoadPanel({
        message: vdata.localizer.processing,
        visible: false,
        showIndicator: true,
        showPane: true,
        shading: true,
        hideOnOutsideClick: false
    }).dxLoadPanel('instance');

    $form.on('submit', function (event: Event) {
        event.preventDefault();

        let validationGroup = DevExpress.validationEngine.getGroupConfig(VALIDATION_GROUP);
        if (validationGroup) {
            let validationResult = validationGroup.validate();
            if (!validationResult.isValid) {
                $('#validation-summary-purchase')[0].scrollIntoView({ behavior: 'smooth' })
                return;
            }
        }

        const data: CheckoutDataModel = {
            BillPerson: {
                FamilyName: $('#bill-person-family-name').dxTextBox('option', 'value'),
                GivenName: $('#bill-person-given-name').dxTextBox('option', 'value')
            },
            BillContactPhone: corprio.geography.getPhoneNumber($('#bill-contact-phone')),
            ChosenDeliveryMethod: chosenDeliveryMethod,
            SalesOrderID: vdata.model.salesOrderID,
        };
        if (chosenDeliveryMethod === DeliveryOption.Shipping) {            
            data.DeliveryAddress = {
                Line1: $('#delivery-address-line1').dxTextBox('option', 'value'),
                Line2: $('#delivery-address-line2').dxTextBox('option', 'value'),
                City: $('#delivery-address-city').dxTextBox('option', 'value'),
                PostalCode: $('#delivery-address-postal').dxTextBox('option', 'value'),
                State: $('#delivery-address-state').dxTextBox('option', 'value'),
                CountryAlphaCode: $('#delivery-address-country-code').dxSelectBox('option', 'value')
            };
            data.DeliveryContact = {
                FamilyName: $('#delivery-contact-family-name').dxTextBox('option', 'value'),
                GivenName: $('#delivery-contact-given-name').dxTextBox('option', 'value')
            };
            data.DeliveryContactPhone = corprio.geography.getPhoneNumber($('#delivery-phone-number'));
        }
        loadPanel.show();
        $.ajax({
            url: vdata.actions.finalizeSalesOrder,
            type: 'POST',
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(data),
            success: function () {
                window.location.replace(`${vdata.settings.paymentPortalUrl}/T42/RecePayment/order/${vdata.model.salesOrderID}?successUrl=${vdata.settings.appUrl}/${vdata.model.organizationID}/thankyou&failUrl=${vdata.settings.appUrl}/${vdata.model.organizationID}/paymentfailed`);
            },
            complete: function () {
                loadPanel.hide();
            },
            error: corprio.formatError
        });
    });
}

function translateDeliveryOption(deliveryOption: DeliveryOption) {
    switch (deliveryOption) {
        case DeliveryOption.SelfPickup:
            return vdata.localizer.selfPickup;
        case DeliveryOption.Shipping:
            return vdata.localizer.shipToCustomer;        
    }
}

function adjustDeliveryCharge() {
    // note: if the customer has already proceeded to payment, then any delivery charge would have been included in the order line
    if (isPaymentClicked || chosenDeliveryMethod !== DeliveryOption.Shipping) {
        actualDeliveryCharge = 0;
    } else if (hasFreeShippingPolicy && freeShippingAmount < totalAmount) {
        actualDeliveryCharge = 0;
    } else {
        actualDeliveryCharge = parseFloat(vdata.model.deliveryChargeAmount);
        actualDeliveryCharge = isNaN(actualDeliveryCharge) ? 0 : actualDeliveryCharge;
    }
}

function adjustGlobalVariables() {        
    try {
        // note: the JSON string provided by the backend has double quotation marks replaced with &quot;
        orderLines = JSON.parse(vdata.model.orderLineJsonString.replaceAll('&quot;', '"'));
    } catch (e) {
        console.log(`Failed to parse order lines. ${e}`);
        orderLines.length = 0;
    }

    allowSelfPickup = vdata.model.allowSelfPickup.toLowerCase() === 'true';
    provideShipping = vdata.model.provideShipping.toLowerCase() === 'true';
    isOrderVoidOrPaid = vdata.model.isOrderVoidOrPaid.toLowerCase() === 'true';

    // if for some reason (e.g., the merchant has processed the order using SalesMaster) the order has been voided/paid
    // even before the customer has clicked the payment button, we act as if the button has been clicked
    isPaymentClicked = isOrderVoidOrPaid ? true : vdata.model.isPaymentClicked.toLowerCase() === 'true';
    if (isPaymentClicked) {        
        // note: the enums from backend become string literals of their NAMES, not '0', '1', '2', etc.
        switch (vdata.model.chosenDeliveryMethod) {
            case 'SelfPickup':
                chosenDeliveryMethod = DeliveryOption.SelfPickup;
                break;
            case 'Shipping':
                chosenDeliveryMethod = DeliveryOption.Shipping;
                break;
            default:
                chosenDeliveryMethod = DeliveryOption.NoOption;
                break;
        }

        // the merchant may have disabled the delivery method that the buyer had chosen, but we'd act as if the method is still available
        if (chosenDeliveryMethod === DeliveryOption.SelfPickup) {
            allowSelfPickup = true;
        } else if (chosenDeliveryMethod === DeliveryOption.Shipping) {
            provideShipping = true;
        }
    }
        
    if (allowSelfPickup) {
        availableDeliveryMethods.push(DeliveryOption.SelfPickup);
        if (!isPaymentClicked) {
            chosenDeliveryMethod = DeliveryOption.SelfPickup;
        }
    }
    if (provideShipping) {
        availableDeliveryMethods.push(DeliveryOption.Shipping);
        if (!isPaymentClicked && !allowSelfPickup) {
            chosenDeliveryMethod = DeliveryOption.Shipping;
        }
    }
    
    hasFreeShippingPolicy = vdata.model.hasFreeShipping.toLowerCase() === 'true';
    freeShippingAmount = parseFloat(vdata.model.freeShippingAmount);
    freeShippingAmount = isNaN(freeShippingAmount) ? 0 : freeShippingAmount;
    adjustDeliveryCharge();
}

function renderReminder() {
    const $reminder = $('#reminderPopup');

    if (isPaymentClicked) {
        $reminder.append($('<h6 class="my-4">').append(
            $('<span class="text-warning mr-1">').append($('<i class="fa-solid fa-circle-exclamation">')),
            isOrderVoidOrPaid ? vdata.localizer.cannotEdit_VoidedOrPaid : vdata.localizer.cannotEdit_PaymentProcessing
        ));
    } 
}

function editProduct() {
    if (isPaymentClicked) { return; }

    const salesOrderLineID: string = $(this).data(DATASET_ORDERLINE);
    const relevantOrderLine = orderLines.filter(x => x.SalesOrderLineID === salesOrderLineID);
    if (!relevantOrderLine?.length) { return; }  // note: empty array is truthy, even though ([] == false) is evaluated as true

    $("#edit-popup").dxPopup({
        contentTemplate: () => editVariantTemplate(relevantOrderLine[0]),
        showTitle: true,
        title: vdata.localizer.editVariant,
        width: '250px',
        height: '250px',
        resizeEnabled: true,
        position: 'center',
    });

    const $popup = $("#edit-popup").dxPopup("instance");
    $popup.show();
}

function editQty() {
    if (isPaymentClicked) { return; }

    const salesOrderLineID: string = $(this).data(DATASET_ORDERLINE);
    const relevantOrderLine = orderLines.filter(x => x.SalesOrderLineID === salesOrderLineID);
    if (!relevantOrderLine?.length) { return; }  // note: empty array is truthy, even though ([] == false) is evaluated as true

    $("#edit-popup").dxPopup({
        contentTemplate: () => editQtyTemplate(relevantOrderLine[0]),
        showTitle: true,
        title: vdata.localizer.editQuantity,
        width: '250px',
        height: '250px',
        resizeEnabled: true,
        position: 'center',
    });

    const $popup = $("#edit-popup").dxPopup("instance");
    $popup.show();
}

function prepareCartTable() {
    if (!orderLines.length) { return; }

    const $cart = $('#cart-items').empty();    
    for (const line of orderLines) {
        // note: each row has 4 cells
        const $row = $(`<tr class="orderline-row">`).data(DATASET_ORDERLINE, line.SalesOrderLineID).appendTo($cart);

        // cell #1
        const $productImg = line.URL
            ? $('<div class="rounded">').append($(`<img src="${line.URL}" alt="${line.ProductName}" class="cart-image img-cover square">`))
            : $('<div class="empty-image rounded square">').append($('<i class="fa-solid fa-image">'))
        const $deleteButton = $(`<button class="btn p-1">`)
            .attr('title', vdata.localizer.delete)
            .data(DATASET_ORDERLINE, line.SalesOrderLineID)
            .on('click', deleteRow)
            .append($('<i class="fa-solid fa-trash">'));
        $('<td>').appendTo($row).append($('<div>').append(isPaymentClicked ? $productImg : [$productImg, $deleteButton]));

        // cell #2
        const $productDescription = $('<span class="mr-1">').html(line.ProductDesc);
        const $changeVariantButton = $(`<button class="btn p-1">`)
            .attr('title', vdata.localizer.titleEditVariant)
            .data(DATASET_ORDERLINE, line.SalesOrderLineID)
            .on('click', editProduct)
            .append($('<i class="fas fa-pen-to-square">'));
        $('<td class="text-left d-flex">').appendTo($row).append(
            $('<div class="flex-even mr-2">').html(line.ProductName),
            $('<div class="flex-even mr-2">').append(
                (!isPaymentClicked && line.ChildProductInfo?.length > 1)
                    ? [$productDescription, $changeVariantButton]
                    : $productDescription
            ) ,
        );

        // cell #3
        $('<td>').appendTo($row).text(StaticData.FormatCurrency(line.NetUnitPrice, vdata.model.currencyCode));

        // cell #4
        const $lineQty = $('<span class="mr-1">').html(String(line.Quantity));
        const $adjustQtyButton = $(`<button class="btn p-1">`)
            .attr('title', vdata.localizer.editQuantity)
            .data(DATASET_ORDERLINE, line.SalesOrderLineID)
            .on('click', editQty)
            .append($('<i class="fas fa-pen-to-square">'))
        $('<td class="text-left d-flex">').appendTo($row).append(
            $('<div class="flex-even mr-2">').append(isPaymentClicked ? $lineQty : [$lineQty, $adjustQtyButton]),
            $('<div class="flex-even mr-2">').html(line.UOMCode),
        );
    }
}

$(function () {        
    adjustGlobalVariables();
    prepareCartTable();
    recalculateTotals();
    renderReminder();
    prepareCustomerInfoForm();
    renderTotals();

    if (!isPaymentClicked) {
        $('.add-prd-btn').on('click', handleAddProduct);
    }
});