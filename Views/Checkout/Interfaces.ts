import { CorprioPhoneNumberType, DeliveryOption } from './Enums';

export interface CorprioAddress {
    Line1: string,
    Line2: string,
    City: string,
    State: string,
    PostalCode: string,
    CountryAlphaCode: string,
}

export interface CorprioPerson {
    GivenName: string,
    FamilyName: string,
}

export interface CorprioPhoneNumber {
    NumberType: CorprioPhoneNumberType,
    SubscriberNumber: string,
    NationalDestinationCode: string,
    CountryCallingCode: string,
}

// IMPORTANT: the properties of this interface must be in line with that defined on the server side
export interface CheckoutDataModel {
    BillPerson: CorprioPerson,
    BillContactPhone: CorprioPhoneNumber,
    ChosenDeliveryMethod: DeliveryOption,
    DeliveryAddress?: CorprioAddress,
    DeliveryContact?: CorprioPerson,
    DeliveryContactPhone?: CorprioPhoneNumber,
    SalesOrderID: string,
}

// IMPORTANT: the properties of this interface must be in line with that defined on the server side
export interface CheckoutOrderLine {    
    ChildProductInfo: ChildProductInfo[],
    DisallowOutOfStock: boolean,
    NetUnitPrice: number,
    ProductDesc: string,
    ProductID: string,
    ProductName: string,    
    ProductStockLevel: number,    
    Quantity: number,
    SalesOrderLineID: string,
    UOMCode: string,
    URL: string,
}

export interface ProductVariationInfo {
    Attribute: string,
    Code: string,
    Name: string,
}

export interface ChildProductInfo {
    ID: string,
    ChildProductAttributes: ProductVariationInfo[],
    DisallowOutOfStock: boolean,
    ProductStockLevel: number,
}