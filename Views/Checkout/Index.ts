declare const vdata: {
    actions: {
        deleteSalesOrderLine: string;
        editSalesOrderLine: string;
        finalizeSalesOrder: string;
    };
    localizer: {
    };
    settings: {
        allowSelfPickup: string;
        appUrl: string;
        billedPersonFamilyName: string;
        billedPersonGivenName: string;
        currencyCode: string;
        defaultCountryCode: string;
        deliveryChargeAmount: string;
        freeShippingAmount: string;
        hasFreeShipping: string;
        organizationID: string;
        paymentPortalUrl: string;
        provideShipping: string;
        salesOrderID: string;
        separator: string;
    };
};

enum DeliveryOption {    
    SelfPickup,
    Shipping
}

let allowSelfPickup: boolean;
let provideShipping: boolean;
let hasFreeShippingPolicy: boolean;

function deleteSalesOrderLine(salesOrderLineID: string) {
    return $.post({
        url: vdata.actions.deleteSalesOrderLine,
        data: {
            salesOrderID: vdata.settings.salesOrderID,
            salesOrderLineID: salesOrderLineID
        }
    }).done(() => {
        console.log(`Sales order line ${salesOrderLineID} was deleted.`);        
    }).fail(corprio.formatError);
}

function saveSalesOrderLine(salesOrderLineID: string, quantity: number) {
    return $.post({
        url: vdata.actions.editSalesOrderLine,
        data: {
            salesOrderID: vdata.settings.salesOrderID,
            salesOrderLineID: salesOrderLineID,
            quantity: quantity
        }
    }).done(() => {
        console.log(`Sales order line ${salesOrderLineID} was updated.`);
    }).fail(corprio.formatError);
}

function finalizeSalesOrder(addressString: string) {
    return $.ajax({
        url: vdata.actions.finalizeSalesOrder,
        type: 'POST',
        data: {
            salesOrderID: vdata.settings.salesOrderID,
            addressString: addressString,
            deliveryPreference: $('#delivery-method-value').val(),
        },
        success: function () {
            window.location.replace(`${vdata.settings.paymentPortalUrl}/T42/RecePayment/order/${vdata.settings.salesOrderID}?successUrl=${vdata.settings.appUrl}/${vdata.settings.organizationID}/thankyou&failUrl=${vdata.settings.appUrl}/${vdata.settings.organizationID}/paymentfailed`);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status !== 412) {
                corprio.formatError(jqXHR, textStatus, errorThrown);
                return;
            }

            $('#reminderPopup').dxPopup({
                visible: true,
                width: "300px",
                height: "200px",
                contentTemplate: (content) => {
                    var wrapper = $('<div class="d-flex flex-column h-100">');
                    wrapper.append(
                        $('<p>').text(textStatus).css({ "font-size": "15px" }),
                        $('<button id="reminderBtnCatalogue" class="btn btn-default btn-lg w-100 mt-auto">').text("Settings").on('click', () => { window.location.href = `/${vdata.settings.organizationID}/GetStarted/index` })
                    )
                    content.append(wrapper);
                },
                hideOnOutsideClick: false,
                showCloseButton: true,
                showTitle: true,
                title: "Error",
            })
        }
    });    
}

async function proceedToPayment() {    
    
    let addressString: string;

    let shippingSelected = false;
    
    // validate the delivery address only when it is necessary
    if (provideShipping && (!allowSelfPickup || $('#delivery-method-value').val() == 2)) {

        const line1 = $.trim(String($('#DeliveryAddress_Line1').val()));
        if (line1 === '') {
            $('#address-feedback1').show();
            return;
        }
        $('#address-feedback1').hide();

        const countryCode = $.trim(String($('input[name="DeliveryAddress_CountryAlphaCode"]').val()));
        if (StaticData.CountryList.find(x => x.Key === countryCode)) {
            $('#address-feedback2').hide();
        } else {
            $('#address-feedback2').show();
            return;
        }

        addressString = line1 + vdata.settings.separator +
            $('#DeliveryAddress_Line2').val() + vdata.settings.separator +
            $('#DeliveryAddress_City').val() + vdata.settings.separator +
            $('#DeliveryAddress_State').val() + vdata.settings.separator +
            $('#DeliveryAddress_PostalCode').val() + vdata.settings.separator +
            countryCode;        
    }
    await finalizeSalesOrder(addressString);
    return;
}

