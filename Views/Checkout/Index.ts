declare const vdata: {
    actions: {
        deleteSalesOrderLine: string;
        editSalesOrderLine: string;
        updateDeliveryAddress: string;
    };
    localizer: {
    };
    settings: {
        organizationID: string;
        salesOrderID: string;
        separator: string;
    };
};

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

function updateDeliveryAddress(addressString: string) {
    return $.post({
        url: vdata.actions.updateDeliveryAddress,
        data: {
            salesOrderID: vdata.settings.salesOrderID,
            addressString: addressString
        }
    }).done(() => {
        console.log(`Delivery address for ${vdata.settings.salesOrderID} was updated.`);
    }).fail(corprio.formatError);
}

async function confirmDeliveryAddress() {    
    const line1 = $.trim(String($('#DeliveryAddress_Line1').val()));
    if (line1 === '') {
        $('#address-feedback1').show();
        return;
    }
    $('#address-feedback1').hide();

    const countryCode = $.trim(String($('input[name="DeliveryAddress_CountryAlphaCode"]').val()));
    if (countryCode !== '' && countryCode.length != 2) {
        $('#address-feedback2').show();
        return;
    }    
    $('#address-feedback2').hide();

    const addressString = line1 + vdata.settings.separator +
        $('#DeliveryAddress_Line2').val() + vdata.settings.separator +
        $('#DeliveryAddress_City').val() + vdata.settings.separator +
        $('#DeliveryAddress_State').val() + vdata.settings.separator +
        $('#DeliveryAddress_PostalCode').val() + vdata.settings.separator +
        countryCode;
    
    await updateDeliveryAddress(addressString);
    return;
}

async function deleteRow(obj: HTMLElement) {        
    // note: the event target is supposed to be a button within a td within a tr
    const $row = $(obj.parentNode.parentNode);
    if (!$row.is('tr')) { return; }

    const salesOrderLineID = String($(obj.parentNode).find('input').val());
    await deleteSalesOrderLine(salesOrderLineID);
    $row.remove();
    return;
}

function editRow(obj: HTMLElement) {    
    // note: the event target is supposed to be a button within a td within a tr
    const $row = $(obj.parentNode.parentNode);
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
    // note: the event target is supposed to be a button within a td within a tr
    const $row = $(obj.parentNode.parentNode);
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
    $input.val(quantity.toFixed(2));  // display the quantity that is rounded
    const salesOrderLineID = String($(obj.parentNode).find('input').val());
    $(obj.parentNode).find('.edit-line-btn').show();
    $(obj).hide();    
    await saveSalesOrderLine(salesOrderLineID, quantity);
    return;
}

$(async function () {    
    $('.save-line-btn').hide().on('click', function () { saveRow(this) });
    $('.edit-line-btn').on('click', function () { editRow(this) });
    $('.delete-line-btn').on('click', function () { deleteRow(this) });
    $('#confirm-address-btn').on('click', confirmDeliveryAddress);
});