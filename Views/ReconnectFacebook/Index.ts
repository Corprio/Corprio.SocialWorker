import { PERMISSIONS } from '../Shared/Constants';

declare const vdata: {
    actions: {
        refreshAccessToken: string;
    };
    localizer: {
        cancel: string;
        confirm: string;
        confirmation: string;
        fbConnected: string;
        fbNotConnected: string;
        finalWarning: string;
    };
    settings: {
        metaApiID: string;
        metaApiVersion: string;
        shortName: string;
    };
};

// https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/facebook-js-sdk/index.d.ts
/// <reference types="facebook-js-sdk" />

// The order of the following three code blocks is extremely important. It has to be:
// (1) fbAsyncInit, (2) IIFE, then (3) whatever needs to run upon DOM being loaded
// For reference, see: https://www.devils-heaven.com/facebook-javascript-sdk-login
window.fbAsyncInit = function () {    
    FB.init({
        appId: vdata.settings.metaApiID,
        cookie: true,                     // Enable cookies to allow the server to access the session.
        xfbml: true,                     // Parse social plugins on this webpage.
        version: vdata.settings.metaApiVersion
    });
    
    FB.getLoginStatus(function (response) {   // Called after the JS SDK has been initialized.
        statusChangeCallback(response);        // Returns the login status.
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
    $('#loginBtn').on('click', function () {        
        FB.login((response: facebook.StatusResponse) => {
            if (response.authResponse) { checkLoginState(); }
        }, {
            scope: PERMISSIONS.toString(),
            return_scopes: true
        });
    });

    $('#logoutBtn').on('click', function () {        
        FB.logout(checkLoginState);
    });

    corprio.page.initTour({ defaultTour: 'reconnectfacebook.index', autoStart: true, driverCssLoaded: true });
})

/**
 * Check the user's FB login status
 */
function checkLoginState() {
    FB.getLoginStatus(function (response) {
        statusChangeCallback(response)
    })
}

/**
 * React to change in the user's Facebook login status
 * @param response-Facebook's response to getLoginStatus()
 */
function statusChangeCallback(response: facebook.StatusResponse) {  // Called with the results from FB.getLoginStatus().    
    if (response?.status === 'connected') {
        $('#loginBtn').hide();
        $('#logoutBtn').show();
        reassignMetaProfile(response.authResponse?.userID, response.authResponse?.accessToken, false);
    } else {
        // Not logged into Facebook or we are unable to tell.
        $('#loginBtn').show();
        $('#logoutBtn').hide();
        $('#reconnect-warning').show();
        const alert =
            `<div class="alert alert-warning">` +
                `<i class="fa-regular fa-circle-exclamation"></i>` +
                `&nbsp;${vdata.localizer.fbNotConnected.replaceAll('{0}', vdata.settings.shortName)}` +
            `</div>`
        $('#reconnect-dialogue').empty().append(alert);
    }
}

/**
 * Pass the short-lived user access token to server, which will check if another active Meta profile exists for the same Meta ID
 * @param metaId-Facebook user ID
 * @param accessToken-Short-lived user access token 
 * @param reAssignMetaProfile-True if the Facebook account can be reassigned from one organization to another
 * @returns
 */
function reassignMetaProfile(metaId: string, accessToken: string, reAssignMetaProfile: boolean) {
    return $.post({
        url: vdata.actions.refreshAccessToken,
        data: {
            metaId: metaId,
            token: accessToken,
            reAssignMetaProfile: reAssignMetaProfile,
        }
    }).done(function () {
        $('#reconnect-warning').hide();
        FB.api('/me', { fields: 'name' }, function (response: facebook.User) {
            const alert =
                `<div class="alert alert-success my-3">` +
                `<i class="fa-regular fa-circle-check"></i>` +
                `&nbsp;${vdata.localizer.fbConnected.replaceAll('{0}', response.name).replaceAll('{1}', vdata.settings.shortName)}` +
                `</div>`
            $('#reconnect-dialogue').empty().append(alert);
        });
    }).fail(function (jqXHR, textStatus, errorThrown) {
        // note: 409 means that another organization is connected with the Facebook account
        if (jqXHR.status !== 409) {
            FB.logout(checkLoginState);
            return corprio.formatError(jqXHR, textStatus, errorThrown);
        }
        
        var prompt = DevExpress.ui.dialog.custom({
            title: vdata.localizer.confirmation,
            messageHtml: `<p>${vdata.localizer.finalWarning}</p>`,
            buttons: [{
                text: vdata.localizer.confirm,
                onClick: function () { reassignMetaProfile(metaId, accessToken, true); },
                type: "danger",
            }, {
                text: vdata.localizer.cancel,
                onClick: function () { FB.logout(checkLoginState); },
                type: "normal",
            }]
        });
        prompt.show();        
    });
}