async function deleteRow(obj: HTMLElement) {            
    const $row = $(obj).closest('.orderline-row');    
    if (!$row.is('tr')) { return; }

    const salesOrderLineID = String($(obj.parentNode).find('input').val());
    await deleteSalesOrderLine(salesOrderLineID);
    $row.remove();
    recalculateTotals();
    return;
}

function editRow(obj: HTMLElement) {        
    const $row = $(obj).closest('.orderline-row');
    if (!$row.is('tr')) { return; }

    const $input = $row.find('.sales-order-line-qty').find('.qty-face-value');
    $input.removeAttr('disabled');

    // display the quantity that is NOT rounded    
    const unroundedQuantity = parseFloat(String($input.parent().find('.qty-true-value').val()));
    if (!isNaN(unroundedQuantity)) {        
        $input.val(unroundedQuantity);
    }

    $(obj.parentNode).find('.save-line-btn').show();
    $(obj).hide();    
    return;
}

async function saveRow(obj: HTMLElement) {        
    const $row = $(obj).closest('.orderline-row');
    if (!$row.is('tr')) { return; }

    const $input = $row.find('.sales-order-line-qty').find('.qty-face-value');    
    const quantity = parseFloat(String($input.val()));
    if (isNaN(quantity) || quantity <= 0) {
        $input.addClass('is-invalid');
        return;
    }
    $input.removeClass('is-invalid');

    $input.attr('disabled', 'disabled');    
    $input.parent().find('.qty-true-value').val(quantity);  // save the quantity that is NOT rounded in the hidden input
    $input.val(commaAndDecimals(quantity));  // display the quantity that is rounded
    const salesOrderLineID = String($(obj.parentNode).find('input').val());
    $(obj.parentNode).find('.edit-line-btn').show();
    $(obj).hide();    
    await saveSalesOrderLine(salesOrderLineID, quantity);
    recalculateTotals();
    return;
}

/**
 * Format a number into N,NNN.DD format (e.g., 1234.556 => 1,234.56)
 * @param num-A number
 * @returns A string with comma(s) and 2 decimals
 */
function commaAndDecimals(num: number) {
    const formattingOptions = { minimumFractionDigits: 2, maximumFractionDigits: 2 };
    return num.toLocaleString('en', formattingOptions)
}

function recalculateTotals() {
    let qtyTotal = 0;
    let subTotal = 0;

    $('.orderline-row').each(function () {
        const $qty = $(this).find('.qty-true-value');
        const qty = parseFloat(String($qty.val()));
        qtyTotal += isNaN(qty) ? 0 : qty;

        const $price = $(this).find('.price-true-value');
        const price = parseFloat(String($price.val()));
        subTotal += isNaN(price) ? 0 : qty * price;
    });
   
    $('#order-subtotal').text(vdata.settings.currencyCode + ' ' + commaAndDecimals(subTotal));    

    // note: we don't show the rows for delivery charge and free shipping unless shipping is selected
    let deliveryCharge = 0;
    if (provideShipping && (!allowSelfPickup || $('#delivery-method-value').val() == 2)) {        
        deliveryCharge = parseFloat(vdata.settings.deliveryChargeAmount);        
        $('#delivery-charge-div').show();
        $('#delivery-charge-amt').text(vdata.settings.currencyCode + ' ' + commaAndDecimals(deliveryCharge));        

        // note: we don't show the row for free shipping unless there is a free shipping policy
        if (hasFreeShippingPolicy) {            
            const threshold = parseFloat(vdata.settings.freeShippingAmount);
            if (subTotal >= threshold) {
                $('#free-shipping-div').show();
                const text = '-' + vdata.settings.currencyCode + ' ' + commaAndDecimals(deliveryCharge)
                $('#free-shipping-amt').text(text);
                deliveryCharge = 0;
            } else {
                $('#free-shipping-div').hide();
            }
        } 
    } else {
        $('#delivery-charge-div').hide();
        $('#free-shipping-div').hide();
    }

    $('#order-total').text(vdata.settings.currencyCode + ' ' + commaAndDecimals(subTotal + deliveryCharge));
}

