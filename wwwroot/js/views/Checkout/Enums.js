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
  !*** ./Views/Checkout/Enums.ts ***!
  \*********************************/

Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.CorprioPhoneNumberType = exports.DeliveryOption = void 0;
// IMPORTANT: the order of the following options must be in line with that defined on the server side
var DeliveryOption;
(function (DeliveryOption) {
    DeliveryOption[DeliveryOption["NoOption"] = 0] = "NoOption";
    DeliveryOption[DeliveryOption["SelfPickup"] = 1] = "SelfPickup";
    DeliveryOption[DeliveryOption["Shipping"] = 2] = "Shipping";
})(DeliveryOption || (exports.DeliveryOption = DeliveryOption = {}));
var CorprioPhoneNumberType;
(function (CorprioPhoneNumberType) {
    CorprioPhoneNumberType[CorprioPhoneNumberType["General"] = 0] = "General";
    CorprioPhoneNumberType[CorprioPhoneNumberType["Home"] = 1] = "Home";
    CorprioPhoneNumberType[CorprioPhoneNumberType["Work"] = 2] = "Work";
    CorprioPhoneNumberType[CorprioPhoneNumberType["Mobile"] = 3] = "Mobile";
    CorprioPhoneNumberType[CorprioPhoneNumberType["Fax"] = 4] = "Fax";
})(CorprioPhoneNumberType || (exports.CorprioPhoneNumberType = CorprioPhoneNumberType = {}));

})();

/******/ 	return __webpack_exports__;
/******/ })()
;
});
//# sourceMappingURL=Enums.js.map