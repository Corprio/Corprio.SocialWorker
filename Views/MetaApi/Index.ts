declare const vdata: {
    actions: {
        refreshAccessToken: string;
    };
    applicationSetting: {
    };
};

// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />

window.fbAsyncInit = function () {
    console.log('fbAsyncInit is doing its things...');
    FB.init({
        appId: '312852768233605',
        cookie: true,                     // Enable cookies to allow the server to access the session.
        xfbml: true,                     // Parse social plugins on this webpage.
        version: 'v18.0'           // Use this Graph API version for this call.
    });

    document.addEventListener("DOMContentLoaded", (event) => {
        console.log("DOM fully loaded and parsed");
        $('fb:login-button').on('click', checkLoginState);
    });

    FB.getLoginStatus(function (response) {   // Called after the JS SDK has been initialized.
        statusChangeCallback(response);        // Returns the login status.
    });
};

function statusChangeCallback(response: facebook.StatusResponse) {  // Called with the results from FB.getLoginStatus().
    console.log('statusChangeCallback');
    console.log(response);                   // The current login status of the person.
    if (response?.status === 'connected') {   // Logged into your webpage and Facebook.                                        
        getPages();
        refreshAccessToken(response.authResponse?.userID, response.authResponse?.accessToken);
    } else {                                 // Not logged into your webpage or we are unable to tell.
        $('#status').text('Please log into this webpage.');
    }
}

function checkLoginState() {
    // Called when a person is finished with the Login Button.    
    FB.getLoginStatus(function (response) {
        // See the onlogin handler
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