function selectDeliveryOption(obj: HTMLElement) {
    if (obj.id === 'radio-self-pickup') {
        $('#delivery-method-value').val(1);
    } else if (obj.id === 'radio-shipping') {
        $('#delivery-method-value').val(2);
    }
    recalculateTotals();
    return;
}

function prepareBillToSection() {    
    const billToDiv = $('#bill-person');    
    $("<div>")
        .addClass("d-flex mb-2")
        .appendTo(billToDiv)
        .append(
            $('<div id="BillPerson_GivenName">')
                .dxTextBox({                    
                    value: vdata.settings.billedPersonGivenName,
                    maxLength: 100,
                    label: "Given Name",
                    labelMode: 'floating',
                    inputAttr: {
                        autocomplete: "given-name",
                        name: 'BillPerson.GivenName',
                        id: 'BillPerson.GivenName'
                    },
                    onValueChanged: function (e) {
                        const deliveryName = $("#DeliveryContactPerson_GivenName");
                        if (deliveryName.length) deliveryName.dxTextBox('option', 'value', e.value);
                    }
                })
                .dxValidator({
                    validationGroup: "purchase",
                    validationRules: [{ type: "required", message: "Given name cannot be blank." }]
                }),
            $('<div id="BillPerson_FamilyName">')
                .dxTextBox({                    
                    value: vdata.settings.billedPersonFamilyName,
                    maxLength: 100,
                    label: "Family Name",
                    labelMode: 'floating',
                    inputAttr: {
                        autocomplete: "family-name",
                        name: 'BillPerson.FamilyName',
                        id: 'BillPerson.FamilyName'
                    },
                    onValueChanged: function (e) {
                        const deliveryName = $("#DeliveryContactPerson_FamilyName");
                        if (deliveryName.length) deliveryName.dxTextBox('option', 'value', e.value)
                    }
                })
                .dxValidator({
                    validationGroup: "purchase",
                    validationRules: [{ type: "required", message: "Family name cannot be blank." }]
                }),
        );
    
    const phoneWidget = $('<div id="ContactPhone">').appendTo(billToDiv);
    corprio.geography.addPhoneNumberTo(
        phoneWidget,
        { NumberType: 3, CountryCallingCode: vdata.settings.defaultCountryCode },
        "ContactPhone",
        null,
        function (p) {
            const deliveryPhone = $("#DeliveryPhoneNumber");
            if (deliveryPhone.length) corprio.geography.setPhoneNumber(deliveryPhone, p);
        },
        true,
        'purchase');
}

$(async function () {    
    allowSelfPickup = vdata.settings.allowSelfPickup.toLowerCase() === 'true';
    provideShipping = vdata.settings.provideShipping.toLowerCase() === 'true';
    hasFreeShippingPolicy = vdata.settings.hasFreeShipping.toLowerCase() === 'true';

    $('#delivery-charge-div').hide();
    $('#free-shipping-div').hide();    
    recalculateTotals();

    prepareBillToSection();
    
    $("input[name=radio-delivery-method]:radio").on('change', function () {
        selectDeliveryOption(this);
    });

    // note: self-pickup is selected by default, regardless if shipping is provided as an option
    if (allowSelfPickup) {
        $('#radio-self-pickup').attr('checked', 'checked');
        $('#delivery-method-value').val(1);
    } else if (provideShipping) {
        $('#radio-shipping').attr('checked', 'checked');
        $('#delivery-method-value').val(2);
    }

    $('.save-line-btn').hide().on('click', function () { saveRow(this) });
    $('.edit-line-btn').on('click', function () { editRow(this) });
    $('.delete-line-btn').on('click', function () { deleteRow(this) });
    $('#confirm-address-btn').on('click', proceedToPayment);
});