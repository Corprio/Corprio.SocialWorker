# Social Worker Application
## What is Social Worker for?
The **Social Worker Application** is an add-in web application for posting products/catalogues on Facebook/Instagram and adding sales order in [Corprio](https://www.corprio.com) using conversations with a chatbot.
## Technology
The application is developed using ASP.NET Core 6. It communicates with Facebook via Meta API.
## Register Social Worker in Corprio
The Social Worker Application must be registered in [Corprio Portal](https://portal.corprio.com) before users can subscribe to it.  Register the application with the parameters below.

**Application ID**: corprio-socialworker

**Permissions on data objects**:

|Entity Type|Permissions|
|-----------|----------|
|Sales Order|Read, Add New, Update, Approve|
|Invoice|Read, Add New, Update|
|Selling Price|Read|
|Warehouse|Read|
|Product|Read|
|Customer|Read, Write|

**Sending email**: Required
