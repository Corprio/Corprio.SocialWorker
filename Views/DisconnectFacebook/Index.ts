declare const vdata: {
    actions: {
        terminateConnection: string;
    };
    localizer: {
        cancel: string;
        confirm: string;
        confirmation: string;
        fbDisconnected: string;
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

/**
 * Handle the user's decision to disconnet their organization from Facebook
 */
function disconnectFacebook() {
    // if the user has logged into Facebook, we should log them out now
    FB.getLoginStatus(function (response: facebook.StatusResponse) {
        if (response?.status === 'connected') { FB.logout(); }
    })

    $.post({
        url: vdata.actions.terminateConnection,
    }).done(function () {
        const alert =
            `<div class="alert alert-success my-3">` +
            `<i class="fa-regular fa-circle-check"></i>` +
            `&nbsp;${vdata.localizer.fbDisconnected.replaceAll('{0}', vdata.settings.shortName)}` +
            `</div>`
        $('#disconnect-dialogue').append(alert);
    }).fail(corprio.formatError);
}

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
    $('#disconnectBtn').on('click', function () {
        var prompt = DevExpress.ui.dialog.custom({
            title: vdata.localizer.confirmation,
            messageHtml: `<p>${vdata.localizer.finalWarning}</p>`,
            buttons: [{
                text: vdata.localizer.confirm,
                onClick: disconnectFacebook,
                type: "danger",
            }, {
                text: vdata.localizer.cancel,                
                type: "normal",
            }]
        });
        prompt.show();  
    });    
})

