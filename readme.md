# Social Worker Application
## What is Social Worker for?
The **Social Worker Application** is an add-in web application for posting products/catalogues on Facebook/Instagram and adding sales order in [Corprio](https://www.corprio.com) using conversations with a chatbot.
## Technology
The application is developed using ASP.NET Core 8 MVC. It communicates with Facebook via Meta API.
## Register Social Worker in Corprio
The Social Worker Application must be registered in [Corprio Portal](https://portal.corprio.com) before users can subscribe to it.  Register the application with the parameters below.

**Application ID**: corprio-socialworker

**Permissions on data objects**:

|Entity Type|Permissions|
|-----------|----------|
|Sales Order|Read, Add New, Update, Approve|
|Selling Price|Read|
|Warehouse|Read|
|Product|Read, Update|
|Product Type|Read|
|Brand|Read|
|Stock without Cost|Read|
|Customer|Read, Add New, Update|
|Customer Payment Method|Read|
|Organization Setting|Read|

**Sending email**: Required

**Application type**: Automated

## Set up webhook for development
The application uses webhook to respond facebook for messages inputted by customers in messengers.  In development, developers can use ngrok to redirect the facebook requests to localhost.
1. Sign up at [ngrok](https://ngrok.com/)
1. Download and install the ngrok agent in the developer machine
1. 
1. Connect the ngrok agent to your account
1. Create a static domain in ngrok (optional step)
1. Redirect web requests to localhost by running the command
```
ngrok http --domain=pleasant-definitely-macaw.ngrok-free.app https://localhost:44330 --host-header="localhost:44330"
```
![ngrok in action](resources/images/ngrok_in_action.png)