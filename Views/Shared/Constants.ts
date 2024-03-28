//export const PERMISSIONS: string[] = ['email', 'public_profile', 'business_management', 'pages_manage_metadata',
//    'pages_messaging', 'pages_manage_posts', 'pages_manage_engagement', 'instagram_basic', 'instagram_content_publish',
//    'instagram_manage_comments', 'instagram_manage_messages'];

/**
 * updates on 8 Feb 2024:
 * - advanced access for 'instagram_manage_comments' is NOT required for receiving webhook triggered by no-role users
 * - advanced access for 'pages_messaging' is NOT required for sending messages to no-role users
 * - advanced access for 'instagram_manage_messages' is required for sending messages to no-role users
 * - 'business_management' is required for viewing pages managed by the user
 * - 'pages_read_engagement' is required for creating posts on FB pages
 * - 'pages_manage_metadata' is required for managing webhooks
 * - 'pages_read_user_content' is required for reading comments on posts on FB pages
 * - 'pages_manage_engagement' is retained for making comments on other comments on FB pages
 */
//export const PERMISSIONS: string[] = ['email', 'public_profile', 'business_management', 'pages_show_list', 'pages_manage_metadata',
//    'pages_messaging', 'pages_manage_posts', 'instagram_basic', 'instagram_content_publish', 'pages_manage_engagement',
//    'instagram_manage_comments', 'instagram_manage_messages', 'pages_read_engagement', 'pages_read_user_content'];

export const PERMISSIONS: string[] = ['email', 'public_profile', 'business_management', 'pages_show_list', 'pages_manage_metadata',
    'pages_messaging', 'pages_manage_posts', 'instagram_basic', 'instagram_content_publish', 'instagram_manage_messages'];

export const WEBHOOKS: string[] = [
    /*'feed',*/
    // webhook for pages: https://developers.facebook.com/docs/graph-api/webhooks/getting-started/webhooks-for-pages/
    'messages',
    // any other webhook event: https://developers.facebook.com/docs/messenger-platform/webhook/#events
];