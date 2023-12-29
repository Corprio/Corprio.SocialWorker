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

/***/ "./Views/Checkout/Index.ts":
/*!*********************************!*\
  !*** ./Views/Checkout/Index.ts ***!
  \*********************************/
/***/ (function(__unused_webpack_module, exports) {


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
function deleteSalesOrderLine(salesOrderLineID) {
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
function saveSalesOrderLine(salesOrderLineID, quantity) {
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
function updateDeliveryAddress(addressString) {
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
function confirmDeliveryAddress() {
    return __awaiter(this, void 0, void 0, function* () {
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
        yield updateDeliveryAddress(addressString);
        return;
    });
}
function deleteRow(obj) {
    return __awaiter(this, void 0, void 0, function* () {
        // note: the event target is supposed to be a button within a td within a tr
        const $row = $(obj.parentNode.parentNode);
        if (!$row.is('tr')) {
            return;
        }
        const salesOrderLineID = String($(obj.parentNode).find('input').val());
        yield deleteSalesOrderLine(salesOrderLineID);
        $row.remove();
        return;
    });
}
function editRow(obj) {
    // note: the event target is supposed to be a button within a td within a tr
    const $row = $(obj.parentNode.parentNode);
    if (!$row.is('tr')) {
        return;
    }
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
function saveRow(obj) {
    return __awaiter(this, void 0, void 0, function* () {
        // note: the event target is supposed to be a button within a td within a tr
        const $row = $(obj.parentNode.parentNode);
        if (!$row.is('tr')) {
            return;
        }
        const $input = $row.find('.sales-order-line-qty').find('.qty-face-value');
        const quantity = parseFloat(String($input.val()));
        if (isNaN(quantity) || quantity <= 0) {
            $input.addClass('is-invalid');
            return;
        }
        $input.removeClass('is-invalid');
        $input.attr('disabled', 'disabled');
        $input.parent().find('.qty-true-value').val(quantity); // save the quantity that is NOT rounded in the hidden input
        $input.val(quantity.toFixed(2)); // display the quantity that is rounded
        const salesOrderLineID = String($(obj.parentNode).find('input').val());
        $(obj.parentNode).find('.edit-line-btn').show();
        $(obj).hide();
        yield saveSalesOrderLine(salesOrderLineID, quantity);
        return;
    });
}
$(function () {
    return __awaiter(this, void 0, void 0, function* () {
        $('.save-line-btn').hide().on('click', function () { saveRow(this); });
        $('.edit-line-btn').on('click', function () { editRow(this); });
        $('.delete-line-btn').on('click', function () { deleteRow(this); });
        $('#confirm-address-btn').on('click', confirmDeliveryAddress);
    });
});


/***/ })

/******/ 	});
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module is referenced by other modules so it can't be inlined
/******/ 	var __webpack_exports__ = {};
/******/ 	__webpack_modules__["./Views/Checkout/Index.ts"](0, __webpack_exports__);
/******/ 	
/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Index.js.map