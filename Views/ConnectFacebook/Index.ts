declare const vdata: {
    actions: {
        refreshAccessToken: string;
    };
    applicationSetting: {
    };
};

// 'business_management' is required for viewing pages managed by the user
const permissions: string[] = ['email', 'public_profile', 'business_management', 'pages_manage_metadata',
    'pages_messaging', 'pages_manage_posts', 'pages_manage_engagement', 'instagram_basic', 'instagram_content_publish',
    'instagram_manage_comments', 'instagram_manage_messages'];

// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />

// Reference for the order of the following three code blocks: https://www.devils-heaven.com/facebook-javascript-sdk-login
window.fbAsyncInit = function () {
    console.log('fbAsyncInit is doing its things...');       
    FB.init({
        appId: '312852768233605',
        cookie: true,                     // Enable cookies to allow the server to access the session.
        xfbml: true,                     // Parse social plugins on this webpage.
        version: 'v18.0'           // Use this Graph API version for this call.
    });
    
    FB.getLoginStatus(function (response) {   // Called after the JS SDK has been initialized.
        statusChangeCallback(response);        // Returns the login status.
    });
};

(function (element: Document, tagName: string, selector: string) {
    var js, fjs = element.getElementsByTagName(tagName)[0];
    if (element.getElementById(selector)) { return; }
    js = element.createElement(tagName); js.id = selector;
    js.src = "https://connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk')
);

document.addEventListener("DOMContentLoaded", (event) => {
    console.log("DOM fully loaded and parsed");

    $('#loginBtn').on('click', function() {
        console.log('Logging into Facebook...');
        FB.login((response: facebook.StatusResponse) => {
            if (response.authResponse) {
                //user just authorized your app                
                checkLoginState();
            } 
        }, {            
            scope: permissions.toString(),
            return_scopes: true
        });
    });

    $('#logoutBtn').on('click', function () {
        console.log('Logging out from Facebook...')
        FB.logout(checkLoginState);
    });
});

function statusChangeCallback(response: facebook.StatusResponse) {  // Called with the results from FB.getLoginStatus().
    console.log('statusChangeCallback');
    console.log(response);
    if (response?.status === 'connected') {
        // Logged into your webpage and Facebook.
        $('#loginBtn').hide();
        $('#logoutBtn').show();
        FB.api('/me', { fields: 'name' }, function (response: facebook.User) {            
            $('#page-headline').text('Good to have you back, ' + response.name + '!');
        });
        $('#page-var-text').text('Corprio Social Worker can now do the following: ');
        getPages();
        refreshAccessToken(response.authResponse?.userID, response.authResponse?.accessToken);
    } else {
        // Not logged into your webpage or we are unable to tell.
        $('#loginBtn').show();
        $('#logoutBtn').hide();
        $('#page-headline').text('Welcome to Corprio Social Worker!');        
        $('#page-var-text').text('Click the login button below to connect your Facebook page(s) and Instagram accounts with Corprio. ' +
            'Once it is done, Corprio Social Worker can do the following magic for you: ');
    }
}

function checkLoginState() {
    // Called when a person is finished with the Login/Logout Button.    
    FB.getLoginStatus(function (response) {        
        statusChangeCallback(response)
    })
}

function refreshAccessToken(metaId: string, accessToken: string) {
    return $.post({
        url: vdata.actions.refreshAccessToken,
        data: {
            metaId: metaId,
            token: accessToken,            
        }
    }).done(function () {
        console.log(`Token for ${metaId} is fed to backend successfully.`);
    }).fail(function () {
        console.log(`Failed to pass the token for ${metaId} to backend.`);
    });
}

// add webhooks to page subscriptions (IMPORTANT: subscribe to the fields as those subscribed on App level)
// https://developers.facebook.com/docs/messenger-platform/webhooks/#connect-your-app
function addPageSubscriptions(page_id: string, page_access_token: string) {
    console.log(`Subscribing to page ${page_id}...`);
    return FB.api(
        `/${page_id}/subscribed_apps`,
        'post',
        {
            subscribed_fields: [
                'feed',
                // webhook for pages: https://developers.facebook.com/docs/graph-api/webhooks/getting-started/webhooks-for-pages/
                'messages',
                // any other webhook event: https://developers.facebook.com/docs/messenger-platform/webhook/#events
            ],
            access_token: page_access_token,
        },
        function (response: any) {
            if (response && !response.error) {
                console.log({ response });                
            } else {
                console.error(response?.error);
            }
        },
    )
}

function getPages() {
    FB.api('/me/accounts', function (response: facebook.AuthResponse | any) {
        if (response && !response.error) {
            console.log('response from getPages()...');
            console.log({ response });            
            for (let i = 0; i < response.data.length; i++) {
                const page_id = response.data[i].id;
                const page_access_token = response.data[i].access_token;                
                addPageSubscriptions(page_id, page_access_token);
            }
        } else {
            console.error(response.error);
        }
    })
}