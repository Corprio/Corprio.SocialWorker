# Social Media Marketer Application
## What is Social Media Marketer for?
The **Social Media Marketer Application** is an add-in web application for posting products on social media (including Facebook, Instagram and Line) and handles sales inquiry with a chatbot. The chatbot can add sales order in [Corprio](https://www.corprio.com).
## Technology
The application is developed using ASP.NET Core 8 MVC. It communicates with Facebook via Meta API and with Line via Line Messaging API.
## Register Social Media Marketer in Corprio
The Social Media Marketer Application must be registered in [Corprio Portal](https://portal.corprio.com) before users can subscribe to it.  Register the application with the parameters below.

**Application ID**: socialmediamarketer

**Permissions on data objects**:

|Entity Type|Permissions|
|-----------|----------|
|Sales Order|Read, Add New, Update, Approve, Void|
|Selling Price|Read|
|Warehouse|Read|
|Product|Read, Update|
|Product Type|Read|
|Brand|Read|
|Stock without Cost|Read|
|Customer|Read, Add New, Update|
|Customer Payment Method|Read|
|Company Settings|Read|
|One Time Password|Read, Add New|

**Sending email**: Required

**Application type**: Automated

## Set up webhook for development
When a customer sends a message to the chatbot (via Messenger or Line), the application receives a webhook notification. For Meta, the endpoint for receiving webhook must be manually configured in Meta Developer Portal. For Line, the endpoint is set programmatically (see _LineApiService.cs_). It is important to note that neither Meta nor Line accepts webhook endpoint on localhost. In development, developers can use ngrok to redirect the facebook requests to localhost.
1. Sign up at [ngrok](https://ngrok.com/)
2. Download and install the ngrok agent in the developer machine
3. Connect the ngrok agent to your account
4. Create a static domain in ngrok (optional step)
5. Redirect web requests to localhost by running the command
6. Update [WebhookCallbackUrl] in the appsettings.json file
```
ngrok http https://localhost:44330 --host-header="localhost:44330" --domain="YOUR_STATIC_DOMAIN_NAME"
```
![ngrok in action](resources/images/ngrok_in_action.png)

## To connect IG account from FB page
Follow the instructions in [https://www.facebook.com/business/help/connect-instagram-to-page](https://www.facebook.com/business/help/connect-instagram-to-page).