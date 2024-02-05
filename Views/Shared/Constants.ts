// 'business_management' is required for viewing pages managed by the user
//export const PERMISSIONS: string[] = ['email', 'public_profile', 'business_management', 'pages_manage_metadata',
//    'pages_messaging', 'pages_manage_posts', 'pages_manage_engagement', 'instagram_basic', 'instagram_content_publish',
//    'instagram_manage_comments', 'instagram_manage_messages'];

// updates on 5 Feb 2024:
// tested that 'pages_manage_engagement' was uncessary
// 'business_management' is required for viewing pages managed by the user
// 'pages_read_engagement' is required for creating posts on FB pages
// 'pages_manage_metadata' is required for managing webhooks
// 'pages_read_user_content' is required for reading comments on posts on FB pages
export const PERMISSIONS: string[] = ['public_profile', 'business_management', 'pages_show_list', 'pages_manage_metadata',
    'pages_messaging', 'pages_manage_posts', 'instagram_basic', 'instagram_content_publish',
    'instagram_manage_comments', 'instagram_manage_messages', 'pages_read_engagement', 'pages_read_user_content'];