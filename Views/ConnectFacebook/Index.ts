import { PERMISSIONS, WEBHOOKS } from '../Shared/Constants';

declare const vdata: {
    actions: {
        refreshAccessToken: string;
    };
};

// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />

// For the order of the following three code blocks, see: https://www.devils-heaven.com/facebook-javascript-sdk-login
window.fbAsyncInit = function () {    
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
            scope: PERMISSIONS.toString(),
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
    /*console.log(response);*/
    if (response?.status === 'connected') {
        // Logged into Facebook.
        $('#loginBtn').hide();
        $('#logoutBtn').show();
        FB.api('/me', { fields: 'name' }, function (response: facebook.User) {            
            $('#page-headline').text('Good to have you back, ' + response.name + '!');
        });
        $('#page-var-text').text('Corprio Social Worker can now do the following: ');
        getPages();
        refreshAccessToken(response.authResponse?.userID, response.authResponse?.accessToken);
    } else {
        // Not logged into Facebook or we are unable to tell.
        $('#loginBtn').show();
        $('#logoutBtn').hide();
        $('#page-headline').text('Welcome to Corprio Social Worker!');        
        $('#page-var-text').text('Click the login button below to connect your Facebook page(s) and Instagram accounts with Corprio. ' +
            'Once it is done, Corprio Social Worker can do the following magic for you: ');
    }
}

// Called when the user is finished with the Login/Logout Button.
function checkLoginState() {    
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

// turn on Meta's Built-in NLP to help detect locale (and meaning)
// (note: we run this function on the client side to reduce workload on the server side)
// https://developers.facebook.com/docs/graph-api/reference/page/nlp_configs/
// https://developers.facebook.com/docs/messenger-platform/built-in-nlp/
function turnOrNLP(page_id: string, page_access_token: string) {
    console.log(`Turning on NLP for page ${page_id}`);
    return FB.api(
        `/${page_id}/nlp_configs`,
        'post',
        {
            nlp_enabled: true,
            model: 'CHINESE',
            access_token: page_access_token
        },
        function (response: any) {
            console.log('Response from nlp_configs:');
            console.log(response);
        }
    );
}

// add webhooks to page subscriptions (IMPORTANT: subscribe to the fields as those subscribed on App level)
// (note: we run this function on the client side to reduce workload on the server side)
// https://developers.facebook.com/docs/messenger-platform/webhooks/#connect-your-app
function addPageSubscriptions(page_id: string, page_access_token: string) {
    console.log(`Subscribing to page ${page_id}`);
    return FB.api(
        `/${page_id}/subscribed_apps`,
        'post',
        {
            subscribed_fields: WEBHOOKS,
            access_token: page_access_token,
        },
        function (response: any) {
            console.log('Response from subscribed_apps:');
            console.log(response);
            if (response && !response.error) {
                return turnOrNLP(page_id, page_access_token);
            }
        },
    )
}

function getPages() {
    FB.api('/me/accounts', function (response: facebook.AuthResponse | any) {
        if (response && !response.error) {
            /*console.log('response from getPages()...');*/
            /*console.log({ response });*/
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