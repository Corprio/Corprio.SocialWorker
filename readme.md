# Social Worker Application
## What is Social Worker for?
The **Social Worker Application** is an add-in web application for adding sales order and invoices in [Corprio](https://www.corprio.com) using conversations with a chatbot.
## Technology
The application is developed using ASP.NET Core 6.
## Register Social Worker in Corprio
The Social Worker Application must be registered in [Corprio Portal](https://portal.corprio.com) before users can subscribe it.  Register the application with the following parameters.

**Application ID**: corprio-socialworker

**Permissions on data objects**:

|Entity Type|Permissions|
|-----------|----------|
|Sales Order|Read, Add New, Update, Approve|
|Invoice|Read, Add New, Update|
|Selling Price|Read|
|Warehouse|Read|
|Product|Read|
|Customer|Read|

**Sending email**: Not required
