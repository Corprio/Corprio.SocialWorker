import { PERMISSIONS, WEBHOOKS } from '../Shared/Constants';

declare const vdata: {
    localizer: {
        error: string;
        fbAlreadyConnected: string;
        fbConnected: string;
        fbNotConnected: string;
        storyMentionCreator: string;
        storyMentionDate: string;
        storyMentionHint: string;
        storyMentionMentioned: string;
    };
    settings: {
        appBaseUrl: string;
        metaApiID: string;
        metaApiVersion: string;
        organizationID: string;
        shortName: string;
    };
    url: {
        getPage: string;
        testCDN: string;
    };
};

// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />


/**
 * React to change in the user's Facebook login status
 * @param response-Facebook's response to getLoginStatus()
 */
function handleFbLoginStatusChange(response: facebook.StatusResponse) {
    /*console.log(response);*/
    if (response?.status === 'connected') {                
        $('#loginBtn').hide();
        $('#logoutBtn').show();
        FB.api('/me', { fields: 'name' }, function (response: facebook.User) {            
            const alert =
                `<div class="alert alert-success my-3">` +
                    `<i class="fa-regular fa-circle-check"></i>` +
                    `&nbsp;${vdata.localizer.fbConnected.replaceAll('{0}', response.name).replaceAll('{1}', vdata.settings.shortName)}` +
                `</div>`
            $('#fb-dialogue').empty().append(alert);
            $('#mentions').show();
            //$.ajax({
            //    url: vdata.url.getPage,
            //    method: 'GET',
            //    data: { metaUserId: response.id },
            //    success: function (data) {
            //        console.log('mentions:');
            //        console.log(data);
            //    }
            //});

            $('#mentions').dxDataGrid({
                dataSource: {
                    store: {
                        type: 'odata',
                        version: 2,
                        url: vdata.url.getPage,                        
                        beforeSend(request) {
                            request.params.metaUserId = response.id;
                        },
                    },
                },
                paging: { pageSize: 10 },
                pager: {
                    showPageSizeSelector: true,
                    allowedPageSizes: [10, 25, 50, 100],
                },
                remoteOperations: false,
                searchPanel: {
                    visible: true,
                    highlightCaseSensitive: true,
                },
                /*groupPanel: { visible: true },*/
                grouping: { autoExpandAll: false },
                allowColumnReordering: true,
                rowAlternationEnabled: true,
                showBorders: true,
                width: '100%',
                columns: [
                    {
                        dataField: 'CreateDate',
                        caption: vdata.localizer.storyMentionDate,
                        dataType: 'datetime',
                        sortOrder: 'desc'
                    },
                    {
                        dataField: 'Mentioned',
                        caption: vdata.localizer.storyMentionMentioned,
                        dataType: 'string'
                    },
                    {
                        dataField: 'CreatorName',
                        caption: vdata.localizer.storyMentionCreator,
                        dataType: 'string'
                    },                    
                    {
                        type: 'buttons',
                        buttons: [{
                            name: 'Story',
                            icon: 'find',
                            hint: vdata.localizer.storyMentionHint,
                            onClick: function (e) {
                                window.open(e.row.data.CDNUrl, '_blank');                                
                            }
                        }]
                    }
                    
                ],                
            });
        });
        /*The following functions run in a non-blocking manner because there is no interdependence*/
        getPages();        
    } else {                
        $('#loginBtn').show();
        $('#logoutBtn').hide();
        const alert =
            `<div class="alert alert-warning">` +
                `<i class="fa-regular fa-circle-exclamation"></i>` +
                `&nbsp;${vdata.localizer.fbNotConnected.replaceAll('{0}', vdata.settings.shortName)}` +
            `</div>`
        $('#fb-dialogue').empty().append(alert);
        $('#mentions').hide();   
    }
}


/**
 * Check the user's FB login status
 */
function checkLoginState() {
    FB.getLoginStatus(function (response) {
        handleFbLoginStatusChange(response)
    })
}

/**
 * Turn on Meta's Built-in NLP to help detect locale (and meaning)
 * (note: we run this function on the client side to reduce workload on the server side)
 * https://developers.facebook.com/docs/graph-api/reference/page/nlp_configs/
 * https://developers.facebook.com/docs/messenger-platform/built-in-nlp/
 * @param page_id-ID of Facebook page
 * @param page_access_token-Page access token
 * @returns
 */
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

/**
 * Add webhooks to page subscriptions (IMPORTANT: subscribe to the fields as those subscribed on App level)
 * (note: we run this function on the client side to reduce workload on the server side)
 * https://developers.facebook.com/docs/messenger-platform/webhooks/#connect-your-app
 * @param page_id-ID of Facebook page
 * @param page_access_token-Page access token
 * @returns
 */
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

/**
 * Query the pages on which the user has a role and trigger webhook subscription for each of them  
 */
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

// The order of the following three code blocks is extremely important. It has to be:
// (1) fbAsyncInit, (2) IIFE, then (3) whatever needs to run upon DOM being loaded
// For reference, see: https://www.devils-heaven.com/facebook-javascript-sdk-login
window.fbAsyncInit = function () {    
    FB.init({
        appId: vdata.settings.metaApiID,
        cookie: true,  // Enable cookies to allow the server to access the session.
        xfbml: true,  // Parse social plugins on this webpage.
        version: vdata.settings.metaApiVersion
    });

    FB.getLoginStatus(function (response) {   // Called after the JS SDK has been initialized.
        handleFbLoginStatusChange(response);        // Returns the login status.
    });
};

/**
 * IIFE to make a reference to the SDK, if it does not already exist
 */
(function (element: Document, tagName: string, selector: string) {
    var js, fjs = element.getElementsByTagName(tagName)[0];
    if (element.getElementById(selector)) { return; }
    js = element.createElement(tagName); js.id = selector;
    js.src = "https://connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk')
);

/**
 * Entry point
 */
$(function () {
    

    /*corprio.page.initTour({ defaultTour: 'getstarted.index', autoStart: true });*/

    // facebook-related stuff
    $('#loginBtn').on('click', function () {
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